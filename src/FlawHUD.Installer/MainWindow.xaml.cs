using AutoUpdaterDotNET;
using log4net;
using log4net.Config;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace FlawHUD.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string appPath = System.Windows.Forms.Application.StartupPath;
        public static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            logger.Info("INITIALIZING...");
            InitializeComponent();
            SetupDirectory();
            LoadHUDSettings();
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start(Properties.Resources.app_update);
        }

        private void DownloadHUD()
        {
            logger.Info("Downloading the latest FlawHUD...");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var client = new WebClient();
            client.DownloadFile(chkStreamerMode.IsChecked == true ? Properties.Resources.app_download : Properties.Resources.app_download_streamer, "flawhud.zip");
            client.Dispose();
            logger.Info("Downloading the latest FlawHUD...Done!");
            ExtractHUD();
            CleanDirectory();
        }

        /// <summary>
        /// Calls to extract FlawHUD to the tf/custom directory
        /// </summary>
        /// <remarks>TODO: Refactor the update-refresh-install process</remarks>
        private void ExtractHUD(bool update = false)
        {
            var settings = Properties.Settings.Default;
            logger.Info("Extracting downloaded FlawHUD to " + settings.hud_directory);
            ZipFile.ExtractToDirectory(appPath + "\\flawhud.zip", settings.hud_directory);
            if (update)
                Directory.Delete(settings.hud_directory + "\\flawhud", true);
            if (Directory.Exists(settings.hud_directory + "\\flawhud"))
                Directory.Delete(settings.hud_directory + "\\flawhud", true);
            if (Directory.Exists(settings.hud_directory + "\\flawhud-master"))
                Directory.Move(settings.hud_directory + "\\flawhud-master", settings.hud_directory + "\\flawhud");
            else if (Directory.Exists(settings.hud_directory + "\\flawhud-stream"))
                Directory.Move(settings.hud_directory + "\\flawhud-stream", settings.hud_directory + "\\flawhud");
            logger.Info("Extracting downloaded FlawHUD...Done!");
        }

        /// <summary>
        /// Set the tf/custom directory if not already set
        /// </summary>
        /// <remarks>TODO: Possible bug, consider refactoring</remarks>
        private void SetupDirectory(bool userSet = false)
        {
            if ((!SearchRegistry() && !CheckUserPath()) || userSet)
            {
                logger.Info("Setting the tf/custom directory. Opening folder browser, asking the user.");
                using (var browser = new FolderBrowserDialog { Description = Properties.Resources.info_folder_browser, ShowNewFolderButton = true })
                {
                    while (!browser.SelectedPath.Contains("tf\\custom"))
                    {
                        if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK && browser.SelectedPath.Contains("tf\\custom"))
                        {
                            var settings = Properties.Settings.Default;
                            settings.hud_directory = browser.SelectedPath;
                            lblStatus.Content = settings.hud_directory;
                            settings.Save();
                            logger.Info("Directory has been set to " + lblStatus.Content);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (!CheckUserPath())
                {
                    logger.Error("Unable to set the tf/custom directory. Exiting.");
                    System.Windows.Forms.MessageBox.Show(Properties.Resources.error_app_directory, "Directory Not Set", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Windows.Application.Current.Shutdown();
                }
            }
            CleanDirectory();
            SetFormControls();
        }

        /// <summary>
        /// Cleans up the tf/custom and installer directories
        /// </summary>
        private void CleanDirectory()
        {
            logger.Info("Cleaning-up FlawHUD directories...");
            var settings = Properties.Settings.Default;

            // Clean the application directory
            if (File.Exists(appPath + "\\flawhud.zip"))
                File.Delete(appPath + "\\flawhud.zip");
            if (File.Exists(appPath + "\\CastingEssentials.zip"))
                File.Delete(appPath + "\\CastingEssentials.zip");

            // Clean the tf/custom directory
            var hudDirectory = Directory.Exists(settings.hud_directory + "\\flawhud-master") ? settings.hud_directory + "\\flawhud-master" : string.Empty;
            hudDirectory = Directory.Exists(settings.hud_directory + "\\flawhud-stream") ? settings.hud_directory + "\\flawhud-stream" : hudDirectory;

            if (!string.IsNullOrEmpty(hudDirectory))
            {
                // Remove the previous backup if found.
                if (File.Exists(settings.hud_directory + "\\flawhud-backup.zip"))
                    File.Delete(settings.hud_directory + "\\flawhud-backup.zip");

                logger.Info("Found a FlawHUD installation. Creating a back-up.");
                ZipFile.CreateFromDirectory(hudDirectory, settings.hud_directory + "\\flawhud-backup.zip");
                Directory.Delete(hudDirectory, true);
                System.Windows.Forms.MessageBox.Show(Properties.Resources.info_create_backup, "Backup Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            logger.Info("Cleaning-up FlawHUD directories...Done!");
        }

        private bool SearchRegistry()
        {
            logger.Info("Looking for the Team Fortress 2 directory...");
            var is64Bit = (Environment.Is64BitProcess) ? "Wow6432Node\\" : string.Empty;
            var directory = (string)Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam", "InstallPath", null);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                directory += "\\steamapps\\common\\Team Fortress 2\\tf\\custom";
                if (Directory.Exists(directory))
                {
                    var settings = Properties.Settings.Default;
                    settings.hud_directory = directory;
                    settings.Save();
                    logger.Info("Directory found at " + settings.hud_directory);
                    return true;
                }
            }
            logger.Info("Directory not found. Continuing...");
            return false;
        }

        /// <summary>
        /// Display the error message box
        /// </summary>
        public static void ShowErrorMessage(string title, string message, string exception)
        {
            System.Windows.Forms.MessageBox.Show($"{message} {exception}", "Error: " + title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            logger.Error(exception);
        }

        /// <summary>
        /// Check the FlawHUD version number
        /// </summary>
        public void CheckHUDVersion()
        {
            try
            {
                logger.Info("Checking FlawHUD version...");
                var client = new WebClient();
                var readme_text = client.DownloadString(Properties.Resources.app_readme).Split('\n');
                client.Dispose();
                var current = readme_text[readme_text.Length - 2];
                var local = File.ReadLines(Properties.Settings.Default.hud_directory + "\\flawhud\\README.md").Last().Trim();
                if (!string.Equals(local, current))
                {
                    logger.Info("Version Mismatch. New FlawHUD update available!");
                    Install.Content = "Update";
                    lblNews.Content = "Update Available!";
                }
                logger.Info("Local version: " + local + "\t Live version: " + current);
                logger.Info("Checking FlawHUD version...Done!");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// Check if FlawHUD is installed in the tf/custom directory
        /// </summary>
        public bool CheckHUDPath()
        {
            return Directory.Exists(Properties.Settings.Default.hud_directory + "\\flawhud");
        }

        /// <summary>
        /// Check if user's directory setting is valid
        /// </summary>
        public bool CheckUserPath()
        {
            return !string.IsNullOrWhiteSpace(Properties.Settings.Default.hud_directory) && Properties.Settings.Default.hud_directory.Contains("tf\\custom");
        }

        /// <summary>
        /// Update the installer controls like labels and buttons
        /// </summary>
        private void SetFormControls()
        {
            if (Directory.Exists(Properties.Settings.Default.hud_directory) && CheckUserPath())
            {
                var isInstalled = CheckHUDPath();
                if (isInstalled) CheckHUDVersion();
                Start.IsEnabled = true;
                Install.IsEnabled = true;
                Install.Content = isInstalled ? "Refresh" : "Install";
                Save.IsEnabled = isInstalled ? true : false;
                Uninstall.IsEnabled = isInstalled ? true : false;
                lblStatus.Content = $"FlawHUD is {(!isInstalled ? "not " : "")}installed...";
                Properties.Settings.Default.Save();
            }
            else
            {
                Start.IsEnabled = false;
                Install.IsEnabled = false;
                Save.IsEnabled = false;
                Uninstall.IsEnabled = false;
                lblStatus.Content = "Directory is not set...";
            }
        }

        #region CLICK_EVENTS

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info("Installing FlawHUD...");
                DownloadHUD();
                SaveHUDSettings();
                ApplyHUDSettings();
                SetFormControls();
                lblNews.Content = "Installation finished at " + DateTime.Now;
                logger.Info("Installing FlawHUD...Done!");
                System.Windows.Forms.MessageBox.Show("FlawHUD has been successfully installed", "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Installing FlawHUD.", Properties.Resources.error_app_install, ex.Message);
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info("Uninstalling FlawHUD...");
                if (!CheckHUDPath()) return;
                Directory.Delete(Properties.Settings.Default.hud_directory + "\\flawhud", true);
                lblNews.Content = "Uninstalled FlawHUD at " + DateTime.Now;
                SetupDirectory();
                logger.Info("Uninstalling FlawHUD...Done!");
                System.Windows.Forms.MessageBox.Show("FlawHUD has been successfully uninstalled", "Uninstall Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Uninstalling FlawHUD.", Properties.Resources.error_app_uninstall, ex.Message);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveHUDSettings();
            ApplyHUDSettings();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetHUDSettings();
        }

        private void ChangeDirectory_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Opening Directory Browser...");
            SetupDirectory(true);
        }

        private void ReportIssue_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Opening Issue Tracker...");
            Process.Start("https://github.com/CriticalFlaw/FlawHUD.Installer/issues");
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Launching Team Fortress 2...");
            Process.Start("steam://rungameid/440");
        }

        #endregion CLICK_EVENTS

        #region SAVE_LOAD

        /// <summary>
        /// Save user settings to the file
        /// </summary>
        private void SaveHUDSettings()
        {
            try
            {
                logger.Info("Saving HUD Settings...");
                var settings = Properties.Settings.Default;
                settings.color_health_normal = cpHealthNormal.SelectedColor.Value.ToString();
                settings.color_health_low = cpHealthLow.SelectedColor.Value.ToString();
                settings.color_ammo_clip = cpAmmoClip.SelectedColor.Value.ToString();
                settings.color_ammo_clip_low = cpAmmoClipLow.SelectedColor.Value.ToString();
                settings.color_uber_bar = cpUberBarColor.SelectedColor.Value.ToString();
                settings.color_uber_full = cpUberFullColor.SelectedColor.Value.ToString();
                settings.color_xhair_normal = cpXHairColor.SelectedColor.Value.ToString();
                settings.color_xhair_pulse = cpXHairPulse.SelectedColor.Value.ToString();
                settings.val_xhair_size = cbXHairSize.SelectedIndex;
                settings.val_xhair_x = tbXHairXPos.Value ?? 25;
                settings.val_xhair_y = tbXHairYPos.Value ?? 24;
                settings.toggle_xhair_enable = chkXHairEnable.IsChecked ?? false;
                settings.toggle_xhair_pulse = chkXHairPulse.IsChecked ?? false;
                settings.toggle_disguise_image = chkDisguiseImage.IsChecked ?? false;
                settings.toggle_stock_backgrounds = chkDefaultBG.IsChecked ?? false;
                settings.toggle_menu_images = chkMenuImages.IsChecked ?? false;
                settings.toggle_transparent_viewmodels = chkTransparentVM.IsChecked ?? false;
                settings.toggle_code_pro_fonts = chkCodeProFonts.IsChecked ?? false;
                settings.toggle_streamer_mode = chkStreamerMode.IsChecked ?? false;
                settings.toggle_casting_essentials = chkCastingEssentials.IsChecked ?? false;
                settings.Save();
                logger.Info("Saving HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Saving HUD Settings.", Properties.Resources.error_app_save, ex.Message);
            }
        }

        /// <summary>
        /// Load GUI with user settings from the file
        /// </summary>
        private void LoadHUDSettings()
        {
            try
            {
                logger.Info("Loading HUD Settings...");
                var settings = Properties.Settings.Default;
                var cc = new ColorConverter();
                cpHealthNormal.SelectedColor = (Color)cc.ConvertFrom(settings.color_health_normal);
                cpHealthLow.SelectedColor = (Color)cc.ConvertFrom(settings.color_health_low);
                cpAmmoClip.SelectedColor = (Color)cc.ConvertFrom(settings.color_ammo_clip);
                cpAmmoClipLow.SelectedColor = (Color)cc.ConvertFrom(settings.color_ammo_clip_low);
                cpUberBarColor.SelectedColor = (Color)cc.ConvertFrom(settings.color_uber_bar);
                cpUberFullColor.SelectedColor = (Color)cc.ConvertFrom(settings.color_uber_full);
                cpXHairColor.SelectedColor = (Color)cc.ConvertFrom(settings.color_xhair_normal);
                cpXHairPulse.SelectedColor = (Color)cc.ConvertFrom(settings.color_xhair_pulse);
                cbXHairSize.SelectedIndex = settings.val_xhair_size;
                tbXHairXPos.Value = settings.val_xhair_x;
                tbXHairYPos.Value = settings.val_xhair_y;
                chkXHairEnable.IsChecked = settings.toggle_xhair_enable;
                chkXHairPulse.IsChecked = settings.toggle_xhair_pulse;
                chkDisguiseImage.IsChecked = settings.toggle_disguise_image;
                chkDefaultBG.IsChecked = settings.toggle_stock_backgrounds;
                chkMenuImages.IsChecked = settings.toggle_menu_images;
                chkTransparentVM.IsChecked = settings.toggle_transparent_viewmodels;
                chkCodeProFonts.IsChecked = settings.toggle_code_pro_fonts;
                chkStreamerMode.IsChecked = settings.toggle_streamer_mode;
                chkCastingEssentials.IsChecked = settings.toggle_casting_essentials;
                logger.Info("Loading HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Loading HUD Settings.", Properties.Resources.error_app_load, ex.Message);
            }
        }

        /// <summary>
        /// Reset user settings to their default values
        /// </summary>
        private void ResetHUDSettings()
        {
            try
            {
                logger.Info("Resetting HUD Settings...");
                var cc = new ColorConverter();
                cpHealthNormal.SelectedColor = (Color)cc.ConvertFrom("#00AA7F");
                cpHealthLow.SelectedColor = (Color)cc.ConvertFrom("#BE1414");
                cpAmmoClip.SelectedColor = (Color)cc.ConvertFrom("#00AA7F");
                cpAmmoClipLow.SelectedColor = (Color)cc.ConvertFrom("#BE1414");
                cpUberBarColor.SelectedColor = (Color)cc.ConvertFrom("#00AA7F");
                cpUberFullColor.SelectedColor = (Color)cc.ConvertFrom("#00787F");
                cpXHairColor.SelectedColor = (Color)cc.ConvertFrom("#F2F2F2");
                cpXHairPulse.SelectedColor = (Color)cc.ConvertFrom("#FF0000");
                cbXHairSize.SelectedIndex = 16;
                tbXHairXPos.Value = 25;
                tbXHairYPos.Value = 24;
                chkXHairEnable.IsChecked = false;
                chkXHairPulse.IsChecked = true;
                chkDisguiseImage.IsChecked = false;
                chkDefaultBG.IsChecked = false;
                chkMenuImages.IsChecked = true;
                chkTransparentVM.IsChecked = false;
                chkCodeProFonts.IsChecked = false;
                chkStreamerMode.IsChecked = false;
                chkCastingEssentials.IsChecked = false;
                lblNews.Content = "Settings Reset at " + DateTime.Now;
                logger.Info("Resetting HUD Settings...Done!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Resetting HUD Settings.", Properties.Resources.error_app_reset, ex.Message);
            }
        }

        /// <summary>
        /// Apply user settings to FlawHUD files
        /// </summary>
        private void ApplyHUDSettings()
        {
            logger.Info("Applying HUD Settings...");
            if (chkStreamerMode.IsChecked == true)
                DownloadHUD();
            var writer = new HUDController();
            writer.MainMenuBackground();
            writer.DisguiseImage();
            writer.CrosshairPulse();
            writer.MainMenuClassImage();
            writer.Crosshair();
            writer.Colors();
            writer.TransparentViewmodels();
            writer.CodeProFonts();
            writer.CastingEssentials();
            lblNews.Content = "Settings Saved at " + DateTime.Now;
            logger.Info("Resetting HUD Settings...Done!");
        }

        #endregion SAVE_LOAD
    }
}