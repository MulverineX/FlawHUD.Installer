using FlawHUD.Installer.Properties;
using System;
using System.IO;
using System.Linq;

namespace FlawHUD.Installer
{
    public class HUDController
    {
        private readonly string hudPath = Settings.Default.hud_directory;

        /// <summary>
        /// Set the client scheme colors
        /// </summary>
        /// <remarks>TODO: Find variables by name instead of index.</remarks>
        /// <remarks>TODO: Separate health colors from Ammo (FlawHUD)</remarks>
        public void Colors()
        {
            try
            {
                MainWindow.logger.Info("Updating Colors.");
                var file = hudPath + Resources.file_clientscheme_colors;
                var lines = File.ReadAllLines(file);
                // Health
                lines[28] = $"\t\t\"Overheal\"\t\t\t\t\"{RGBConverter(Settings.Default.color_health_normal)}\"";
                lines[30] = $"\t\t\"LowValue\"\t\t\t\t\"{RGBConverter(Settings.Default.color_health_low)}\"";
                // Ammo
                lines[31] = $"\t\t\"OverhealPulse\"\t\t\t\t\"{RGBConverter(Settings.Default.color_ammo_clip)}\"";
                lines[32] = $"\t\t\"LowValuePulse\"\t\t\t\"{RGBConverter(Settings.Default.color_ammo_clip_low)}\"";
                // Crosshair
                lines[35] = $"\t\t\"Crosshair\"\t\t\t\t\t\"{RGBConverter(Settings.Default.color_xhair_normal)}\"";
                lines[36] = $"\t\t\"CrosshairDamage\"\t\t\t\"{RGBConverter(Settings.Default.color_xhair_pulse)}\"";
                // Ubercharge
                lines[40] = $"\t\t\"Ubercharge1\"\t\t\t\"{RGBConverter(Settings.Default.color_uber_bar)}\"";
                lines[41] = $"\t\t\"Ubercharge2\"\t\t\t\"{RGBConverter(Settings.Default.color_uber_full)}\"";
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
                lines[11] = "\t\t\"visible\"\t\t\"0\"";
                lines[12] = "\t\t\"enabled\"\t\t\"0\"";
                lines[17] = "\t\t\"xpos\"\t\t\"c-25\"";
                lines[18] = "\t\t\"xpos\"\t\t\"c-24\"";
                lines[21] = "\t\t\"font\"\t\t\"size:26,outline:off\"";
                File.WriteAllLines(file, lines);

                if (Settings.Default.toggle_xhair_enable)
                {
                    lines[11] = "\t\t\"visible\"\t\t\"1\"";
                    lines[12] = "\t\t\"enabled\"\t\t\"1\"";
                    lines[17] = $"\t\t\"xpos\"\t\t\"c-{Settings.Default.val_xhair_x}\"";
                    lines[18] = $"\t\t\"ypos\"\t\t\"c-{Settings.Default.val_xhair_y}\"";
                    lines[21] = $"\t\t\"font\"\t\t\"size:{Settings.Default.val_xhair_size},outline:off\"";
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
                lines[96] = CommentOutTextLine(lines[133]);
                lines[97] = CommentOutTextLine(lines[134]);

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
                lines[109] = CommentOutTextLine(lines[109]);
                lines[110] = CommentOutTextLine(lines[110]);
                lines[115] = CommentOutTextLine(lines[115]);
                lines[116] = CommentOutTextLine(lines[116]);

                if (Settings.Default.toggle_disguise_image)
                {
                    lines[109] = lines[109].Replace("//", string.Empty);
                    lines[110] = lines[110].Replace("//", string.Empty);
                    lines[115] = lines[115].Replace("//", string.Empty);
                    lines[116] = lines[116].Replace("//", string.Empty);
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
                var chapterbackgrounds_temp = hudPath + (Resources.file_chapterbackgrounds.Replace(".txt", ".file"));

                if (Settings.Default.toggle_stock_backgrounds)
                {
                    foreach (FileInfo file in directory.GetFiles())
                        file.Delete();
                    if (File.Exists(chapterbackgrounds))
                        File.Move(chapterbackgrounds, chapterbackgrounds_temp);
                }
                else
                {
                    if (Directory.GetFiles(directory.ToString()).Count() == 0)
                        CopyBackgroundFiles();
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
                lines[1012] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
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
                lines[699] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[700] = "\t\t\"enabled\"\t\t\t\"0\"";

                if (Settings.Default.toggle_transparent_viewmodels)
                {
                    lines[699] = "\t\t\"visible\"\t\t\t\"1\"";
                    lines[700] = "\t\t\"enabled\"\t\t\t\"1\"";
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
                lines[2] = $"#base \"scheme\\{value}.res\"";
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Updating Custom Fonts.", Resources.error_set_fonts, ex.Message);
            }
        }

        /// <summary>
        /// Copy the background images to the materials/console folder.
        /// </summary>
        public void CopyBackgroundFiles()
        {
            var directory = new DirectoryInfo(hudPath + Resources.dir_console);
            var background_base = hudPath + Resources.dir_console + "upward.vtf";
            var background_wide = hudPath + Resources.dir_console + "upward_widescreen.vtf";

            foreach (FileInfo file in directory.GetFiles())
                file.Delete();
            File.Copy(background_base, directory + "background_upward.vtf");
            File.Copy(background_wide, directory + "background_upward_widescreen.vtf");
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
        private static string RGBConverter(string hex)
        {
            var color = System.Drawing.ColorTranslator.FromHtml(hex);
            return $"{color.R} {color.G} {color.B} {color.A}";
        }
    }
}