namespace BotwTrainer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;

    using BotwTrainer.Properties;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        // The original list of values that take effect when you save / load
        private const uint SaveItemStart = 0x3FCE8FF0;

        private const uint CodeHandlerStart = 0x01133000;

        private const uint CodeHandlerEnd = 0x01134300;

        private const uint CodeHandlerEnabled = 0x10014CFC;

        private readonly List<TextBox> tbChanged = new List<TextBox>();

        private readonly List<ComboBox> ddChanged = new List<ComboBox>();

        private readonly List<CheckBox> cbChanged = new List<CheckBox>();

        // Technically your first item as they are stored in reverse so we work backwards
        private const uint ItemEnd = 0x43CA2AEC;

        //private uint itemStart = 0x43C6B2AC;

        private int itemTotal = 0;

        private List<Item> items;

        private JToken json;

        private TcpConn tcpConn;

        private Gecko gecko;

        private Codes codes;

        private bool connected;

        public MainWindow()
        {
            this.InitializeComponent();

            this.Loaded += this.MainWindowLoaded;
        }

        private bool HasChanged
        {
            get
            {
                return this.tbChanged.Any() || this.cbChanged.Any() || this.ddChanged.Any();
            }
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Testing
            // this.TabControl.IsEnabled = true;

            this.Title = string.Format("{0} v{1}", this.Title, Settings.Default.CurrentVersion);

            this.items = new List<Item>();

            this.codes = new Codes(this);

            var client = new WebClient
            {
                BaseAddress = Settings.Default.VersionUrl,
                Encoding = Encoding.UTF8,
                CachePolicy =
                    new System.Net.Cache.RequestCachePolicy(
                    System.Net.Cache.RequestCacheLevel.BypassCache)
            };

            client.Headers.Add("Cache-Control", "no-cache");
            client.DownloadStringCompleted += this.ClientDownloadStringCompleted;

            // try to get current version
            try
            {
                client.DownloadStringAsync(new Uri(string.Format("{0}{1}", client.BaseAddress, "version.txt")));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading current version.");
            }

            // try to load json data
            try
            {
                var file = Assembly.GetExecutingAssembly().GetManifestResourceStream("BotwTrainer.items.json");
                if (file != null)
                {
                    using (var reader = new StreamReader(file))
                    {
                        var data = reader.ReadToEnd();
                        this.json = JObject.Parse(data);

                        this.JsonViewer.Load(data);

                        // Shrine data
                        var shrines = this.json.SelectToken("Shrines").Value<JObject>().Properties().ToList().OrderBy(x => x.Name);
                        foreach (var shrine in shrines)
                        {
                            this.ShrineList.Items.Add(new ComboBoxItem { Content = shrine.Value["Name"], Tag = shrine.Name });
                        }

                        // Tower data
                        var towers = this.json.SelectToken("Towers").Value<JObject>().Properties().ToList().OrderBy(x => x.Name);
                        foreach (var tower in towers)
                        {
                            this.TowerList.Items.Add(new ComboBoxItem { Content = tower.Value["Name"], Tag = tower.Name });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading json.");
            }

            IpAddress.Text = Settings.Default.IpAddress;

            this.Save.IsEnabled = this.HasChanged;
        }

        private bool LoadData()
        {
            try
            {
                //this.itemEnd = this.gecko.GetUInt(0x43C6B068) + 0x8;

                //this.itemStart = this.gecko.GetUInt(0x43C6B064) + 0x8;

                this.itemTotal = this.gecko.GetInt(0x43C6B06C);

                var currentItemAddress = ItemEnd;

                for (var x = 1; x <= this.itemTotal; x++)
                {
                    var itemData = this.gecko.ReadBytes(currentItemAddress, 0x70);

                    var page = BitConverter.ToInt32(itemData.Take(4).Skip(0).Reverse().ToArray(), 0);
                    if (page < 0 || page > 9)
                    {
                        currentItemAddress -= 0x220;
                        continue;
                    }

                    var builder = new StringBuilder();
                    for (var i = 0; i < 36; i++)
                    {
                        var data = itemData.Skip(i + 28).Take(1).ToArray()[0];
                        if (data == 0)
                        {
                            break;
                        }

                        builder.Append((char)data);
                    }

                    var id = builder.ToString();
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        currentItemAddress -= 0x220;
                        continue;
                    }

                    var unknown = BitConverter.ToInt32(itemData.Skip(4).Take(4).Reverse().ToArray(), 0);
                    var value = BitConverter.ToUInt32(itemData.Skip(8).Take(4).Reverse().ToArray(), 0);
                    var equipped = BitConverter.ToBoolean(itemData.Skip(12).Take(1).Reverse().ToArray(), 0);
                    var current = BitConverter.ToBoolean(itemData.Skip(13).Take(1).Reverse().ToArray(), 0);
                    var nameStart = currentItemAddress + 0x1C;

                    var item = new Item
                                   {
                                       BaseAddress = currentItemAddress,
                                       Page = page,
                                       Unknown = unknown,
                                       Value = value,
                                       Equipped = equipped,
                                       Current = current,
                                       NameStart = nameStart,
                                       Id = id,
                                       Modifier1Value = this.gecko.ByteToHexBitFiddle(itemData.Skip(92).Take(4).ToArray()),
                                       Modifier2Value = this.gecko.ByteToHexBitFiddle(itemData.Skip(96).Take(4).ToArray()),
                                       Modifier3Value = this.gecko.ByteToHexBitFiddle(itemData.Skip(100).Take(4).ToArray()),
                                       Modifier4Value = this.gecko.ByteToHexBitFiddle(itemData.Skip(104).Take(4).ToArray()),
                                       Modifier5Value = this.gecko.ByteToHexBitFiddle(itemData.Skip(108).Take(4).ToArray())
                                   };

                    // look for name in json
                    var name = this.GetNameFromId(item.Id, item.PageName);
                    item.Name = name;

                    this.items.Add(item);

                    var currentPercent = (100m / this.itemTotal) * x;
                    Dispatcher.Invoke(
                        () =>
                            {
                                ProgressText.Text = string.Format("{0}/{1}", x, this.itemTotal);
                                this.UpdateProgress(Convert.ToInt32(currentPercent));
                            });

                    currentItemAddress -= 0x220;
                }

                return true;
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => this.LogError(ex));
                return false;
            }
        }

        private bool SaveData(TabItem tab)
        {
            // Clear old errors
            ErrorLog.Document.Blocks.Clear();

            if (!this.HasChanged)
            {
                // Nothing to update
                return false;
            }

            #region SaveLoad
            try
            {
                // For these we amend the 0x3FCE7FF0 area which requires save/load
                if (Equals(tab, this.Weapons) || Equals(tab, this.Bows) || Equals(tab, this.Shields)
                    || Equals(tab, this.Armor))
                {
                    var weaponList = this.items.Where(x => x.Page == 0).ToList();
                    var bowList = this.items.Where(x => x.Page == 1).ToList();
                    var arrowList = this.items.Where(x => x.Page == 2).ToList();
                    var shieldList = this.items.Where(x => x.Page == 3).ToList();
                    var armorList = this.items.Where(x => x.Page == 4 || x.Page == 5 || x.Page == 6).ToList();

                    var y = 0;
                    if (Equals(tab, this.Weapons))
                    {
                        foreach (var item in weaponList)
                        {
                            var foundTextBox = (TextBox)this.FindName("Value_" + item.ValueAddressHex);
                            if (foundTextBox != null)
                            {
                                var offset = (uint)(SaveItemStart + (y * 0x8));
                                this.gecko.WriteUInt(offset, Convert.ToUInt32(foundTextBox.Text));
                            }

                            y++;
                        }
                    }

                    if (Equals(tab, this.Bows))
                    {
                        // jump past weapons before we start
                        y += weaponList.Count;

                        foreach (var item in bowList)
                        {
                            var foundTextBox = (TextBox)this.FindName("Value_" + item.ValueAddressHex);
                            if (foundTextBox != null)
                            {
                                var offset = (uint)(SaveItemStart + (y * 0x8));

                                this.gecko.WriteUInt(offset, Convert.ToUInt32(foundTextBox.Text));
                            }

                            y++;
                        }
                    }

                    if (Equals(tab, this.Shields))
                    {
                        // jump past weapons/bows/arrows before we start
                        y += weaponList.Count + bowList.Count + arrowList.Count;

                        foreach (var item in shieldList)
                        {
                            var foundTextBox = (TextBox)this.FindName("Value_" + item.ValueAddressHex);
                            if (foundTextBox != null)
                            {
                                var offset = (uint)(SaveItemStart + (y * 0x8));

                                this.gecko.WriteUInt(offset, Convert.ToUInt32(foundTextBox.Text));
                            }

                            y++;
                        }
                    }

                    if (Equals(tab, this.Armor))
                    {
                        // jump past weapons/bows/arrows/shields before we start
                        y += weaponList.Count + bowList.Count + arrowList.Count + shieldList.Count;

                        foreach (var item in armorList)
                        {
                            var offset = (uint)(SaveItemStart + (y * 0x8));

                            var foundTextBox = (TextBox)this.FindName("Value_" + item.ValueAddressHex);
                            if (foundTextBox != null)
                            {
                                this.gecko.WriteUInt(offset, Convert.ToUInt32(foundTextBox.Text));
                            }

                            y++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Attempting to save data in 0x3FCE7FF0 region.");
            }
            #endregion

            #region Modified
            try
            {
                // Only update what has changed to avoid corruption.
                foreach (var tb in this.tbChanged)
                {
                    if (string.IsNullOrEmpty(tb.Text))
                    {
                        continue;
                    }

                    // These text boxes have been edited
                    var type = tb.Name.Split('_')[0];
                    var tag = tb.Tag;

                    if (type == "Id")
                    {
                        var newName = Encoding.Default.GetBytes(tb.Text);

                        var address = uint.Parse(tag.ToString(), NumberStyles.HexNumber);
                        var thisItem = this.items.Single(i => i.NameStart == address);

                        // clear current name
                        var zeros = new byte[36];
                        for (var i = 0; i < zeros.Length; i++)
                        {
                            zeros[i] = 0x0;
                        }

                        this.gecko.WriteBytes(address, zeros);

                        uint x = 0x0;
                        foreach (var b in newName)
                        {
                            this.gecko.WriteBytes(address + x, new[] { b });
                            x = x + 0x1;
                        }

                        thisItem.Id = tb.Text;

                        // Name
                        var foundTextBox = (TextBox)this.FindName("JsonName_" + tag);
                        if (foundTextBox != null)
                        {
                            foundTextBox.Text = this.GetNameFromId(thisItem.Id, thisItem.PageName);
                        }
                    }

                    if (type == "Value")
                    {
                        var address = uint.Parse(tag.ToString(), NumberStyles.HexNumber);
                        int val;
                        bool parsed = int.TryParse(tb.Text, out val);
                        if (parsed)
                        {
                            this.gecko.WriteUInt(address, Convert.ToUInt32(val));
                        }
                    }

                    if (type == "Page")
                    {
                        var address = uint.Parse(tag.ToString(), NumberStyles.HexNumber);
                        int val;
                        bool parsed = int.TryParse(tb.Text, out val);
                        if (parsed && val < 10 && val >= 0)
                        {
                            this.gecko.WriteUInt(address, Convert.ToUInt32(val));
                        }
                    }

                    if (type == "Mod")
                    {
                        var address = uint.Parse(tag.ToString(), NumberStyles.HexNumber);
                        uint val;
                        bool parsed = uint.TryParse(tb.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out val);
                        if (parsed)
                        {
                            this.gecko.WriteUInt(address, val);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Attempting to update changed fields");
            }
            #endregion

            #region Codes
            try
            {
                // For the 'Codes' tab we mimic JGecko and send cheats to codehandler
                if (Equals(tab, this.Codes))
                {
                    // Disable codehandler before we modify
                    this.gecko.WriteUInt(CodeHandlerEnabled, 0x00000000);

                    // clear current codes
                    var array = new byte[4864];
                    Array.Clear(array, 0, array.Length);
                    this.gecko.WriteBytes(CodeHandlerStart, array);

                    var codelist = this.codes.CreateCodeList();

                    // Write our selected codes to mem stream
                    var ms = new MemoryStream();
                    foreach (var code in codelist)
                    {
                        var b = BitConverter.GetBytes(code);
                        ms.Write(b.Reverse().ToArray(), 0, 4);
                    }

                    var bytes = ms.ToArray();
                    this.gecko.WriteBytes(CodeHandlerStart, bytes);

                    // Re-enable codehandler
                    this.gecko.WriteUInt(CodeHandlerEnabled, 0x00000001);

                    // Save controller choice
                    if (Controller.SelectedValue.ToString() != Settings.Default.Controller)
                    {
                        Settings.Default.Controller = Controller.SelectedValue.ToString();
                        Settings.Default.Save();
                    }
                }

                DebugGrid.ItemsSource = this.items;
                DebugGrid.UpdateLayout();
                Debug.UpdateLayout();

                // clear changed after save
                this.tbChanged.Clear();
                this.cbChanged.Clear();
                this.ddChanged.Clear();
            }
            catch (Exception ex)
            {
                this.LogError(ex);
            }
            #endregion

            return true;
        }

        private async void EnableCoordsOnChecked(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => this.LoadCoords());
        }

        private async void LoadClick(object sender, RoutedEventArgs e)
        {
            this.ToggleControls("Load");

            this.items.Clear();

            try
            {
                // talk to wii u and get mem dump of data
                var result = await Task.Run(() => this.LoadData());

                if (result)
                {
                    this.GetNonItemData();

                    this.LoadTab(this.Weapons, 0);
                    this.LoadTab(this.Bows, 1);
                    this.LoadTab(this.Arrows, 2);
                    this.LoadTab(this.Shields, 3);
                    this.LoadTab(this.Armor, 4);
                    this.LoadTab(this.Materials, 7);
                    this.LoadTab(this.Food, 8);
                    this.LoadTab(this.KeyItems, 9);

                    this.Notification.Content = string.Format("Items found: {0}", this.itemTotal);

                    this.ToggleControls("DataLoaded");

                    this.cbChanged.Clear();
                    this.tbChanged.Clear();
                    this.ddChanged.Clear();

                    this.Save.IsEnabled = this.HasChanged;

                    DebugGrid.ItemsSource = this.items;
                    DebugGrid.UpdateLayout();
                    Debug.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Load Data");
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            this.Save.IsEnabled = false;

            var result = this.SaveData((TabItem)TabControl.SelectedItem);

            if (!result)
            {
                MessageBox.Show("No changes have been made");
            }
        }

        private void CoordsGoClick(object sender, RoutedEventArgs e)
        {
            var x = Convert.ToSingle(CoordsXValue.Text);
            var y = Convert.ToSingle(CoordsYValue.Text);
            var z = Convert.ToSingle(CoordsZValue.Text);

            var xByte = BitConverter.GetBytes(x).Reverse().ToArray();
            var yByte = BitConverter.GetBytes(y).Reverse().ToArray();
            var zByte = BitConverter.GetBytes(z).Reverse().ToArray();

            var ms = new MemoryStream();
            ms.Write(xByte, 0, xByte.Length);
            ms.Write(yByte, 0, yByte.Length);
            ms.Write(zByte, 0, zByte.Length);

            var bytes = ms.ToArray();

            var pointer = this.gecko.GetUInt(0x439C0794);
            var address = pointer + 0x140;

            this.gecko.WriteBytes(address, bytes);
        }

        private void ChangeTimeClick(object sender, RoutedEventArgs e)
        {
            var hour = Convert.ToSingle(CurrentTime.Text) * 15;

            var timePointer = this.gecko.GetUInt(0x407ABBF0);
            this.gecko.WriteFloat(timePointer + 0x9C, hour);
        }

        private void LoadCoords()
        {
            var run = false;

            try
            {
                var pointer = this.gecko.GetUInt(0x439C0794);
                var address = pointer + 0x140;

                Dispatcher.Invoke(
                    () =>
                    {
                        run = this.connected && EnableCoords.IsChecked == true;
                        CoordsAddress.Content = "0x" + address.ToString("x8").ToUpper() + " <- Memory Address";
                    });

                while (run)
                {
                    var coords = this.gecko.ReadBytes(address, 0xC);

                    if (!coords.Any())
                    {
                        MessageBox.Show("No data found");
                        break;
                    }

                    var x = coords.Take(4).Reverse().ToArray();
                    var y = coords.Skip(4).Take(4).Reverse().ToArray();
                    var z = coords.Skip(8).Take(4).Reverse().ToArray();

                    var xFloat = BitConverter.ToSingle(x, 0);
                    var yFloat = BitConverter.ToSingle(y, 0);
                    var zFloat = BitConverter.ToSingle(z, 0);

                    Dispatcher.Invoke(
                        () =>
                            {
                                CoordsX.Content = string.Format("{0}", Math.Round(xFloat, 2));
                                CoordsY.Content = string.Format("{0}", Math.Round(yFloat, 2));
                                CoordsZ.Content = string.Format("{0}", Math.Round(zFloat, 2));
                                run = this.connected && EnableCoords.IsChecked == true;
                            });

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => this.LogError(ex, "Coords Tab"));
            }
        }

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.tcpConn = new TcpConn(this.IpAddress.Text, 7331);
                this.connected = this.tcpConn.Connect();

                if (!this.connected)
                {
                    this.LogError(new Exception("Failed to connect"));
                    return;
                }

                // init gecko
                this.gecko = new Gecko(this.tcpConn, this);

                if (this.connected)
                {
                    var status = this.gecko.GetServerStatus();
                    if (status == 0)
                    {
                        return;
                    }

                    this.GetNonItemData();

                    Settings.Default.IpAddress = IpAddress.Text;
                    Settings.Default.Save();

                    Controller.SelectedValue = Settings.Default.Controller;

                    this.ToggleControls("Connected");
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.connected = false;

                MessageBox.Show("Wrong IP");
            }
            catch (Exception ex)
            {
                this.LogError(ex);
            }
        }

        private void DisconnectClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.tcpConn.Close();

                this.ToggleControls("Disconnected");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DebugGrid.SelectAllCells();
                DebugGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, DebugGrid);
                var result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                DebugGrid.UnselectAllCells();

                var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var excelFile = new StreamWriter(path + @"\debug.csv");
                excelFile.WriteLine(result);
                excelFile.Close();

                MessageBox.Show("File exported to " + path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Excel Export");
            }
        }

        private void TestClick(object sender, RoutedEventArgs e)
        {
            var server = this.gecko.GetServerVersion();
            var os = this.gecko.GetOsVersion();

            MessageBox.Show(string.Format("Server: {0}\nOs: {1}", server, os));
        }

        private void RefreshCodeClick(object sender, RoutedEventArgs e)
        {
            this.GetNonItemData();
        }

        private void LoadTab(ContentControl tab, int page)
        {
            var scroll = new ScrollViewer { Name = "ScrollContent", Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Top };

            var holder = new WrapPanel { Margin = new Thickness(0), VerticalAlignment = VerticalAlignment.Top };

            // setup grid
            var grid = this.GenerateTabGrid(tab.Name);

            var x = 1;
            var list = this.items.Where(i => i.Page == page).OrderBy(i => i.Name);

            if (page == 4)
            {
                list = this.items.Where(i => i.Page == 4 || i.Page == 5 || i.Page == 6).OrderBy(i => i.Name);
            }

            foreach (var item in list)
            {
                grid.RowDefinitions.Add(new RowDefinition());

                // Name - Readonly data
                var name = new TextBox
                {
                    Text = item.Name,
                    Margin = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    Height = 22,
                    Width = 190,
                    IsReadOnly = true,
                    Name = "JsonName_" + item.NameStartHex
                };

                // we register the name so we can update it later without having to refresh
                var check = (TextBox)this.FindName("JsonName_" + item.NameStartHex);
                if (check != null)
                {
                    this.UnregisterName("JsonName_" + item.NameStartHex);
                }

                this.RegisterName("JsonName_" + item.NameStartHex, name);

                // Id
                var id = new TextBox
                {
                    Text = item.Id,
                    Tag = item.NameStartHex,
                    ToolTip = item.NameStartHex,
                    Margin = new Thickness(0), 
                    Height = 22, 
                    Width = 130,
                    IsReadOnly = false,
                    Name = "Id_" + item.NameStartHex
                };

                id.TextChanged += this.TextChanged;

                check = (TextBox)this.FindName("Id_" + item.NameStartHex);
                if (check != null)
                {
                    this.UnregisterName("Id_" + item.NameStartHex);
                }

                this.RegisterName("Id_" + item.NameStartHex, id);

                // Current item is red
                if (item.Equipped)
                {
                    id.Foreground = Brushes.Red;
                    name.Foreground = Brushes.Red;
                }

                // add first 2 fields
                Grid.SetRow(name, x);
                Grid.SetColumn(name, 0);
                grid.Children.Add(name);

                Grid.SetRow(id, x);
                Grid.SetColumn(id, 1);
                grid.Children.Add(id);

                // Value to 0 if its FFFFF etc
                var value = item.Value;
                if (value > int.MaxValue)
                {
                    value = 0;
                }

                var val = this.GenerateGridTextBox(value.ToString(CultureInfo.InvariantCulture), item.ValueAddressHex, "Value_", x, 2, 70);
                val.PreviewTextInput += this.NumberValidationTextBox;
                grid.Children.Add(val);

                // Page
                var pgtb = this.GenerateGridTextBox(item.Page.ToString(CultureInfo.InvariantCulture), item.BaseAddressHex, "Page_", x, 3, 20);
                pgtb.PreviewTextInput += this.NumberValidationTextBox;
                grid.Children.Add(pgtb);

                // Mod1
                var mtb1 = this.GenerateGridTextBox(item.Modifier1Value, item.Modifier1Address, "Mod_", x, 4, 70);
                grid.Children.Add(mtb1);

                // Mod2
                var mtb2 = this.GenerateGridTextBox(item.Modifier2Value, item.Modifier2Address, "Mod_", x, 5, 70);
                grid.Children.Add(mtb2);

                // Mod3s
                var mtb3 = this.GenerateGridTextBox(item.Modifier3Value, item.Modifier3Address, "Mod_", x, 6, 70);
                grid.Children.Add(mtb3);

                // Mod4
                var mtb4 = this.GenerateGridTextBox(item.Modifier4Value, item.Modifier4Address, "Mod_", x, 7, 70);
                grid.Children.Add(mtb4);

                // Mod5
                var mtb5 = this.GenerateGridTextBox(item.Modifier5Value, item.Modifier5Address, "Mod_", x, 8, 70);
                grid.Children.Add(mtb5);

                x++;
            }

            grid.Height = x * 35;

            holder.Children.Add(new TextBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(20, 10, 0, 0),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                Text = "Items move around. What you see below may not be what is in memory. Refresh to get the latest data before you try to save anything.",
                Foreground = Brushes.Red
            });

            holder.Children.Add(grid);

            scroll.Content = holder;

            tab.Content = scroll;
        }

        private void GetNonItemData()
        {
            // Code Tab Values
            CurrentStamina.Text = this.gecko.GetString(0x4243A598);
            var healthPointer = this.gecko.GetUInt(0x4225C780);
            CurrentHealth.Text = this.gecko.GetInt(healthPointer + 0x388).ToString(CultureInfo.InvariantCulture);
            CurrentRupees.Text = this.gecko.GetInt(0x4010BA4C).ToString(CultureInfo.InvariantCulture);
            CurrentMon.Text = this.gecko.GetInt(0x4010C18C).ToString(CultureInfo.InvariantCulture);
            CurrentWeaponSlots.Text = this.gecko.GetInt(0x4010C38C).ToString(CultureInfo.InvariantCulture);
            CurrentBowSlots.Text = this.gecko.GetInt(0x401122AC).ToString(CultureInfo.InvariantCulture);
            CurrentShieldSlots.Text = this.gecko.GetInt(0x401122CC).ToString(CultureInfo.InvariantCulture);

            var damagePointer = this.gecko.GetUInt(0x43AB9C30);
            CbDamage.SelectedValue = this.gecko.GetString(damagePointer + 0x770);
            //CbSpeed.SelectedValue = this.gecko.GetString(0x439C0514);
            CbWeather.SelectedValue = this.gecko.GetString(0x407B5CE4);

            var time = this.GetCurrentTime();
            CurrentTime.Text = time.ToString(CultureInfo.InvariantCulture);
            TimeSlider.Value = time;

            this.tbChanged.Clear();
            this.cbChanged.Clear();
            this.ddChanged.Clear();
        }

        private void ToggleControls(string state)
        {
            if (state == "Connected")
            {
                this.Load.IsEnabled = true;
                this.Connect.IsEnabled = false;
                this.Connect.Visibility = Visibility.Hidden;

                this.Disconnect.IsEnabled = true;
                this.Disconnect.Visibility = Visibility.Visible;

                this.IpAddress.IsEnabled = false;

                if (this.Load.Visibility == Visibility.Hidden)
                {
                    this.Refresh.IsEnabled = true;
                }

                this.Test.IsEnabled = true;
                this.GetBufferSize.IsEnabled = false;

                this.TabControl.IsEnabled = true;
            }

            if (state == "Disconnected")
            {
                this.Load.IsEnabled = false;
                this.Connect.IsEnabled = true;
                this.Connect.Visibility = Visibility.Visible;
                this.Disconnect.IsEnabled = false;
                this.Disconnect.Visibility = Visibility.Hidden;
                this.IpAddress.IsEnabled = true;

                this.Refresh.IsEnabled = false;
                this.Test.IsEnabled = false;
                this.TabControl.IsEnabled = false;
                this.GetBufferSize.IsEnabled = true;
            }

            if (state == "Load")
            {
                TabControl.IsEnabled = false;
                this.Load.IsEnabled = false;
                this.Load.Visibility = Visibility.Hidden;

                this.Refresh.IsEnabled = false;
                this.Test.IsEnabled = false;
                this.Weapons.IsEnabled = true;
                this.Bows.IsEnabled = true;
                this.Shields.IsEnabled = true;
                this.Weapons.IsEnabled = true;
                this.Armor.IsEnabled = true;
                this.Arrows.IsEnabled = true;
                this.Materials.IsEnabled = true;
                this.Food.IsEnabled = true;
                this.KeyItems.IsEnabled = true;
                this.Debug.IsEnabled = true;
            }

            if (state == "DataLoaded")
            {
                TabControl.IsEnabled = true;
                this.Refresh.IsEnabled = true;
                this.Test.IsEnabled = true;
            }

            if (state == "ForceRefresh")
            {
                TabControl.IsEnabled = false;
                this.Save.IsEnabled = false;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var result = e.Result;
                if (result != Settings.Default.CurrentVersion)
                {
                    MessageBox.Show(string.Format("An update is available: {0}", result));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error checking for new version.");
            }
        }

        private void UpdateProgress(int percent)
        {
            Progress.Value = percent;
        }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Save == null)
            {
                return;
            }

            if (Debug.IsSelected || Help.IsSelected || Credits.IsSelected)
            {
                this.Save.IsEnabled = false;
                return;
            }

            this.Save.IsEnabled = this.HasChanged;

            if (!Codes.IsSelected)
            {
                EnableCoords.IsChecked = false;
            }
        }

        private void ShrineListChanged(object sender, SelectionChangedEventArgs e)
        {
            var shrine = (ComboBoxItem)ShrineList.SelectedItem;
            var tag = shrine.Tag.ToString();

            var data = (JObject)this.json.SelectToken("Shrines");

            CoordsXValue.Text = data[tag]["LocX"].ToString();
            CoordsYValue.Text = data[tag]["LocY"].ToString();
            CoordsZValue.Text = data[tag]["LocZ"].ToString();
        }

        private void TowerListChanged(object sender, SelectionChangedEventArgs e)
        {
            var tower = (ComboBoxItem)TowerList.SelectedItem;
            var tag = tower.Tag.ToString();

            var data = (JObject)this.json.SelectToken("Towers");

            CoordsXValue.Text = data[tag]["LocX"].ToString();
            CoordsYValue.Text = data[tag]["LocY"].ToString();
            CoordsZValue.Text = data[tag]["LocZ"].ToString();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ddChanged.Add(sender as ComboBox);

            this.Save.IsEnabled = this.HasChanged;
        }

        private void TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var thisTb = sender as TextBox;

            var exists = this.tbChanged.Where(x => thisTb != null && x.Tag == thisTb.Tag);

            if (exists.Any())
            {
                return;
            }

            this.tbChanged.Add(thisTb);

            this.Save.IsEnabled = this.HasChanged;
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            this.cbChanged.Add(sender as CheckBox);

            this.Save.IsEnabled = this.HasChanged;
        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public void LogError(Exception ex, string more = null)
        {
            var paragraph = new Paragraph
            {
                FontSize = 14,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                LineHeight = 14
            };

            if (more != null)
            {
                paragraph.Inlines.Add(more + Environment.NewLine);
            }

            paragraph.Inlines.Add(ex.Message);
            if (ex.StackTrace != null)
            {
                paragraph.Inlines.Add(ex.StackTrace);
            }

            ErrorLog.Document.Blocks.Add(paragraph);

            ErrorLog.Document.Blocks.Add(new Paragraph());

            TabControl.IsEnabled = true;
            Error.IsEnabled = true;

            //MessageBox.Show("Error caught. Check Error Tab");
        }

        private TextBox GenerateGridTextBox(string value, string field, string type, int x, int col, int width = 75)
        {
            var tb = new TextBox
            {
                Text = value,
                ToolTip = field,
                Tag = field,
                Width = width,
                Height = 22,
                Margin = new Thickness(10, 0, 10, 0),
                Name = type + field,
                IsEnabled = true,
                CharacterCasing = CharacterCasing.Upper,
                MaxLength = 8
            };

            tb.TextChanged += this.TextChanged;

            var check = (TextBox)this.FindName(type + field);
            if (check != null)
            {
                this.UnregisterName(type + field);
            }

            this.RegisterName(type + field, tb);

            Grid.SetRow(tb, x);
            Grid.SetColumn(tb, col);

            return tb;
        }

        private Grid GenerateTabGrid(string tab)
        {
            var grid = new Grid
            {
                Name = "TabGrid",
                Margin = new Thickness(10),
                ShowGridLines = true,
                VerticalAlignment = VerticalAlignment.Top
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(210) }); // Name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Id
            grid.ColumnDefinitions.Add(new ColumnDefinition()); // Value
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) }); // Page

            grid.RowDefinitions.Add(new RowDefinition());

            // Headers
            var nameHeader = new TextBlock
            {
                Text = "Item Name",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Width = 190
            };
            Grid.SetRow(nameHeader, 0);
            Grid.SetColumn(nameHeader, 0);
            grid.Children.Add(nameHeader);

            var idHeader = new TextBlock
            {
                Text = "Item Id",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Width = 150,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetRow(idHeader, 0);
            Grid.SetColumn(idHeader, 1);
            grid.Children.Add(idHeader);

            var valueHeader = new TextBlock
            {
                Text = "Item Value",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(valueHeader, 0);
            Grid.SetColumn(valueHeader, 2);
            grid.Children.Add(valueHeader);

            var pageHeader = new TextBlock
            {
                Text = "Page",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(pageHeader, 0);
            Grid.SetColumn(pageHeader, 3);
            grid.Children.Add(pageHeader);

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var headerNames = new[] { "Mod. 1", "Mod. 2", "Mod. 3", "Mod. 4", "Mod. 5" };

            for (int y = 0; y < 5; y++)
            {
                if (tab == "Food")
                {
                    headerNames = new[] { "Hearts", "Duration", "Mod Value?", "Mod Type", "Mod Level" };
                }

                if (tab == "Weapons" || tab == "Bows" || tab == "Shields")
                {
                    headerNames = new[] { "Mod Amt.", "N/A", " Mod Type", "N/A", "N/A" };
                }

                var header = new TextBlock
                {
                    Text = headerNames[y],
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetRow(header, 0);
                Grid.SetColumn(header, y + 4);
                grid.Children.Add(header);
            }

            return grid;
        }

        private string GetNameFromId(string id, string pagename)
        {
            try
            {
                if (pagename == "Head" || pagename == "Torso" || pagename == "Legs")
                {
                    pagename = "Armor";
                }

                var name = "Unknown";
                var path = string.Format("Items.{0}.{1}.Name", pagename.Replace(" ", string.Empty), id);
                var obj = this.json.SelectToken(path);
                if (obj != null)
                {
                    name = obj.ToString();
                }

                return name;
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        private int GetCurrentTime()
        {
            try
            {
                var timePointer = this.gecko.GetUInt(0x407ABBF0);

                var time = this.gecko.GetFloat(timePointer + 0x98);

                var hour = Convert.ToInt32(time) / 15;

                return hour;
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Time");
            }

            return 1;
        }
    }
}