using System.Diagnostics.CodeAnalysis;
using DAZ_Installer.IO.Wrappers;
namespace DAZ_Installer.IO
{
    public class DPFileInfo : DPIONodeBase, IDPFileInfo
    {
        public override AbstractFileSystem FileSystem
        {
            get => fileSystem;
            internal set
            {
                ArgumentNullException.ThrowIfNull(value);
                fileSystem = value;
                Invalidate();
            }
        }
        
        public IDPFileScopeSettings Scope => FileSystem.Scope;
        public override FileAttributes Attributes { get => fileInfo.Attributes; set => fileInfo.Attributes = value; }

        public IDPDirectoryInfo? Directory => directory ??= tryCreateDirectoryInfo(fileInfo, FileSystem);

        public override string Name => fileInfo.Name;
        public override string Path => fileInfo.FullName;
        public override bool Exists => fileInfo.Exists;
        public override bool Whitelisted => whitelisted;
        protected IFileInfo fileInfo;
        protected AbstractFileSystem fileSystem;
        protected IDPDirectoryInfo? directory;
        protected bool whitelisted;


        internal DPFileInfo(FileInfo fileInfo, AbstractFileSystem fs) : this(fileInfo, fs, tryCreateDirectoryInfo(fileInfo, fs)) { }
        internal DPFileInfo(IFileInfo fileInfo, AbstractFileSystem fs) : this(fileInfo, fs, tryCreateDirectoryInfo(fileInfo, fs)) { }

        internal DPFileInfo(string path, AbstractFileSystem fs) : this(new FileInfo(path), fs) { }
        internal DPFileInfo(FileInfo info, AbstractFileSystem fs, IDPDirectoryInfo? directory) : 
            this(new FileInfoWrapper(info), fs, directory) { }
        /// <summary>
        /// Constructor used for testing.
        /// </summary>
        /// <param name="info">The file info to use.</param>
        /// <param name="fs">The file system to use.</param>
        /// <param name="directory">The parent directory of this file.</param>
        internal DPFileInfo(IFileInfo info, AbstractFileSystem fs, IDPDirectoryInfo? directory)
        {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(fs);
            fileSystem = fs;
            whitelisted = Scope.IsFilePathWhitelisted(info.FullName);
            fileInfo = info;
            this.directory = directory;
        }

        public IDPFileInfo CopyTo(string path, bool overwrite)
        {
            throwIfNotWhitelisted(path);
            return new DPFileInfo(fileInfo.CopyTo(path, overwrite), FileSystem);
        }
        public Stream Create() {
            throwIfNotWhitelisted();
            return fileInfo.Create();
        }
        /// <inheritdoc cref="FileInfo.MoveTo(string, bool)"/>
        public void MoveTo(string path, bool overwrite)
        {
            throwIfNotWhitelisted();
            throwIfNotWhitelisted(path);
            fileInfo.MoveTo(path, overwrite);
        }
        /// <inheritdoc cref="FileInfo.OpenRead()"/>
        public Stream OpenRead() => Open(FileMode.Open, FileAccess.Read);
        /// <inheritdoc cref="FileInfo.OpenWrite()"/>
        public Stream OpenWrite() => Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
        /// <inheritdoc cref="FileInfo.Open(FileMode, FileAccess)"/>
        public Stream Open(FileMode mode, FileAccess access)
        {
            if (mode != FileMode.Open || access != FileAccess.Read) throwIfNotWhitelisted();
            return fileInfo.Open(mode, access);
        }
        /// <inheritdoc cref="FileInfo.Delete()"/>
        public void Delete()
        {
            throwIfNotWhitelisted();
            fileInfo.Delete();
        }

        public override string ToString() => "DPFileInfo: " + Path;

        #region Private methods

        private void throwIfNotWhitelisted()
        {
            if (!Whitelisted) throw new OutOfScopeException(Path);
        }

        private void throwIfNotWhitelisted(string path)
        {
            if (!Scope.IsFilePathWhitelisted(path)) throw new OutOfScopeException(path);
        }

