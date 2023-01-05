﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using DAZ_Installer.DP;

namespace DAZ_Installer.External
{
    // GC Collect
    public class RAR : IDisposable

    {
        #region Event Delegate Definitions

        /// <summary>
        /// Represents the method that will handle data available events
        /// </summary>
        public delegate void DataAvailableHandler(RAR sender, DataAvailableEventArgs e);
        /// <summary>
        /// Represents the method that will handle extraction progress events
        /// </summary>
        public delegate void ExtractionProgressHandler(RAR sender, ExtractionProgressEventArgs e);
        /// <summary>
        /// Represents the method that will handle missing archive volume events
        /// </summary>
        public delegate void MissingVolumeHandler(RAR sender, MissingVolumeEventArgs e);
        /// <summary>
        /// Represents the method that will handle new volume events
        /// </summary>
        public delegate void NewVolumeHandler(RAR sender, NewVolumeEventArgs e);
        /// <summary>
        /// Represents the method that will handle new file notifications
        /// </summary>
        public delegate void NewFileHandler(RAR sender, NewFileEventArgs e);
        /// <summary>
        /// Represents the method that will handle password required events
        /// </summary>
        public delegate void PasswordRequiredHandler(RAR sender, PasswordRequiredEventArgs e);

        #endregion
        #region RAR DLL enumerations

        /// <summary>
        /// Mode in which archive is to be opened for processing.
        /// </summary>
        public enum OpenMode
        {
            /// <summary>
            /// Open archive for listing contents only
            /// </summary>
            List = 0,
            /// <summary>
            /// Open archive for testing or extracting contents
            /// </summary>
            Extract = 1
        }

        internal enum RarError : uint
        {
            EndOfArchive = 10,
            InsufficientMemory = 11,
            BadData = 12,
            BadArchive = 13,
            UnknownFormat = 14,
            OpenError = 15,
            CreateError = 16,
            CloseError = 17,
            ReadError = 18,
            WriteError = 19,
            BufferTooSmall = 20,
            UnknownError = 21,
            MissingPassword = 22,
            InternalError = 23,
            BadPassword = 24
        }

        internal enum Operation : uint
        {
            Skip = 0,
            Test = 1,
            Extract = 2
        }

        internal enum VolumeMessage : uint
        {
            Ask = 0,
            Notify = 1
        }

        [Flags]
        internal enum ArchiveFlags : uint
        {
            Volume = 0x1,                                       // Volume attribute (archive volume)
            CommentPresent = 0x2,                       // Archive comment present
            Lock = 0x4,                                         // Archive lock attribute
            SolidArchive = 0x8,                         // Solid attribute (solid archive)
            NewNamingScheme = 0x10,                 // New volume naming scheme ('volname.partN.rar')
            AuthenticityPresent = 0x20,         // Authenticity information present
            RecoveryRecordPresent = 0x40,       // Recovery record present
            EncryptedHeaders = 0x80,                // Block headers are encrypted
            FirstVolume = 0x100                         // 0x0100  - First volume (set only by RAR 3.0 and later)
        }

        internal enum CallbackMessages : uint
        {
            VolumeChange = 0,
            ProcessData = 1,
            NeedPassword = 2,
            VolumeChangeAgain = 3,
            NeedPasswordAgain = 4
        }

        #endregion

        #region RAR DLL structure definitions

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct RARHeaderData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string ArcName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string FileName;
            public uint Flags;
            public uint PackSize;
            public uint UnpSize;
            public uint HostOS;
            public uint FileCRC;
            public uint FileTime;
            public uint UnpVer;
            public uint Method;
            public uint FileAttr;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;

