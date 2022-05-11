// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace DAZ_Installer.DP
{
    /// <summary>
    /// This class is used to gather important registry values.
    /// </summary>
    internal static class DPRegistry
    {
        internal static string[] ContentDirectories { get; set; }
        internal static string DazAppPath { get; set; } = "";
        internal static bool foundRegistry = false;
        internal static bool initalized = false;

        static DPRegistry()
        {
            var DazStudioKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DAZ\Studio4");
            if (DazStudioKey != null)
            {
                ContentDirectories = GetContentDirectories(DazStudioKey);

                // Get App Path.
                var valueNames = DazStudioKey.GetValueNames();
                string installPathName = "InstallPath-64";
                foreach (var name in valueNames)
                {
                    if (name.Contains("InstallPath")) installPathName = name;
                }
                DazAppPath = DazStudioKey.GetValue(installPathName, "") as string;
            }
        }

        private static string[] GetContentDirectories(RegistryKey key)
        {
            var dirs = new List<string>();
            byte i = 0;
            while (i < byte.MaxValue)
            {
                var contentDirName = "ContentDir" + i.ToString();
                string contentDirVal = key.GetValue(contentDirName, "") as string;
                if (string.IsNullOrEmpty(contentDirVal)) break;
                dirs.Add(contentDirVal);
                i++;
            }
            return dirs.ToArray();
        }
    }
}
