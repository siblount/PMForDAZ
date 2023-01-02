// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace DAZ_Installer.DP
{
    public enum SettingOptions
    {
        No, Yes, Prompt
    }

    /// <summary>
    /// Options for user to handle how to handle extraction (ie: manifest only, etc)
    /// </summary>
    public enum InstallOptions
    {
        ManifestOnly, ManifestAndAuto, Automatic
    }

    public class DPSettings
    {
        // Note: The JSONSerializer will not use the setter for HashSet, therefore the 
        [JsonIgnore]
        public static DPSettings currentSettingsObject;
        public string destinationPath { get; set; } // todo : Ask for daz content directory if no detected daz content paths found.
        // TO DO: Use HashSet instead of list.
        public string[] detectedDazContentPaths;
        public SettingOptions downloadImages { get; set; } = SettingOptions.Prompt;
        public string thumbnailsPath { get; set; } = "Thumbnails";
        public InstallOptions handleInstallation { get; set; } = InstallOptions.ManifestAndAuto;
        [JsonIgnore]
        public static HashSet<string> inititalCommonContentFolderNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "aniBlocks", "Animals", "Architecture", "Camera Presets", "data", "DAZ Studio Tutorials", "Documentation", "Documents", "Environments", "General", "Light Presets", "Lights", "People", "Presets", "Props", "Render Presets", "Render Settings", "Runtime", "Scene Builder", "Scene Subsets", "Scenes", "Scripts", "Shader Presets", "Shaders", "Support", "Templates", "Textures", "Vehicles" };
        public HashSet<string> commonContentFolderNames { get; set; } = new HashSet<string>(inititalCommonContentFolderNames, StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> folderRedirects { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "docs", "Documentation" }, { "Documents", "Documentation" } };
        public string tempPath { get; set; } = Path.Combine(Path.GetTempPath(), "DazProductInstaller"); //
        public uint maxTagsToShow { get; set; } = 8; // Keep low because GDI+ slow.
        public SettingOptions permDeleteSource { get; set; } = SettingOptions.Prompt;
        public SettingOptions installPrevProducts { get; set; } = SettingOptions.Prompt;
        public SettingOptions OverwriteFiles { get; set; } = SettingOptions.Yes;
        public RecycleOption DeleteAction { get; set; } = RecycleOption.DeletePermanently;
        public static string databasePath { get; set; } = "Database";
        [JsonIgnore]
        public static bool initalized { get; set; } = false;
        [JsonIgnore]
        public static bool invalidSettings = false;
        private const string SETTINGS_PATH = "settings.json";

        static DPSettings()
        {
            currentSettingsObject = new DPSettings();
            currentSettingsObject.Initalize();
        }

        public DPSettings() { }

        public static DPSettings GetCopy()
        {
            var settings = (DPSettings)currentSettingsObject.MemberwiseClone();
            settings.commonContentFolderNames = new HashSet<string>(currentSettingsObject.commonContentFolderNames, StringComparer.OrdinalIgnoreCase);
            settings.folderRedirects = new Dictionary<string, string>(currentSettingsObject.folderRedirects, StringComparer.OrdinalIgnoreCase);
            settings.detectedDazContentPaths = (string[]) currentSettingsObject.detectedDazContentPaths.Clone();
            return settings;
        }
        
        // TODO: Handle situation where new settings were added; ex, OverWriteFiles
        public void Initalize()
        {
            if (initalized) return;
            var exists = File.Exists(SETTINGS_PATH);
            if (!exists) goto FINISH;

            var settingsObj = ParseSettings(File.ReadAllText(SETTINGS_PATH));
            if (settingsObj == null)
                MessageBox.Show("There was an error processing settings. Settings have been reset to default values.", 
                    "Failed to process settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                currentSettingsObject = settingsObj;

            FINISH:
            ValidateDirectoryPaths();
            detectedDazContentPaths = DPRegistry.ContentDirectories;
            DPProcessor.ClearTemp();
            initalized = true;
        }

        public DPSettings? ParseSettings(string str)
        {
            try
            {
                return JsonConvert.DeserializeObject<DPSettings>(str);
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to parse settings. REASON: {ex}");
            }
            return null;
        }

        private void ValidateDirectoryPaths()
        {
            bool destExists = !string.IsNullOrEmpty(destinationPath) && Directory.Exists(destinationPath);
            bool thumbExists = !string.IsNullOrEmpty(thumbnailsPath) && Directory.Exists(thumbnailsPath);
            bool tempExists = !string.IsNullOrEmpty(tempPath) && (Directory.Exists(tempPath) || Path.Combine(Path.GetTempPath(), "DazProductInstaller") == tempPath);
            bool databaseExists = !string.IsNullOrEmpty(databasePath) && Directory.Exists(databasePath);
            bool anyNotEmpty = !string.IsNullOrEmpty(databasePath) ||
                                !string.IsNullOrEmpty(tempPath) ||
                                !string.IsNullOrEmpty(thumbnailsPath) ||
                                !string.IsNullOrEmpty(destinationPath);
            invalidSettings = anyNotEmpty && (!destExists || !thumbExists || !tempExists || !databaseExists);
            if (!destExists)
            {
                if (DPRegistry.ContentDirectories.Length == 0)
                {
                    MessageBox.Show("Couldn't find DAZ directories located in registry. On the next prompt, please select where you want your products to be installed to. You can always change this later in the settings.",
                        "No Daz content directories found in registry", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    var path = Settings.settingsPage.AskForDirectory();
                    while (path == string.Empty)
                    {
                        MessageBox.Show("No directory was selected. It is required that you select a directory for products you wish to install. Please select where you want your products to be installed to. You can always change this later in the settings.", "Folder selection required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        path = Settings.settingsPage.AskForDirectory();
                    }
                }
                else
                {
                    destinationPath = DPRegistry.ContentDirectories[0];
                }
            }
            if (!thumbExists)
            {
                thumbnailsPath = "Thumbnails";
                try
                {
                    Directory.CreateDirectory(thumbnailsPath);
                }
                catch (Exception ex)
                {
                    DPCommon.WriteToLog($"Failed to create directories for default thumbnail path. REASON: {ex}");
                }
            }
            if (!tempExists)
            {
                tempPath = Path.Combine(Path.GetTempPath(), "DazProductInstaller");
                try
                {
                    Directory.CreateDirectory(tempPath);
                }
                catch (Exception ex)
                {
                    DPCommon.WriteToLog($"Failed to create directories for default temp path. REASON: {ex}");
                }
            }
            if (!databaseExists)
            {
                databasePath = "Database";
                try
                {
                    Directory.CreateDirectory(tempPath);
                }
                catch (Exception ex)
                {
                    DPCommon.WriteToLog($"Failed to create directories for default database path. REASON: {ex}");
                }
            }
            if (invalidSettings) MessageBox.Show("Some paths are invalid and have been reverted to default.", "Settings defaulted",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool SaveSettings()
        {
            try
            {
                var s = JsonConvert.SerializeObject(currentSettingsObject, Formatting.Indented);
                using var file = File.CreateText(SETTINGS_PATH);
                file.Write(s);
                return true;
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to parse settings. REASON: {ex}");
            }
            return false;
        }

    }
}
