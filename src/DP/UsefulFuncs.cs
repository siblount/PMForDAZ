// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace DAZ_Installer.DP
{
    internal struct DPCommon
    {
        internal static bool IsOnMainThread { get => 
                DPGlobal.mainThreadID == 
                Thread.CurrentThread.ManagedThreadId; 
        }
        internal static DragDropEffects dropEffect = DragDropEffects.All;
        public static string Up(string str)
        {
            if (str == "")
            {
                return "";
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
        public static string ConvertToUnicode(string defaultString)
        {
            // Convert string to bytes.
            byte[] bytes = new byte[defaultString.Length];
            for (int i = 0; i < defaultString.Length; ++i)
            {
                bytes[i] = (byte)defaultString[i];
            }
            // Convert default encoding to unicode and output it to bytes.
            byte[] unicodeBytes = Encoding.Convert(Encoding.Default, Encoding.Unicode, bytes);

            // Convert Unicode byte array to Unicode string.
            return Encoding.Unicode.GetString(unicodeBytes);
        }
        public static Control[] RecursivelyGetControls(Control obj)
        {
            if (obj.Controls.Count == 0)
            {
                return null;
            }
            else
            {
                var workingArr = new List<object>(obj.Controls.Count);
                foreach (Control control in obj.Controls)
                {
                    var result = RecursivelyGetControls(control);
                    if (result != null)
                    {
                        foreach (Control childControl in result)
                        {
                            var _index = ArrayHelper.GetNextOpenSlot(workingArr);
                            if (_index == -1)
                            {
                                workingArr.Add(childControl);
                            }
                            else
                            {
                                workingArr[_index] = childControl;
                            }
                        }
                    }
                    var index = ArrayHelper.GetNextOpenSlot(workingArr);
                    if (index == -1)
                    {
                        workingArr.Add(control);
                    }
                    else
                    {
                        workingArr[index] = control;
                    }
                }
                var controlArr = workingArr.OfType<Control>().ToArray();
                return controlArr;
            }

        }
        
        
        public static void WriteToLog(params object[] args)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(string.Join(' ', args));
            // TO DO: Call log.
#endif
        }
    }

    internal readonly struct ArrayHelper
    {
        internal static int GetIndex(object[] array, object obj)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == obj)
                {
                    return i;
                }
            }
            return -1;
        }
        internal static int GetIndex<T>(T[] array, T obj)
        {
            if (obj == null) return -1;
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(obj))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Returns the first null available in given array.
        /// Currently O(N) lookup.
        /// </summary>
        /// <param name="array">The array to search for an open slot.</param>
        /// <returns> Returns the index of the next available slot. Returns -1 if no open slot is found.</returns>
        internal static int GetNextOpenSlot(object[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }
        internal static int GetNextOpenSlot(List<object> array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                if (array[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        internal static bool Contains(string[] array, string obj)
        {
            for (var i = 0; i < array.Length; i++)
            {
                // Index of is 5000x faster than Contains()
                if (obj.IndexOf(array[i]) != -1) return true;
            }
            return false;
        }

        internal static bool Contains(ICollection<string> array, string obj)
        {
            foreach (var _obj in array)
            {
                if (obj.IndexOf(_obj) != -1) return true;
            }
            return false;
        }
        internal static void ClearArray(object[] array) => Array.Clear(array);
    }

    internal readonly struct PathHelper
    {
        /// <summary>
        /// Returns the relative path of the given path.
        /// </summary>
        /// <param name="path"></param> - The absolute path (or partial path) to compare.
        /// <param name="relativeTo"></param> - The absolute path (usually bigger) than path.
        /// <returns>The relative path of the given path.</returns>
        internal static string GetRelativePath(string path, string relativeTo)
        {
            char rSeperator = GetSeperator(relativeTo);
            char pSeperator = GetSeperator(path);

            var pNameSections = path.Split(pSeperator); // i 
            var rNameSections = relativeTo.Split(rSeperator); // j
            // We want find the last index of rNameSections 
            var findIndex = ArrayHelper.GetIndex(pNameSections, rNameSections[rNameSections.Length - 1]);
            var pathBuilder = "";
            for (int i = findIndex; i < pNameSections.Length; i++)
            {
                pathBuilder += pNameSections[i] + rSeperator;
            }
            return pathBuilder.TrimEnd(rSeperator);
        }

        internal static char GetSeperator(string path)
        {
            var forwardSlash = path.LastIndexOf('\\') != -1;
            var backwardSlash = path.LastIndexOf('/') != -1;

            if (forwardSlash && !backwardSlash)
            {
                return '\\';
            }
            else return '/';
        }


        internal static string GetLastDir(string path, bool isFilePath)
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

        internal static string GetFileName(string path)
        {
            char seperator = GetSeperator(path);
            return path.Split(seperator).Last();
        }

        internal static string GetAbsoluteUpPath(string path)
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

        internal static sbyte GetNumOfLevelsAbove(string path, string relativeTo)
        {
            var relPath = GetRelativePath(path, relativeTo);
            var seperator = GetSeperator(relPath);
            return (sbyte)relPath.Count((c) => c == seperator);
        }

        internal static string SwitchSeperators(string path)
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

        internal static string GetDirectoryPath(string path)
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
    }

}
