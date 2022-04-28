// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

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
    public static class DPSettings
    {
        // TO DO : Initalize and load settings.
        // If no settings found - regenerate.
        public static string destinationPath
        {
            get;
            set;
        } // todo : Ask for daz content directory if no detected daz content paths found.
        // TO DO: Use HashSet instead of list.
        public static string[] detectedDazContentPaths;
        public static SettingOptions downloadImages { get; set; } = SettingOptions.Prompt;
        public static string thumbnailsPath { get; set; } = "Thumbnails";
        public static InstallOptions handleInstallation { get; set; } = InstallOptions.ManifestAndAuto;
        // TO DO: Use HashSet instead of list.
        public static string[] inititalCommonContentFolderNames { get; } = new string[] { "aniBlocks", "Animals", "Architecture", "Camera Presets", "data", "DAZ Studio Tutorials", "Documentation", "Documents", "Environments", "General", "Light Presets", "Lights", "People", "Presets", "Props", "Render Presets", "Render Settings", "Runtime", "Scene Builder", "Scene Subsets", "Scenes", "Scripts", "Shader Presets", "Shaders", "Support", "Templates", "Textures", "Vehicles" };
        // TO DO: Use HashSet instead of list.
        public static string[] commonContentFolderNames { get; set; }
        public static Dictionary<string, string> folderRedirects { get; set; } = new Dictionary<string, string>() { { "docs", "Documentation" } };
        public static string tempPath { get; set; } = Path.Combine(Path.GetTempPath(), "DazProductInstaller"); //
        public static uint maxTagsToShow { get; set; } = 8; // Keep low because GDI+ slow.
        public static SettingOptions permDeleteSource { get; set; } = SettingOptions.Prompt;
        public static SettingOptions installPrevProducts { get; set; } = SettingOptions.Prompt;
        public static string databasePath { get; set; } = "Database";
        public static bool initalized { get; set; } = false;


        // Constants
        const string cfnLocation = "Settings/cfn.txt"; // Content Folder Names Location
        const string frLocation = "Settings/fn.txt"; // Folder Redirects Location
        const string oLocation = "Settings/o.txt"; // Other settings Location

        public static void Initalize()
        {
            if (initalized) return;
            DPRegistry.Initalize();
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
            }
            else
            {
                // Since getting other settings failed... we will use the default parameters, but some can't go unpunished.
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

            if (GetContentFolderNames(out string[] folders))
            {
                commonContentFolderNames = folders;
            }
            else
            {
                commonContentFolderNames = inititalCommonContentFolderNames;
            }

            if (GetFolderRedirects(out Dictionary<string, string> dict))
            {
                folderRedirects = dict;
            }

            initalized = true;
            //DPRegistry.Initalize();
            detectedDazContentPaths = DPRegistry.ContentDirectories;
            DeleteTempFiles();
        }

        internal static void DeleteTempFiles()
        {
            // Issue: UnauthorizedAccessException for weird reason.
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
                DPCommon.WriteToLog("Deleted temp files.");
            }
        }

        public static bool SaveSettings(out string errorMsg)
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

        public static bool GetContentFolderNames(out string[] arr)
        {

            if (File.Exists(cfnLocation))
            {
                try
                {
                    var contents = File.ReadAllLines(cfnLocation);
                    arr = contents;
                    return true;
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Unable to get content folder names file. Reason: {e}");
                    arr = null;
                    return false;
                }
            }
            else
            {
                arr = null;
                return false;
            }
        }
        public static bool WriteContentFolderNames()
        {
            try
            {
                Directory.CreateDirectory("Settings");
                if (commonContentFolderNames != null && commonContentFolderNames.Length > 0)
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
        public static bool GetFolderRedirects(out Dictionary<string, string> dict)
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


        public static bool WriteFolderRedirects()
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
        public static bool GetOtherSettings(out string[] settings)
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
        public static bool WriteOtherSettings()
        {
            Directory.CreateDirectory("Settings");
            try
            {
                var lines = new string[] { destinationPath, downloadImages.ToString(),
                    thumbnailsPath, handleInstallation.ToString(), permDeleteSource.ToString(), tempPath,
                    installPrevProducts.ToString(), databasePath};
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
