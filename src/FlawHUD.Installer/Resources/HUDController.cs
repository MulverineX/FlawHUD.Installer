using System;
using System.Drawing;
using System.IO;
using System.Linq;
using FlawHUD.Installer.Properties;

namespace FlawHUD.Installer
{
    public class HUDController
    {
        private readonly string _appPath = Directory.GetCurrentDirectory();
        private readonly string _hudPath = Settings.Default.hud_directory;

        /// <summary>
        ///     Set the client scheme colors
        /// </summary>
        public void Colors()
        {
            try
            {
                MainWindow.Logger.Info("Updating Colors.");
                var file = _hudPath + Resources.file_clientscheme_colors;
                var lines = File.ReadAllLines(file);
                // Health
                lines[FindIndex(lines, "\"Overheal\"")] =
                    $"\t\t\"Overheal\"\t\t\t\t\t\"{RgbConverter(Settings.Default.color_health_buff)}\"";
                lines[FindIndex(lines, "OverhealPulse")] =
                    $"\t\t\"OverhealPulse\"\t\t\t\t\"{RgbConverter(Settings.Default.color_health_buff, true)}\"";
                lines[FindIndex(lines, "\"LowHealth\"")] =
                    $"\t\t\"LowHealth\"\t\t\t\t\t\"{RgbConverter(Settings.Default.color_health_low)}\"";
                lines[FindIndex(lines, "LowHealthPulse")] =
                    $"\t\t\"LowHealthPulse\"\t\t\t\"{RgbConverter(Settings.Default.color_health_low, true)}\"";
                // Ammo
                lines[FindIndex(lines, "\"LowAmmo\"")] =
                    $"\t\t\"LowAmmo\"\t\t\t\t\t\"{RgbConverter(Settings.Default.color_ammo_low)}\"";
                lines[FindIndex(lines, "LowAmmoPulse")] =
                    $"\t\t\"LowAmmoPulse\"\t\t\t\t\"{RgbConverter(Settings.Default.color_ammo_low, true)}\"";
                // Misc
                lines[FindIndex(lines, "\"PositiveValue\"")] =
                    $"\t\t\"PositiveValue\"\t\t\t\t\"{RgbConverter(Settings.Default.color_health_buff)}\"";
                lines[FindIndex(lines, "NegativeValue")] =
                    $"\t\t\"NegativeValue\"\t\t\t\t\"{RgbConverter(Settings.Default.color_health_low, true)}\"";
                // Crosshair
                lines[FindIndex(lines, "\"Crosshair\"")] =
                    $"\t\t\"Crosshair\"\t\t\t\t\t\"{RgbConverter(Settings.Default.color_xhair_normal)}\"";
                lines[FindIndex(lines, "CrosshairDamage")] =
                    $"\t\t\"CrosshairDamage\"\t\t\t\"{RgbConverter(Settings.Default.color_xhair_pulse)}\"";
                // ÜberCharge
                lines[FindIndex(lines, "UberCharged1")] =
                    $"\t\t\"UberCharged1\"\t\t\t\t\"{RgbConverter(Settings.Default.color_uber_full)}\"";
                lines[FindIndex(lines, "UberCharged2")] =
                    $"\t\t\"UberCharged2\"\t\t\t\t\"{RgbConverter(Settings.Default.color_uber_full, pulse: true)}\"";
                lines[FindIndex(lines, "UberCharging")] =
                    $"\t\t\"UberCharging\"\t\t\t\t\"{RgbConverter(Settings.Default.color_uber_bar)}\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Colors.", Resources.error_set_colors, ex.Message);
            }
        }

