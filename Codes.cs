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
            Whips = 15, //Not Working even after update - Code doesn't work on JGeckoU either?
            Damage = 16, //BROKEN
            Weather = 17,
            WolfHealth = 18,
            WeaponDurability = 19,
            ShieldDurability = 20,
            BowDurability = 21,
            MasterCharge = 22,
            StasisCooldown = 23,
            Stealthy = 24,
            Amiibo = 25,
            Immune = 26,
            MasterGlow = 27
        }

        private List<Cheat> GetSelected()
        {
            var selected = new List<Cheat>();

            if (mainWindow.Stamina.IsChecked == true)
            {
                selected.Add(Cheat.Stamina);
            }

            if (mainWindow.Health.IsChecked == true)
            {
                selected.Add(Cheat.Health);
            }

            if (mainWindow.Rupees.IsChecked == true)
            {
                selected.Add(Cheat.Rupees);
            }

            if (mainWindow.Mon.IsChecked == true)
            {
                selected.Add(Cheat.Mon);
            }

            if (mainWindow.Speed.IsChecked == true)
            {
                selected.Add(Cheat.Speed);
            }

            if (mainWindow.MoonJump.IsChecked == true)
            {
                selected.Add(Cheat.MoonJump);
            }

            if (mainWindow.WeaponSlots.IsChecked == true)
            {
                selected.Add(Cheat.WeaponInv);
            }

            if (mainWindow.BowSlots.IsChecked == true)
            {
                selected.Add(Cheat.BowInv);
            }

            if (mainWindow.ShieldSlots.IsChecked == true)
            {
                selected.Add(Cheat.ShieldInv);
            }

            if (mainWindow.Urbosa.IsChecked == true)
            {
                selected.Add(Cheat.Urbosa);
            }

            if (mainWindow.Revali.IsChecked == true)
            {
                selected.Add(Cheat.Revali);
            }

            if (mainWindow.Daruk.IsChecked == true)
            {
                selected.Add(Cheat.Daruk);
            }

            if (mainWindow.Mipha.IsChecked == true)
            {
                selected.Add(Cheat.Mipha);
            }

            if (mainWindow.BombTime.IsChecked == true)
            {
                selected.Add(Cheat.Bombs);
            }

            if (mainWindow.HorseWhips.IsChecked == true)
            {
                selected.Add(Cheat.Whips);
            }

            if (mainWindow.Damage.IsChecked == true)
            {
                selected.Add(Cheat.Damage);
            }

            if (mainWindow.Weather.IsChecked == true)
            {
                selected.Add(Cheat.Weather);
            }

            if (mainWindow.WolfHealth.IsChecked == true)
            {
                selected.Add(Cheat.WolfHealth);
            }

            if (mainWindow.WeaponDurability.IsChecked == true)
            {
                selected.Add(Cheat.WeaponDurability);
            }

            if (mainWindow.ShieldDurability.IsChecked == true)
            {
                selected.Add(Cheat.ShieldDurability);
            }

            if (mainWindow.BowDurability.IsChecked == true)
            {
                selected.Add(Cheat.BowDurability);
            }

            if (mainWindow.MasterCharge.IsChecked == true)
            {
                selected.Add(Cheat.MasterCharge);
            }

            if (mainWindow.MasterGlow.IsChecked == true)
            {
                selected.Add(Cheat.MasterGlow);
            }

            if (mainWindow.StasisCooldown.IsChecked == true)
            {
                selected.Add(Cheat.StasisCooldown);
            }

            if (mainWindow.Stealthy.IsChecked == true)
            {
                selected.Add(Cheat.Stealthy);
            }

            if (mainWindow.Amiibo.IsChecked == true)
            {
                selected.Add(Cheat.Amiibo);
            }

            if (mainWindow.Immune.IsChecked == true)
            {
                selected.Add(Cheat.Immune);
            }

            return selected;
        }

        public IEnumerable<uint> CreateCodeList()
        {
            var codes = new List<uint>();

            var cheats = GetSelected();

            //
            if (cheats.Contains(Cheat.Stamina))
            {
                // Max 453B8000
                var value = uint.Parse(mainWindow.CurrentStamina.Text, NumberStyles.HexNumber);

                //1.3.0
                /*
                codes.Add(0x00020000);
                codes.Add(0x4228B0CC);
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x4228B0D0);
                codes.Add(value);
                codes.Add(0x00000000);
                */

                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF3F0);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x402C96A6);
                codes.Add(0x4424CA31);
                codes.Add(0x31000000);
                codes.Add(0x00000060);
                codes.Add(0x12100000);
                codes.Add(0x00000004);
                codes.Add(0x13100000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);

            }

            //
            if (cheats.Contains(Cheat.Health))
            {
                var value = Convert.ToUInt32(mainWindow.CurrentHealth.Text);

                codes.Add(0x30000000);
                codes.Add(0x420ACBF0);
                codes.Add(0x43000000);
                codes.Add(0x46000000);
                codes.Add(0x00120388);
                codes.Add(value);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Rupees))
            {
                var value = Convert.ToUInt32(mainWindow.CurrentRupees.Text);

                codes.Add(0x00020000);
                codes.Add(0x13000188);
                codes.Add(value);
                codes.Add(0x00000000);
                codes.Add(0x12000001);
                codes.Add(0x13000188);
                codes.Add(0x13000001);
                codes.Add(0x43D6675C);
                codes.Add(0x13000001);
                codes.Add(0x43D66760);
                codes.Add(0x13000001);
                codes.Add(0x3FAD4678);
                codes.Add(0x13000001);
                codes.Add(0x3FF52244);
            }

            //
            if (cheats.Contains(Cheat.Mon))
            {
                var value = Convert.ToUInt32(mainWindow.CurrentMon.Text);

                codes.Add(0x00020000);
                codes.Add(0x1300018C);
                codes.Add(value);
                codes.Add(0x00000000);
                codes.Add(0x12000001);
                codes.Add(0x1300018C);
                codes.Add(0x13000001);
                codes.Add(0x449AD5CC);
                codes.Add(0x13000001);
                codes.Add(0x449AD5D0);
                codes.Add(0x13000001);
                codes.Add(0x3FB84728);
                codes.Add(0x13000001);
                codes.Add(0x3FF52984);
            }

            //
            if (cheats.Contains(Cheat.Speed))
            {
                var value = uint.Parse(mainWindow.CbSpeed.SelectedValue.ToString(), NumberStyles.HexNumber);

                uint button;
                uint activator;
                if (mainWindow.Controller.SelectedValue.ToString() == "Pro")
                {
                    activator = 0x112B4FA2;
                    button = 0x00000080;
                }
                else
                {
                    activator = 0x102F48AA;
                    button = 0x00000080;
                }

                codes.Add(0x30000000);
                codes.Add(0x3F93B768);
                codes.Add(0x417A9202);
                codes.Add(0x45876E2D);
                codes.Add(0x31000000);
                codes.Add(0x000000CC);
                codes.Add(0x00120000);
                codes.Add(0x3F800000);
                codes.Add(0x09010000);
                codes.Add(activator);
                codes.Add(button);
                codes.Add(0x00000000);
                codes.Add(0x00120000);
                codes.Add(value);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.MoonJump))
            {
                uint button;
                uint activator;
                if (mainWindow.Controller.SelectedValue.ToString() == "Pro")
                {
                    activator = 0x112B4FA2;
                    button = 0x00000008;
                }
                else
                {
                    activator = 0x102F48AA;
                    button = 0x00002000;
                }

                codes.Add(0x30000000);
                codes.Add(0x3F93B768);
                codes.Add(0x417A9202);
                codes.Add(0x45876E2D);
                codes.Add(0x31000000);
                codes.Add(0x000000E0);
                codes.Add(0x00120000);
                codes.Add(0x3F800000);
                codes.Add(0x09010000);
                codes.Add(activator);
                codes.Add(button);
                codes.Add(0x00000000);
                codes.Add(0x00120000);
                codes.Add(0xBF800000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.WeaponInv))
            {
                var value = Convert.ToUInt32(mainWindow.CurrentWeaponSlots.Text);

                codes.Add(0x00020000);
                codes.Add(0x3FB3E000); // 0x3FD14480
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x3FF52B84);
                codes.Add(value);
                codes.Add(0x00000000);
            }

            //
            if (cheats.Contains(Cheat.BowInv))
            {
                var value = Convert.ToUInt32(mainWindow.CurrentBowSlots.Text);

                codes.Add(0x00020000);
                codes.Add(0x3FB8F4E8); // 0x3FD64B28
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x3FF58AA4); // 0x4012A404
                codes.Add(value);
                codes.Add(0x00000000);
            }

            //
            if (cheats.Contains(Cheat.ShieldInv))
            {
                var value = Convert.ToUInt32(mainWindow.CurrentShieldSlots.Text);

                codes.Add(0x00020000);
                codes.Add(0x3FB02708); // 0x3FCD9B40
                codes.Add(value);
                codes.Add(0x00000000);

                codes.Add(0x00020000);
                codes.Add(0x3FF58AC4); // 0x4012A424
                codes.Add(value);
                codes.Add(0x00000000);
            }

            //
            if (cheats.Contains(Cheat.Urbosa))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001828);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001828);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Revali))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001810);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001810);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Daruk))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x0000181C);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x0000181C);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Mipha))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001834);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001834);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Bombs))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387F8);
                codes.Add(0x4163401D);
                codes.Add(0x456EAB02);
                codes.Add(0x31000000);
                codes.Add(0x000002E4);
                codes.Add(0x00120018);
                codes.Add(0x45B70000);
                codes.Add(0x00120000);
                codes.Add(0x45B70000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938AB8);
                codes.Add(0x4163401D);
                codes.Add(0x456EAB02);
                codes.Add(0x31000000);
                codes.Add(0x000002E4);
                codes.Add(0x00120018);
                codes.Add(0x45B70000);
                codes.Add(0x00120000);
                codes.Add(0x45B70000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Whips))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x4221FE74);
                codes.Add(0x456A6046);
                codes.Add(0x49B59319);
                codes.Add(0x31000000);
                codes.Add(0x00003578);
                codes.Add(0x03100000);
                codes.Add(0x00000002);
                codes.Add(0x000000FF);
                codes.Add(0x00000000);
                codes.Add(0x03100000);
                codes.Add(0x00000000);
                codes.Add(0x00000000);
                codes.Add(0x00000000);
                codes.Add(0x00110000);
                codes.Add(0x00000008);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                //Changed to latest version, but still doesn't seem to work
                codes.Add(0x30000000);
                codes.Add(0x4221FE74);
                codes.Add(0x456A6046);
                codes.Add(0x49B59319);
                codes.Add(0x31000000);
                codes.Add(0x00003578);
                codes.Add(0x03100000);
                codes.Add(0x00000002);
                codes.Add(0x000000FF);
                codes.Add(0x00000000);
                codes.Add(0x03100000);
                codes.Add(0x00000000);
                codes.Add(0x00000000);
                codes.Add(0x00000000);
                codes.Add(0x00110000);
                codes.Add(0x00000008);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Damage))
            {
                var value = uint.Parse(mainWindow.CbDamage.SelectedValue.ToString(), NumberStyles.HexNumber);

                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001AA0);
                codes.Add(0x00120000);
                codes.Add(value);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Weather))
            {
                var value = uint.Parse(mainWindow.CbWeather.SelectedValue.ToString(), NumberStyles.HexNumber);
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x10937E90);
                codes.Add(0x3E719999);
                codes.Add(0x424E6666);
                codes.Add(0x31000000);
                codes.Add(0x0000008C);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x3E7198E7);
                codes.Add(0x424E65A8);
                codes.Add(0x31000000);
                codes.Add(0x00002340);
                codes.Add(0x00120000);
                codes.Add(value);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938150);
                codes.Add(0x3E719999);
                codes.Add(0x424E6666);
                codes.Add(0x31000000);
                codes.Add(0x0000008C);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x3E7198E7);
                codes.Add(0x424E65A8);
                codes.Add(0x31000000);
                codes.Add(0x00002340);
                codes.Add(0x00120000);
                codes.Add(value);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);

            }

            //
            if (cheats.Contains(Cheat.WolfHealth))
            {
                //1.3.0
                /*
                codes.Add(0x30000000);
                codes.Add(0x1093886C);
                codes.Add(0x40000000);
                codes.Add(0x4C89FFFF);
                codes.Add(0x31000000);
                codes.Add(0x0000004C);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x40000000);
                codes.Add(0x4C89FFFF);
                codes.Add(0x00120160);
                codes.Add(0x00000028);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */

                //1.3.1 By Targren
                //[[0x10938B28] + 0x50] + 0x160
                codes.Add(0x30000000);
                codes.Add(0x10938B28);
                codes.Add(0x40000000);
                codes.Add(0x4C89FFFF);
                codes.Add(0x31000000);
                codes.Add(0x00000050);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x40000000);
                codes.Add(0x4C89FFFF);
                codes.Add(0x31000000);
                codes.Add(0x00000160);
                codes.Add(0x00120000);
                codes.Add(0x00000028);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.WeaponDurability))
            {
                //1.3.0
                /*
                codes.Add(0x30000000);
                codes.Add(0x10938798);
                codes.Add(0x42EBBFD6);
                codes.Add(0x470F71F9);
                codes.Add(0x31000000);
                codes.Add(0x0000009C);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x4024066B);
                codes.Add(0x441BB25C);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x4302537A);
                codes.Add(0x47276B1D);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */

                //Version-Free up to 1.3.0 by Skoolzout1
                //Tested working with 1.3.1
                /*
                codes.Add(0x30000000);
                codes.Add(0x101C1D40);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00002580);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000B24);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */

                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA1BA);
                codes.Add(0x4424D5F5);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x4173423E);
                codes.Add(0x457FAAA1);
                codes.Add(0x31000000);
                codes.Add(0x000007CC);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x40253955);
                codes.Add(0x441CF842);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x43597711);
                codes.Add(0x4783F28E);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);                
            }

            //
            if (cheats.Contains(Cheat.ShieldDurability))
            {
                //1.3.0
                /*
                codes.Add(0x30000000);
                codes.Add(0x10938798);
                codes.Add(0x42EBBFD6);
                codes.Add(0x470F71F9);
                codes.Add(0x31000000);
                codes.Add(0x000000F8);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x402406B1);
                codes.Add(0x441BB2A6);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x43025E8E);
                codes.Add(0x472776E1);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */

                //Version-Free 1.3.0 by Skoolzout1
                //Tested working on 1.3.1
                /*
                codes.Add(0x30000000);
                codes.Add(0x101C1D40);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00002580);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000B80);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */

                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA1BA);
                codes.Add(0x4424D5F5);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x4173423E);
                codes.Add(0x457FAAA1);
                codes.Add(0x31000000);
                codes.Add(0x000007DC);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x40244BFC);
                codes.Add(0x441BFC3B);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x430A1511);
                codes.Add(0x472FA786);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);                
            }

            //
            if (cheats.Contains(Cheat.BowDurability))
            {
                //1.3.0
                /*
                codes.Add(0x30000000);
                codes.Add(0x10938798);
                codes.Add(0x42EBBFD6);
                codes.Add(0x470F71F9);
                codes.Add(0x31000000);
                codes.Add(0x00000154);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x402406F6);
                codes.Add(0x441BB2F1);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x430269A2);
                codes.Add(0x472782A5);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */

                //1.3.0 Version Free by Skoolzout1 
                //Tested working on 1.3.1
                /*
                codes.Add(0x30000000);
                codes.Add(0x101C1D40);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00002580);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000BDC);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x10000000);
                codes.Add(0x50000000);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */

                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA1BA);
                codes.Add(0x4424D5F5);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x4173423E);
                codes.Add(0x457FAAA1);
                codes.Add(0x31000000);
                codes.Add(0x000007EC);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x40244BFC);
                codes.Add(0x441BFC3B);
                codes.Add(0x31000000);
                codes.Add(0x00000040);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x430A1511);
                codes.Add(0x472FA786);
                codes.Add(0x31000000);
                codes.Add(0x00000980);
                codes.Add(0x00120000);
                codes.Add(0x0000FFFF);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);                
            }

            //
            if (cheats.Contains(Cheat.MasterCharge))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001840);
                codes.Add(0x05120000);
                codes.Add(0x00000000);
                codes.Add(0x40800000);
                codes.Add(0x00000000);
                codes.Add(0x00120000);
                codes.Add(0x40200000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001840);
                codes.Add(0x05120000);
                codes.Add(0x00000000);
                codes.Add(0x40800000);
                codes.Add(0x00000000);
                codes.Add(0x00120000);
                codes.Add(0x40200000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);

            }

            //
            if (cheats.Contains(Cheat.MasterGlow))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF3F0);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x402C96A6);
                codes.Add(0x4424CA31);
                codes.Add(0x31000000);
                codes.Add(0x00002E6C);
                codes.Add(0x00100000);
                codes.Add(0x00000001);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                //Causes crashes if activated after beating Trials
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF3F0);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x402C96A6);
                codes.Add(0x4424CA31);
                codes.Add(0x31000000);
                codes.Add(0x00002E6C);
                codes.Add(0x00100000);
                codes.Add(0x00000001);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);

            }

            //
            if (cheats.Contains(Cheat.StasisCooldown))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387F8);
                codes.Add(0x4163401D);
                codes.Add(0x456EAB02);
                codes.Add(0x31000000);
                codes.Add(0x0000036C);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938AB8);
                codes.Add(0x4163401D);
                codes.Add(0x456EAB02);
                codes.Add(0x31000000);
                codes.Add(0x0000036C);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Stealthy))
            {
                /*
                //1.3.0
                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x000003C4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x41843686);
                codes.Add(0x4591AB61);
                codes.Add(0x31000000);
                codes.Add(0x00000134);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
                */
                //1.3.1 by Skoolzout1
                codes.Add(0x30000000);
                codes.Add(0x10938A8C);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x000003C4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x41843686);
                codes.Add(0x4591AB61);
                codes.Add(0x31000000);
                codes.Add(0x00000134);
                codes.Add(0x00120000);
                codes.Add(0x00000000);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Amiibo))
            {
                codes.Add(0x30000000);
                codes.Add(0x3EB76A78);
                codes.Add(0x3E0AECF6);
                codes.Add(0x41E15FE9);
                codes.Add(0x31000000);
                codes.Add(0x00000414);
                codes.Add(0x00120000);
                codes.Add(0x012C9985);
                codes.Add(0xD0000000);
                codes.Add(0xDEADCAFE);
            }

            //
            if (cheats.Contains(Cheat.Immune))
            {
                codes.Add(0x30000000);
                codes.Add(0x109387CC);
                codes.Add(0x402CA193);
                codes.Add(0x4424D5CC);
                codes.Add(0x31000000);
                codes.Add(0xFFFFF4E4);
                codes.Add(0x30100000);
                codes.Add(0x00000000);
                codes.Add(0x417A0192);
                codes.Add(0x4586D4CD);
                codes.Add(0x31000000);
                codes.Add(0x00001A2C);
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
 