using FlawHUD.Installer.Properties;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace FlawHUD.Installer
{
    public class HUDController
    {
        private readonly string hudPath = Settings.Default.hud_directory;
        private readonly string appPath = System.Windows.Forms.Application.StartupPath;

        /// <summary>
        /// Set the client scheme colors
        /// </summary>
        /// <remarks>TODO: Implement the ability to have colors display on text instead of panels.</remarks>
        public void Colors()
        {
            try
            {
                MainWindow.logger.Info("Updating Colors.");
                var file = hudPath + Resources.file_clientscheme_colors;
                var lines = File.ReadAllLines(file);
                // Health
                lines[FindIndex(lines, "\"Overheal\"")] = $"\t\t\"Overheal\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_health_buff)}\"";
                lines[FindIndex(lines, "OverhealPulse")] = $"\t\t\"OverhealPulse\"\t\t\t\t\"{RGBConverter(Settings.Default.color_health_buff, alpha: true)}\"";
                lines[FindIndex(lines, "\"LowHealth\"")] = $"\t\t\"LowHealth\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_health_low)}\"";
                lines[FindIndex(lines, "LowHealthPulse")] = $"\t\t\"LowHealthPulse\"\t\t\t\"{RGBConverter(Settings.Default.color_health_low, alpha: true)}\"";
                // Ammo
                lines[FindIndex(lines, "\"LowAmmo\"")] = $"\t\t\"LowAmmo\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_ammo_low)}\"";
                lines[FindIndex(lines, "LowAmmoPulse")] = $"\t\t\"LowAmmoPulse\"\t\t\t\t\"{RGBConverter(Settings.Default.color_ammo_low, alpha: true)}\"";
                // Crosshair
                lines[FindIndex(lines, "\"Crosshair\"")] = $"\t\t\"Crosshair\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_xhair_normal)}\"";
                lines[FindIndex(lines, "CrosshairDamage")] = $"\t\t\"CrosshairDamage\"\t\t\t\"{RGBConverter(Settings.Default.color_xhair_pulse)}\"";
                // ÜberCharge
                lines[FindIndex(lines, "UberCharged1")] = $"\t\t\"UberCharged1\"\t\t\t\t\"{RGBConverter(Settings.Default.color_uber_full)}\"";
                lines[FindIndex(lines, "UberCharged2")] = $"\t\t\"UberCharged2\"\t\t\t\t\"{RGBConverter(Settings.Default.color_uber_full, pulse: true)}\"";
                lines[FindIndex(lines, "UberCharging")] = $"\t\t\"UberCharging\"\t\t\t\t\"{RGBConverter(Settings.Default.color_uber_bar)}\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Colors.", Resources.error_set_colors, ex.Message);
            }
        }

        /// <summary>
        /// Set the crosshair
        /// </summary>
        public void Crosshair()
        {
            try
            {
                MainWindow.logger.Info("Updating Crosshair.");
                var file = hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "KnucklesCrosses");
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                lines[FindIndex(lines, "xpos", start)] = "\t\t\"xpos\"\t\t\t\t\"c-25\"";
                lines[FindIndex(lines, "ypos", start)] = "\t\t\"ypos\"\t\t\t\t\"c-24\"";
                lines[FindIndex(lines, "font", start)] = "\t\t\"font\"\t\t\t\t\"size:26,outline:off\"";
                File.WriteAllLines(file, lines);

                if (Settings.Default.toggle_xhair_enable)
                {
                    lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                    lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                    lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"c-{Settings.Default.val_xhair_x}\"";
                    lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"c-{Settings.Default.val_xhair_y}\"";
                    lines[FindIndex(lines, "font", start)] = $"\t\t\"font\"\t\t\t\t\"size:{Settings.Default.val_xhair_size},outline:off\"";
                    File.WriteAllLines(file, lines);
                }
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Crosshair.", Resources.error_set_xhair, ex.Message);
            }
        }

        /// <summary>
        /// Set the crosshair hitmarker
        /// </summary>
        public void CrosshairPulse()
        {
            try
            {
                MainWindow.logger.Info("Updating Crosshair Pulse.");
                var file = hudPath + Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "DamagedPlayer");
                var index1 = FindIndex(lines, "StopEvent", start);
                var index2 = FindIndex(lines, "RunEvent", start);
                lines[index1] = CommentOutTextLine(lines[index1]);
                lines[index2] = CommentOutTextLine(lines[index2]);

                if (Settings.Default.toggle_xhair_pulse)
                {
                    lines[index1] = lines[index1].Replace("//", string.Empty);
                    lines[index2] = lines[index2].Replace("//", string.Empty);
                }
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Crosshair Pulse.", Resources.error_set_xhair_pulse, ex.Message);
            }
        }

        /// <summary>
        /// Set the visibility of the Spy's disguise image
        /// </summary>
        public void DisguiseImage()
        {
            try
            {
                MainWindow.logger.Info("Updating Spy Disguise Image.");
                var file = hudPath + Resources.file_hudanimations;
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

                if (Settings.Default.toggle_disguise_image)
                {
                    lines[index1] = lines[index1].Replace("//", string.Empty);
                    lines[index2] = lines[index2].Replace("//", string.Empty);
                    lines[index3] = lines[index3].Replace("//", string.Empty);
                    lines[index4] = lines[index4].Replace("//", string.Empty);
                }
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Spy Disguise Image.", Resources.error_set_spy_disguise_image, ex.Message);
            }
        }

        /// <summary>
        /// Set the main menu backgrounds
        /// </summary>
        public void MainMenuBackground()
        {
            try
            {
                MainWindow.logger.Info("Updating Main Menu Backgrounds.");
                var directory = new DirectoryInfo(hudPath + Resources.dir_console);
                var chapterbackgrounds = hudPath + Resources.file_chapterbackgrounds;
                var chapterbackgrounds_temp = chapterbackgrounds.Replace(".txt", ".file");

                if (Settings.Default.toggle_stock_backgrounds)
                {
                    foreach (var file in directory.GetFiles())
                        File.Move(file.FullName, file.FullName.Replace("upward", "off"));
                    if (File.Exists(chapterbackgrounds))
                        File.Move(chapterbackgrounds, chapterbackgrounds_temp);
                }
                else
                {
                    foreach (var file in directory.GetFiles())
                        File.Move(file.FullName, file.FullName.Replace("off", "upward"));
                    if (File.Exists(chapterbackgrounds_temp))
                        File.Move(chapterbackgrounds_temp, chapterbackgrounds);
                }
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Main Menu Backgrounds.", Resources.error_set_menu_backgrounds, ex.Message);
            }
        }

        /// <summary>
        /// Set the visibility of the main menu class image
        /// </summary>
        public void MainMenuClassImage()
        {
            try
            {
                MainWindow.logger.Info("Updating Main Menu Class Image.");
                var file = hudPath + Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "TFCharacterImage");
                var value = (Settings.Default.toggle_menu_images) ? "-80" : "9999";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Main Menu Class Image.", Resources.error_set_menu_class_image, ex.Message);
            }
        }

        /// <summary>
        /// Set the weapon viewmodel transparency
        /// </summary>
        public void TransparentViewmodels()
        {
            try
            {
                MainWindow.logger.Info("Updating Transparent Viewmodels.");
                var file = hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"TransparentViewmodel\"");
                var index1 = FindIndex(lines, "visible", start);
                var index2 = FindIndex(lines, "enabled", start);
                lines[index1] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"0\"";
                if (File.Exists(hudPath + Resources.file_cfg))
                    File.Delete(hudPath + Resources.file_cfg);

                if (Settings.Default.toggle_transparent_viewmodels)
                {
                    lines[index1] = "\t\t\"visible\"\t\t\t\"1\"";
                    lines[index2] = "\t\t\"enabled\"\t\t\t\"1\"";
                    File.Copy(appPath + "\\hud.cfg", hudPath + Resources.file_cfg);
                }
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Transparent Viewmodels.", Resources.error_set_transparent_viewmodels, ex.Message);
            }
        }

        /// <summary>
        /// Set the visibility of the main menu class image
        /// </summary>
        public void CodeProFonts()
        {
            try
            {
                MainWindow.logger.Info("Updating Custom Fonts.");
                var file = hudPath + Resources.file_clientscheme;
                var lines = File.ReadAllLines(file);
                var value = (Settings.Default.toggle_code_pro_fonts) ? "clientscheme_fonts" : "clientscheme_fonts_tf";
                lines[FindIndex(lines, "clientscheme_fonts")] = $"#base \"scheme/{value}.res\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Custom Fonts.", Resources.error_set_fonts, ex.Message);
            }
        }

        /// <summary>
        /// Installs the latest version of CastingEssentials
        /// </summary>
        public void CastingEssentials()
        {
            try
            {
                // Skip this step if CastingEssentials is already installed and the user doesn't have the option checked.
                if (!Directory.Exists(hudPath + "\\CastingEssentials") && Settings.Default.toggle_casting_essentials == true)
                {
                    // Download the latest version of CastingEssentials.
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    var client = new WebClient();
                    client.DownloadFile("https://github.com/PazerOP/CastingEssentials/releases/download/r21/CastingEssentials_r21.zip", "CastingEssentials.zip");
                    client.Dispose();

                    // Extract it into the tf/custom directory
                    var appPath = System.Windows.Forms.Application.StartupPath;
                    ZipFile.ExtractToDirectory(appPath + "\\CastingEssentials.zip", Settings.Default.hud_directory);

                    // Remove the downloaded file from the installer directory.
                    if (File.Exists(appPath + "\\CastingEssentials.zip"))
                        File.Delete(appPath + "\\CastingEssentials.zip");
                }
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Custom Fonts.", Resources.error_set_fonts, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the index of where a given value was found in a string array.
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
        /// Clear all existing comment identifiers, then apply a fresh one.
        /// </summary>
        public string CommentOutTextLine(string value)
        {
            value = value.Replace("//", string.Empty);
            return string.Concat("//", value);
        }

        /// <summary>
        /// Convert color HEX code to RGB
        /// </summary>
        /// <param name="hex">The HEX code representing the color to convert to RGB</param>
        /// <param name="pulse">Flag the color as a pulse, slightly lowering the alpha</param>
        private static string RGBConverter(string hex, bool alpha = false, bool pulse = false)
        {
            var color = System.Drawing.ColorTranslator.FromHtml(hex);
            var alpha_new = (alpha == true) ? "200" : color.A.ToString();
            var pulse_new = (pulse == true && color.G >= 50) ? color.G - 50 : color.G;
            return $"{color.R} {pulse_new} {color.B} {alpha_new}";
        }
    }
}