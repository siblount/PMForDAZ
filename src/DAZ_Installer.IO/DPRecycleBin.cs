using Microsoft.VisualBasic.FileIO;
using System.Runtime.InteropServices;

namespace DAZ_Installer.IO
{
    public static class DPRecycleBin
    {
        // VB says ushort instead of uint
        [Flags]
        public enum FileOperationFlags : ushort
        {
            FOF_SILENT = 0x0004,
            FOF_NOCONFIRMATION = 0x0010,
            FOF_ALLOWUNDO = 0x0040,
            FOF_SIMPLEPROGRESS = 0x0100,
            FOF_NOCONFIRMMKDIR = 0x0200,
            FOF_NOERRORUI = 0x0400,
            FOF_WANTNUKEWARNING = 0x4000,
        }
        public enum FileOperationType : uint
        {
            FO_DELETE = 0x0003,
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FileOperationType wFunc;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszProgressTitle;
        }

        // Error code 142 or 32 could be returned, indicating that the file is in use despite what the exception message says.
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);
        /// <summary>
        /// Sends the specified file or directory to the recycle bin. If the <paramref name="path"/> is a directory, it will be sent to the recycle bin with all of its contents.
        /// There will be no warnings or prompts. The operation will be silent. <para/>
        /// <b>WARNING!</b> Absolutely make sure that the paths are valid!
        /// Not doing so could result in unexpected files being lost OR files expected to be moved to the recycle bin not being moved. <para/>
        /// A safer alternative is to use <see cref="SendToRecycleBin(IEnumerable{IDPIONode})"/> as several checks are performed to ensure that the paths are valid
        /// and that the file/directory exists.
        /// </summary>
        /// <param name="path">The file or directory to send to recycle bin.</param>
        /// <returns>Whether the operation was successful or not</returns>
        public static bool SendToRecycleBin(string path) => SendToRecycleBin(new[] { path });
        /// <summary>
        /// Sends the specified files or directories to the recycle bin. If a path is a directory, it will be sent to the recycle bin with all of its contents.
        /// There will be no warnings or prompts. The operation will be silent. <para/>
        /// <b>WARNING!</b> Absolutely make sure that the paths are valid!
        /// Not doing so could result in unexpected files being lost OR files expected to be moved to the recycle bin not being moved.
        /// </summary>
        /// <param name="paths">The file or directory to send to recycle bin.</param>
        /// <returns>Whether the operation was successful or not</returns>
        public static bool SendToRecycleBin(IEnumerable<string> paths)
        {
            try
            {
                var fs = new SHFILEOPSTRUCT
                {
                    wFunc = FileOperationType.FO_DELETE,
                    pFrom = string.Join('\0', getFullPaths(paths)) + '\0' + '\0',
                    fFlags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI | FileOperationFlags.FOF_SILENT,
                };
                if (fs.pFrom == "\0\0") return false;
                var result = SHFileOperation(ref fs);
                return result == 0;
            } catch (Exception ex)
            {
                // log
            }
            return false;
        }
        /// <summary>
        /// Sends the specified file or directory to the recycle bin. If the <paramref name="fileOrDir"/> is a directory, it will be sent to the recycle bin with all of its contents.
        /// There will be no warnings or prompts. The operation will be silent.
        /// </summary>
        /// <param name="fileOrDir">The file or directory to send to recycle bin.</param>
        /// <returns>Whether the operation was successful or not</returns>
        public static bool SendToRecycleBin(IDPIONode fileOrDir) => SendToRecycleBin(new[] { fileOrDir });
        /// <summary>
        /// Sends the specified file or directory to the recycle bin. If an object is a directory, it will be sent to the recycle bin with all of its contents.
        /// There will be no warnings or prompts. The operation will be silent.
        /// </summary>
        /// <param name="fileOrDirs">The file or directory to send to recycle bin.</param>
        /// <returns>Whether the operation was successful or not</returns>
        public static bool SendToRecycleBin(IEnumerable<IDPIONode> fileOrDirs)
        {
            try
            {
                var enumerable = fileOrDirs.Where(node => node.Exists).Select(x => x.Path);
                var fs = new SHFILEOPSTRUCT
                {
                    wFunc = FileOperationType.FO_DELETE,
                    pFrom = string.Join('\0', enumerable) + '\0' + '\0',
                    fFlags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI | FileOperationFlags.FOF_SILENT,
                };
                if (fs.pFrom == "\0\0") return false;
                var result = SHFileOperation(ref fs);
                return result == 0;
            }
            catch (Exception ex)
            {
                // log
            }
            return false;
        }

        private static List<string> getFullPaths(IEnumerable<string> paths)
        {
            var normalizedPaths = new List<string>();
            foreach (var path in paths)
            {
                var normalizedPath = Path.GetFullPath(path);
                if (string.IsNullOrEmpty(normalizedPath)) 
                    throw new ArgumentException("The path is invalid", nameof(paths));
                normalizedPaths.Add(normalizedPath);
            }
            return normalizedPaths;
        }
    }
}
