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

        private const uint ObjectStart = 0x4090FEF8;

        private const uint ObjectEnd = 0x40A4A88C;

        private readonly List<TextBox> tbChanged = new List<TextBox>();

        private readonly List<ComboBox> ddChanged = new List<ComboBox>();

        private readonly List<CheckBox> cbChanged = new List<CheckBox>();

        // Technically your first item as they are stored in reverse so we work backwards
        private const uint ItemEnd = 0x43CA2AEC;

        //private uint itemStart = 0x43C6B2AC;

        private int itemTotal = 0;

        private List<Item> items;

        private List<Obj> objects;

        private JToken json;

        private TcpConn tcpConn;

        private Gecko gecko;

        private Codes codes;

        private bool connected;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindowLoaded;
        }

        private bool HasChanged
        {
            get
            {
                return tbChanged.Any() || cbChanged.Any() || ddChanged.Any();
            }
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Testing
            // this.TabControl.IsEnabled = true;

            Title = string.Format("{0} v{1}", Title, Settings.Default.CurrentVersion);

            var netVersion = new GetDotNetVersion().Get45PlusFromRegistry();
            if(netVersion.Version == null)
            {
                MessageBoxResult choice = MessageBox.Show("Required .NET Version 4.6.2 not found. Please update.", "New Version", MessageBoxButton.OKCancel);

                if (choice == MessageBoxResult.OK)
                {
                    Process.Start("https://www.microsoft.com/en-us/download/details.aspx?id=53345");
                }
                else
                {
                    return;
                }
            }
            else
            {
                Title += string.Format(" | .NET Version: {0}", netVersion.Version);
            }            

            items = new List<Item>();

            objects = new List<Obj>();

            codes = new Codes(this);

            var client = new WebClient
            {
                BaseAddress = Settings.Default.VersionUrl,
                Encoding = Encoding.UTF8,
                CachePolicy =
                    new System.Net.Cache.RequestCachePolicy(
                    System.Net.Cache.RequestCacheLevel.BypassCache)
            };

            client.Headers.Add("Cache-Control", "no-cache");
            client.DownloadStringCompleted += ClientDownloadStringCompleted;

            // try to get current version
            try
            {
                client.DownloadStringAsync(new Uri(string.Format("{0}{1}", client.BaseAddress, "version.txt")));
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading current version.");
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
                        json = JObject.Parse(data);

                        JsonViewer.Load(data);

                        // Shrine data
                        var shrines = json.SelectToken("Shrines").Value<JObject>().Properties().ToList().OrderBy(x => x.Name);
                        foreach (var shrine in shrines)
                        {
                            ShrineList.Items.Add(new ComboBoxItem { Content = shrine.Value["Name"], Tag = shrine.Name });
                        }

                        // Tower data
                        var towers = json.SelectToken("Towers").Value<JObject>().Properties().ToList().OrderBy(x => x.Name);
                        foreach (var tower in towers)
                        {
                            TowerList.Items.Add(new ComboBoxItem { Content = tower.Value["Name"], Tag = tower.Name });
                        }

                        // Ranches
                        var ranches = json.SelectToken("Ranches").Value<JObject>().Properties().ToList().OrderBy(x => x.Name);
                        foreach (var ranch in ranches)
                        {
                            RanchList.Items.Add(new ComboBoxItem { Content = ranch.Value["Name"], Tag = ranch.Name });
                        }

                        // Misc
                        var misc = json.SelectToken("Misc").Value<JObject>().Properties().ToList().OrderBy(x => x.Name);
                        foreach (var m in misc)
                        {
                            MiscList.Items.Add(new ComboBoxItem { Content = m.Value["Name"], Tag = m.Name });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading json.");
            }

            IpAddress.Text = Settings.Default.IpAddress;

            Save.IsEnabled = HasChanged;
        }

        private bool LoadItemData()
        {
            try
            {
                itemTotal = gecko.GetInt(0x43C6B06C);

                var currentItemAddress = ItemEnd;

                for (var x = 1; x <= itemTotal; x++)
                {
                    var itemData = gecko.ReadBytes(currentItemAddress, 0x70);

                    var page = BitConverter.ToInt32(itemData.Take(4).Skip(0).Reverse().ToArray(), 0);
                    if (page < 0 || page > 9)
                    {
                        currentItemAddress -= 0x220;
                        x = x - 1;
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
                        x = x - 1;
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
                                       Modifier1Value = gecko.ByteToHexBitFiddle(itemData.Skip(92).Take(4).ToArray()),
                                       Modifier2Value = gecko.ByteToHexBitFiddle(itemData.Skip(96).Take(4).ToArray()),
                                       Modifier3Value = gecko.ByteToHexBitFiddle(itemData.Skip(100).Take(4).ToArray()),
                                       Modifier4Value = gecko.ByteToHexBitFiddle(itemData.Skip(104).Take(4).ToArray()),
                                       Modifier5Value = gecko.ByteToHexBitFiddle(itemData.Skip(108).Take(4).ToArray())
                                   };

                    // look for name in json
                    var name = GetNameFromId(item.Id, item.PageName);
                    item.Name = name;

                    items.Add(item);

                    var currentPercent = (100m / itemTotal) * x;
                    Dispatcher.Invoke(
                        () =>
                            {
                                ProgressText.Text = string.Format("{0}/{1}", x, itemTotal);
                                UpdateProgress(Convert.ToInt32(currentPercent));
                            });

                    currentItemAddress -= 0x220;
                }

                return true;
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogError(ex));
                return false;
            }
        }

        private bool SaveItemData(TabItem tab)
        {
            // Clear old errors
            ErrorLog.Document.Blocks.Clear();

            if (!HasChanged)
            {
                // Nothing to update
                return false;
            }

            #region SaveLoad
            try
            {
                // For these we amend the 0x3FCE7FF0 area which requires save/load
                if (Equals(tab, Weapons) || Equals(tab, Bows) || Equals(tab, Shields)
                    || Equals(tab, Armor))
                {
                    var weaponList = items.Where(x => x.Page == 0).ToList();
                    var bowList = items.Where(x => x.Page == 1).ToList();
                    var arrowList = items.Where(x => x.Page == 2).ToList();
                    var shieldList = items.Where(x => x.Page == 3).ToList();
                    var armorList = items.Where(x => x.Page == 4 || x.Page == 5 || x.Page == 6).ToList();

                    var y = 0;
                    if (Equals(tab, Weapons))
                    {
                        foreach (var item in weaponList)
                        {
                            var foundTextBox = (TextBox)FindName("Value_" + item.ValueAddressHex);
                            if (foundTextBox != null)
                            {
                                var offset = (uint)(SaveItemStart + (y * 0x8));
                                gecko.WriteUInt(offset, Convert.ToUInt32(foundTextBox.Text));
                            }

                            y++;
                        }
                    }

                    if (Equals(tab, Bows))
                    {
                        // jump past weapons before we start
                        y += weaponList.Count;

                        foreach (var item in bowList)
                        {
                            var foundTextBox = (TextBox)FindName("Value_" + item.ValueAddressHex);
                            if (foundTextBox != null)
                            {
                                var offset = (uint)(SaveItemStart + (y * 0x8));

                                gecko.WriteUInt(offset, Convert.ToUInt32(foundTextBox.Text));
                            }

                            y++;
                        }
                    }

                    if (Equals(tab, Shields))
                    {
                        // jump past weapons/bows/arrows before we start
                        y += weaponList.Count + bowList.Count + arrowList.Count;

                        foreach (var item in shieldList)
                        {
                            var foundTextBox = (TextBox)FindName("Value_" + item.ValueAddressHex);
                            if (foundTextBox != null)
                            {
                                var offset = (uint)(SaveItemStart + (y * 0x8));

                                gecko.WriteUInt(offset, Convert.ToUInt32(foundTextBox.Text));
                            }

                            y++;
                        }
                    }

                    if (Equals(tab, Armor))
                    {
                        // jump past weapons/bows/arrows/shields before we start
                        y += weaponList.Count + bowList.Count + arrowList.Count + shieldList.Count;

                        foreach (var item in armorList)
                        {
                            var offset = (uint)(SaveItemStart + (y * 0x8));

                            var foundTextBox = (TextBox)FindName("Value_" + item.ValueAddressHex);
                            if (foundTextBox != null)
                            {
                                gecko.WriteUInt(offset, Convert.ToUInt32(foundTextBox.Text));
                            }

                            y++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Attempting to save data in 0x3FCE7FF0 region.");
            }
            #endregion

            #region Modified
            try
            {
                // Only update what has changed to avoid corruption.
                foreach (var tb in tbChanged)
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
                        var thisItem = items.Single(i => i.NameStart == address);

                        // clear current name
                        var zeros = new byte[36];
                        for (var i = 0; i < zeros.Length; i++)
                        {
                            zeros[i] = 0x0;
                        }

                        gecko.WriteBytes(address, zeros);

                        uint x = 0x0;
                        foreach (var b in newName)
                        {
                            gecko.WriteBytes(address + x, new[] { b });
                            x = x + 0x1;
                        }

                        thisItem.Id = tb.Text;

                        // Name
                        var foundTextBox = (TextBox)FindName("JsonName_" + tag);
                        if (foundTextBox != null)
                        {
                            foundTextBox.Text = GetNameFromId(thisItem.Id, thisItem.PageName);
                        }
                    }

                    if (type == "Value")
                    {
                        var address = uint.Parse(tag.ToString(), NumberStyles.HexNumber);
                        int val;
                        bool parsed = int.TryParse(tb.Text, out val);
                        if (parsed)
                        {
                            gecko.WriteUInt(address, Convert.ToUInt32(val));
                        }
                    }

                    if (type == "Page")
                    {
                        var address = uint.Parse(tag.ToString(), NumberStyles.HexNumber);
                        int val;
                        bool parsed = int.TryParse(tb.Text, out val);
                        if (parsed && val < 10 && val >= 0)
                        {
                            gecko.WriteUInt(address, Convert.ToUInt32(val));
                        }
                    }

                    if (type == "Mod")
                    {
                        var address = uint.Parse(tag.ToString(), NumberStyles.HexNumber);
                        uint val;
                        bool parsed = uint.TryParse(tb.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out val);
                        if (parsed)
                        {
                            gecko.WriteUInt(address, val);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Attempting to update changed fields");
            }
            #endregion

            #region Codes
            try
            {
                // For the 'Codes' tab we mimic JGecko and send cheats to codehandler
                if (Equals(tab, Codes))
                {
                    // Disable codehandler before we modify
                    gecko.WriteUInt(CodeHandlerEnabled, 0x00000000);

                    // clear current codes
                    var array = new byte[4864];
                    Array.Clear(array, 0, array.Length);
                    gecko.WriteBytes(CodeHandlerStart, array);

                    var codelist = codes.CreateCodeList();

                    // Write our selected codes to mem stream
                    var ms = new MemoryStream();
                    foreach (var code in codelist)
                    {
                        var b = BitConverter.GetBytes(code);
                        ms.Write(b.Reverse().ToArray(), 0, 4);
                    }

                    var bytes = ms.ToArray();
                    gecko.WriteBytes(CodeHandlerStart, bytes);

                    // Re-enable codehandler
                    gecko.WriteUInt(CodeHandlerEnabled, 0x00000001);

                    // Save controller choice
                    if (Controller.SelectedValue.ToString() != Settings.Default.Controller)
                    {
                        Settings.Default.Controller = Controller.SelectedValue.ToString();
                        Settings.Default.Save();
                    }
                }

                DebugGrid.ItemsSource = items;
                DebugGrid.UpdateLayout();
                Debug.UpdateLayout();

                // clear changed after save
                tbChanged.Clear();
                cbChanged.Clear();
                ddChanged.Clear();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            #endregion

            return true;
        }

        private bool LoadObjectData()
        {
            try
            {
                var objectTotal = 4414;

                var currentObjectAddress = ObjectStart;

                for (var x = 1; x <= objectTotal; x++)
                {
                    var objectData = gecko.ReadBytes(currentObjectAddress, 0x120);

                    var builder = new StringBuilder();
                    for (var i = 0; i < 36; i++)
                    {
                        var data = objectData.Skip(i).Take(1).ToArray()[0];
                        if (data == 0)
                        {
                            break;
                        }

                        builder.Append((char)data);
                    }

                    var id = builder.ToString();

                    var obj = new Obj
                    {
                        Id = id,
                        Address = currentObjectAddress
                    };

                    objects.Add(obj);

                    var currentPercent = (100m / objectTotal) * x;
                    Dispatcher.Invoke(
                        () =>
                        {
                            ProgressText.Text = string.Format("{0}/{1}", x, objectTotal);
                            UpdateProgress(Convert.ToInt32(currentPercent));
                        });

                    currentObjectAddress += 0x124;
                }

                return true;
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogError(ex));
                return false;
            }
        }

        private async void EnableCoordsOnChecked(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => LoadCoords());
        }

        private async void LoadItemsClick(object sender, RoutedEventArgs e)
        {
            ToggleControls("Load");

            items.Clear();

            try
            {
                // talk to wii u and get mem dump of data
                var result = await Task.Run(() => LoadItemData());

                if (result)
                {
                    GetNonItemData();

                    LoadTab(Weapons, 0);
                    LoadTab(Bows, 1);
                    LoadTab(Arrows, 2);
                    LoadTab(Shields, 3);
                    LoadTab(Armor, 4);
                    LoadTab(Materials, 7);
                    LoadTab(Food, 8);
                    LoadTab(KeyItems, 9);

                    Notification.Content = string.Format("Items found: {0}", itemTotal);

                    ToggleControls("DataLoaded");

                    cbChanged.Clear();
                    tbChanged.Clear();
                    ddChanged.Clear();

                    Save.IsEnabled = HasChanged;

                    DebugGrid.ItemsSource = items;
                    DebugGrid.UpdateLayout();
                    Debug.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Load Items");
            }
        }

        private async void LoadObjectsClick(object sender, RoutedEventArgs e)
        {
            objects.Clear();

            LoadObjects.IsEnabled = false;

            try
            {
                var result = await Task.Run(() => LoadObjectData());

                if (result)
                {
                    using (StreamWriter sw = File.CreateText("objects.csv"))
                    {
                        for (int i = 0; i < objects.Count; i++)
                        {
                            sw.WriteLine(objects[i].Id + "," + objects[i].Address.ToString("X"));
                        }                        
                    }

                    MessageBox.Show("List saved (objects.csv)");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Load Objects");
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            Save.IsEnabled = false;

            var result = SaveItemData((TabItem)TabControl.SelectedItem);

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

            var pointer = gecko.GetUInt(0x439C0794);
            var address = pointer + 0x140;

            gecko.WriteBytes(address, bytes);
        }

        private void ChangeTimeClick(object sender, RoutedEventArgs e)
        {
            var hour = Convert.ToSingle(CurrentTime.Text) * 15;

            var timePointer = gecko.GetUInt(0x407B3AF4);
            gecko.WriteFloat(timePointer + 0xAC, hour);
        }

        private void LoadCoords()
        {
            var run = false;

            try
            {
                var pointer = gecko.GetUInt(0x439C0794);
                var address = pointer + 0x140;

                Dispatcher.Invoke(
                    () =>
                    {
                        run = connected && EnableCoords.IsChecked == true;
                        CoordsAddress.Content = "0x" + address.ToString("x8").ToUpper() + " <- Memory Address";
                    });

                while (run)
                {
                    var coords = gecko.ReadBytes(address, 0xC);

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

                                run = connected && EnableCoords.IsChecked == true;
                            });

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogError(ex, "Coords Tab"));
            }
        }
                
        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            try
            {
                tcpConn = new TcpConn(IpAddress.Text, 7331);
                connected = tcpConn.Connect();

                if (!connected)
                {
                    LogError(new Exception("Failed to connect"));
                    return;
                }

                // init gecko
                gecko = new Gecko(tcpConn, this);

                if (connected)
                {
                    var status = gecko.GetServerStatus();
                    if (status == 0)
                    {
                        return;
                    }

                    GetNonItemData();

                    Settings.Default.IpAddress = IpAddress.Text;
                    Settings.Default.Save();

                    Controller.SelectedValue = Settings.Default.Controller;

                    ToggleControls("Connected");
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                connected = false;

                MessageBox.Show("Wrong IP");
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void DisconnectClick(object sender, RoutedEventArgs e)
        {
            try
            {
                tcpConn.Close();

                ToggleControls("Disconnected");
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
            var server = gecko.GetServerVersion();
            var os = gecko.GetOsVersion();

            MessageBox.Show(string.Format("Server: {0}\nOs: {1}", server, os));
        }

        private void RefreshCodeClick(object sender, RoutedEventArgs e)
        {
            GetNonItemData();
        }

        private void LoadTab(ContentControl tab, int page)
        {
            var scroll = new ScrollViewer { Name = "ScrollContent", Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Top };

            var holder = new WrapPanel { Margin = new Thickness(0), VerticalAlignment = VerticalAlignment.Top };

            // setup grid
            var grid = GenerateTabGrid(tab.Name);

            var x = 1;
            var list = items.Where(i => i.Page == page).OrderBy(i => i.Name);

            if (page == 4)
            {
                list = items.Where(i => i.Page == 4 || i.Page == 5 || i.Page == 6).OrderBy(i => i.Name);
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
                var check = (TextBox)FindName("JsonName_" + item.NameStartHex);
                if (check != null)
                {
                    UnregisterName("JsonName_" + item.NameStartHex);
                }

                RegisterName("JsonName_" + item.NameStartHex, name);

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

                id.TextChanged += TextChanged;

                check = (TextBox)FindName("Id_" + item.NameStartHex);
                if (check != null)
                {
                    UnregisterName("Id_" + item.NameStartHex);
                }

                RegisterName("Id_" + item.NameStartHex, id);

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

                var val = GenerateGridTextBox(value.ToString(CultureInfo.InvariantCulture), item.ValueAddressHex, "Value_", x, 2, 70);
                val.PreviewTextInput += NumberValidationTextBox;
                grid.Children.Add(val);

                // Page
                var pgtb = GenerateGridTextBox(item.Page.ToString(CultureInfo.InvariantCulture), item.BaseAddressHex, "Page_", x, 3, 20);
                pgtb.PreviewTextInput += NumberValidationTextBox;
                grid.Children.Add(pgtb);

                // Mod1
                var mtb1 = GenerateGridTextBox(item.Modifier1Value, item.Modifier1Address, "Mod_", x, 4, 70);
                grid.Children.Add(mtb1);

                // Mod2
                var mtb2 = GenerateGridTextBox(item.Modifier2Value, item.Modifier2Address, "Mod_", x, 5, 70);
                grid.Children.Add(mtb2);

                // Mod3s
                var mtb3 = GenerateGridTextBox(item.Modifier3Value, item.Modifier3Address, "Mod_", x, 6, 70);
                grid.Children.Add(mtb3);

                // Mod4
                var mtb4 = GenerateGridTextBox(item.Modifier4Value, item.Modifier4Address, "Mod_", x, 7, 70);
                grid.Children.Add(mtb4);

                // Mod5
                var mtb5 = GenerateGridTextBox(item.Modifier5Value, item.Modifier5Address, "Mod_", x, 8, 70);
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
            CurrentStamina.Text = gecko.GetString(0x4243A598);
            var healthPointer = gecko.GetUInt(0x4225C780);
            CurrentHealth.Text = gecko.GetInt(healthPointer + 0x388).ToString(CultureInfo.InvariantCulture);
            CurrentRupees.Text = gecko.GetInt(0x4010BA4C).ToString(CultureInfo.InvariantCulture);
            CurrentMon.Text = gecko.GetInt(0x4010C18C).ToString(CultureInfo.InvariantCulture);
            CurrentWeaponSlots.Text = gecko.GetInt(0x4010C38C).ToString(CultureInfo.InvariantCulture);
            CurrentBowSlots.Text = gecko.GetInt(0x401122AC).ToString(CultureInfo.InvariantCulture);
            CurrentShieldSlots.Text = gecko.GetInt(0x401122CC).ToString(CultureInfo.InvariantCulture);

            var damagePointer = gecko.GetUInt(0x43AB9C30);
            CbDamage.SelectedValue = gecko.GetString(damagePointer + 0x770);
            CbWeather.SelectedValue = gecko.GetString(0x407B5CF0);

            var time = GetCurrentTime();
            CurrentTime.Text = time.ToString(CultureInfo.InvariantCulture);
            TimeSlider.Value = time;

            tbChanged.Clear();
            cbChanged.Clear();
            ddChanged.Clear();
        }

        private void ToggleControls(string state)
        {
            if (state == "Connected")
            {
                LoadItems.IsEnabled = true;
                LoadObjects.IsEnabled = true;
                Connect.IsEnabled = false;
                Connect.Visibility = Visibility.Hidden;

                Disconnect.IsEnabled = true;
                Disconnect.Visibility = Visibility.Visible;

                IpAddress.IsEnabled = false;

                if (LoadItems.Visibility == Visibility.Hidden)
                {
                    Refresh.IsEnabled = true;
                }

                Test.IsEnabled = true;
                GetBufferSize.IsEnabled = false;

                TabControl.IsEnabled = true;
            }

            if (state == "Disconnected")
            {
                LoadItems.IsEnabled = false;
                Connect.IsEnabled = true;
                Connect.Visibility = Visibility.Visible;
                Disconnect.IsEnabled = false;
                Disconnect.Visibility = Visibility.Hidden;
                IpAddress.IsEnabled = true;

                Refresh.IsEnabled = false;
                Test.IsEnabled = false;
                TabControl.IsEnabled = false;
                GetBufferSize.IsEnabled = true;
            }

            if (state == "Load")
            {
                TabControl.IsEnabled = false;
                LoadItems.IsEnabled = false;
                LoadItems.Visibility = Visibility.Hidden;

                Refresh.IsEnabled = false;
                Test.IsEnabled = false;
                Weapons.IsEnabled = true;
                Bows.IsEnabled = true;
                Shields.IsEnabled = true;
                Weapons.IsEnabled = true;
                Armor.IsEnabled = true;
                Arrows.IsEnabled = true;
                Materials.IsEnabled = true;
                Food.IsEnabled = true;
                KeyItems.IsEnabled = true;
                Debug.IsEnabled = true;
            }

            if (state == "DataLoaded")
            {
                TabControl.IsEnabled = true;
                Refresh.IsEnabled = true;
                Test.IsEnabled = true;
            }

            if (state == "ForceRefresh")
            {
                TabControl.IsEnabled = false;
                Save.IsEnabled = false;
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
                    MessageBoxResult choice = MessageBox.Show(string.Format("An update is available: {0}. Download?", result), "New Version", MessageBoxButton.OKCancel);

                    if (choice == MessageBoxResult.OK)
                    {
                        Process.Start("https://github.com/joffnerd/botw-trainer/releases");
                    }
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
            if (Save == null)
            {
                return;
            }

            if (Debug.IsSelected || Help.IsSelected || Credits.IsSelected)
            {
                Save.IsEnabled = false;
                return;
            }

            Save.IsEnabled = HasChanged;

            if (!Codes.IsSelected)
            {
                EnableCoords.IsChecked = false;
            }
        }

        private void ShrineListChanged(object sender, SelectionChangedEventArgs e)
        {
            var shrine = (ComboBoxItem)ShrineList.SelectedItem;
            var tag = shrine.Tag.ToString();

            var data = (JObject)json.SelectToken("Shrines");

            CoordsXValue.Text = data[tag]["LocX"].ToString();
            CoordsYValue.Text = data[tag]["LocY"].ToString();
            CoordsZValue.Text = data[tag]["LocZ"].ToString();
        }

        private void TowerListChanged(object sender, SelectionChangedEventArgs e)
        {
            var tower = (ComboBoxItem)TowerList.SelectedItem;
            var tag = tower.Tag.ToString();

            var data = (JObject)json.SelectToken("Towers");

            CoordsXValue.Text = data[tag]["LocX"].ToString();
            CoordsYValue.Text = data[tag]["LocY"].ToString();
            CoordsZValue.Text = data[tag]["LocZ"].ToString();
        }

        private void RanchListChanged(object sender, SelectionChangedEventArgs e)
        {
            var ranch = (ComboBoxItem)RanchList.SelectedItem;
            var tag = ranch.Tag.ToString();

            var data = (JObject)json.SelectToken("Ranches");

            CoordsXValue.Text = data[tag]["LocX"].ToString();
            CoordsYValue.Text = data[tag]["LocY"].ToString();
            CoordsZValue.Text = data[tag]["LocZ"].ToString();
        }

        private void MiscListChanged(object sender, SelectionChangedEventArgs e)
        {
            var misc = (ComboBoxItem)MiscList.SelectedItem;
            var tag = misc.Tag.ToString();

            var data = (JObject)json.SelectToken("Misc");

            CoordsXValue.Text = data[tag]["LocX"].ToString();
            CoordsYValue.Text = data[tag]["LocY"].ToString();
            CoordsZValue.Text = data[tag]["LocZ"].ToString();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ddChanged.Add(sender as ComboBox);

            Save.IsEnabled = HasChanged;
        }

        private void TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var thisTb = sender as TextBox;

            var exists = tbChanged.Where(x => thisTb != null && x.Tag == thisTb.Tag);

            if (exists.Any())
            {
                return;
            }

            tbChanged.Add(thisTb);

            Save.IsEnabled = HasChanged;
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            cbChanged.Add(sender as CheckBox);

            Save.IsEnabled = HasChanged;
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

            tb.TextChanged += TextChanged;

            var check = (TextBox)FindName(type + field);
            if (check != null)
            {
                UnregisterName(type + field);
            }

            RegisterName(type + field, tb);

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
                var obj = json.SelectToken(path);
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
                var timePointer = gecko.GetUInt(0x407B3AF4);

                var time = gecko.GetFloat(timePointer + 0xA8);

                var hour = Convert.ToInt32(time) / 15;

                return hour;
            }
            catch (Exception ex)
            {
                LogError(ex, "Time");
            }

            return 1;
        }
    }
}