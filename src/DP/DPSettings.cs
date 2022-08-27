﻿// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

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
        // TO DO : Create a settings object to save settings used for an extraction and used at DPExtractJob to be passed into DPProcessor.ProcessArchive().
        // If no settings found - regenerate.
        public static DPSettings currentSettingsObject;
        public string destinationPath { get; set; } // todo : Ask for daz content directory if no detected daz content paths found.
        // TO DO: Use HashSet instead of list.
        public string[] detectedDazContentPaths;
        public SettingOptions downloadImages { get; set; } = SettingOptions.Prompt;
        public string thumbnailsPath { get; set; } = "Thumbnails";
        public InstallOptions handleInstallation { get; set; } = InstallOptions.ManifestAndAuto;
        public static HashSet<string> inititalCommonContentFolderNames { get; } = new HashSet<string>() { "aniBlocks", "Animals", "Architecture", "Camera Presets", "data", "DAZ Studio Tutorials", "Documentation", "Documents", "Environments", "General", "Light Presets", "Lights", "People", "Presets", "Props", "Render Presets", "Render Settings", "Runtime", "Scene Builder", "Scene Subsets", "Scenes", "Scripts", "Shader Presets", "Shaders", "Support", "Templates", "Textures", "Vehicles" };
        public HashSet<string> commonContentFolderNames { get; set; }
        public Dictionary<string, string> folderRedirects { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "docs", "Documentation" }, { "Documents", "Documentation" } };
        public string tempPath { get; set; } = Path.Combine(Path.GetTempPath(), "DazProductInstaller"); //
        public uint maxTagsToShow { get; set; } = 8; // Keep low because GDI+ slow.
        public SettingOptions permDeleteSource { get; set; } = SettingOptions.Prompt;
        public SettingOptions installPrevProducts { get; set; } = SettingOptions.Prompt;
        public SettingOptions OverwriteFiles { get; set; } = SettingOptions.Yes;
        public static string databasePath { get; set; } = "Database";
        public static bool initalized { get; set; } = false;
        public static bool invalidSettings = false;


        // Constants
        const string cfnLocation = "Settings/cfn.txt"; // Content Folder Names Location
        const string frLocation = "Settings/fn.txt"; // Folder Redirects Location
        const string oLocation = "Settings/o.txt"; // Other settings Location

        static DPSettings()
        {
            currentSettingsObject = new DPSettings();
            currentSettingsObject.Initalize();
        }

        public DPSettings() { }

        public static DPSettings GetCopy()
        {
            var settings = (DPSettings) currentSettingsObject.MemberwiseClone();
            settings.commonContentFolderNames = new HashSet<string>(currentSettingsObject.commonContentFolderNames);
            settings.folderRedirects = new Dictionary<string, string>(currentSettingsObject.folderRedirects);
            settings.detectedDazContentPaths = (string[]) currentSettingsObject.detectedDazContentPaths.Clone();
            return settings;
        }
        
        // TODO: Handle situation where new settings were added; ex, OverWriteFiles
        public void Initalize()
        {
            if (initalized) return;
            // TODO: Catch situation where the parse fails.
            if (GetOtherSettings(out string[] settings))
            {
                destinationPath = settings[0];
                downloadImages = Enum.Parse<SettingOptions>(settings[1]);
                thumbnailsPath = settings[2];
                handleInstallation = Enum.Parse<InstallOptions>(settings[3]);
                permDeleteSource = Enum.Parse<SettingOptions>(settings[4]);
                tempPath = settings[5];
                installPrevProducts = Enum.Parse<SettingOptions>(settings[6]);
                databasePath = settings[7];
                // TODO: Catch situation where the parse fails.
                //OverwriteFiles = Enum.Parse<SettingOptions>(settings[8]);
                OverwriteFiles = SettingOptions.Yes;

                ValidateDirectoryPaths();
                if (invalidSettings) MessageBox.Show("Some paths are invalid and have been reverted to default.", "Settings defaulted", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            } else
            {
                // If the settings existed but we failed to parse, let the user know. Otherwise, assume they are a new user.
                if (File.Exists(oLocation))
                    MessageBox.Show("Settings file was found but was unable to parse settings. Settings have been reset to default values.", 
                        "Settings failed to parse", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ValidateDirectoryPaths();

            }

            commonContentFolderNames = GetContentFolderNames(out HashSet<string> folders) ? folders : inititalCommonContentFolderNames;

            if (GetFolderRedirects(out Dictionary<string, string> dict))
            {
                folderRedirects = dict;
            }

            initalized = true;
            //DPRegistry.Initalize();
            detectedDazContentPaths = DPRegistry.ContentDirectories;
            DPProcessor.ClearTemp();
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
        }

        public bool SaveSettings(out string errorMsg)
        {
            var stringbuilder = new StringBuilder(50 * 3);
            var contentWriteResult = WriteContentFolderNames();
            var contentRedirectsWriteResult = WriteFolderRedirects();
            var otherSettingsWriteResult = WriteOtherSettings();

            var allSuccessful = contentWriteResult && contentRedirectsWriteResult && otherSettingsWriteResult;

            if (!allSuccessful)
            {
                if (!contentWriteResult)
                {
                    stringbuilder.AppendLine("Unable to write to content folder to disk.");
                }
                if (!contentRedirectsWriteResult)
                {
                    stringbuilder.AppendLine("Unable to write to content folder redirects to disk.");
                }
                if (!otherSettingsWriteResult)
                {
                    stringbuilder.AppendLine("Unable to write other settings to disk.");
                }
                errorMsg = stringbuilder.ToString().Trim();
                return false;
            }
            errorMsg = null;
            return true;
        }

        public bool GetContentFolderNames(out HashSet<string>? map)
        {

            if (File.Exists(cfnLocation))
            {
                try
                {
                    var contents = File.ReadAllLines(cfnLocation);
                    map = new HashSet<string>(contents);
                    return true;
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Unable to get content folder names file. Reason: {e}");
                }
            }
            map = null;
            return false;
        }
        public bool WriteContentFolderNames()
        {
            try
            {
                Directory.CreateDirectory("Settings");
                if (commonContentFolderNames != null && commonContentFolderNames.Count > 0)
                {
                    File.WriteAllLines(cfnLocation, commonContentFolderNames);
                }
                else
                {
                    // Default.
                    File.WriteAllLines(cfnLocation, inititalCommonContentFolderNames);
                }
                return true;
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"Unable to write content folder names. REASON: {e}");
                return false;
            }
        }

        // Key: Redirectee (not in common foldr names), Value: A common folder name
        // Ex: { "Docs" : "Documentation" }
        public bool GetFolderRedirects(out Dictionary<string, string> dict)
        {
            if (File.Exists(frLocation))
            {
                try
                {
                    var lines = File.ReadAllLines(frLocation);
                    var workingDict = new Dictionary<string, string>(lines.Length);
                    foreach (var line in lines)
                    {
                        var keyValue = line.Split('=');
                        workingDict.Add(keyValue[0], keyValue[1].Trim());
                    }
                    dict = workingDict;
                    return true;
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Unable to get folder redirects. REASON: {e}");
                    dict = null;
                    return false;
                }
            }
            else
            {
                dict = null;
                return false;
            }
        }


        public bool WriteFolderRedirects()
        {

            List<string> dictLines = new List<string>(folderRedirects.Count);
            foreach (var key in folderRedirects.Keys)
            {
                dictLines.Add($"{key}={folderRedirects[key]}");
            }
            try
            {
                Directory.CreateDirectory("Settings");
                File.WriteAllLines(frLocation, dictLines);
                return true;
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"Unable to write folder redirects file. REASON: {e}");
                return false;
            }
        }
        public bool GetOtherSettings(out string[] settings)
        {
            if (File.Exists(oLocation))
            {
                try
                {
                    var lines = File.ReadAllLines(oLocation);
                    settings = lines;
                    return true;
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Unable to get other settings. REASON: {e}");
                    settings = null;
                    return false;
                }
            }
            else
            {
                settings = null;
                DPCommon.WriteToLog($"Other settings not found.");
                return false;
            }
        }
        public bool WriteOtherSettings()
        {
            Directory.CreateDirectory("Settings");
            try
            {
                var lines = new string[] { destinationPath, downloadImages.ToString(),
                    thumbnailsPath, handleInstallation.ToString(), permDeleteSource.ToString(), tempPath,
                    installPrevProducts.ToString(), databasePath, OverwriteFiles.ToString()};
                File.WriteAllLines(oLocation, lines);
                return true;
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"Unable to write other settings. REASON: {e}");
                return false;
            }
        }
    }
}
