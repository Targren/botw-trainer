namespace BotwTrainer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class Codes
    {
        private readonly MainWindow mainWindow;

        public Codes(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        private enum Cheat
        {
            Stamina = 0,
            Health = 1,
            //Run = 2,
            Rupees = 3,
            MoonJump = 4,
            WeaponInv = 5,
            BowInv = 6,
            ShieldInv = 7,
            Speed = 8,
            Mon = 9,
            Urbosa = 10,
            Revali = 11,
            Daruk = 12,
            Mipha = 13,
            Bombs = 14,
            Whips = 15,
            Damage = 16,
            Weather = 17,
            WolfHealth = 18,
            WeaponDurability = 19,
            ShieldDurability = 20,
            BowDurability = 21,
            MasterCharge = 22,
            StasisCooldown = 23,
            Stealthy = 24,
            Amiibo = 25,
            Immune = 26
        }

        private List<Cheat> GetSelected()
        {
            var selected = new List<Cheat>();

            if (this.mainWindow.Stamina.IsChecked == true)
            {
                selected.Add(Cheat.Stamina);
            }

            if (this.mainWindow.Health.IsChecked == true)
            {
                selected.Add(Cheat.Health);
            }

            if (this.mainWindow.Rupees.IsChecked == true)
            {
                selected.Add(Cheat.Rupees);
            }

            if (this.mainWindow.Mon.IsChecked == true)
            {
                selected.Add(Cheat.Mon);
            }

            if (this.mainWindow.Speed.IsChecked == true)
            {
                selected.Add(Cheat.Speed);
            }

            if (this.mainWindow.MoonJump.IsChecked == true)
            {
                selected.Add(Cheat.MoonJump);
            }

            if (this.mainWindow.WeaponSlots.IsChecked == true)
            {
                selected.Add(Cheat.WeaponInv);
            }

            if (this.mainWindow.BowSlots.IsChecked == true)
            {
                selected.Add(Cheat.BowInv);
            }

            if (this.mainWindow.ShieldSlots.IsChecked == true)
            {
                selected.Add(Cheat.ShieldInv);
            }

            if (this.mainWindow.Urbosa.IsChecked == true)
            {
                selected.Add(Cheat.Urbosa);
            }

            if (this.mainWindow.Revali.IsChecked == true)
            {
                selected.Add(Cheat.Revali);
            }

            if (this.mainWindow.Daruk.IsChecked == true)
            {
                selected.Add(Cheat.Daruk);
            }

            if (this.mainWindow.Mipha.IsChecked == true)
            {
                selected.Add(Cheat.Mipha);
            }

            if (this.mainWindow.BombTime.IsChecked == true)
            {
                selected.Add(Cheat.Bombs);
            }

            if (this.mainWindow.HorseWhips.IsChecked == true)
            {
                selected.Add(Cheat.Whips);
            }

            if (this.mainWindow.Damage.IsChecked == true)
            {
                selected.Add(Cheat.Damage);
            }

            if (this.mainWindow.Weather.IsChecked == true)
            {
                selected.Add(Cheat.Weather);
            }

            if (this.mainWindow.WolfHealth.IsChecked == true)
            {
                selected.Add(Cheat.WolfHealth);
            }

            if (this.mainWindow.WeaponDurability.IsChecked == true)
            {
                selected.Add(Cheat.WeaponDurability);
            }

            if (this.mainWindow.ShieldDurability.IsChecked == true)
            {
                selected.Add(Cheat.ShieldDurability);
            }

            if (this.mainWindow.BowDurability.IsChecked == true)
            {
                selected.Add(Cheat.BowDurability);
            }

            if (this.mainWindow.MasterCharge.IsChecked == true)
            {
                selected.Add(Cheat.MasterCharge);
            }

            if (this.mainWindow.StasisCooldown.IsChecked == true)
            {
                selected.Add(Cheat.StasisCooldown);
            }

            if (this.mainWindow.Stealthy.IsChecked == true)
            {
                selected.Add(Cheat.Stealthy);
            }

            if (this.mainWindow.Amiibo.IsChecked == true)
            {
                selected.Add(Cheat.Amiibo);
            }

            if (this.mainWindow.Immune.IsChecked == true)
            {
                selected.Add(Cheat.Immune);
            }

            return selected;
        }

        public IEnumerable<uint> CreateCodeList()
        {
            var codes = new List<uint>();

            var cheats = this.GetSelected();

            if (cheats.Contains(Cheat.Stamina))
            {
                // Max 453B8000
                var value = uint.Parse(this.mainWindow.CurrentStamina.Text, NumberStyles.HexNumber);

                codes.Add(0x00020000);
                codes.Add(0x4243A594);
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x4243A598);
                codes.Add(value);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.Health))
            {
                var value = Convert.ToUInt32(this.mainWindow.CurrentHealth.Text);

                codes.Add(0x30000000);
                codes.Add(0x4225C780);
                codes.Add(0x43000000);
                codes.Add(0x46000000);
                codes.Add(0x00120388);
                codes.Add(value);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.Speed))
            {
                var value = uint.Parse(this.mainWindow.CbSpeed.SelectedValue.ToString(), NumberStyles.HexNumber);

                uint activator;
                if (this.mainWindow.Controller.SelectedValue.ToString() == "Pro")
                {
                    activator = 0x112671AB;
                }
                else
                {
                    activator = 0x102F48AB;
                }

                codes.Add(0x09000000);
                codes.Add(activator);
                codes.Add(0x00000080);
                codes.Add(0x00000000);
                codes.Add(0x00020000);
                codes.Add(0x439C0514);
                codes.Add(value);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);

                codes.Add(0x06000000);
                codes.Add(activator);
                codes.Add(0x00000080);
                codes.Add(0x00000000);
                codes.Add(0x00020000);
                codes.Add(0x439C0514);
                codes.Add(0x3F800000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.Rupees))
            {
                var value = Convert.ToUInt32(this.mainWindow.CurrentRupees.Text);

                codes.Add(0x00020000);
                codes.Add(0x3FC93D10);
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x4010BA4C);
                codes.Add(value);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.Mon))
            {
                var value = Convert.ToUInt32(this.mainWindow.CurrentMon.Text);

                codes.Add(0x00020000);
                codes.Add(0x3FD42158);
                codes.Add(value);
                codes.Add(0x00000000);
                
                codes.Add(0x00020000);
                codes.Add(0x4010C18C);
                codes.Add(value);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.MoonJump))
            {
                uint button;
                uint activator;
                if (this.mainWindow.Controller.SelectedValue.ToString() == "Pro")
                {
                    activator = 0x112671AB;
                    button = 0x00000008;
                }
                else
                {
                    activator = 0x102F48AA;
                    button = 0x00000020;
                }

                codes.Add(0x09000000);
                codes.Add(activator);
                codes.Add(button);
                codes.Add(0x00000000);
                codes.Add(0x00020000);
                codes.Add(0x439C0528);
                codes.Add(0xBE800000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);

                codes.Add(0x06000000);
                codes.Add(activator);
                codes.Add(button);
                codes.Add(0x00000000);
                codes.Add(0x00020000);
                codes.Add(0x439C0528);
                codes.Add(0x3F800000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.WeaponInv))
            {
                var value = Convert.ToUInt32(this.mainWindow.CurrentWeaponSlots.Text);

                codes.Add(0x00020000);
                codes.Add(0x3FCFC498);
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x4010C38C);
                codes.Add(value);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.BowInv))
            {
                var value = Convert.ToUInt32(this.mainWindow.CurrentBowSlots.Text);

                codes.Add(0x00020000);
                codes.Add(0x3FD4CB50);
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x401122AC);
                codes.Add(value);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.ShieldInv))
            {
                var value = Convert.ToUInt32(this.mainWindow.CurrentShieldSlots.Text);

                codes.Add(0x00020000);
                codes.Add(0x3FCC1B40);
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x401122CC);
                codes.Add(value);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.Urbosa))
            {
                codes.Add(0x30000000);
                codes.Add(0x43ABA020);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000470);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.Revali))
            {
                codes.Add(0x30000000);
                codes.Add(0x43ABA020);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000458);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.Daruk))
            {
                codes.Add(0x30000000);
                codes.Add(0x43ABA020);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000464);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.Mipha))
            {
                codes.Add(0x30000000);
                codes.Add(0x43ABA020);
                codes.Add(0x40000000);
                codes.Add(0x48000000);
                codes.Add(0x31000000);
                codes.Add(0x0000047C);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.Bombs))
            {
                codes.Add(0x00020000);
                codes.Add(0x4383EA34);
                codes.Add(0x45B70000);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x4383EA4C);
                codes.Add(0x45B70000);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.Whips))
            {
                codes.Add(0x00000000);
                codes.Add(0x4011224F);
                codes.Add(0x000000FF);
                codes.Add(0x00000000);
                /*
                codes.Add(0x00000000);
                codes.Add(0x44AFFA8F);
                codes.Add(0x000000FF);
                codes.Add(0x00000000);
                 */
            }

            if (cheats.Contains(Cheat.Damage))
            {
                var value = uint.Parse(this.mainWindow.CbDamage.SelectedValue.ToString(), NumberStyles.HexNumber);

                codes.Add(0x30000000);
                codes.Add(0x43AB9C30);
                codes.Add(0x41000000);
                codes.Add(0x46000000);
                codes.Add(0x00120770);
                codes.Add(value);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.Weather))
            {
                var value = uint.Parse(this.mainWindow.CbWeather.SelectedValue.ToString(), NumberStyles.HexNumber);

                codes.Add(0x00020000);
                codes.Add(0x407B5CF0);
                codes.Add(value);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.WolfHealth))
            {
                codes.Add(0x30000000);
                codes.Add(0x108FC4D4);
                codes.Add(0x45000000);
                codes.Add(0x4C89FFFF);
                codes.Add(0x31000000);
                codes.Add(0x00000050);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x45000000);
                codes.Add(0x4C89FFFF);
                codes.Add(0x00120160);
                codes.Add(0x00000028);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.WeaponDurability))
            {
                codes.Add(0x30000000);
                codes.Add(0x45188784);
                codes.Add(0x40000000);
                codes.Add(0x48000000);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x40000000);
                codes.Add(0x48000000);
                codes.Add(0x00120980);
                codes.Add(0x0063FF9C);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.ShieldDurability))
            {
                codes.Add(0x30000000);
                codes.Add(0x451887E0);
                codes.Add(0x40000000);
                codes.Add(0x48000000);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x40000000);
                codes.Add(0x48000000);
                codes.Add(0x00120980);
                codes.Add(0x0063FF9C);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.BowDurability))
            {
                codes.Add(0x30000000);
                codes.Add(0x4518883C);
                codes.Add(0x40000000);
                codes.Add(0x48000000);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x40000000);
                codes.Add(0x48000000);
                codes.Add(0x00120980);
                codes.Add(0x0063FF9C);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.MasterCharge))
            {
                codes.Add(0x30000000);
                codes.Add(0x43ABA020);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000488);
                codes.Add(0x05120000);
                codes.Add(0x00000000);
                codes.Add(0x40800000);
                codes.Add(0x00000000);
                codes.Add(0x00120000);
                codes.Add(0x40200000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            if (cheats.Contains(Cheat.StasisCooldown))
            {
                codes.Add(0x00020000);
                codes.Add(0x4383EABC);
                codes.Add(0x00000000);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.Stealthy))
            {
                codes.Add(0x00020000);
                codes.Add(0x43A5FCA8);
                codes.Add(0x00000000);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.Amiibo))
            {
                codes.Add(0x00020000);
                codes.Add(0x4011C28C);
                codes.Add(0x012C9985);
                codes.Add(0x00000000);
            }

            if (cheats.Contains(Cheat.Immune))
            {
                codes.Add(0x30000000);
                codes.Add(0x43AB9C30);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x000006FC);
                codes.Add(0x00120000);
                codes.Add(0x00000003);
                codes.Add(0x0012001C);
                codes.Add(0x00000003);
                codes.Add(0x001204D4);
                codes.Add(0x00000000);
                codes.Add(0x001204D0);
                codes.Add(0x00000000);
                codes.Add(0x0012040C);
                codes.Add(0x00000010);
                codes.Add(0x00110066);
                codes.Add(0x00000101);
                codes.Add(0x0012006C);
                codes.Add(0x461C3C00);
                codes.Add(0x00120070);
                codes.Add(0xC61C3C00);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            return codes;
        } 
    }
}
