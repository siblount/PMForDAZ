using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core
{
    public readonly struct PathHelper
    {
        /// <summary>
        /// Returns the relative path of the given path.
        /// </summary>
        /// <param name="path"></param> - The absolute path (or partial path) to compare.
        /// <param name="relativeTo"></param> - The absolute path of the path to compare to minus the sublevel..
        /// <returns>The relative path of the given path.</returns>
        public static string GetRelativePath(ReadOnlySpan<char> path, ReadOnlySpan<char> relativeTo)
        {
            char rSeperator = GetSeperator(relativeTo);
            char pSeperator = GetSeperator(path);
            var pNameSections = path.ToString().Split(pSeperator); // i 
            var rNameSections = relativeTo.ToString().Split(rSeperator); // j
            // We want find the last index of rNameSections 

            var findIndex = ArrayHelper.GetIndex(pNameSections, rNameSections[rNameSections.Length - 1]);
            if (findIndex == -1) return path.ToString();
            StringBuilder pathBuilder = new StringBuilder(path.Length);
            for (int i = findIndex; i < pNameSections.Length; i++)
            {
                pathBuilder.Append(pNameSections[i] + rSeperator);
            }
            return pathBuilder.Length == 0 ? string.Empty : pathBuilder.ToString().TrimEnd(rSeperator);
        }

        public static char GetSeperator(ReadOnlySpan<char> path)
        {
            var forwardSlash = path.LastIndexOf('\\') != -1;
            var backwardSlash = path.LastIndexOf('/') != -1;

            if (forwardSlash && !backwardSlash)
            {
                return '\\';
            }
            else return '/';
        }


        public static string GetLastDir(string path, bool isFilePath)
        {
            char seperator = GetSeperator(path);
            if (!isFilePath)
            {
                return path.Split(seperator).Last();
            }
            else
            {
                var arr = path.Split(seperator);
                if (arr.Length >= 2 && arr[^1].Contains('.'))
                {
                    return arr[^2];
                }
                return arr[0];
            }
        }

        public static string GetParent(string path)
        {
            char seperator = GetSeperator(path);
            var lastSeperatorIndex = path.LastIndexOf(seperator);
            return lastSeperatorIndex != -1 ? path.Substring(0, lastSeperatorIndex) : path;
        }

        public static string GetFileName(string path)
        {
            char seperator = GetSeperator(path);
            return path.Split(seperator).Last();
        }

        public static string GetAbsoluteUpPath(string path)
        {
            var seperator = GetSeperator(path);
            var strBuilder = "";
            foreach (var str in path.Split(seperator))
            {
                if (str.Trim() == "") continue;
                strBuilder += str + seperator;
            }
            return strBuilder.TrimEnd(seperator);
        }

        public static byte GetNumOfLevelsAbove(string path, string relativeTo)
        {
            var relPath = GetRelativePath(path, relativeTo);
            var seperator = GetSeperator(relPath);
            return (byte)relPath.Count((c) => c == seperator);
        }

        public static byte GetNumOfLevels(string path)
        {
            var seperator = GetSeperator(path);
            return (byte)path.Count((c) => c == seperator);
        }

        public static string SwitchSeperators(string path)
        {
            try
            {
                var chars = path.ToCharArray();
                var seperator = GetSeperator(path);
                char oppositeSeparator;

                if (seperator == '\\') oppositeSeparator = '/';
                else oppositeSeparator = '\\';

                for (var i = 0; i < chars.Length; i++)
                {
                    if (chars[i] == seperator)
                    {
                        chars[i] = oppositeSeparator;
                    }
                }
                return new string(chars);
            }
            catch { }
            return path;
        }

        public static string GetDirectoryPath(string path)
        {
            var seperator = GetSeperator(path);
            var strBuilder = "";
            foreach (var str in path.Split(seperator))
            {
                if (str.Trim() == "") continue;
                strBuilder += str + seperator;
            }
            strBuilder = strBuilder.TrimEnd(seperator);

            if (seperator == '/') return SwitchSeperators(strBuilder);
            else return strBuilder;
        }

        /// <summary>
        /// Replaces all forward slashes (/) with back slashes (\).
        /// </summary>
        /// <param name="path">The path to cleanize. Cannot be null.</param>
        /// <returns></returns>
        public static string NormalizePath(string path) => path.Replace('/', '\\');

        public static string Up(string str)
        {
            if (str == string.Empty)
            {
                return string.Empty;
            }
            if (Path.HasExtension(str))
            {
                var fileName = PathHelper.GetFileName(str);
                return str.Remove(str.LastIndexOf(fileName)).TrimEnd(PathHelper.GetSeperator(str));
            }
            else
            {
                var dirName = PathHelper.GetLastDir(str, false);
                if (dirName == "" && PathHelper.GetAbsoluteUpPath(str) != dirName) dirName = PathHelper.GetAbsoluteUpPath(str);
                var trimmedPath = str.Remove(str.LastIndexOf(dirName));
                return PathHelper.GetAbsoluteUpPath(trimmedPath);
            }


        }
    }
}
