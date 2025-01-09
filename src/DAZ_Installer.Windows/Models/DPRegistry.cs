// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;

namespace DAZ_Installer.Windows.DP
{
    /// <inheritdoc/>
    public class DPRegistry : IDPPlatformRegistry
    {
        /// <inheritdoc/>
        public string[] ContentDirectories { get; set; } = [];
        /// <inheritdoc/>
        public string DazAppPath { get; private set; } = string.Empty;
        /// <summary>
        /// The logger for this class.
        /// </summary>
        public ILogger Logger = Log.ForContext<DPRegistry>();
        /// <inheritdoc/>
        public readonly static DPRegistry Instance = new();

        private DPRegistry()
        {
            Refresh();
        }

        /// <inheritdoc/>
        public void Refresh()
        {
            try
            {
                using RegistryKey? DazStudioKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DAZ\Studio4");
                if (DazStudioKey is null) return;
                ContentDirectories = GetContentDirectories(DazStudioKey);
                // Get App Path.
                var valueNames = DazStudioKey.GetValueNames();
                var installPathName = "InstallPath-64";
                foreach (var name in valueNames)
                {
                    if (name.Contains("InstallPath")) installPathName = name;
                }
                DazAppPath = DazStudioKey.GetValue(installPathName, "") as string ?? string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while refreshing the registry");
            }
        }

        /// <summary>
        /// Fetches the content directories from the registry.
        /// </summary>
        /// <param name="key">The parent registry subkey, example: <c>SOFTWARE\DAZ\Studio4</c>.</param>
        /// <returns>The content directories found from registry.</returns>
        private static string[] GetContentDirectories(RegistryKey key)
        {
            var dirs = new List<string>();
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                var contentDirName = "ContentDir" + i.ToString();
                var contentDirVal = key.GetValue(contentDirName, string.Empty) as string;
                if (string.IsNullOrEmpty(contentDirVal)) break;
                dirs.Add(contentDirVal);
            }
            return [.. dirs];
        }
    }
}
