using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Build
{
    public partial class CSProjVersionUpdater {
        [GeneratedRegex(@"<FileVersion>(\d+\.\d+\.\d+)</FileVersion>")]
        private static partial Regex FileVersionRegex();
        [GeneratedRegex(@"<VersionSuffix>(.*)</VersionSuffix>")]
        private static partial Regex VersionSuffixRegex();
        [GeneratedRegex(@"<AssemblyVersion>(\d+\.\d+\.\d+)</AssemblyVersion>")]
        private static partial Regex AssemblyVersionRegex();

        internal const uint MAX_FILE_SIZE = 15 * 1024; // 15 KB

        /// <summary>
        /// Updates the version of the specified project file.
        /// </summary>
        /// <param name="content">The content of the cs proj.</param>
        /// <param name="version">The version to set (format: major.minor.patch).</param>
        /// <param name="suffix">The version suffix to set (ex: "Pre-Alpha", "Alpha").</param>
        public static string UpdateVersion(string content, string version, string suffix)
        {
            // Update the <Version>, <AssemblyVersion>, and <FileVersion> elements in the project file.
            // Update the <VersionSuffix> element in the project file.
            content = AssemblyVersionRegex().Replace(content, $"<AssemblyVersion>{version}</AssemblyVersion>");
            content = FileVersionRegex().Replace(content, $"<FileVersion>{version}</FileVersion>");
            content = VersionSuffixRegex().Replace(content, $"<VersionSuffix>{suffix}</VersionSuffix>");
            return content;
        }

    }

    public partial class ISSVersionUpdater
    {
        internal const uint MAX_FILE_SIZE = 15 * 1024; // 15 KB
        /// <summary>
        /// Updates the version of the specified project file.
        /// </summary>
        /// <param name="content">The content of the cs proj.</param>
        /// <param name="version">The version to set (format: major.minor.patch).</param>
        /// <param name="suffix">The version suffix to set (ex: "Pre-Alpha", "Alpha").</param>
        public static string UpdateVersion(string content, string version, string suffix)
        {
            return AppVersionRegex().Replace(content, $"#define MyAppVersion \"{version}\"");
        }

        [GeneratedRegex(@"#define MyAppVersion ""(\d+\.\d+\.\d+)""")]
        private static partial Regex AppVersionRegex();
    }

    public class VersionVersionUpdater
    {
        internal const uint MAX_FILE_SIZE = 256; // 256 bytes

        public static string UpdateVersion(string version, string suffix)
        {
            return $"{version}\n{suffix}";
        }
    }
}
