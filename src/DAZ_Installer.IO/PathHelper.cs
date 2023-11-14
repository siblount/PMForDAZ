using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DAZ_Installer.IO
{
    /// <summary>
    /// Path helper methods designed for archive paths (not on-disk paths).
    /// </summary>
    public static class PathHelper
    {
        public const char DEFAULT_SEPERATOR = '/';
        /// <summary>
        /// Returns a relative path using <paramref name="relativeTo"/>'s parent directory as a base to <paramref name="path"/>.
        /// This function is used to get the path relative to a content folder.  <para/>
        /// This function returns the seperator of the <paramref name="relativeTo"/> path.
        /// </summary>
        /// <param name="path">The absolute path (or partial path) to compare.</param>
        /// <param name="relativeTo">The absolute path of the path to compare to minus the parent directory.</param>
        /// <returns>The relative path of the given path.</returns>
        public static string GetRelativePathOfRelativeParent(ReadOnlySpan<char> path, ReadOnlySpan<char> relativeTo)
        {
            // String cannot end in a slash.
            path = Path.TrimEndingDirectorySeparator(path);
            relativeTo = Path.TrimEndingDirectorySeparator(relativeTo);
            var rSeperator = GetSeperator(relativeTo);
            var pSeperator = GetSeperator(path);
            var pNameSections = path.ToString().Split(pSeperator); // i 
            var rNameSections = relativeTo.ToString().Split(rSeperator); // j
            // We want find the last index of rNameSections
            var findIndex = Array.IndexOf(pNameSections, rNameSections[^1]);
            if (findIndex == -1) return path.ToString();
            var pathBuilder = new StringBuilder(path.Length);
            for (var i = findIndex; i < pNameSections.Length; i++)
            {
                pathBuilder.Append(pNameSections[i]).Append(rSeperator);
            }
            return pathBuilder.Length == 0 ? string.Empty : pathBuilder.ToString().TrimEnd(rSeperator);
        }
        /// <summary>
        /// Returns the seperator of the given path. If there are multiple seperators, then it will return a forward slash.
        /// If there are no seperators, it will return a forward slash.
        /// </summary>
        /// <param name="path">The path to determine the seperator from.</param>
        /// <returns>The seperator char</returns>
        public static char GetSeperator(ReadOnlySpan<char> path)
        {
            var backsSlash = path.LastIndexOf('\\') != -1;
            var forwardSlash = path.LastIndexOf('/') != -1;
            return backsSlash && !forwardSlash ? '\\' : '/';
        }

        /// <summary>
        /// Returns the name of the last/parent/rightmost directory in the path depending on whether the path is a directory
        /// path or not.
        /// </summary>
        /// <param name="path">The path you wish to get the "last" directory of.</param>
        /// <param name="isFilePath">Determines whether the path provided is a path to a directory or a file.</param>
        /// <returns>The name of the last directory.</returns>
        public static string GetLastDir(string path, bool isFilePath)
        {
            path = Path.TrimEndingDirectorySeparator(path);
            var seperator = GetSeperator(path);
            var arr = path.Split(seperator);
            string result;
            if (isFilePath)
            {
                result = arr.Length switch
                {
                    >= 2 when arr[^1].Contains('.') => arr[^2],
                    _ => string.Empty
                };
            } else
            result = arr.Length >= 2 ? path.Split(seperator)[^2] : string.Empty;
            // Do not return anything if the result is a drive letter (eg: C:)
            return result.EndsWith(':') ? string.Empty : result;
        }
        /// <summary>
        /// Returns the parent directory of the given path.
        /// </summary>
        /// <param name="path">The path to get the parent of.</param>
        [Obsolete("Use PathHelper.Up() instead.", false)]
        public static string GetParent(string path) => Up(path);
        /// <summary>
        /// Returns the file name of a path.
        /// </summary>
        /// <param name="path">The path to use.</param>
        public static string GetFileName(string path)
        {
            var seperator = GetSeperator(path);
            return path.Split(seperator)[^1];
        }

        /// <summary>
        /// Clean the directory path by ensuring a consistent seperator and removing the trailing seperator. <para/>
        /// The only difference from <see cref="NormalizePath(string)"/> is that it uses the seperator of the given path 
        /// (versus using the default seperator - forward slash).
        /// </summary>
        /// <param name="path">The path to process.</param>
        /// <returns>The path with no trailing seperator and consistent seperator.</returns>
        public static string CleanDirPath(string path)
        {
            var seperator = GetSeperator(path);
            path = SwitchToSeperator(path, seperator);
            var strBuilder = new StringBuilder(path.Length);
            foreach (var str in path.Split(seperator, options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                strBuilder.Append(str).Append(seperator);
            }
            if (strBuilder.Length == 0) return string.Empty;
            return strBuilder.Remove(strBuilder.Length - 1, 1).ToString();
        }
        /// <summary>
        /// Determines how many levels/directories above the given path is from the relative path.
        /// </summary>
        /// <param name="path">The path to see how many levels it is above <paramref name="relativeTo"/>.</param>
        /// <param name="relativeTo">The relative path to see how many levels the relative path is below <paramref name="path"/>.</param>
        /// <returns>The amount of levels/directorires above <paramref name="relativeTo"/>.</returns>
        public static byte GetNumOfLevelsAbove(string path, string relativeTo)
        {
            var relPath = GetRelativePathOfRelativeParent(path, relativeTo);
            var seperator = GetSeperator(relPath);
            return (byte)relPath.Count((c) => c == seperator);
        }

        /// <summary>
        /// Returns the amount of subfolders/sub-directories in the given path. For example, <c>Users\John\Documents</c> would return 2.
        /// Do not use for on-disk file paths. Use for archive paths.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        // TODO: Ensure that path does not end in a seperator.
        public static byte GetSubfoldersCount(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            var seperator = GetSeperator(path);
            var c = path.Count((c) => c == seperator);
            return Convert.ToByte(path[^1] == seperator ? c - 1 : c);
        }
        /// <summary>
        /// Switches the seperators of the given path to the opposite seperator. For example, <c>C:\Users\John\Documents</c> would return <c>C:/Users/John/Documents</c>.
        /// For paths that contain multiple seperators, it will make all seperators back-slashes.
        /// </summary>
        /// <param name="path">The path to switch seperators.</param>
        /// <returns>The path with seperators switched.</returns>
        public static string SwitchSeperators(string path)
        {
            var seperator = GetSeperator(path);
            var oppositeSeperator = seperator == '\\' ? '/' : '\\';
            return path.Replace(seperator, oppositeSeperator);
        }
        /// <summary>
        /// Switches the seperators of the given path to the opposite seperator. For example, if you want to switch to backslashes, 
        /// <c>C:/Users/John/Documents</c> would return <c>C:\Users\John\Documents</c>.
        /// </summary>
        /// <param name="path">The path to switch seperators.</param>
        /// <param name="seperator">The seperator to switch to.</param>
        /// <returns>The path with seperators switched.</returns>
        public static string SwitchToSeperator(string path, char seperator)
        {
            var oppositeSeperator = seperator == '\\' ? '/' : '\\';
            return path.Replace(oppositeSeperator, seperator);
        }

        /// <summary>
        /// GetDirectoryPath slightly differs from <see cref="Path.GetDirectoryName(string?)"/> in a few scenarios. <br/>
        /// If <paramref name="path"/> ends with a seperator, this function will return <see cref="string.Empty"/>. <br/>
        /// If <paramref name="path"/> contains only the drive (eg: <c>C:\</c>), this function will return <see cref="string.Empty"/>; whereas
        /// <see cref="Path.GetDirectoryName(string?)"/> would return <c>C:</c>. <br/>
        /// If <paramref name="path"/> does not contain a seperator but has words, such as <c>Documents</c>, this function will return <c>Documents</c>;
        /// whereas <see cref="Path.GetDirectoryName(string?)"/> will return <see cref="string.Empty"/>.
        /// </summary>
        /// <param name="path">The path to get the directory. </param>
        /// <returns></returns>
        public static string GetDirectoryPath(string path)
        {
            var strBuilder = new StringBuilder(path.Length);
            var seperator = GetSeperator(path);
            foreach (var str in path.Split(seperator, options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                strBuilder.Append(str).Append(seperator);
            }
            return CleanDirPath(strBuilder.ToString());
        }

        /// <summary>
        /// Replaces all back slashes (\) with forward slashes (/) and trims the seperator if it is not a drive letter.
        /// </summary>
        /// <param name="path">The path to normalize. Cannot be null.</param>
        public static string NormalizePath(string path)
        {
            var s = path.Replace('\\', '/');
            // Issue #24: Prevent Path.GetFullPath from unexpectedly returning the current directory when the path is trimmed to a drive letter
            // and a colon. For example: "D:/" -> "D:" then "D:" -> current directory.
            if (s.Length >= 2 && s[^2] != ':') return s.TrimEnd('/');
            return s;
        }
        /// <summary>
        /// Returns the full directory path of <paramref name="str"/>. It must be a directory path, not a file path. For example,
        /// if <paramref name="str"/> is <c>C:\Users\John\Documents</c>, then <c>C:\Users\John</c> will be returned.
        /// </summary>
        /// <param name="str">A path of a directory.</param>
        /// <returns>The full directory path of <paramref name="str"/>.</returns>
        public static string Up(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            if (Path.EndsInDirectorySeparator(str))
                return str.Remove(str.LastIndexOf(GetLastDir(str, false)));
            var fileName = Path.GetFileName(str);
            return CleanDirPath(str.Remove(str.LastIndexOf(fileName)));
        }
        /// <summary>
        /// Checks for whether the given <paramref name="path"/> is attempting to directory tranverse.
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns><see langword="true"/> if the path traverses, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="path"/> is <see langword="null"/>.</exception>
        public static bool CheckForTranversal(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            // Normalize the path and split it.
            foreach (var part in NormalizePath(path).Split(DEFAULT_SEPERATOR))
            {
                if (part == ".." || part == ".") return true;
            }
            return false;
        }
    }
}
