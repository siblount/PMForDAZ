// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.Windows.Forms;
using DAZ_Installer.Windows.DP;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using DAZ_Installer.IO;

namespace DAZ_Installer.Windows.Pages
{
    public partial class Settings : UserControl
    {
        public ILogger Logger { get; set; } = Log.Logger.ForContext<Settings>();
        internal static bool setupComplete { get; set; } = false;
        internal static readonly string[] names = new string[] { "Manifest Only", "Manifest and File Sense", "File Sense Only" };
        internal static bool validating { get; set; } = false;
        internal static Settings settingsPage { get; set; } = null;

        private const string SETTINGS_PATH = "settings.json";

        public Settings()
        {
            Logger.Debug("Creating Settings object");
            InitializeComponent();
            settingsPage = this;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            Logger.Debug("Settings_Load called");
            Task.Run(LoadSettings);
            loadingPanel.Visible = true;
            loadingPanel.BringToFront();
        }

        public void SwitchToSettings(bool settingsInvalid = false)
        {
            Logger.Debug("Switching to settings page");
            applySettingsBtn.Enabled = settingsInvalid;
            MainForm.SwitchPage(settingsPage);
        }

        // ._.
        private void LoadSettings()
        {
            Logger.Information("Loading Settings");
            // Get our settings.
            validating = true;
            applySettingsBtn.Enabled = false;
            if (!DPSettings.CurrentSettingsObject.Valid)
            {
                DPSettings.CurrentSettingsObject = SetupSettings();
                applySettingsBtn.Enabled = true;
            }

            ValidateDirectoryPaths(DPSettings.CurrentSettingsObject);

            Logger.Information("Setting up Settings' controls");
            SetupDownloadThumbnailsSetting();
            SetupDestinationPathSetting();
            SetupFileHandling();
            SetupTempPath();
            SetupContentFolders();
            SetupContentRedirects();
            SetupDeleteSourceFiles();
            SetupPreviouslyInstalledProducts();
            SetupAllowOverwriting();
            SetupRemoveAction();

            loadingPanel.Visible = false;
            loadingPanel.Dispose();
            validating = false;
            Logger.Information("Settings loaded");
        }
        public bool SaveSettings()
        {
            try
            {
                using StreamWriter file = File.CreateText(SETTINGS_PATH);
                file.Write(DPSettings.CurrentSettingsObject.ToJson());
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save settings");
            }
            return false;
        }