        /// <summary>
        ///     Set the health and ammo colors to be displayed on text instead of a panel
        /// </summary>
        public void ColorText(bool colorText = false)
        {
            try
            {
                var file = _hudPath + Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                // Panels
                CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "HealthBG", !colorText);
                CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "HealthBG", !colorText);
                CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoBG", !colorText);
                // Text
                CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "PlayerStatusHealthValue", colorText);
                CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "PlayerStatusHealthValue", colorText);
                CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoInClip", colorText);
                CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoInReserve", colorText);
                CommentOutTextLineSuper(lines, "HudLowAmmoPulse", "AmmoNoClip", colorText);
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Colors.", Resources.error_set_colors, ex.Message);
            }
        }

        /// <summary>
        ///     Set the crosshair
        /// </summary>
        public void Crosshair(string style, int? size, string effect)
        {
            try
            {
                MainWindow.Logger.Info("Updating Crosshair.");
                var file = _hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "CustomCrosshair");
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                lines[FindIndex(lines, "\"labelText\"", start)] = "\t\t\"labelText\"\t\t\t\"<\"";
                lines[FindIndex(lines, "xpos", start)] = "\t\t\"xpos\"\t\t\t\t\"c-50\"";
                lines[FindIndex(lines, "ypos", start)] = "\t\t\"ypos\"\t\t\t\t\"c-49\"";
                lines[FindIndex(lines, "font", start)] = "\t\t\"font\"\t\t\t\t\"Size:18 | Outline:OFF\"";
                File.WriteAllLines(file, lines);

                if (!Settings.Default.toggle_xhair_enable) return;
                var strEffect = effect != "None" ? $"{effect}:ON" : "Outline:OFF";
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                lines[FindIndex(lines, "\"labelText\"", start)] = $"\t\t\"labelText\"\t\t\t\"{style}\"";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"c-{Settings.Default.val_xhair_x}\"";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"c-{Settings.Default.val_xhair_y}\"";
                lines[FindIndex(lines, "font", start)] = $"\t\t\"font\"\t\t\t\t\"Size:{size} | {strEffect}\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Crosshair.", Resources.error_set_xhair, ex.Message);
            }
        }

        /// <summary>
        ///     Set the crosshair hitmarker
        /// </summary>
        public void CrosshairPulse()
        {
            try
            {
                MainWindow.Logger.Info("Updating Crosshair Pulse.");
                var file = _hudPath + Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "DamagedPlayer");
                var index1 = FindIndex(lines, "StopEvent", start);
                var index2 = FindIndex(lines, "RunEvent", start);
                lines[index1] = CommentOutTextLine(lines[index1]);
                lines[index2] = CommentOutTextLine(lines[index2]);
                File.WriteAllLines(file, lines);

                if (!Settings.Default.toggle_xhair_pulse) return;
                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Crosshair Pulse.", Resources.error_set_xhair_pulse, ex.Message);
            }
        }

        /// <summary>
        ///     Set the rotating crosshair
        /// </summary>
        public void CrosshairRotate()
        {
            try
            {
                MainWindow.Logger.Info("Updating Rotating Crosshair.");

                var file = _hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"Crosshair\"");
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                start = FindIndex(lines, "\"CrosshairPulse\"");
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (!Settings.Default.toggle_xhair_rotate) return;
                start = FindIndex(lines, "\"Crosshair\"");
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                start = FindIndex(lines, "\"CrosshairPulse\"");
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Rotating Crosshair.", Resources.error_set_xhair, ex.Message);
            }
        }

        /// <summary>
        ///     Set the visibility of the Spy's disguise image
        /// </summary>
        public void DisguiseImage()
        {
            try
            {
                MainWindow.Logger.Info("Updating Spy Disguise Image.");
                var file = _hudPath + Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudSpyDisguiseFadeIn");
                var index1 = FindIndex(lines, "RunEvent", start);
                var index2 = FindIndex(lines, "Animate", start);
                start = FindIndex(lines, "HudSpyDisguiseFadeOut");
                var index3 = FindIndex(lines, "RunEvent", start);
                var index4 = FindIndex(lines, "Animate", start);
                lines[index1] = CommentOutTextLine(lines[index1]);
                lines[index2] = CommentOutTextLine(lines[index2]);
                lines[index3] = CommentOutTextLine(lines[index3]);
                lines[index4] = CommentOutTextLine(lines[index4]);
                File.WriteAllLines(file, lines);

                if (!Settings.Default.toggle_disguise_image) return;
                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                lines[index3] = lines[index3].Replace("//", string.Empty);
                lines[index4] = lines[index4].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Spy Disguise Image.", Resources.error_set_spy_disguise_image,
                    ex.Message);
            }
        }

        /// <summary>
        ///     Set the main menu backgrounds
        /// </summary>
        public void MainMenuBackground()
        {
            try
            {
                MainWindow.Logger.Info("Updating Main Menu Backgrounds.");
                var directory = new DirectoryInfo(_hudPath + Resources.dir_console);
                var chapterbackgrounds = _hudPath + Resources.file_chapterbackgrounds;
                var chapterbackgroundsTemp = chapterbackgrounds.Replace(".txt", ".file");

                if (Settings.Default.toggle_stock_backgrounds)
                {
                    foreach (var file in directory.GetFiles())
                        File.Move(file.FullName, file.FullName.Replace("upward", "off"));
                    if (File.Exists(chapterbackgrounds))
                        File.Move(chapterbackgrounds, chapterbackgroundsTemp);
                }
                else
                {
                    foreach (var file in directory.GetFiles())
                        File.Move(file.FullName, file.FullName.Replace("off", "upward"));
                    if (File.Exists(chapterbackgroundsTemp))
                        File.Move(chapterbackgroundsTemp, chapterbackgrounds);
                }
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Main Menu Backgrounds.", Resources.error_set_menu_backgrounds,
                    ex.Message);
            }
        }

        /// <summary>
        ///     Set the visibility of the main menu class image
        /// </summary>
        public void MainMenuClassImage()
        {
            try
            {
                MainWindow.Logger.Info("Updating Main Menu Class Image.");
                var file = _hudPath + Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "TFCharacterImage");
                var value = Settings.Default.toggle_menu_images ? "-80" : "9999";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Main Menu Class Image.", Resources.error_set_menu_class_image,
                    ex.Message);
            }
        }

        /// <summary>
        ///     Set the weapon viewmodel transparency
        /// </summary>
        public void TransparentViewmodels()
        {
            try
            {
                MainWindow.Logger.Info("Updating Transparent Viewmodels.");
                var file = _hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"TransparentViewmodel\"");
                var index1 = FindIndex(lines, "visible", start);
                var index2 = FindIndex(lines, "enabled", start);
                lines[index1] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (!Settings.Default.toggle_transparent_viewmodels) return;
                lines[index1] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"1\"";

                if (!Directory.Exists(_hudPath + "\\flawhud\\cfg"))
                    Directory.CreateDirectory(_hudPath + "\\flawhud\\cfg");
                if (File.Exists(_hudPath + Resources.file_cfg))
                    File.Delete(_hudPath + Resources.file_cfg);
                File.Copy(_appPath + "\\hud.cfg", _hudPath + Resources.file_cfg);
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Transparent Viewmodels.",
                    Resources.error_set_transparent_viewmodels, ex.Message);
            }
        }

        /// <summary>
        ///     Set the health indicator to the cross style.
        /// </summary>
        public void HealthStyle()
        {
            try
            {
                var file = _hudPath + Resources.file_playerhealth;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"PlayerStatusHealthBonusImage\"");
                var index = FindIndex(lines, "image", start);
                lines[index] = "\t\t\"image\"\t\t\t\"\"";

                ColorText(Settings.Default.val_health_style == 1);

                if (Settings.Default.val_health_style == 2)
                {
                    lines[index] = "\t\t\"image\"\t\t\t\"../hud/health_over_bg\"";
                    File.WriteAllLines(file, lines);

                    file = _hudPath + Resources.file_hudanimations;
                    lines = File.ReadAllLines(file);
                    CommentOutTextLineSuper(lines, "HudHealthBonusPulse", "HealthBG", false);
                    CommentOutTextLineSuper(lines, "HudHealthDyingPulse", "HealthBG", false);
                }
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Colors.", Resources.error_set_colors, ex.Message);
            }
        }

        /// <summary>
        ///     Set the visibility of the main menu class image
        /// </summary>
        public void CodeProFonts()
        {
            try
            {
                MainWindow.Logger.Info("Updating Custom Fonts.");
                var file = _hudPath + Resources.file_clientscheme;
                var lines = File.ReadAllLines(file);
                var value = Settings.Default.toggle_code_fonts ? "clientscheme_fonts" : "clientscheme_fonts_tf";
                lines[FindIndex(lines, "clientscheme_fonts")] = $"#base \"scheme/{value}.res\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Custom Fonts.", Resources.error_set_fonts, ex.Message);
            }
        }

        /// <summary>
        ///     Retrieves the index of where a given value was found in a string array.
        /// </summary>
        public int FindIndex(string[] array, string value, int skip = 0)
        {
            var list = array.Skip(skip);
            var index = list.Select((v, i) => new { Index = i, Value = v }) // Pair up values and indexes
                .Where(p => p.Value.Contains(value)) // Do the filtering
                .Select(p => p.Index); // Keep the index and drop the value
            return index.First() + skip;
        }

        /// <summary>
        ///     Clear all existing comment identifiers, then apply a fresh one.
        /// </summary>
        public string CommentOutTextLine(string value)
        {
            value = value.Replace("//", string.Empty);
            return string.Concat("//", value);
        }

        /// <summary>
        ///     Clear all existing comment identifiers, then apply a fresh one.
        /// </summary>
        public string[] CommentOutTextLineSuper(string[] lines, string start, string query, bool commentOut)
        {
            var index1 = FindIndex(lines, query, FindIndex(lines, start));
            var index2 = FindIndex(lines, query, index1++);
            lines[index1] = commentOut ? lines[index1].Replace("//", string.Empty) : CommentOutTextLine(lines[index1]);
            lines[index2] = commentOut ? lines[index2].Replace("//", string.Empty) : CommentOutTextLine(lines[index2]);
            return lines;
        }

        /// <summary>
        ///     Convert color HEX code to RGB
        /// </summary>
        /// <param name="hex">The HEX code representing the color to convert to RGB</param>
        /// <param name="alpha">Flag the code as having a lower alpha value than normal</param>
        /// <param name="pulse">Flag the color as a pulse, slightly lowering the alpha</param>
        private static string RgbConverter(string hex, bool alpha = false, bool pulse = false)
        {
            var color = ColorTranslator.FromHtml(hex);
            var alphaNew = alpha ? "200" : color.A.ToString();
            var pulseNew = pulse && color.G >= 50 ? color.G - 50 : color.G;
            return $"{color.R} {pulseNew} {color.B} {alphaNew}";
        }
    }
}