// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAZ_Installer.WinApp.Forms;
using Microsoft.VisualBasic.FileIO;

namespace DAZ_Installer.WinApp.Pages
{
    public partial class Settings : UserControl
    {
        internal static bool setupComplete { get; set; } = false;
        internal static readonly string[] names = new string[] { "Manifest Only", "Manifest and File Sense", "File Sense Only" };
        internal static bool validating { get; set; } = false;
        internal static Settings settingsPage { get; set; } = null;
        public Settings()
        {
            InitializeComponent();
            settingsPage = this;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            Task.Run(LoadSettings);
            loadingPanel.Visible = true;
            loadingPanel.BringToFront();
        }

        // ._.
        private void LoadSettings()
        {
            DPCommon.WriteToLog("Loading settings...");
            // Get our settings.
            DPSettings.currentSettingsObject.Initalize();
            validating = true;
            
            
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

            applySettingsBtn.Enabled = DPSettings.invalidSettings;
        }

        private void SetupContentRedirects()
        {
            foreach (var keypair in DPSettings.currentSettingsObject.folderRedirects)
            {
                contentFolderRedirectsListBox.Items.Add($"{keypair.Key} --> {keypair.Value}");
            }
        }

        private void SetupContentFolders()
        {
            foreach (var folder in DPSettings.currentSettingsObject.commonContentFolderNames)
            {
                contentFoldersListBox.Items.Add(folder);
            }
        }

        private void SetupTempPath()
        {
            tempTxtBox.Text = DPSettings.currentSettingsObject.tempPath;
        }

        private void SetupDestinationPathSetting()
        {
            // If no detected daz content paths, all handled in the initalization phase of DPSettings.currentSettingsObject.
            // First, we will add our selected path.
            destinationPathCombo.Items.Add(DPSettings.currentSettingsObject.destinationPath);
            destinationPathCombo.SelectedIndex = 0;
            foreach (var path in DPSettings.currentSettingsObject.detectedDazContentPaths)
            {
                destinationPathCombo.Items.Add(path);
            }
        }

        private void SetupFileHandling()
        {
            
            fileHandlingCombo.Items.AddRange(names);

            // Now show the one we selected.
            var fileMethod = DPSettings.currentSettingsObject.handleInstallation;
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
        }

        private void SetupDownloadThumbnailsSetting()
        {
            
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                downloadThumbnailsComboBox.Items.Add(option);
            }

            var choice = DPSettings.currentSettingsObject.downloadImages;
            downloadThumbnailsComboBox.SelectedItem = Enum.GetName(choice);
        }

        private void SetupDeleteSourceFiles()
        {
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                removeSourceFilesCombo.Items.Add(option);
            }

