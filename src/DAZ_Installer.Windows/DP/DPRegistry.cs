// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// This class is used to gather important registry values.
    /// </summary>
    internal static class DPRegistry
    {
        /// <summary>
        /// The DAZ Content Directories. May be empty if none found.
        /// </summary>
        internal static string[] ContentDirectories { get; set; } = Array.Empty<string>();
        /// <summary>
        /// The application path to DAZ Studio. Value may be <see cref="string.Empty"/> if not found.
        /// </summary>
        internal static string DazAppPath { get; private set; } = string.Empty;

        static DPRegistry() => Refresh();
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
            return dirs.ToArray();
        }

        /// <summary>
        /// Updates DPRegistry values.
        /// </summary>
        internal static void Refresh()
        {
            RegistryKey? DazStudioKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DAZ\Studio4");
            if (DazStudioKey == null) return;
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

    }
}