        internal override void Invalidate()
        {
            whitelisted = Scope.IsFilePathWhitelisted(Path);
        }
        private static IDPDirectoryInfo? tryCreateDirectoryInfo(FileInfo info, AbstractFileSystem fs) => 
            info.Directory is null ? null : new DPDirectoryInfo(info.Directory, fs);
        private static IDPDirectoryInfo? tryCreateDirectoryInfo(IFileInfo info, AbstractFileSystem fs) => 
            info.Directory is null ? null : new DPDirectoryInfo(info.Directory, fs);

        #endregion
        // The Preview methods are to check whether the operation is possible (ie: are we blacklisted or not?).
        #region Preview methods
        public bool PreviewCreate() => Whitelisted;
        public bool PreviewDelete() => Whitelisted;
        public bool PreviewOpen(FileMode mode, FileAccess access) => (mode == FileMode.Open && access == FileAccess.Read) || Whitelisted;
        public bool PreviewMoveTo(string path, bool overwrite) => Whitelisted && Scope.IsFilePathWhitelisted(path);
        public bool PreviewCopyTo(string path, bool overwrite) => Scope.IsFilePathWhitelisted(path);
        #endregion

        // The Try methods are to perform the operation, and return whether it was successful or not. No errors are thrown.
        #region Try methods
        public bool TryCreate([NotNullWhen(true)] out Stream? stream) 
        {
            stream = null;
            if (!Whitelisted) return false;
            try
            {
                stream = Create();
                return true;
            } catch { }
            return false;
        }
        public bool TryDelete()
        {
            if (!Whitelisted) return false;
            try
            {
                Delete();
                return true;
            } catch { return false; }
        }
        public bool TryMoveTo(string path, bool overwrite)
        {
            if (!Whitelisted && !Scope.IsFilePathWhitelisted(path)) return false;
            try
            {
                MoveTo(path, overwrite);
                return true;
            } catch { return false; }
        }

        public bool TryCopyTo(string path, bool overwrite, [NotNullWhen(true)] out IDPFileInfo? info)
        {
            info = null;
            if (!Scope.IsFilePathWhitelisted(path)) return false;
            try
            {
                info = CopyTo(path, overwrite);
                return true;
            } catch { return false; }
        }

        public bool TryOpenRead([NotNullWhen(true)] out Stream? stream) => TryOpen(FileMode.Open, FileAccess.Read, out stream);
        public bool TryOpenWrite([NotNullWhen(true)] out Stream? stream) => TryOpen(FileMode.OpenOrCreate, FileAccess.ReadWrite, out stream);
        public bool TryOpen(FileMode mode, FileAccess access, [NotNullWhen(true)] out Stream? stream)
        {
            stream = null;
            if (!Whitelisted) return false;
            try
            {
                stream = Open(mode, access);
                return true;
            } catch { return false; }
        }

        #endregion
        
