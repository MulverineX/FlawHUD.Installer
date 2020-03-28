using FlawHUD.Installer.Properties;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace FlawHUD.Installer
{
    public class HUDController
    {
        private readonly string hudPath = Settings.Default.hud_directory;

        /// <summary>
        /// Set the client scheme colors
        /// </summary>
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
        /// <remarks>TODO: Implement the ability to have colors display on text instead of panels.</remarks>
        public void Colors()
        {
            try
            {
                MainWindow.logger.Info("Updating Colors.");
                var file = hudPath + Resources.file_clientscheme_colors;
                var lines = File.ReadAllLines(file);
                // Health
                lines[28] = $"\t\t\"Overheal\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_health_buff)}\"";
                lines[29] = $"\t\t\"OverhealPulse\"\t\t\t\t\"{RGBConverter(Settings.Default.color_health_buff, alpha: true)}\"";
                lines[30] = $"\t\t\"LowHealth\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_health_low)}\"";
                lines[31] = $"\t\t\"LowHealthPulse\"\t\t\t\"{RGBConverter(Settings.Default.color_health_low, alpha: true)}\"";
                // Ammo
                lines[32] = $"\t\t\"LowAmmo\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_ammo_low)}\"";
                lines[33] = $"\t\t\"LowAmmoPulse\"\t\t\t\t\"{RGBConverter(Settings.Default.color_ammo_low, alpha: true)}\"";
                // Crosshair
                lines[36] = $"\t\t\"Crosshair\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_xhair_normal)}\"";
                lines[37] = $"\t\t\"CrosshairDamage\"\t\t\t\"{RGBConverter(Settings.Default.color_xhair_pulse)}\"";
                // ÜberCharge
                lines[40] = $"\t\t\"UberCharged1\"\t\t\t\t\"{RGBConverter(Settings.Default.color_uber_full)}\"";
                lines[41] = $"\t\t\"UberCharged2\"\t\t\t\t\"{RGBConverter(Settings.Default.color_uber_full, pulse: true)}\"";
                lines[42] = $"\t\t\"UberCharging\"\t\t\t\t\"{RGBConverter(Settings.Default.color_uber_bar)}\"";
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
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
        public void Crosshair()
        {
            try
            {
                MainWindow.logger.Info("Updating Crosshair.");
                var file = hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                lines[11] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[12] = "\t\t\"enabled\"\t\t\t\"0\"";
                lines[17] = "\t\t\"xpos\"\t\t\t\t\"c-25\"";
                lines[18] = "\t\t\"ypos\"\t\t\t\t\"c-24\"";
                lines[21] = "\t\t\"font\"\t\t\t\t\"size:26,outline:off\"";
                File.WriteAllLines(file, lines);

                if (Settings.Default.toggle_xhair_enable)
                {
                    lines[11] = "\t\t\"visible\"\t\t\t\"1\"";
                    lines[12] = "\t\t\"enabled\"\t\t\t\"1\"";
                    lines[17] = $"\t\t\"xpos\"\t\t\t\t\"c-{Settings.Default.val_xhair_x}\"";
                    lines[18] = $"\t\t\"ypos\"\t\t\t\t\"c-{Settings.Default.val_xhair_y}\"";
                    lines[21] = $"\t\t\"font\"\t\t\t\t\"size:{Settings.Default.val_xhair_size},outline:off\"";
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
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
        public void CrosshairPulse()
        {
            try
            {
                MainWindow.logger.Info("Updating Crosshair Pulse.");
                var file = hudPath + Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                lines[96] = CommentOutTextLine(lines[96]);
                lines[97] = CommentOutTextLine(lines[97]);

                if (Settings.Default.toggle_xhair_pulse)
                {
                    lines[96] = lines[96].Replace("//", string.Empty);
                    lines[97] = lines[97].Replace("//", string.Empty);
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
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
        public void DisguiseImage()
        {
            try
            {
                MainWindow.logger.Info("Updating Spy Disguise Image.");
                var file = hudPath + Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                lines[105] = CommentOutTextLine(lines[105]);
                lines[106] = CommentOutTextLine(lines[106]);
                lines[111] = CommentOutTextLine(lines[111]);
                lines[112] = CommentOutTextLine(lines[112]);

                if (Settings.Default.toggle_disguise_image)
                {
                    lines[105] = lines[105].Replace("//", string.Empty);
                    lines[106] = lines[106].Replace("//", string.Empty);
                    lines[111] = lines[111].Replace("//", string.Empty);
                    lines[112] = lines[112].Replace("//", string.Empty);
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
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
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
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
        public void MainMenuClassImage()
        {
            try
            {
                MainWindow.logger.Info("Updating Main Menu Class Image.");
                var file = hudPath + Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(file);
                var value = (Settings.Default.toggle_menu_images) ? "-80" : "9999";
                lines[199] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
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
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
        /// <remarks>TODO: Add the transparent viewmodels configuration file.</remarks>
        public void TransparentViewmodels()
        {
            try
            {
                MainWindow.logger.Info("Updating Transparent Viewmodels.");
                var file = hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                lines[39] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[40] = "\t\t\"enabled\"\t\t\t\"0\"";

                if (Settings.Default.toggle_transparent_viewmodels)
                {
                    lines[39] = "\t\t\"visible\"\t\t\t\"1\"";
                    lines[40] = "\t\t\"enabled\"\t\t\t\"1\"";
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
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
        public void CodeProFonts()
        {
            try
            {
                MainWindow.logger.Info("Updating Custom Fonts.");
                var file = hudPath + Resources.file_clientscheme;
                var lines = File.ReadAllLines(file);
                var value = (Settings.Default.toggle_code_pro_fonts) ? "clientscheme_fonts" : "clientscheme_fonts_tf";
                lines[2] = $"#base \"scheme/{value}.res\"";
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