// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer
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
            Task setupSettingsPage = new Task(LoadSettings);
            setupSettingsPage.Start();
            setupSettingsPage.ContinueWith((Task a) => a.Dispose());
        }

        // ._.
        private void LoadSettings()
        {
            DPCommon.WriteToLog("Loading settings...");
            // Get our settings.
            DPSettings.Initalize();
            loadingPanel.Visible = true;
            loadingPanel.BringToFront();
            validating = true;
            var result1 = BeginInvoke(new Action(SetupDownloadThumbnailsSetting));
            var result2 = BeginInvoke(new Action(SetupDestinationPathSetting));
            var result3 = BeginInvoke(new Action(SetupFileHandling));
            var result4 = BeginInvoke(new Action(SetupTempPath));
            var result5 = BeginInvoke(new Action(SetupContentFolders));
            var result6 = BeginInvoke(new Action(SetupContentRedirects));
            var result7 = BeginInvoke(new Action(SetupDeleteSourceFiles));
            var result8 = BeginInvoke(new Action(SetupPreviouslyInstalledProducts));

            result1.AsyncWaitHandle.WaitOne();
            result2.AsyncWaitHandle.WaitOne();
            result3.AsyncWaitHandle.WaitOne();
            result4.AsyncWaitHandle.WaitOne();
            result5.AsyncWaitHandle.WaitOne();
            result6.AsyncWaitHandle.WaitOne();
            result7.AsyncWaitHandle.WaitOne();
            result8.AsyncWaitHandle.WaitOne();

            result1.AsyncWaitHandle.Dispose();
            result2.AsyncWaitHandle.Dispose();
            result3.AsyncWaitHandle.Dispose();
            result4.AsyncWaitHandle.Dispose();
            result5.AsyncWaitHandle.Dispose();
            result6.AsyncWaitHandle.Dispose();
            result7.AsyncWaitHandle.Dispose();
            result8.AsyncWaitHandle.Dispose();

            loadingPanel.Visible = false;
            loadingPanel.Dispose();
            validating = false;
        }

        private void SetupContentRedirects()
        {
            foreach (var keypair in DPSettings.folderRedirects)
            {
                contentFolderRedirectsListBox.Items.Add($"{keypair.Key} --> {keypair.Value}");
            }
        }

        private void SetupContentFolders()
        {
            foreach (var folder in DPSettings.commonContentFolderNames)
            {
                contentFoldersListBox.Items.Add(folder);
            }
        }

        private void SetupTempPath()
        {
            tempTxtBox.Text = DPSettings.tempPath;
        }

        private void SetupDestinationPathSetting()
        {
            // If no detected daz content paths, all handled in the initalization phase of DPSettings.
            // First, we will add our selected path.
            destinationPathCombo.Items.Add(DPSettings.destinationPath);
            destinationPathCombo.SelectedIndex = 0;
            foreach (var path in DPSettings.detectedDazContentPaths)
            {
                destinationPathCombo.Items.Add(path);
            }
        }

        private void SetupFileHandling()
        {
            
            fileHandlingCombo.Items.AddRange(names);

            // Now show the one we selected.
            var fileMethod = DPSettings.handleInstallation;
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

            var choice = DPSettings.downloadImages;
            downloadThumbnailsComboBox.SelectedItem = Enum.GetName(choice);
        }

        private void SetupDeleteSourceFiles()
        {
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                removeSourceFilesComboBox.Items.Add(option);
            }

            var choice = DPSettings.permDeleteSource;
            removeSourceFilesComboBox.SelectedItem = Enum.GetName(choice);
        }

        private void SetupPreviouslyInstalledProducts()
        {
            foreach (var option in Enum.GetNames(typeof(SettingOptions)))
            {
                installPrevProducts.Items.Add(option);
            }

            var choice = DPSettings.installPrevProducts;
            installPrevProducts.SelectedItem = Enum.GetName(choice);
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
            var saveResult = DPSettings.SaveSettings(out string errorMsg);

            // If something failed...
            if (!saveResult)
            {
                errorMsg += "\nSee log for more info.";
                MessageBox.Show(errorMsg, "Error saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } else
            {
                applySettingsBtn.Enabled = false;
            }
        }

        private bool UpdateSettings()
        {
            // We don't update content folders.
            if (validating) return false;
            var invalidReponses = false;
            DPSettings.downloadImages = Enum.Parse<SettingOptions>((string) downloadThumbnailsComboBox.SelectedItem);
            validating = true;
            // Destination Path
            DESTCHECK:
            if (Directory.Exists(destinationPathCombo.Text.Trim())) DPSettings.destinationPath = destinationPathCombo.Text.Trim();
            else {
                try
                {
                    Directory.CreateDirectory(tempTxtBox.Text.Trim());
                    goto DESTCHECK;
                } catch { };
                destinationPathCombo.Text = DPSettings.destinationPath;
                invalidReponses = true;
            }

            // Temp Path
            TEMPCHECK:
            if (Directory.Exists(tempTxtBox.Text.Trim())) DPSettings.tempPath = tempTxtBox.Text.Trim();
            else
            {
                try
                {
                    Directory.CreateDirectory(tempTxtBox.Text.Trim());
                    goto TEMPCHECK;
                } catch {}
                tempTxtBox.Text = DPSettings.tempPath;
                invalidReponses = true;
            }

            // File Handling Method
            DPSettings.handleInstallation = (InstallOptions)fileHandlingCombo.SelectedIndex;

            if (invalidReponses)
            {
                MessageBox.Show("Some inputs were invalid and were reset to their previous state. See log for more info.", "Invalid inputs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                validating = false;
                return false;
            }
            validating = false;
            return true;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void modifyContentRedirectsBtn_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tempTxtBox_Leave(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && tempTxtBox.Text != DPSettings.tempPath)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void tempTxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!applySettingsBtn.Enabled && tempTxtBox.Text != DPSettings.tempPath)
            {
                 applySettingsBtn.Enabled = true;
            }
        }

        private void destinationPathCombo_Leave(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && destinationPathCombo.Text != DPSettings.destinationPath)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void destinationPathCombo_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && destinationPathCombo.Text != DPSettings.destinationPath)
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void downloadThumbnailsComboBox_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && downloadThumbnailsComboBox.Text != Enum.GetName(DPSettings.downloadImages))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void fileHandlingCombo_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && fileHandlingCombo.Text != names[(int) DPSettings.handleInstallation])
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void removeSourceFiles_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && removeSourceFilesComboBox.Text != Enum.GetName(DPSettings.permDeleteSource))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        private void installPrevProducts_TextChanged(object sender, EventArgs e)
        {
            if (!applySettingsBtn.Enabled && installPrevProducts.Text != Enum.GetName(DPSettings.installPrevProducts))
            {
                applySettingsBtn.Enabled = true;
            }
        }

        internal string AskForDirectory()
        {
            if (InvokeRequired)
            {
                string result = (string) Invoke(new Func<string>(AskForDirectory));
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
    }
}
