// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.Core.Utilities;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DAZ_Installer.WinApp
{
    public enum SettingOptions
    {
        No, Yes, Prompt
    }

    /// <summary>
    /// DPSettings is a data class that holds all the settings for the application. It is serialized to a JSON file.
    /// </summary>
    public class DPSettings
    {
        // Note: The JSONSerializer will not use the setter for HashSet, therefore the 
        [JsonIgnore]
        public static DPSettings CurrentSettingsObject;
        /// <summary>
        /// Represents the DAZ content library (or destination) to install the products to.
        /// </summary>
        public string DestinationPath { get; set; } // todo : Ask for daz content directory if no detected daz content paths found.
        // TO DO: Use HashSet instead of list.
        public string[] detectedDazContentPaths;
        /// <summary>
        /// Determines whether to download thumbnail images of the product.
        /// </summary>
        public SettingOptions DownloadImages { get; set; } = SettingOptions.Prompt;
        /// <summary>
        /// The thumbnail directory to use for the application to use.
        /// </summary>
        public string ThumbnailsDir { get; set; } = "Thumbnails";
        /// <summary>
        /// The install option to use as part of <see cref="DPProcessSettings"/>. It is used to determine what to do with the manifest and whether to auto install.
        /// </summary>
        public InstallOptions HandleInstallation { get; set; } = InstallOptions.ManifestAndAuto;
        /// <summary>
        /// The default common content folder names. These are used to determine if a folder is a common content folder. For example, if a folder is called "data", it will be considered a common content folder.
        /// </summary>
        [JsonIgnore]
        public static HashSet<string> InititalCommonContentFolderNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "aniBlocks", "Animals", "Architecture", "Camera Presets", "data", "DAZ Studio Tutorials", "Documentation", "Documents",
                                                                                                                                "Environments", "General", "Light Presets", "Lights", "People", "Presets", "Props", "Render Presets", "Render Settings", "Runtime",
                                                                                                                                "Scene Builder", "Scene Subsets", "Scenes", "Scripts", "Shader Presets", "Shaders", "Support", "Templates", "Textures", "Vehicles" };
        /// <summary>
        /// Common content folder names are used to determine if a folder is a common content folder. For example, if a folder is called "data", it will be considered a common content folder.
        /// </summary>
        public HashSet<string> CommonContentFolderNames { get; set; } = new HashSet<string>(InititalCommonContentFolderNames, StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Folder Redirects (or aliases) are used to redirect folders to another folder. For example, if you have a folder called "docs" in your DAZ content directory, you can redirect it to "Documentation" by adding "docs" to the key and "Documentation" to the value.
        /// </summary>
        public Dictionary<string, string> FolderRedirects { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "docs", "Documentation" }, { "Documents", "Documentation" } };
        /// <summary>
        /// The temporary directory to use for the application to use.
        /// </summary>
        public string TempDir { get; set; } = Path.Combine(Path.GetTempPath(), "DazProductInstaller");
        /// <summary>
        /// Set the maximum tags to show on <see cref="LibraryItem"/>s. This should be kept low because GDI+ is slow. Default is set to 8.
        /// </summary>
        public uint MaxTagsToShow { get; set; } = 8;
        /// <summary>
        /// Determines whether to delete the source files after installation.
        /// </summary>
        public SettingOptions PermDeleteSource { get; set; } = SettingOptions.Prompt;
        /// <summary>
        /// Determines whether to install products that are already installed (or in the library/database).
        /// </summary>
        public SettingOptions InstallPrevProducts { get; set; } = SettingOptions.Prompt;
        /// <summary>
        /// Determines whether to overwrite files when installing products.
        /// </summary>
        public SettingOptions OverwriteFiles { get; set; } = SettingOptions.Yes;
        /// <summary>
        /// The delete action to use when deleting files from DAZ content directories. This does not apply to temp files.
        /// </summary>
        public RecycleOption DeleteAction { get; set; } = RecycleOption.DeletePermanently;
        /// <summary>
        /// The directory for the database to use. This is not the database file itself.
        /// </summary>
        public string DatabaseDir { get; set; } = "Database";
        /// <summary>
        /// Determines whether the current settings are valid. It checks whtehr the directories exist and have access to them.
        /// </summary>
        public bool Valid => Verify();
        private const string SETTINGS_PATH = "settings.json";

        static DPSettings()
        {
            var fileInfo = new FileInfo(SETTINGS_PATH);
            CurrentSettingsObject = fileInfo.Exists ? FromJson(File.ReadAllText(SETTINGS_PATH)) ?? new DPSettings() : new DPSettings();
        }
        public DPSettings() => detectedDazContentPaths = DPRegistry.ContentDirectories;
        /// <summary>
        /// Clones the current settings object and returns it.
        /// </summary>
        /// <returns></returns>

        public static DPSettings GetCopy()
        {
            var settings = (DPSettings)CurrentSettingsObject.MemberwiseClone();
            settings.CommonContentFolderNames = new HashSet<string>(CurrentSettingsObject.CommonContentFolderNames, StringComparer.OrdinalIgnoreCase);
            settings.FolderRedirects = new Dictionary<string, string>(CurrentSettingsObject.FolderRedirects, StringComparer.OrdinalIgnoreCase);
            settings.detectedDazContentPaths = (string[])CurrentSettingsObject.detectedDazContentPaths.Clone();
            return settings;
        }
        /// <summary>
        /// Returns the parsed DPSettings object from the JSON string. Returns null if failed to parse.
        /// </summary>
        /// <param name="json">The JSON of a DPSettings object.</param>
        public static DPSettings? FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<DPSettings>(json);
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to parse settings. REASON: {ex}");
            }
            return null;
        }
        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
        /// <summary>
        /// Verifies whether the directories exist and have access to them. <see cref="Valid"/> calls this function.
        /// </summary>
        /// <returns>Whether the current settings object is valid or not.</returns>
        public bool Verify() => Directory.Exists(DestinationPath) && Directory.Exists(ThumbnailsDir) && Directory.Exists(TempDir) && Directory.Exists(DatabaseDir);
        /// <summary>
        /// Updates this settings object to default the directory values of <see cref="ThumbnailsDir"/>, <see cref="TempDir"/>, and <see cref="DatabaseDir"/>. 
        /// Note that default paths may not be valid (exist or have access to)."/>
        /// </summary>
        public void DefaultDirectories()
        {
            ThumbnailsDir = createDirectoryIfNotExists("Thumbnails");
            TempDir = createDirectoryIfNotExists(Path.Combine(Path.GetTempPath(), "DazProductInstaller"));
            DatabaseDir = createDirectoryIfNotExists("Database");
        }

        /// <summary>
        /// Creates the directory if it does not exist (or app does not have access to it). No exception is thrown if it fails.
        /// </summary>
        /// <param name="path">The path of the directory to create.</param>
        private string createDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                TryHelper.Try(() => Directory.CreateDirectory(path));
            }
            return path;
        }
        /// <summary>
        /// Resets this settings object to its default values. Note that default paths may not be valid (exist or have access to).
        /// </summary>
        public void Reset()
        {
            // Reset all properties to default values.
            detectedDazContentPaths = DPRegistry.ContentDirectories;
            HandleInstallation = InstallOptions.ManifestAndAuto;
            CommonContentFolderNames = new HashSet<string>(InititalCommonContentFolderNames, StringComparer.OrdinalIgnoreCase);
            FolderRedirects = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "docs", "Documentation" }, { "Documents", "Documentation" } };
            MaxTagsToShow = 8;
            PermDeleteSource = SettingOptions.Prompt;
            InstallPrevProducts = SettingOptions.Prompt;
            OverwriteFiles = SettingOptions.Yes;
            DeleteAction = RecycleOption.DeletePermanently;
            DefaultDirectories();
        }


    }
}
