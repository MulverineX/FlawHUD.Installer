using System;
using System.Drawing;
using System.IO;
using System.Linq;
using FlawHUD.Installer.Properties;

namespace FlawHUD.Installer
{
    public class HUDController
    {
        private readonly string _hudPath = Settings.Default.hud_directory;

        /// <summary>
        ///     Update the client scheme colors.
        /// </summary>
        public bool Colors()
        {
            try
            {
                MainWindow.Logger.Info("Updating the color client scheme.");
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
                lines[FindIndex(lines, "\"TargetHealth\"")] =
                    $"\t\t\"TargetHealth\"\t\t\t\t\"{RgbConverter(Settings.Default.color_target_health)}\"";
                lines[FindIndex(lines, "TargetDamage")] =
                    $"\t\t\"TargetDamage\"\t\t\t\t\"{RgbConverter(Settings.Default.color_target_damage)}\"";
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
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating colors.", Resources.error_set_colors, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the health and ammo colors to be displayed on text instead of a panel.
        /// </summary>
        public bool ColorText(bool colorText = false)
        {
            try
            {
                MainWindow.Logger.Info("Setting player health color style.");
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
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating player health color style.", Resources.error_set_colors,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the crosshair style, position and effect.
        /// </summary>
        public bool Crosshair(string style, int? size, string effect)
        {
            try
            {
                MainWindow.Logger.Info("Updating crosshair settings.");
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

                if (Settings.Default.toggle_xhair_rotate) return true;
                if (!Settings.Default.toggle_xhair_enable) return true;
                var strEffect = effect != "None" ? $"{effect}:ON" : "Outline:OFF";
                lines[FindIndex(lines, "visible", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "enabled", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                lines[FindIndex(lines, "\"labelText\"", start)] = $"\t\t\"labelText\"\t\t\t\"{style}\"";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"c-{Settings.Default.val_xhair_x}\"";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"c-{Settings.Default.val_xhair_y}\"";
                lines[FindIndex(lines, "font", start)] = $"\t\t\"font\"\t\t\t\t\"Size:{size} | {strEffect}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating crosshair settings.", Resources.error_set_xhair,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the crosshair hitmarker.
        /// </summary>
        public bool CrosshairPulse()
        {
            try
            {
                MainWindow.Logger.Info("Toggling crosshair hitmarker.");
                var file = _hudPath + Resources.file_hudanimations;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "DamagedPlayer");
                var index1 = FindIndex(lines, "StopEvent", start);
                var index2 = FindIndex(lines, "RunEvent", start);
                lines[index1] = CommentOutTextLine(lines[index1]);
                lines[index2] = CommentOutTextLine(lines[index2]);
                File.WriteAllLines(file, lines);

                if (!Settings.Default.toggle_xhair_pulse) return true;
                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling crosshair hitmarker.", Resources.error_set_xhair_pulse,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the rotating crosshair.
        /// </summary>
        public bool CrosshairRotate()
        {
            try
            {
                MainWindow.Logger.Info("Toggling rotating crosshairs.");

                var file = _hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"Crosshair\"");
                lines[FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                start = FindIndex(lines, "\"CrosshairPulse\"");
                lines[FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (!Settings.Default.toggle_xhair_enable) return true;
                if (!Settings.Default.toggle_xhair_rotate) return true;
                start = FindIndex(lines, "\"Crosshair\"");
                lines[FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                start = FindIndex(lines, "\"CrosshairPulse\"");
                lines[FindIndex(lines, "\"visible\"", start)] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[FindIndex(lines, "\"enabled\"", start)] = "\t\t\"enabled\"\t\t\t\"1\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling rotating crosshairs.", Resources.error_set_xhair,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the visibility of the Spy's disguise image.
        /// </summary>
        public bool DisguiseImage()
        {
            try
            {
                MainWindow.Logger.Info("Toggling the Spy's disguise image.");
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

                if (!Settings.Default.toggle_disguise_image) return true;
                lines[index1] = lines[index1].Replace("//", string.Empty);
                lines[index2] = lines[index2].Replace("//", string.Empty);
                lines[index3] = lines[index3].Replace("//", string.Empty);
                lines[index4] = lines[index4].Replace("//", string.Empty);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling the Spy's disguise image.",
                    Resources.error_set_spy_disguise_image, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the custom main menu backgrounds.
        /// </summary>
        public bool MainMenuBackground()
        {
            try
            {
                MainWindow.Logger.Info("Toggling custom main menu backgrounds.");
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

                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling custom main menu backgrounds.",
                    Resources.error_set_menu_backgrounds,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the visibility of the main menu class images.
        /// </summary>
        public bool MainMenuClassImage()
        {
            try
            {
                MainWindow.Logger.Info("Toggling main menu class images.");
                var file = _hudPath + Resources.file_mainmenuoverride;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "TFCharacterImage");
                var value = Settings.Default.toggle_menu_images ? "-80" : "9999";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling main menu class images.",
                    Resources.error_set_menu_class_image,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle the weapon viewmodel transparency.
        /// </summary>
        public bool TransparentViewmodels()
        {
            try
            {
                MainWindow.Logger.Info("Toggling transparent viewmodels.");
                var file = _hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "\"TransparentViewmodel\"");
                var index1 = FindIndex(lines, "visible", start);
                var index2 = FindIndex(lines, "enabled", start);
                lines[index1] = "\t\t\"visible\"\t\t\t\"0\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"0\"";
                File.WriteAllLines(file, lines);

                if (!Settings.Default.toggle_transparent_viewmodels) return true;
                lines[index1] = "\t\t\"visible\"\t\t\t\"1\"";
                lines[index2] = "\t\t\"enabled\"\t\t\t\"1\"";

                if (!Directory.Exists(_hudPath + "\\flawhud\\cfg"))
                    Directory.CreateDirectory(_hudPath + "\\flawhud\\cfg");
                if (File.Exists(_hudPath + Resources.file_cfg))
                    File.Delete(_hudPath + Resources.file_cfg);
                File.Copy(Directory.GetCurrentDirectory() + "\\hud.cfg", _hudPath + Resources.file_cfg);
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error toggling transparent viewmodels.",
                    Resources.error_set_transparent_viewmodels, ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Update the health indicator to use the cross style.
        /// </summary>
        public bool HealthStyle()
        {
            try
            {
                MainWindow.Logger.Info("Setting player health style.");
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
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error setting player health style.", Resources.error_set_colors,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Toggle to a Code Pro font instead of the default.
        /// </summary>
        public bool CodeProFonts()
        {
            try
            {
                MainWindow.Logger.Info("Setting to the preferred font.");
                var file = _hudPath + Resources.file_clientscheme;
                var lines = File.ReadAllLines(file);
                var value = Settings.Default.toggle_code_fonts ? "clientscheme_fonts_pro" : "clientscheme_fonts";
                lines[FindIndex(lines, "clientscheme_fonts")] = $"#base \"scheme/{value}.res\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error setting to the preferred font.", Resources.error_set_fonts,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Set the number of rows shown on the killfeed.
        /// </summary>
        public bool KillFeedRows()
        {
            try
            {
                MainWindow.Logger.Info("Setting the killfeed row count.");
                var file = _hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudDeathNotice");
                var value = Settings.Default.val_killfeed_rows;
                lines[FindIndex(lines, "MaxDeathNotices", start)] = $"\t\t\"MaxDeathNotices\"\t\t\"{value}\"";
                File.WriteAllLines(file, lines);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error setting the killfeed row count.",
                    Resources.error_set_menu_class_image,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Lowers the player health and ammo.
        /// </summary>
        public bool LowerPlayerStats()
        {
            try
            {
                MainWindow.Logger.Info("Updating player health and ammo positions.");
                var file = _hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudWeaponAmmo");
                var value = Settings.Default.toggle_lower_stats ? "r83" : "c93";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudMannVsMachineStatus");
                value = Settings.Default.toggle_lower_stats ? "-55" : "0";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "CHealthAccountPanel");
                value = Settings.Default.toggle_lower_stats ? "r150" : "267";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "CSecondaryTargetID");
                value = Settings.Default.toggle_lower_stats ? "325" : "355";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudMenuSpyDisguise");
                value = Settings.Default.toggle_lower_stats ? "c60" : "c130";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudSpellMenu");
                value = Settings.Default.toggle_lower_stats ? "c-270" : "c-210";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Resources.file_huddamageaccount;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "\"DamageAccountValue\"");
                value = Settings.Default.toggle_lower_stats ? "r105" : "0";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudPlayerHealth");
                value = Settings.Default.toggle_lower_stats ? "r108" : "c68";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                SetItemEffectPosition(string.Format(_hudPath + Resources.file_itemeffectmeter, ""));
                SetItemEffectPosition(string.Format(_hudPath + Resources.file_itemeffectmeter, "_cleaver"),
                    Positions.Middle);
                SetItemEffectPosition(string.Format(_hudPath + Resources.file_itemeffectmeter, "_sodapopper"),
                    Positions.Top);
                SetItemEffectPosition(_hudPath + Resources.dir_resource_ui + "\\huddemomancharge.res", Positions.Middle,
                    "ChargeMeter");
                SetItemEffectPosition(_hudPath + Resources.dir_resource_ui + "\\huddemomanpipes.res", Positions.Default,
                    "PipesPresentPanel");
                SetItemEffectPosition(_hudPath + Resources.dir_resource_ui + "\\hudrocketpack.res", Positions.Middle);
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error updating player health and ammo positions.",
                    Resources.error_set_lower_player_stats,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Repositions the player health and ammo.
        /// </summary>
        public bool AlternatePlayerStats()
        {
            try
            {
                // Skip if the player already has "Lowered Player Stats" enabled.
                if (Settings.Default.toggle_lower_stats) return true;
                MainWindow.Logger.Info("Repositioning player health and ammo.");
                var file = _hudPath + Resources.file_hudlayout;
                var lines = File.ReadAllLines(file);
                var start = FindIndex(lines, "HudWeaponAmmo");
                var value = Settings.Default.toggle_alt_stats ? "r110" : "c90";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "r50" : "c93";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudMedicCharge");
                value = Settings.Default.toggle_alt_stats ? "c60" : "c38";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "CHealthAccountPanel");
                value = Settings.Default.toggle_alt_stats ? "113" : "c-180";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "r90" : "267";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "DisguiseStatus");
                value = Settings.Default.toggle_alt_stats ? "115" : "100";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "r62" : "r38";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "CMainTargetID");
                value = Settings.Default.toggle_alt_stats ? "r200" : "265";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "HudAchievementTracker");
                value = Settings.Default.toggle_alt_stats ? "135" : "335";
                lines[FindIndex(lines, "NormalY", start)] = $"\t\t\"NormalY\"\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "9999" : "335";
                lines[FindIndex(lines, "EngineerY", start)] = $"\t\t\"EngineerY\"\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "9999" : "95";
                lines[FindIndex(lines, "tall", start)] = $"\t\t\"tall\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudPlayerHealth");
                value = Settings.Default.toggle_alt_stats ? "137" : "c-120";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "r47" : "c70";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Resources.file_huddamageaccount;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "\"DamageAccountValue\"");
                value = Settings.Default.toggle_alt_stats ? "137" : "c-120";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "r47" : "c70";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Resources.file_playerhealth;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudPlayerHealth");
                value = Settings.Default.toggle_alt_stats ? "10" : "c-190";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "r75" : "c68";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Resources.file_playerclass;
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "classmodelpanel");
                value = Settings.Default.toggle_alt_stats ? "r230" : "r200";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "180" : "200";
                lines[FindIndex(lines, "tall", start)] = $"\t\t\"tall\"\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(_hudPath + Resources.file_itemeffectmeter, string.Empty);
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudItemEffectMeter");
                value = Settings.Default.toggle_alt_stats ? "r110" : "c-60";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "r65" : "c120";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "ItemEffectMeterLabel");
                value = Settings.Default.toggle_alt_stats ? "100" : "120";
                lines[FindIndex(lines, "wide", start)] = $"\t\t\"wide\"\t\t\t\t\"{value}\"";
                start = FindIndex(lines, "\"ItemEffectMeter\"");
                value = Settings.Default.toggle_alt_stats ? "100" : "120";
                lines[FindIndex(lines, "wide", start)] = $"\t\t\"wide\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(_hudPath + Resources.file_itemeffectmeter, "_cleaver");
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudItemEffectMeter");
                value = Settings.Default.toggle_alt_stats ? "r85" : "c110";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(_hudPath + Resources.file_itemeffectmeter, "_sodapopper");
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudItemEffectMeter");
                value = Settings.Default.toggle_alt_stats ? "r75" : "c100";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = string.Format(_hudPath + Resources.file_itemeffectmeter, "_killstreak");
                lines = File.ReadAllLines(file);
                start = FindIndex(lines, "HudItemEffectMeter");
                value = Settings.Default.toggle_alt_stats ? "115" : "2";
                lines[FindIndex(lines, "xpos", start)] = $"\t\t\"xpos\"\t\t\t\t\"{value}\"";
                value = Settings.Default.toggle_alt_stats ? "r33" : "r28";
                lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
                File.WriteAllLines(file, lines);

                file = _hudPath + Resources.file_hudanimations;
                File.WriteAllText(file,
                    Settings.Default.toggle_alt_stats
                        ? File.ReadAllText(file).Replace("Blank", "HudBlack")
                        : File.ReadAllText(file).Replace("HudBlack", "Blank"));
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage("Error repositioning player health and ammo.",
                    Resources.error_set_lower_player_stats,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Retrieves the index of where a given value was found in a string array.
        /// </summary>
        public static int FindIndex(string[] array, string value, int skip = 0)
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
        public static string CommentOutTextLine(string value)
        {
            return string.Concat("//", value.Replace("//", string.Empty));
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

        private static void SetItemEffectPosition(string file, Positions position = Positions.Bottom,
            string search = "HudItemEffectMeter")
        {
            // positions 1 = top, 2 = middle, 3 = bottom
            var lines = File.ReadAllLines(file);
            var start = FindIndex(lines, search);
            var value = position switch
            {
                Positions.Top => Settings.Default.toggle_lower_stats ? "r70" : "c100",
                Positions.Middle => Settings.Default.toggle_lower_stats ? "r60" : "c110",
                Positions.Bottom => Settings.Default.toggle_lower_stats ? "r50" : "c120",
                _ => Settings.Default.toggle_lower_stats ? "r80" : "c92"
            };
            lines[FindIndex(lines, "ypos", start)] = $"\t\t\"ypos\"\t\t\t\t\"{value}\"";
            File.WriteAllLines(file, lines);
        }

        private enum Positions
        {
            Top,
            Middle,
            Bottom,
            Default
        }
    }
}