            var choice = DPSettings.currentSettingsObject.permDeleteSource;
            removeSourceFilesCombo.SelectedItem = Enum.GetName(choice);
        }

        private void SetupPreviouslyInstalledProducts()
        {
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                installPrevProductsCombo.Items.Add(option);
            }

            var choice = DPSettings.currentSettingsObject.installPrevProducts;
            installPrevProductsCombo.SelectedItem = Enum.GetName(choice);
        }

        private void SetupAllowOverwriting()
        {
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                allowOverwritingCombo.Items.Add(option);
            }
            allowOverwritingCombo.SelectedItem = Enum.GetName(DPSettings.currentSettingsObject.OverwriteFiles);
        }

        private void SetupRemoveAction()
        {
            removeActionCombo.Items.AddRange(new string[]{ "Delete permanently", "Move to Recycle Bin"});
            switch (DPSettings.currentSettingsObject.DeleteAction)
            {
                case RecycleOption.DeletePermanently:
                    removeActionCombo.SelectedItem = "Delete permanently";
                    return;
                default:
                    removeActionCombo.SelectedItem = "Move to Recycle Bin";
                    return;
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void downloadThumbnailsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void applySettingsBtn_Click(object sender, EventArgs e)
        {
            // Update settings.
            var updateResult = UpdateSettings();

            if (!updateResult) return;

            // Try saving settings.
            var saveResult = DPSettings.currentSettingsObject.SaveSettings();

            // If something failed...
            if (!saveResult)
                MessageBox.Show("An error occurred while saving settings. You're settings have NOT been saved. Please try saving again.", 
                    "Error saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);

            applySettingsBtn.Enabled = false;
        }

        private bool UpdateSettings()
        {
            // We don't update content folders.
            if (validating) return false;
            var invalidReponses = false;
            DPSettings.currentSettingsObject.downloadImages = Enum.Parse<SettingOptions>((string) downloadThumbnailsComboBox.SelectedItem);
            validating = true;
            // Destination Path
            // A loop occurred when the path was G:/ but G:/ was not mounted.
            DESTCHECK:
            if (Directory.Exists(destinationPathCombo.Text.Trim())) DPSettings.currentSettingsObject.destinationPath = destinationPathCombo.Text.Trim();
            else {
                try
                {
                    Directory.CreateDirectory(tempTxtBox.Text.Trim());
                    goto DESTCHECK;
                } catch { };
                destinationPathCombo.Text = DPSettings.currentSettingsObject.destinationPath;
                invalidReponses = true;
            }

            // Temp Path
            // TODO: We don't want to delete the temp folder itself.
            // For example: We don't want to delete D:/temp, we want to delete D:/temp/.
            // The difference is that currently D:/temp will be deleted whereas, 
            // D:/temp/ will not delete the temp folder but all subfolders and files in it.
            TEMPCHECK:
            if (Directory.Exists(tempTxtBox.Text.Trim())) DPSettings.currentSettingsObject.tempPath = tempTxtBox.Text.Trim();
            else
            {
                try
                {
                    Directory.CreateDirectory(tempTxtBox.Text.Trim());
                    goto TEMPCHECK;
                } catch {}
                tempTxtBox.Text = DPSettings.currentSettingsObject.tempPath;
                invalidReponses = true;
            }

            // File Handling Method
            DPSettings.currentSettingsObject.handleInstallation = (InstallOptions)fileHandlingCombo.SelectedIndex;

            //Content Folders
            var contentFolders = new HashSet<string>(contentFoldersListBox.Items.Count);
            for (var i = 0; i < contentFoldersListBox.Items.Count; i++)
            {
                contentFolders.Add((string)contentFoldersListBox.Items[i]);
            }
            DPSettings.currentSettingsObject.commonContentFolderNames = contentFolders;

            // Alias Content Folders
            var aliasMap = new Dictionary<string, string>(contentFolderRedirectsListBox.Items.Count);
            foreach (string item in contentFolderRedirectsListBox.Items)
            {
                var tokens = item.Split(" --> ");
                aliasMap[tokens[0]] = tokens[1];
            }
            DPSettings.currentSettingsObject.folderRedirects = aliasMap;

            // Permanate Delete Source
            DPSettings.currentSettingsObject.permDeleteSource = (SettingOptions)removeSourceFilesCombo.SelectedIndex;

            // Install Prev Products
            DPSettings.currentSettingsObject.installPrevProducts = (SettingOptions)installPrevProductsCombo.SelectedIndex;

            if (invalidReponses)
            {
                MessageBox.Show("Some inputs were invalid and were reset to their previous state. See log for more info.", "Invalid inputs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                validating = false;
                return false;
            }
            validating = false;
            return true;
        }

        private void tempTxtBox_Leave(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && tempTxtBox.Text != DPSettings.currentSettingsObject.tempPath)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void tempTxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!applySettingsBtn.Enabled && tempTxtBox.Text != DPSettings.currentSettingsObject.tempPath)
            {
                 applySettingsBtn.Enabled = true;
            }
        }

        private void destinationPathCombo_Leave(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && destinationPathCombo.Text != DPSettings.currentSettingsObject.destinationPath)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void destinationPathCombo_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && destinationPathCombo.Text != DPSettings.currentSettingsObject.destinationPath)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void downloadThumbnailsComboBox_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && downloadThumbnailsComboBox.Text != Enum.GetName(DPSettings.currentSettingsObject.downloadImages))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void fileHandlingCombo_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && fileHandlingCombo.Text != names[(int) DPSettings.currentSettingsObject.handleInstallation])
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void removeSourceFiles_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && removeSourceFilesCombo.Text != Enum.GetName(DPSettings.currentSettingsObject.permDeleteSource))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void installPrevProducts_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && installPrevProductsCombo.Text != Enum.GetName(DPSettings.currentSettingsObject.installPrevProducts))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        internal string AskForDirectory()
        {
            if (InvokeRequired)
            {
                string result = Invoke(new Func<string>(AskForDirectory));
                return result;
            } else
            {
                using (var folderBrowser = new FolderBrowserDialog())
                {
                    folderBrowser.Description = "Select folder for product installs";
                    folderBrowser.UseDescriptionForTitle = true;
                    var dialogResult = folderBrowser.ShowDialog();
                    if (dialogResult == DialogResult.Cancel) return string.Empty;
                    return folderBrowser.SelectedPath;
                }
            }
        }

        private void chooseDestPathBtn_Click(object sender, EventArgs e)
        {
            using var browser = new FolderBrowserDialog();
            browser.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            browser.Description = "Select folder to install products into";
            browser.UseDescriptionForTitle = true;
            var result = browser.ShowDialog();
            if (result == DialogResult.OK)
            {
                destinationPathCombo.Items[0] = browser.SelectedPath;
                destinationPathCombo.SelectedIndex = 0;
                destinationPathCombo_TextChanged(null, null);
            }
        }

        private void chooseTempPathBtn_Click(object sender, EventArgs e)
        {
            using var browser = new FolderBrowserDialog();
            browser.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            browser.Description = "Select folder to temporarily extract products into";
            browser.UseDescriptionForTitle = true;
            var result = browser.ShowDialog();
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

        private void allowOverwritingCombo_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && allowOverwritingCombo.Text != Enum.GetName(DPSettings.currentSettingsObject.OverwriteFiles))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void openDatabaseBtn_Click(object _, EventArgs __) => new DatabaseView().ShowDialog();
    }
}