            public void Initialize()
            {
                CmtBuf = new string((char)0, 65536);
                CmtBufSize = 65536;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct RARHeaderDataEx
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string ArcName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string ArcNameW;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string FileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string FileNameW;
            public uint Flags;
            public uint PackSize;
            public uint PackSizeHigh;
            public uint UnpSize;
            public uint UnpSizeHigh;
            public uint HostOS;
            public uint FileCRC;
            public uint FileTime;
            public uint UnpVer;
            public uint Method;
            public uint FileAttr;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public uint[] Reserved;

            public void Initialize()
            {
                CmtBuf = new string((char)0, 65536);
                CmtBufSize = 65536;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RAROpenArchiveData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string ArcName;
            public uint OpenMode;
            public uint OpenResult;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;

            public void Initialize()
            {
                CmtBuf = new string((char)0, 65536);
                CmtBufSize = 65536;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAROpenArchiveDataEx
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string ArcName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ArcNameW;
            public uint OpenMode;
            public uint OpenResult;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public uint[] Reserved;

            public void Initialize()
            {
                CmtBuf = new string((char)0, 65536);
                CmtBufSize = 65536;
                Reserved = new uint[32];
            }
        }

        #endregion

        #region RAR function declarations

        [DllImport("unrar.dll")]
        private static extern IntPtr RAROpenArchive(ref RAROpenArchiveData archiveData);

        [DllImport("UNRAR.DLL")]
        private static extern IntPtr RAROpenArchiveEx(ref RAROpenArchiveDataEx archiveData);

        [DllImport("unrar.dll")]
        private static extern int RARCloseArchive(IntPtr hArcData);

        [DllImport("unrar.dll")]
        private static extern int RARReadHeader(IntPtr hArcData, ref RARHeaderData headerData);

        [DllImport("unrar.dll")]
        private static extern int RARReadHeaderEx(IntPtr hArcData, ref RARHeaderDataEx headerData);

        [DllImport("unrar.dll")]
        private static extern int RARProcessFile(IntPtr hArcData, int operation,
            [MarshalAs(UnmanagedType.LPStr)] string destPath,
            [MarshalAs(UnmanagedType.LPStr)] string destName);

        [DllImport("unrar.dll")]
        private static extern int RARProcessFileW(IntPtr hArcData, int operation,
    [MarshalAs(UnmanagedType.LPWStr)] string destPath,
    [MarshalAs(UnmanagedType.LPWStr)] string destName);

        [DllImport("unrar.dll")]
        private static extern void RARSetCallback(IntPtr hArcData, UNRARCallback callback, int userData);

        [DllImport("unrar.dll")]
        private static extern void RARSetPassword(IntPtr hArcData,
            [MarshalAs(UnmanagedType.LPStr)] string password);

        // RAR callback delegate signature
        private delegate int UNRARCallback(uint msg, int UserData, IntPtr p1, int p2);

        #endregion

        #region Public event declarations

        /// <summary>
        /// Event that is raised when a new chunk of data has been extracted
        /// </summary>
        public event DataAvailableHandler DataAvailable;
        /// <summary>
        /// Event that is raised to indicate extraction progress
        /// </summary>
        public event ExtractionProgressHandler ExtractionProgress;
        /// <summary>
        /// Event that is raised when a required archive volume is missing
        /// </summary>
        public event MissingVolumeHandler MissingVolume;
        /// <summary>
        /// Event that is raised when a new file is encountered during processing
        /// </summary>
        public event NewFileHandler NewFile;
        /// <summary>
        /// Event that is raised when a new archive volume is opened for processing
        /// </summary>
        public event NewVolumeHandler NewVolume;
        /// <summary>
        /// Event that is raised when a password is required before continuing
        /// </summary>
        public event PasswordRequiredHandler PasswordRequired;

        #endregion

        #region Private fields

        private string archivePathName = string.Empty;
        private IntPtr archiveHandle = new IntPtr(0);
        private bool retrieveComment = true;
        private string password = string.Empty;
        private string comment = string.Empty;
        private ArchiveFlags archiveFlags = 0;
        private RARHeaderDataEx header = new RARHeaderDataEx();
        private string destinationPath = string.Empty;
        private RARFileInfo currentFile = null;
        private UNRARCallback callback = null;

        #endregion

        #region Object lifetime procedures

        public RAR()
        {
            callback = new UNRARCallback(RARCallback);
        }

        public RAR(string archivePathName) : this()
        {
            this.archivePathName = archivePathName;
        }

        ~RAR()
        {
            if (archiveHandle != IntPtr.Zero)
            {
                RARCloseArchive(archiveHandle);
                archiveHandle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            if (archiveHandle != IntPtr.Zero)
            {
                RARCloseArchive(archiveHandle);
                archiveHandle = IntPtr.Zero;
            }
        }

        #endregion

        #region Public Properties / Varaibles

        /// <summary>
        /// Path and name of RAR archive to open
        /// </summary>
        public string ArchivePathName
        {
            get
            {
                return archivePathName;
            }
            set
            {
                archivePathName = value;
            }
        }

        /// <summary>
        /// Archive comment 
        /// </summary>
        public string Comment
        {
            get
            {
                return comment;
            }
        }

        /// <summary>
        /// Current file being processed
        /// </summary>
        public RARFileInfo CurrentFile
        {
            get
            {
                return currentFile;
            }
        }

        /// <summary>
        /// Default destination path for extraction
        /// </summary>
        public string DestinationPath
        {
            get
            {
                return destinationPath;
            }
            set
            {
                destinationPath = value;
            }
        }

        /// <summary>
        /// Password for opening encrypted archive
        /// </summary>
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                if (archiveHandle != IntPtr.Zero)
                    RARSetPassword(archiveHandle, value);
            }
        }
        /// <summary>
        /// Archive data
        /// </summary>
        public RAROpenArchiveDataEx arcData;

        #endregion

        #region Public Methods
        /// <summary>
        /// Close the currently open archive
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            // Exit without exception if no archive is open
            if (archiveHandle == IntPtr.Zero)
                return;

            // Close archive
            int result = RARCloseArchive(archiveHandle);

            // Check result
            if (result != 0)
            {
                ProcessFileError(result);
            }
            else
            {
                archiveHandle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Opens archive specified by the ArchivePathName property for testing or extraction
        /// </summary>
        public void Open()
        {
            if (ArchivePathName.Length == 0)
                throw new IOException("Archive name has not been set.");
            Open(ArchivePathName, OpenMode.Extract);
        }

        /// <summary>
        /// Opens archive specified by the ArchivePathName property with a specified mode
        /// </summary>
        /// <param name="openMode">Mode in which archive should be opened</param>
        public void Open(OpenMode openMode)
        {
            if (ArchivePathName.Length == 0)
                throw new IOException("Archive name has not been set.");
            Open(ArchivePathName, openMode);
        }

        /// <summary>
        /// Opens specified archive using the specified mode.  
        /// </summary>
        /// <param name="archivePathName">Path of archive to open</param>
        /// <param name="openMode">Mode in which to open archive</param>
        public void Open(string archivePathName, OpenMode openMode)
        {
            IntPtr handle = IntPtr.Zero;

            // Close any previously open archives
            if (archiveHandle != IntPtr.Zero)
                Close();

            // Prepare extended open archive struct
            ArchivePathName = archivePathName;
            RAROpenArchiveDataEx openStruct = new RAROpenArchiveDataEx();
            openStruct.Initialize();
            openStruct.ArcName = this.archivePathName + "\0";
            openStruct.ArcNameW = this.archivePathName + "\0";
            openStruct.OpenMode = (uint)openMode;
            if (retrieveComment)
            {
                openStruct.CmtBuf = new string((char)0, 65536);
                openStruct.CmtBufSize = 65536;
            }
            else
            {
                openStruct.CmtBuf = null;
                openStruct.CmtBufSize = 0;
            }

            // Open archive
            handle = RAROpenArchiveEx(ref openStruct);
            arcData = openStruct;
            // Check for success
            if (openStruct.OpenResult != 0)
            {
                switch ((RarError)openStruct.OpenResult)
                {
                    case RarError.InsufficientMemory:
                        throw new OutOfMemoryException("Insufficient memory to perform operation.");

                    case RarError.BadData:
                        throw new IOException("Archive header broken");

                    case RarError.BadArchive:
                        throw new IOException("File is not a valid archive.");

                    case RarError.OpenError:
                        throw new IOException("File could not be opened.");
                }
            }

            // Save handle and flags
            archiveHandle = handle;
            archiveFlags = (ArchiveFlags)openStruct.Flags;

            // Set callback
            RARSetCallback(archiveHandle, callback, GetHashCode());

            // If comment retrieved, save it
            if (openStruct.CmtState == 1)
                comment = openStruct.CmtBuf.ToString();

            // If password supplied, set it
            if (password.Length != 0)
                RARSetPassword(archiveHandle, password);

            // Fire NewVolume event for first volume
            OnNewVolume(this.archivePathName);
        }

        /// <summary>
        /// Reads the next archive header and populates CurrentFile property data
        /// </summary>
        /// <returns></returns>
        public bool ReadHeader()
        {
            // Throw exception if archive not open
            if (archiveHandle == IntPtr.Zero)
                throw new IOException("Archive is not open.");

            // Initialize header struct
            header = new RARHeaderDataEx();
            header.Initialize();

            // Read next entry
            currentFile = null;
            int result = RARReadHeaderEx(archiveHandle, ref header);

            // Check for error or end of archive
            if ((RarError)result == RarError.EndOfArchive)
                return false;
            else if ((RarError)result == RarError.BadData)
                throw new IOException("Archive data is corrupt.");

            // Determine if new file
            //if (((header.Flags & 0x01) != 0) && currentFile != null)
            if ((header.Flags & 0x01) != 0)
                return true;
            else
            {
                // New file, prepare header
                currentFile = new RARFileInfo();
                currentFile.FileName = header.FileNameW.ToString();
                if ((header.Flags & 0x04) != 0)
                {
                    currentFile.encrypted = true;
                }
                if ((header.Flags & 0x02) != 0)
                    currentFile.ContinuedOnNext = true;
                if (header.PackSizeHigh != 0)
                    currentFile.PackedSize = header.PackSizeHigh * 0x100000000 + header.PackSize;
                else
                    currentFile.PackedSize = header.PackSize;
                if (header.UnpSizeHigh != 0)
                    currentFile.UnpackedSize = header.UnpSizeHigh * 0x100000000 + header.UnpSize;
                else
                    currentFile.UnpackedSize = header.UnpSize;
                try
                {
                    currentFile.HostOS = (int)header.HostOS;
                    currentFile.FileCRC = header.FileCRC;
                    currentFile.FileTime = FromMSDOSTime(header.FileTime);
                    currentFile.VersionToUnpack = (int)header.UnpVer;
                    currentFile.Method = (int)header.Method;
                    currentFile.FileAttributes = (int)header.FileAttr;
                    currentFile.BytesExtracted = 0;
                    if ((header.Flags & 0x20) == 0x20)
                        currentFile.IsDirectory = true;
                    if ((header.Flags & 0x04) != 0) currentFile.encrypted = true;
                    OnNewFile();
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog(e);
                    // Headers are encrypted.
                    // Close archive and try again.
                    return false;
                }
            }

            // Return success
            return true;
        }

        /// <summary>
        /// Returns array of file names contained in archive
        /// </summary>
        /// <returns></returns>
        public string[] ListFiles()
        {
            ArrayList fileNames = new ArrayList();
            while (ReadHeader())
            {
                if (!currentFile.IsDirectory)
                    fileNames.Add(currentFile.FileName);
                Skip();
            }
            string[] files = new string[fileNames.Count];
            fileNames.CopyTo(files);
            return files;
        }

        /// <summary>
        /// Moves the current archive position to the next available header
        /// </summary>
        /// <returns></returns>
        public void Skip()
        {
            int result = RARProcessFileW(archiveHandle, (int)Operation.Skip, string.Empty, string.Empty);

            // Check result
            if (result != 0)
            {
                ProcessFileError(result);
            }
        }

        /// <summary>
        /// Tests the ability to extract the current file without saving extracted data to disk
        /// </summary>
        /// <returns></returns>
        public void Test()
        {
            int result = RARProcessFileW(archiveHandle, (int)Operation.Test, string.Empty, string.Empty);

            // Check result
            if (result != 0)
            {
                ProcessFileError(result);
            }
        }

        /// <summary>
        /// Extracts the current file to the default destination path
        /// </summary>
        /// <returns></returns>
        public void Extract()
        {
            Extract(destinationPath, string.Empty);
        }

        /// <summary>
        /// Extracts the current file to a specified destination path and filename
        /// </summary>
        /// <param name="destinationName">Path and name of extracted file</param>
        /// <returns></returns>
        public void Extract(string destinationName)
        {
            Extract(string.Empty, destinationName);
        }

        /// <summary>
        /// Extracts the current file to a specified directory without renaming file
        /// </summary>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public void ExtractToDirectory(string destinationPath)
        {
            Extract(destinationPath, string.Empty);
        }

        #endregion

        #region Private Methods

        private void Extract(string destinationPath, string destinationName)
        {
            int result = RARProcessFileW(archiveHandle, (int)Operation.Extract, destinationPath, destinationName);

            // Check result
            if (result != 0)
            {
                ProcessFileError(result);
            }
        }

        private DateTime FromMSDOSTime(uint dosTime)
        {
            int day = 0;
            int month = 0;
            int year = 0;
            int second = 0;
            int hour = 0;
            int minute = 0;
            ushort hiWord;
            ushort loWord;
            hiWord = (ushort)((dosTime & 0xFFFF0000) >> 16);
            loWord = (ushort)(dosTime & 0xFFFF);
            year = ((hiWord & 0xFE00) >> 9) + 1980;
            month = (hiWord & 0x01E0) >> 5;
            day = hiWord & 0x1F;
            hour = (loWord & 0xF800) >> 11;
            minute = (loWord & 0x07E0) >> 5;
            second = (loWord & 0x1F) << 1;
            return new DateTime(year, month, day, hour, minute, second);
        }

        private void ProcessFileError(int result)
        {
            switch ((RarError)result)
            {
                case RarError.UnknownFormat:
                    throw new OutOfMemoryException("Unknown archive format.");

                case RarError.BadData:
                    throw new IOException("File CRC Error");

                case RarError.BadArchive:
                    throw new IOException("File is not a valid archive.");

                case RarError.OpenError:
                    throw new IOException("File could not be opened.");

                case RarError.CreateError:
                    throw new IOException("File could not be created.");

                case RarError.CloseError:
                    throw new IOException("File close error.");

                case RarError.ReadError:
                    throw new IOException("File read error.");

                case RarError.WriteError:
                    throw new IOException("File write error.");
                case RarError.MissingPassword:
                case RarError.BadPassword:
                    throw new IOException("Password was incorrect.");
                case RarError.InternalError:
                    throw new IOException("Reference error.");

            }
        }

        private int RARCallback(uint msg, int UserData, IntPtr p1, int p2)
        {
            string volume = string.Empty;
            string newVolume = string.Empty;
            int result = -1;

            switch ((CallbackMessages)msg)
            {
                case CallbackMessages.VolumeChangeAgain:
                    volume = Marshal.PtrToStringUni(p1); // Volume it was expecting.
                    if ((VolumeMessage)p2 == VolumeMessage.Notify)
                        result = OnNewVolume(volume);
                    else if ((VolumeMessage)p2 == VolumeMessage.Ask)
                    {
                        newVolume = OnMissingVolume(volume);
                        if (newVolume.Length == 0)
                            result = -1;
                        else
                        {
                            if (newVolume != volume)
                            {
                                // Encode to Unicode.
                                var lilEndian = System.Text.Encoding.Unicode;
                                var bytes = lilEndian.GetBytes(newVolume + "\0");
                                for (int i = 0; i < bytes.Length; i++)
                                {
                                    Marshal.WriteByte(p1, i, bytes[i]);
                                }
                            }
                            result = 1;
                        }
                    }
                    break;

                case CallbackMessages.ProcessData:
                    result = OnDataAvailable(p1, p2);
                    break;
                case CallbackMessages.NeedPassword:
                    result = OnPasswordRequired(p1, p2);
                    break;
                case CallbackMessages.NeedPasswordAgain:
                    result = OnPasswordRequiredLilE(p1, p2);
                    break;
            }
            return result;
        }

        #endregion

        #region Protected Virtual (Overridable) Methods

        protected virtual void OnNewFile()
        {
            if (NewFile != null)
            {
                NewFileEventArgs e = new NewFileEventArgs(currentFile);
                NewFile(this, e);
            }
        }

        protected virtual int OnPasswordRequired(IntPtr p1, int p2)
        {
            int result = -1;
            if (PasswordRequired != null)
            {
                PasswordRequiredEventArgs e = new PasswordRequiredEventArgs();
                PasswordRequired(this, e);
                if (e.ContinueOperation && e.Password.Length > 0)
                {
                    for (int i = 0; i < e.Password.Length && i < p2; i++)
                        Marshal.WriteByte(p1, i, (byte)e.Password[i]);
                    Marshal.WriteByte(p1, e.Password.Length, 0);
                    result = 1;
                }
            }
            else
            {
                throw new IOException("Password is required for extraction.");
            }
            return result;
        }
        protected virtual int OnPasswordRequiredLilE(IntPtr p1, int p2)
        {
            int result = -1;
            if (PasswordRequired != null)
            {
                PasswordRequiredEventArgs e = new PasswordRequiredEventArgs();
                PasswordRequired(this, e);
                if (e.ContinueOperation && e.Password.Length > 0)
                {
                    var lilEndian = System.Text.Encoding.Unicode;
                    var bytes = lilEndian.GetBytes(e.Password + "\0");

                    for (int i = 0; i < bytes.Length && i < p2; i++) Marshal.WriteByte(p1, i, bytes[i]);
                    //Marshal.WriteByte(p1, e.Password.Length, (byte)18);
                    result = 1;
                }
            }
            else
            {
                throw new IOException("Password is required for extraction.");
            }
            return result;
        }

        protected virtual int OnDataAvailable(IntPtr p1, int p2)
        {
            int result = 1;
            if (currentFile != null)
                currentFile.BytesExtracted += p2;
            if (DataAvailable != null)
            {
                byte[] data = new byte[p2];
                Marshal.Copy(p1, data, 0, p2);
                DataAvailableEventArgs e = new DataAvailableEventArgs(data);
                DataAvailable(this, e);
                if (!e.ContinueOperation)
                    result = -1;
            }
            if (ExtractionProgress != null && currentFile != null)
            {
                ExtractionProgressEventArgs e = new ExtractionProgressEventArgs();
                e.FileName = currentFile.FileName;
                e.FileSize = currentFile.UnpackedSize;
                e.BytesExtracted = currentFile.BytesExtracted;
                e.PercentComplete = currentFile.PercentComplete;
                ExtractionProgress(this, e);
                if (!e.ContinueOperation)
                    result = -1;
            }
            return result;
        }

        protected virtual int OnNewVolume(string volume)
        {
            int result = 1;
            if (NewVolume != null)
            {
                NewVolumeEventArgs e = new NewVolumeEventArgs(volume);
                NewVolume(this, e);
                if (!e.ContinueOperation)
                    result = -1;
            }
            return result;
        }

        protected virtual string OnMissingVolume(string volume)
        {
            string result = string.Empty;
            if (MissingVolume != null)
            {
                MissingVolumeEventArgs e = new MissingVolumeEventArgs(volume);
                MissingVolume(this, e);
                if (e.ContinueOperation)
                    result = e.VolumeName;
            }
            return result;
        }

        #endregion
    }

    #region Event Argument Classes

    public class NewVolumeEventArgs
    {
        public string VolumeName;
        public bool ContinueOperation = true;

        public NewVolumeEventArgs(string volumeName)
        {
            VolumeName = volumeName;
        }
    }

    public class MissingVolumeEventArgs
    {
        public string VolumeName;
        public bool ContinueOperation = false;

        public MissingVolumeEventArgs(string volumeName)
        {
            VolumeName = volumeName;
        }
    }

    public class DataAvailableEventArgs
    {
        public readonly byte[] Data;
        public bool ContinueOperation = true;

        public DataAvailableEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    public class PasswordRequiredEventArgs
    {
        public string Password = string.Empty;
        public bool ContinueOperation = true;
    }

    public class NewFileEventArgs
    {
        public RARFileInfo fileInfo;
        public NewFileEventArgs(RARFileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }
    }

    public class ExtractionProgressEventArgs
    {
        public string FileName;
        public long FileSize;
        public long BytesExtracted;
        public double PercentComplete;
        public bool ContinueOperation = true;
    }

    public class RARFileInfo
    {
        public string FileName;
        public bool ContinuedFromPrevious = false;
        public bool ContinuedOnNext = false;
        public bool IsDirectory = false;
        public long PackedSize = 0;
        public long UnpackedSize = 0;
        public int HostOS = 0;
        public long FileCRC = 0;
        public DateTime FileTime;
        public int VersionToUnpack = 0;
        public int Method = 0;
        public int FileAttributes = 0;
        public long BytesExtracted = 0;
        public bool encrypted;

        public double PercentComplete
        {
            get
            {
                if (UnpackedSize != 0)
                    return BytesExtracted / (double)UnpackedSize * (double)100.0;
                else
                    return 0;
            }
        }

    }

    #endregion
}