        public DPSettings SetupSettings()
        {
            var settings = new DPSettings();
            if (DPRegistry.Instance.ContentDirectories.Length == 0)
            {
                MessageBox.Show("Couldn't find DAZ Studio Content Directories located in registry. On the next prompt, please select where you want your products to be installed to. You can always change this later in the settings.",
                    "Content Directories not found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                var path = AskForDirectory();
                while (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show("No directory was selected. It is required that you select a directory for products you wish to install. " +
                        "Please select where you want your products to be installed to. You can always change this later in the settings.",
                        "Directory selection required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    path = AskForDirectory();
                }
                settings.DestinationPath = path;
            }
            else settings.DestinationPath = DPRegistry.Instance.ContentDirectories[0];
            return settings;
        }

        private void ValidateDirectoryPaths(DPSettings settings)
        {
            var fs = new DPFileSystem(DPFileScopeSettings.All);
            fs.CreateDirectoryInfo(settings.DestinationPath).TryCreate();
            fs.CreateDirectoryInfo(settings.ThumbnailsDir).TryCreate();
            fs.CreateDirectoryInfo(settings.TempDir).TryCreate();
            fs.CreateDirectoryInfo(settings.DatabaseDir).TryCreate();
            var destExists = !string.IsNullOrEmpty(settings.DestinationPath) && Directory.Exists(settings.DestinationPath);
            var thumbExists = !string.IsNullOrEmpty(settings.ThumbnailsDir) && Directory.Exists(settings.ThumbnailsDir);
            var tempExists = !string.IsNullOrEmpty(settings.TempDir) && (Directory.Exists(settings.TempDir));
            var databaseExists = !string.IsNullOrEmpty(settings.DatabaseDir) && Directory.Exists(settings.DatabaseDir);
            var anyEmpty = !string.IsNullOrEmpty(settings.DatabaseDir) ||
                                !string.IsNullOrEmpty(settings.TempDir) ||
                                !string.IsNullOrEmpty(settings.ThumbnailsDir) ||
                                !string.IsNullOrEmpty(settings.DestinationPath);

            var invalidSettings = anyEmpty && (!destExists || !thumbExists || !tempExists || !databaseExists);
            if (invalidSettings) MessageBox.Show("Some paths are invalid and have been reverted to default.", "Settings defaulted",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            applySettingsBtn.Enabled = invalidSettings;
            if (!destExists)
            {
                if (DPRegistry.Instance.ContentDirectories.Length == 0 && !Directory.Exists(settings.DestinationPath))
                {
                    MessageBox.Show("Couldn't find DAZ Studio Content Directories located in registry. On the next prompt, please select where you want your products to be installed to. You can always change this later in the settings.",
                        "Content Directories not found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    var path = AskForDirectory();
                    while (string.IsNullOrEmpty(path))
                    {
                        MessageBox.Show("No directory was selected. It is required that you select a directory for products you wish to install. " +
                            "Please select where you want your products to be installed to. You can always change this later in the settings.",
                            "Directory selection required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        path = AskForDirectory();
                    }
                    settings.DestinationPath = path;
                }
                else settings.DestinationPath = DPRegistry.Instance.ContentDirectories[0];
            }
            if (!thumbExists && !fs.CreateDirectoryInfo("Thumbnails").TryCreate())
                Logger.Warning("Failed to create default Thumbnails directory.");
            if (!tempExists && !fs.CreateDirectoryInfo(Path.Combine(Path.GetTempPath(), "DazProductInstaller")).TryCreate())
                Logger.Warning("Failed to create default temp directory.");
            if (!databaseExists && !fs.CreateDirectoryInfo("Database").TryCreate())
                Logger.Warning("Failed to create default database directory.");
        }

        private void SetupContentRedirects()
        {
            contentFolderRedirectsListBox.BeginUpdate();
            foreach (KeyValuePair<string, string> keypair in DPSettings.CurrentSettingsObject.FolderRedirects)
            {
                contentFolderRedirectsListBox.Items.Add($"{keypair.Key} --> {keypair.Value}");
            }
            contentFolderRedirectsListBox.EndUpdate();
        }

        private void SetupContentFolders()
        {
            contentFoldersListBox.BeginUpdate();
            foreach (var folder in DPSettings.CurrentSettingsObject.CommonContentFolderNames)
            {
                contentFoldersListBox.Items.Add(folder);
            }
            contentFoldersListBox.EndUpdate();
        }

        private void SetupTempPath() => tempTxtBox.Text = DPSettings.CurrentSettingsObject.TempDir;

        private void SetupDestinationPathSetting()
        {
            // If no detected daz content paths, all handled in the initalization phase of DPSettings.currentSettingsObject.
            // First, we will add our selected path.
            destinationPathCombo.BeginUpdate();
            destinationPathCombo.Items.Add(DPSettings.CurrentSettingsObject.DestinationPath);
            destinationPathCombo.SelectedIndex = 0;
            destinationPathCombo.Items.AddRange(DPSettings.CurrentSettingsObject.detectedDazContentPaths);
            destinationPathCombo.EndUpdate();
        }

        private void SetupFileHandling()
        {
            fileHandlingCombo.BeginUpdate();
            fileHandlingCombo.Items.AddRange(names);

            // Now show the one we selected.
            InstallOptions fileMethod = DPSettings.CurrentSettingsObject.HandleInstallation;
            switch (fileMethod)
            {
                case InstallOptions.ManifestOnly:
                    fileHandlingCombo.SelectedIndex = 0;
                    break;
                case InstallOptions.ManifestAndAuto:
                    fileHandlingCombo.SelectedIndex = 1;
                    break;
                case InstallOptions.Automatic:
                    fileHandlingCombo.SelectedIndex = 2;
                    break;
            }
            fileHandlingCombo.EndUpdate();
        }

        private void SetupDownloadThumbnailsSetting()
        {
            downloadThumbnailsComboBox.BeginUpdate();
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                downloadThumbnailsComboBox.Items.Add(option);
            }

            SettingOptions choice = DPSettings.CurrentSettingsObject.DownloadImages;
            downloadThumbnailsComboBox.SelectedItem = Enum.GetName(choice);
            downloadThumbnailsComboBox.EndUpdate();
        }

        private void SetupDeleteSourceFiles()
        {
            removeSourceFilesCombo.BeginUpdate();
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                removeSourceFilesCombo.Items.Add(option);
            }

            SettingOptions choice = DPSettings.CurrentSettingsObject.PermDeleteSource;
            removeSourceFilesCombo.SelectedItem = Enum.GetName(choice);
            removeSourceFilesCombo.EndUpdate();
        }

        private void SetupPreviouslyInstalledProducts()
        {
            installPrevProductsCombo.BeginUpdate();
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                installPrevProductsCombo.Items.Add(option);
            }

            SettingOptions choice = DPSettings.CurrentSettingsObject.InstallPrevProducts;
            installPrevProductsCombo.SelectedItem = Enum.GetName(choice);
            installPrevProductsCombo.EndUpdate();
        }

        private void SetupAllowOverwriting()
        {
            allowOverwritingCombo.BeginUpdate();
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                allowOverwritingCombo.Items.Add(option);
            }
            allowOverwritingCombo.SelectedItem = Enum.GetName(DPSettings.CurrentSettingsObject.OverwriteFiles);
            allowOverwritingCombo.EndUpdate();
        }

        private void SetupRemoveAction()
        {
            removeActionCombo.BeginUpdate();
            removeActionCombo.Items.AddRange(["Delete permanently", "Move to Recycle Bin"]);
            removeActionCombo.SelectedItem = DPSettings.CurrentSettingsObject.DeleteAction switch
            {
                RecycleOption.DeletePermanently => "Delete permanently",
                _ => "Move to Recycle Bin",
            };
            removeActionCombo.EndUpdate();
        }

        private void applySettingsBtn_Click(object sender, EventArgs e)
        {
            // Update settings.
            var updateResult = UpdateSettings();

            if (!updateResult) return;

            // Try saving settings.
            var saveResult = SaveSettings();

            // If something failed...
            if (!saveResult)
                MessageBox.Show("An error occurred while saving settings. You're settings have NOT been saved. Please try saving again.",
                    "Error saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);

            applySettingsBtn.Enabled = !saveResult;
        }

        private bool UpdateSettings()
        {
            // Only get drives that are mounted and ready to use.
            // Do we want to limit this to only local drives (eg. not network drives)?
            DriveInfo[] drives = Array.FindAll(DriveInfo.GetDrives(), d => d.IsReady);
            // We don't update content folders.
            if (validating) return false;
            var invalidReponses = false;
            DPSettings.CurrentSettingsObject.DownloadImages = Enum.Parse<SettingOptions>((string)downloadThumbnailsComboBox.SelectedItem!);
            validating = true;
        // Destination Path

        DESTCHECK:
            // May need to use PathHelper.NormalizePath() here.
            var destinationPath = destinationPathCombo.Text.Trim();
            // this solves issue: A loop occurred when the path was G:/ but G:/ was not mounted.
            if (Directory.Exists(destinationPath)) DPSettings.CurrentSettingsObject.DestinationPath = destinationPath;
            else
            {
                if (Array.Find(drives, d => d.Name == destinationPath) == null)
                {
                    // Means the drive is not mounted/ready, we need to select at least one
                    if (DPRegistry.Instance.ContentDirectories.Length == 0)
                    {
                        MessageBox.Show("The destination path currently selected is not valid because it is not mounted (or ready to be used). Please select a valid destination path in the following prompt.",
                                                       "Invalid destination path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DPSettings.CurrentSettingsObject.DestinationPath = destinationPathCombo.Text = AskForDirectory();
                    }
                    else DPSettings.CurrentSettingsObject.DestinationPath = destinationPathCombo.Text = DPRegistry.Instance.ContentDirectories[0];
                }
                try
                {
                    Directory.CreateDirectory(destinationPath);
                    goto DESTCHECK;
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (!HandleDirectoryUnauthorizedException(destinationPath))
                    {
                        MessageBox.Show("Application does not have permission to access the destination path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // TODO: Log error.
                    }
                }
                catch { }
                destinationPathCombo.Text = DPSettings.CurrentSettingsObject.DestinationPath;
                invalidReponses = true;
            }

            // Temp Path
            // TODO: We don't want to delete the temp folder itself.
            // For example: We don't want to delete D:/temp, we want to delete D:/temp/.
            // The difference is that currently D:/temp will be deleted whereas, 
            // D:/temp/ will not delete the temp folder but all subfolders and files in it.
            var tempPath = tempTxtBox.Text.Trim();
        TEMPCHECK:
            if (Directory.Exists(tempPath)) DPSettings.CurrentSettingsObject.TempDir = tempPath;
            else
            {
                try
                {
                    Directory.CreateDirectory(tempPath);
                    goto TEMPCHECK;
                }
                catch (IOException ex)
                {
                    if (!HandleDirectoryUnauthorizedException(destinationPath))
                    {
                        // Try resetting to default.
                        try
                        {
                            tempPath = Path.Combine(Path.GetTempPath(), "DazProductInstaller");
                            MessageBox.Show("The temp path currently selected is not valid and has been reset to the default temp path.",
                                                                                  "Temp path reset", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch
                        {
                            MessageBox.Show("The temp path currently selected is not valid. Additionally, the application does not have permission to your system's default temp path.",
                                                      "Temp Access Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        MessageBox.Show("The temp path currently selected is not valid because it is not mounted (or ready to be used).",
                                               "Invalid temp path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // TODO: Log error.
                    }
                }
                catch { }
                tempTxtBox.Text = DPSettings.CurrentSettingsObject.TempDir;
                invalidReponses = true;
            }

            // File Handling Method
            DPSettings.CurrentSettingsObject.HandleInstallation = (InstallOptions)fileHandlingCombo.SelectedIndex;

            //Content Folders
            // We want to maintain the comparer.
            DPSettings.CurrentSettingsObject.CommonContentFolderNames.Clear();
            DPSettings.CurrentSettingsObject.CommonContentFolderNames.EnsureCapacity(contentFoldersListBox.Items.Count);
            for (var i = 0; i < contentFoldersListBox.Items.Count; i++)
            {
                DPSettings.CurrentSettingsObject.CommonContentFolderNames.Add((string)contentFoldersListBox.Items[i]);
            }

            // Alias Content Folders
            // We want to maintain the comparer.
            DPSettings.CurrentSettingsObject.FolderRedirects.Clear();
            DPSettings.CurrentSettingsObject.FolderRedirects.EnsureCapacity(contentFolderRedirectsListBox.Items.Count);
            foreach (string item in contentFolderRedirectsListBox.Items)
            {
                var tokens = item.Split(" --> ");
                DPSettings.CurrentSettingsObject.FolderRedirects[tokens[0]] = tokens[1];
            }

            // Permanate Delete Source
            DPSettings.CurrentSettingsObject.PermDeleteSource = (SettingOptions)removeSourceFilesCombo.SelectedIndex;

            // Install Prev Products
            DPSettings.CurrentSettingsObject.InstallPrevProducts = (SettingOptions)installPrevProductsCombo.SelectedIndex;

            if (invalidReponses)
            {
                MessageBox.Show("Some inputs were invalid and were reset to their previous state. See log for more info.", "Invalid inputs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                validating = false;
                return false;
            }
            validating = false;
            return true;
        }

        internal string AskForDirectory()
        {
            if (InvokeRequired)
            {
                var result = Invoke(new Func<string>(AskForDirectory));
                return result;
            }
            else
            {
                using var folderBrowser = new FolderBrowserDialog();
                folderBrowser.Description = "Select folder for product installs";
                folderBrowser.UseDescriptionForTitle = true;
                DialogResult dialogResult = folderBrowser.ShowDialog();
                if (dialogResult == DialogResult.Cancel) return string.Empty;
                return folderBrowser.SelectedPath;
            }
        }

        /// <summary>
        /// Attempts to fix the unauthorized access exception and later attempts to create the directory (if it doesn't exist).
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <returns>Whehther the directory is now accessiable.</returns>
        internal static bool HandleDirectoryUnauthorizedException(string path)
        {
            // Determine whether the path is a folder or a file path.
            DirectoryInfo info = new(path);
            try
            {
                // TODO: Log file attributes.
                if (info.Attributes.HasFlag(FileAttributes.ReadOnly)) info.Attributes &= ~FileAttributes.ReadOnly;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Unauthorized is not due to the readonly flag but because we literally don't have permission.
                // Can't do anything.
            }
            catch (Exception e)
            {
                // Ok...something else happened.
            }
            return Directory.Exists(path);
        }
        #region UI Event Handlers
        private void tempTxtBox_Leave(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && tempTxtBox.Text != DPSettings.CurrentSettingsObject.TempDir)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void tempTxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!applySettingsBtn.Enabled && tempTxtBox.Text != DPSettings.CurrentSettingsObject.TempDir)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void destinationPathCombo_Leave(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && destinationPathCombo.Text != DPSettings.CurrentSettingsObject.DestinationPath)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void destinationPathCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && destinationPathCombo.Text != DPSettings.CurrentSettingsObject.DestinationPath)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void downloadThumbnailsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && downloadThumbnailsComboBox.Text != Enum.GetName(DPSettings.CurrentSettingsObject.DownloadImages))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void fileHandlingCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && fileHandlingCombo.Text != names[(int)DPSettings.CurrentSettingsObject.HandleInstallation])
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void removeSourceFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && removeSourceFilesCombo.Text != Enum.GetName(DPSettings.CurrentSettingsObject.PermDeleteSource))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void installPrevProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && installPrevProductsCombo.Text != Enum.GetName(DPSettings.CurrentSettingsObject.InstallPrevProducts))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void chooseDestPathBtn_Click(object sender, EventArgs e)
        {
            using var browser = new FolderBrowserDialog();
            browser.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            browser.Description = "Select folder to install products into";
            browser.UseDescriptionForTitle = true;
            DialogResult result = browser.ShowDialog();
            if (result == DialogResult.OK)
            {
                destinationPathCombo.Items[0] = browser.SelectedPath;
                destinationPathCombo.SelectedIndex = 0;
                destinationPathCombo_SelectedIndexChanged(null, null);
            }
        }

        private void chooseTempPathBtn_Click(object sender, EventArgs e)
        {
            using var browser = new FolderBrowserDialog();
            browser.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            browser.Description = "Select folder to temporarily extract products into";
            browser.UseDescriptionForTitle = true;
            DialogResult result = browser.ShowDialog();
            if (result == DialogResult.OK)
            {
                tempTxtBox.Text = browser.SelectedPath;
                tempTxtBox_Leave(null, null);
            }
        }

        private void modifyContentFoldersBtn_Click(object sender, EventArgs e)
        {
            var contentManager = new ContentFolderManager();
            contentManager.ShowDialog();
            contentFoldersListBox.Items.Clear();
            foreach (var item in contentManager.ContentFolders)
            {
                contentFoldersListBox.Items.Add(item);
            }
            applySettingsBtn.Enabled = true;
        }

        private void modifyContentRedirectsBtn_Click_1(object sender, EventArgs e)
        {
            var contentManager = new ContentFolderAliasManager();
            contentManager.ShowDialog();
            if (contentManager.AliasListView is null) return;
            contentFolderRedirectsListBox.BeginUpdate();
            contentFolderRedirectsListBox.Items.Clear();
            for (var i = 0; i < contentManager.AliasListView.Items.Count; i++)
            {
                contentFolderRedirectsListBox.Items.Add(contentManager.AliasListView.Items[i].Text);
            }
            contentFolderRedirectsListBox.EndUpdate();
            applySettingsBtn.Enabled = true;
        }

        private void openDatabaseBtn_Click(object _, EventArgs __) => new DatabaseToolsForm().ShowDialog();
        #endregion

        private void removeActionCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && removeActionCombo.Text != Enum.GetName(DPSettings.CurrentSettingsObject.DeleteAction))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void allowOverwritingCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && allowOverwritingCombo.Text != Enum.GetName(DPSettings.CurrentSettingsObject.OverwriteFiles))
            {
                applySettingsBtn.Enabled = true;
            }
        }
    }
}