        // The TryAndFix methods are to help fix common problems such as attemtping to fix an error due to an UnauthorizedAccessException
        // which is caused by a hidden or read-only attribute.
        #region TryAndFix methods
        /// <summary>
        /// Attempts to move the file, and if an exception is thrown (aside from <see cref="OutOfScopeException"/>), it will attempt to fix the problem.
        /// If it couldn't, it will be returned.
        /// </summary>
        /// <returns> Whether the operation successfully executed (whitelisted and no exception after attempted recovery).</returns>
        public bool TryAndFixMoveTo(string path, bool overwrite, out Exception? exception)
        {
            exception = null;
            if (!Whitelisted || !fileInfo.Exists) return false;
            var targetInfo = FileSystem.CreateFileInfo(path);
            if (targetInfo.Exists && (targetInfo.Attributes.HasFlag(FileAttributes.ReadOnly) || targetInfo.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                try
                {
                    targetInfo.Attributes = FileAttributes.Normal;
                } catch (Exception ex)
                {
                    exception = ex;
                    return false;
                }
            }
            if (attemptWithRecovery(() => MoveTo(path, overwrite), this, out exception)) return true;
            return false;
        }
        /// <summary>
        /// Attempts to copy the file, and if an exception is thrown (aside from <see cref="OutOfScopeException"/>), it will attempt to fix the problem.
        /// If it couldn't, it will be returned.
        /// </summary>
        /// <returns> Whether the operation successfully executed (whitelisted and no exception after attempted recovery).</returns>
        public bool TryAndFixCopyTo(string path, bool overwrite, out IDPFileInfo? info, out Exception? exception)
        {
            exception = null;
            info = null;
            if (!Whitelisted || !fileInfo.Exists) return false;
            var targetInfo = FileSystem.CreateFileInfo(path);
            if (targetInfo.Exists && (targetInfo.Attributes.HasFlag(FileAttributes.ReadOnly) || targetInfo.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                try
                {
                    targetInfo.Attributes = FileAttributes.Normal;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return false;
                }
            }
            IDPFileInfo? i = null;
            if (!attemptWithRecovery(() => i = CopyTo(path, overwrite), this, out exception)) return false;
            info = i;
            return true;
        }
        /// <summary>
        /// Attempts to open the file, and if an exception is thrown (aside from <see cref="OutOfScopeException"/>), it will attempt to fix the problem.
        /// If it couldn't, it will return the exception that followed.
        /// </summary>
        /// <returns> Whether the operation successfully executed (whitelisted and no exception after attempted recovery).</returns>
        public bool TryAndFixOpen(FileMode mode, FileAccess access,[NotNullWhen(true)] out Stream? stream, [NotNullWhen(false)] out Exception? exception)
        {
            (exception, stream) = (null, null);
            if (!Whitelisted) return false;
            Stream? s = null;
            if (attemptWithRecovery(() => s = Open(mode, access), this, out exception))
            {
                stream = s;
                return true;
            }
            return false;
        }
        /// <inheritdoc cref="TryAndFixOpen(FileMode, FileAccess, out Stream?, out Exception?)"/>
        public bool TryAndFixOpenRead([NotNullWhen(true)] out Stream? stream, [NotNullWhen(false)] out Exception? exception) => TryAndFixOpen(FileMode.Open, FileAccess.Read, out stream, out exception);
        /// <inheritdoc cref="TryAndFixOpen(FileMode, FileAccess, out Stream?, out Exception?)"/>
        public bool TryAndFixOpenWrite([NotNullWhen(true)] out Stream? stream, [NotNullWhen(false)] out Exception? exception) => TryAndFixOpen(FileMode.OpenOrCreate, FileAccess.ReadWrite, out stream, out exception);
        /// <summary>
        /// Attempts to delete the file, and if an exception is thrown (aside from <see cref="OutOfScopeException"/>), it will attempt to fix the problem.
        /// If it couldn't, it will return the exception that followed.
        /// </summary>
        /// <returns> Whether the operation successfully executed (whitelisted and no exception after attempted recovery).</returns>
        public bool TryAndFixDelete([NotNullWhen(false)] out Exception? exception)
        {
            exception = null;
            if (!Whitelisted || !fileInfo.Exists) return false;
            if (attemptWithRecovery(Delete, this, out exception)) return true;
            return false;
        }
        #endregion

        private static bool attemptWithRecovery(Action a, IDPFileInfo info, [NotNullWhen(false)] out Exception? ex)
        {
            ex = null;
            try
            {
                if (!info.Exists) return false;
                a();
                return true;
            }
            catch (UnauthorizedAccessException e)
            {
                try
                {
                    info.Attributes = FileAttributes.Normal;
                } catch (Exception e2)
                {
                    ex = new AggregateException(e, e2);
                    return false;
                }
                // Try again.
                try
                {
                    a(); return true;
                }
                catch (Exception e2)
                {
                    ex = e2;
                    return false;
                }
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }
    }
}
