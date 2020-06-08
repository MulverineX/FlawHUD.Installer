using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using AutoUpdaterDotNET;
using FlawHUD.Installer.Properties;
using log4net;
using log4net.Config;
using Microsoft.Win32;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace FlawHUD.Installer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _appPath = Application.StartupPath;

        public MainWindow()
        {
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            Logger.Info("INITIALIZING...");
            InitializeComponent();
            SetupDirectory();
            LoadHUDSettings();
            SetCrosshairControls();
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start(Properties.Resources.app_update);
        }

        /// <summary>
        ///     Calls to download the latest version of FlawHUD
        /// </summary>
        private void DownloadHUD()
        {
            Logger.Info("Downloading the latest FlawHUD...");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var client = new WebClient();
            client.DownloadFile(
                CbStreamerMode.IsChecked == true
                    ? Properties.Resources.app_download_streamer
                    : Properties.Resources.app_download, "flawhud.zip");
            client.Dispose();
            Logger.Info("Downloading the latest FlawHUD...Done!");
            ExtractHUD();
            CleanDirectory();
        }

        /// <summary>
        ///     Calls to extract FlawHUD to the tf/custom directory
        /// </summary>
        /// <remarks>TODO: Refactor the update-refresh-install process</remarks>
        private void ExtractHUD(bool update = false)
        {
            var settings = Settings.Default;
            Logger.Info("Extracting downloaded FlawHUD to " + settings.hud_directory);
            ZipFile.ExtractToDirectory(_appPath + "\\flawhud.zip", settings.hud_directory);
            if (update)
                Directory.Delete(settings.hud_directory + "\\flawhud", true);
            if (Directory.Exists(settings.hud_directory + "\\flawhud"))
                Directory.Delete(settings.hud_directory + "\\flawhud", true);
            if (Directory.Exists(settings.hud_directory + "\\flawhud-master"))
                Directory.Move(settings.hud_directory + "\\flawhud-master", settings.hud_directory + "\\flawhud");
            else if (Directory.Exists(settings.hud_directory + "\\flawhud-stream"))
                Directory.Move(settings.hud_directory + "\\flawhud-stream", settings.hud_directory + "\\flawhud");
            Logger.Info("Extracting downloaded FlawHUD...Done!");
        }

        /// <summary>
        ///     Set the tf/custom directory if not already set
        /// </summary>
        /// <remarks>TODO: Possible bug, consider refactoring</remarks>
        private void SetupDirectory(bool userSet = false)
        {
            if (!SearchRegistry() && !CheckUserPath() || userSet)
            {
                Logger.Info("Setting the tf/custom directory. Opening folder browser, asking the user.");
                using (var browser = new FolderBrowserDialog
                    {Description = Properties.Resources.info_folder_browser, ShowNewFolderButton = true})
                {
                    while (!browser.SelectedPath.Contains("tf\\custom"))
                        if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                            browser.SelectedPath.Contains("tf\\custom"))
                        {
                            var settings = Settings.Default;
                            settings.hud_directory = browser.SelectedPath;
                            settings.Save();
                            LblStatus.Content = settings.hud_directory;
                            Logger.Info("Directory has been set to " + LblStatus.Content);
                        }
                        else
                        {
                            break;
                        }
                }

                if (!CheckUserPath())
                {
                    Logger.Error("Unable to set the tf/custom directory. Exiting.");
                    MessageBox.Show(Properties.Resources.error_app_directory,
                        Properties.Resources.error_app_directory_title, MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    System.Windows.Application.Current.Shutdown();
                }
            }

            CleanDirectory();
            SetFormControls();
        }

        /// <summary>
        ///     Cleans up the tf/custom and installer directories
        /// </summary>
        private void CleanDirectory()
        {
            Logger.Info("Cleaning-up FlawHUD directories...");

            // Clean the application directory
            if (File.Exists(_appPath + "\\flawhud.zip"))
                File.Delete(_appPath + "\\flawhud.zip");
            if (File.Exists(_appPath + "\\CastingEssentials.zip"))
                File.Delete(_appPath + "\\CastingEssentials.zip");

            // Clean the tf/custom directory
            var settings = Settings.Default;
            var hudDirectory = Directory.Exists(settings.hud_directory + "\\flawhud-master")
                ? settings.hud_directory + "\\flawhud-master"
                : string.Empty;
            hudDirectory = Directory.Exists(settings.hud_directory + "\\flawhud-stream")
                ? settings.hud_directory + "\\flawhud-stream"
                : hudDirectory;

            if (!string.IsNullOrEmpty(hudDirectory))
            {
                // Remove the previous backup if found.
                if (File.Exists(settings.hud_directory + "\\flawhud-backup.zip"))
                    File.Delete(settings.hud_directory + "\\flawhud-backup.zip");

                Logger.Info("Found a FlawHUD installation. Creating a back-up.");
                ZipFile.CreateFromDirectory(hudDirectory, settings.hud_directory + "\\flawhud-backup.zip");
                Directory.Delete(hudDirectory, true);
                MessageBox.Show(Properties.Resources.info_create_backup, Properties.Resources.info_create_backup_title,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Logger.Info("Cleaning-up FlawHUD directories...Done!");
        }

        private static bool SearchRegistry()
        {
            Logger.Info("Looking for the Team Fortress 2 directory...");
            var is64Bit = Environment.Is64BitProcess ? "Wow6432Node\\" : string.Empty;
            var directory = (string) Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam",
                "InstallPath", null);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                directory += "\\steamapps\\common\\Team Fortress 2\\tf\\custom";
                if (Directory.Exists(directory))
                {
                    var settings = Settings.Default;
                    settings.hud_directory = directory;
                    settings.Save();
                    Logger.Info("Directory found at " + settings.hud_directory);
                    return true;
                }
            }

            Logger.Info("Directory not found. Continuing...");
            return false;
        }

        /// <summary>
        ///     Display the error message box
        /// </summary>
        public static void ShowErrorMessage(string title, string message, string exception)
        {
            MessageBox.Show($@"{message} {exception}", string.Format(Properties.Resources.error_info, title),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Logger.Error(exception);
        }

        /// <summary>
        ///     Check the FlawHUD version number
        /// </summary>
        public void CheckHUDVersion()
        {
            try
            {
                Logger.Info("Checking FlawHUD version...");
                var client = new WebClient();
                var readmeText = client.DownloadString(Properties.Resources.app_readme).Split('\n');
                client.Dispose();
                var current = readmeText[readmeText.Length - 2];
                var local = File.ReadLines(Settings.Default.hud_directory + "\\flawhud\\README.md").Last().Trim();
                if (!string.Equals(local, current))
                {
                    Logger.Info("Version Mismatch. New FlawHUD update available!");
                    BtnInstall.Content = "Update";
                    LblNews.Content = "Update Available!";
                }

                Logger.Info("Local version: " + local + "\t Live version: " + current);
                Logger.Info("Checking FlawHUD version...Done!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        /// <summary>
        ///     Check if FlawHUD is installed in the tf/custom directory
        /// </summary>
        public bool CheckHUDPath()
        {
            return Directory.Exists(Settings.Default.hud_directory + "\\flawhud");
        }

        /// <summary>
        ///     Check if user's directory setting is valid
        /// </summary>
        public bool CheckUserPath()
        {
            return !string.IsNullOrWhiteSpace(Settings.Default.hud_directory) &&
                   Settings.Default.hud_directory.Contains("tf\\custom");
        }

        /// <summary>
        ///     Update the installer controls like labels and buttons
        /// </summary>
        private void SetFormControls()
        {
            if (Directory.Exists(Settings.Default.hud_directory) && CheckUserPath())
            {
                var isInstalled = CheckHUDPath();
                if (isInstalled) CheckHUDVersion();
                BtnStart.IsEnabled = true;
                BtnInstall.IsEnabled = true;
                BtnInstall.Content = isInstalled ? "Refresh" : "Install";
                BtnSave.IsEnabled = isInstalled;
                BtnUninstall.IsEnabled = isInstalled;
                LblStatus.Content = $"FlawHUD is {(!isInstalled ? "not " : "")}installed...";
                Settings.Default.Save();
            }
            else
            {
                BtnStart.IsEnabled = false;
                BtnInstall.IsEnabled = false;
                BtnSave.IsEnabled = false;
                BtnUninstall.IsEnabled = false;
                LblStatus.Content = "Directory is not set...";
            }
        }

        /// <summary>
        ///     Disables certain crosshair options if rotating crosshair is enabled
        /// </summary>
        private void SetCrosshairControls()
        {
            CbXHairHitmarker.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CbXHairRotate.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CpXHairColor.IsEnabled = CbXHairEnable.IsChecked ?? false;
            CpXHairPulse.IsEnabled = CbXHairEnable.IsChecked ?? false;

            IntXHairSize.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            CbXHairStyle.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            IntXHairXPos.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
            IntXHairYPos.IsEnabled = CbXHairEnable.IsChecked & !CbXHairRotate.IsChecked ?? false;
        }

        #region CLICK_EVENTS

        /// <summary>
        ///     Installs FlawHUD to the user's tf/custom folder
        /// </summary>
        private void BtnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Info("Installing FlawHUD...");
                var worker = new BackgroundWorker();
                worker.DoWork += (o, ea) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        DownloadHUD();
                        SaveHUDSettings();
                        ApplyHUDSettings();
                        SetFormControls();
                    });
                };
                worker.RunWorkerCompleted += (o, ea) =>
                {
                    BusyIndicator.IsBusy = false;
                    LblNews.Content = "Installation finished at " + DateTime.Now;
                    Logger.Info("Installing FlawHUD...Done!");
                    MessageBox.Show(Properties.Resources.info_install_complete_desc,
                        Properties.Resources.info_install_complete, MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                BusyIndicator.IsBusy = true;
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Installing FlawHUD.", Properties.Resources.error_app_install, ex.Message);
            }
        }

        /// <summary>
        ///     Removes FlawHUD from the user's tf/custom folder
        /// </summary>
        private void BtnUninstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Info("Uninstalling FlawHUD...");
                if (!CheckHUDPath()) return;
                Directory.Delete(Settings.Default.hud_directory + "\\flawhud", true);
                LblNews.Content = "Uninstalled FlawHUD at " + DateTime.Now;
                SetupDirectory();
                Logger.Info("Uninstalling FlawHUD...Done!");
                MessageBox.Show(Properties.Resources.info_uninstall_complete_desc,
                    Properties.Resources.info_uninstall_complete, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Uninstalling FlawHUD.", Properties.Resources.error_app_uninstall, ex.Message);
            }
        }

        /// <summary>
        ///     Saves then applies the FlawHUD settings
        /// </summary>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (o, ea) =>
            {
                Dispatcher.Invoke(() =>
                {
                    SaveHUDSettings();
                    ApplyHUDSettings();
                });
            };
            worker.RunWorkerCompleted += (o, ea) => { BusyIndicator.IsBusy = false; };
            BusyIndicator.IsBusy = true;
            worker.RunWorkerAsync();
        }

        /// <summary>
        ///     Resets the FlawHUD settings to the default
        /// </summary>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            ResetHUDSettings();
        }

        /// <summary>
        ///     Opens the directory browser to let the user to set their tf/custom directory
        /// </summary>
        private void BtnDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Opening Directory Browser...");
            SetupDirectory(true);
        }

        /// <summary>
        ///     Launches Team Fortress 2 through Steam
        /// </summary>
        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Launching Team Fortress 2...");
            Process.Start("steam://rungameid/440");
        }

        /// <summary>
        ///     Opens the GitHub issue tracker in a web browser
        /// </summary>
        private void BtnReportIssue_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Opening Issue Tracker...");
            Process.Start("https://github.com/CriticalFlaw/FlawHUD.Installer/issues");
        }

        /// <summary>
        ///     Disables certain crosshair options if rotating crosshair is enabled
        /// </summary>
        private void CbXHairEnable_OnClick(object sender, RoutedEventArgs e)
        {
            SetCrosshairControls();
        }

        #endregion CLICK_EVENTS

        #region SAVE_LOAD

        /// <summary>
        ///     Save user settings to the file
        /// </summary>
        private void SaveHUDSettings()
        {
            try
            {
                Logger.Info("Saving HUD Settings...");
                var settings = Settings.Default;
                settings.color_health_buff = CpHealthBuffed.SelectedColor?.ToString();
                settings.color_health_low = CpHealthLow.SelectedColor?.ToString();
                settings.color_ammo_low = CpAmmoLow.SelectedColor?.ToString();
                settings.color_uber_bar = CpUberBarColor.SelectedColor?.ToString();
                settings.color_uber_full = CpUberFullColor.SelectedColor?.ToString();
                settings.color_xhair_normal = CpXHairColor.SelectedColor?.ToString();
                settings.color_xhair_pulse = CpXHairPulse.SelectedColor?.ToString();
                settings.val_xhair_size = IntXHairSize.Value ?? 26;
                settings.val_xhair_style = CbXHairStyle.SelectedIndex;
                settings.val_xhair_x = IntXHairXPos.Value ?? 25;
                settings.val_xhair_y = IntXHairYPos.Value ?? 24;
                settings.toggle_xhair_enable = CbXHairEnable.IsChecked ?? false;
                settings.toggle_xhair_pulse = CbXHairHitmarker.IsChecked ?? false;
                settings.toggle_xhair_rotate = CbXHairRotate.IsChecked ?? false;
                settings.toggle_disguise_image = CbDisguiseImage.IsChecked ?? false;
                settings.toggle_stock_backgrounds = CbDefaultBg.IsChecked ?? false;
                settings.toggle_menu_images = CbMenuImages.IsChecked ?? false;
                settings.toggle_transparent_viewmodels = CbTransparentViewmodel.IsChecked ?? false;
                settings.toggle_code_pro_fonts = CbCodeProFonts.IsChecked ?? false;
                settings.toggle_streamer_mode = CbStreamerMode.IsChecked ?? false;
                settings.toggle_casting_essentials = CbCastingEssentials.IsChecked ?? false;
                settings.toggle_color_text = CbColorText.IsChecked ?? false;
                settings.Save();
                Logger.Info("Saving HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Saving HUD Settings.", Properties.Resources.error_app_save, ex.Message);
            }
        }

        /// <summary>
        ///     Load GUI with user settings from the file
        /// </summary>
        private void LoadHUDSettings()
        {
            try
            {
                Logger.Info("Loading HUD Settings...");
                var settings = Settings.Default;
                var cc = new ColorConverter();
                CpHealthBuffed.SelectedColor = (Color) cc.ConvertFrom(settings.color_health_buff);
                CpHealthLow.SelectedColor = (Color) cc.ConvertFrom(settings.color_health_low);
                CpAmmoLow.SelectedColor = (Color) cc.ConvertFrom(settings.color_ammo_low);
                CpUberBarColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_uber_bar);
                CpUberFullColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_uber_full);
                CpXHairColor.SelectedColor = (Color) cc.ConvertFrom(settings.color_xhair_normal);
                CpXHairPulse.SelectedColor = (Color) cc.ConvertFrom(settings.color_xhair_pulse);
                IntXHairSize.Value = settings.val_xhair_size;
                CbXHairStyle.SelectedIndex = settings.val_xhair_style;
                IntXHairXPos.Value = settings.val_xhair_x;
                IntXHairYPos.Value = settings.val_xhair_y;
                CbXHairEnable.IsChecked = settings.toggle_xhair_enable;
                CbXHairHitmarker.IsChecked = settings.toggle_xhair_pulse;
                CbXHairRotate.IsChecked = settings.toggle_xhair_rotate;
                CbDisguiseImage.IsChecked = settings.toggle_disguise_image;
                CbDefaultBg.IsChecked = settings.toggle_stock_backgrounds;
                CbMenuImages.IsChecked = settings.toggle_menu_images;
                CbTransparentViewmodel.IsChecked = settings.toggle_transparent_viewmodels;
                CbCodeProFonts.IsChecked = settings.toggle_code_pro_fonts;
                CbStreamerMode.IsChecked = settings.toggle_streamer_mode;
                CbCastingEssentials.IsChecked = settings.toggle_casting_essentials;
                CbColorText.IsChecked = settings.toggle_color_text;
                Logger.Info("Loading HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Loading HUD Settings.", Properties.Resources.error_app_load, ex.Message);
            }
        }

        /// <summary>
        ///     Reset user settings to their default values
        /// </summary>
        private void ResetHUDSettings()
        {
            try
            {
                Logger.Info("Resetting HUD Settings...");
                var cc = new ColorConverter();
                CpHealthBuffed.SelectedColor = (Color) cc.ConvertFrom("#00AA7F");
                CpHealthLow.SelectedColor = (Color) cc.ConvertFrom("#BE1414");
                CpAmmoLow.SelectedColor = (Color) cc.ConvertFrom("#BE1414");
                CpUberBarColor.SelectedColor = (Color) cc.ConvertFrom("#00AA7F");
                CpUberFullColor.SelectedColor = (Color) cc.ConvertFrom("#00AA7F");
                CpXHairColor.SelectedColor = (Color) cc.ConvertFrom("#F2F2F2");
                CpXHairPulse.SelectedColor = (Color) cc.ConvertFrom("#FF0000");
                IntXHairSize.Value = 16;
                CbXHairStyle.SelectedIndex = 48;
                IntXHairXPos.Value = 25;
                IntXHairYPos.Value = 24;
                CbXHairEnable.IsChecked = false;
                CbXHairHitmarker.IsChecked = true;
                CbXHairRotate.IsChecked = false;
                CbDisguiseImage.IsChecked = false;
                CbDefaultBg.IsChecked = false;
                CbMenuImages.IsChecked = true;
                CbTransparentViewmodel.IsChecked = false;
                CbCodeProFonts.IsChecked = false;
                CbStreamerMode.IsChecked = false;
                CbCastingEssentials.IsChecked = false;
                CbColorText.IsChecked = false;
                SetCrosshairControls();
                LblNews.Content = "Settings Reset at " + DateTime.Now;
                Logger.Info("Resetting HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Resetting HUD Settings.", Properties.Resources.error_app_reset, ex.Message);
            }
        }

        /// <summary>
        ///     Apply user settings to FlawHUD files
        /// </summary>
        private void ApplyHUDSettings()
        {
            Logger.Info("Applying HUD Settings...");
            if (CbStreamerMode.IsChecked == true)
                DownloadHUD();
            var writer = new HUDController();
            writer.MainMenuBackground();
            writer.DisguiseImage();
            if (CbXHairRotate.IsChecked == true)
                writer.CrosshairRotate();
            else
                writer.CrosshairPulse();
            writer.MainMenuClassImage();
            writer.Crosshair(CbXHairStyle.SelectedValue.ToString(), IntXHairSize.Value);
            writer.Colors();
            writer.ColorText();
            writer.TransparentViewmodels();
            writer.CodeProFonts();
            writer.CastingEssentials();
            LblNews.Content = "Settings Saved at " + DateTime.Now;
            Logger.Info("Resetting HUD Settings...Done!");
        }

        #endregion SAVE_LOAD
    }
}