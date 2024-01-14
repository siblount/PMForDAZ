using DAZ_Installer.External;

namespace DAZ_Installer.Core.Extraction.Fakes
{
    internal class FakeRAR : IRAR
    {
        public IEnumerator<RARFileInfo> FilesEnumerable;
        public virtual bool Disposed { get; set; } = false;
        public virtual bool Closed { get; set; } = true;
        /// <summary>
        /// ActionCalled is a variable used to determine if after a call to <see cref="ReadHeader"/>, 
        /// that either <see cref="Skip"/>, <see cref="Extract(string)"/>, or <see cref="Test"/> was called.
        /// </summary>
        public virtual bool ActionCalled { get; set; } = true;
        public virtual RAR.OpenMode Mode { get; set; } = RAR.OpenMode.List;

        public virtual event RAR.MissingVolumeHandler? MissingVolume;
        public virtual event RAR.NewFileHandler? NewFile;
        public virtual event RAR.PasswordRequiredHandler? PasswordRequired;
        public virtual event RAR.ExtractionProgressHandler? ExtractionProgress;

        internal FakeRAR(IEnumerable<RARFileInfo> files) => FilesEnumerable = files.GetEnumerator();

        public virtual RARFileInfo CurrentFile => FilesEnumerable.Current;

        /// <summary>
        /// Returns the value at <see cref="ArchiveDataToReturn"/>.
        /// </summary>
        public RAR.RAROpenArchiveDataEx ArchiveData => ArchiveDataToReturn;
        public RAR.RAROpenArchiveDataEx ArchiveDataToReturn = new()
        {
            ArcName = "test.rar",
            ArcNameW = "test.rar",
            OpenMode = (uint)RAR.OpenMode.List,
            Flags = (uint)(RAR.ArchiveFlags.FirstVolume | RAR.ArchiveFlags.Volume),
        };

        public virtual string DestinationPath { get; set; } = string.Empty;

        /// <summary>
        /// Sets the <see cref="Closed"/> flag to true.
        /// </summary>
        public virtual void Close() => Closed = true;
        /// <summary>
        /// Sets the <see cref="Disposed"/> flag to true.
        /// </summary>
        public virtual void Dispose() => Disposed = true;
        /// <summary>
        /// Changes destination path and throws if <see cref="Disposed"/> or <see cref="Closed"/> is true.
        /// </summary>
        /// <param name="destinationName">The destination to extract to.</param>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual void Extract(string destinationName)
        {
            _ = throwIfDisposed() && throwIfClosed();
            if (Mode != RAR.OpenMode.Extract) throw new InvalidOperationException("Archive is not open for extraction.");
            if (CurrentFile is null) throw new InvalidOperationException("No file is selected.");
            if (CurrentFile.encrypted) throw new IOException("File could not be opened."); // do not change this err message or type.
            DestinationPath = Path.GetDirectoryName(destinationName) ?? string.Empty;
            ActionCalled = true;
        }
        /// <summary>
        /// Resets the enumerator and sets the <see cref="Closed"/> flag to false. Throws if <see cref="Disposed"/> is true or <see cref="Closed"/> is false
        /// </summary>
        /// <param name="mode">The mode to use</param>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual void Open(RAR.OpenMode mode)
        {
            throwIfDisposed();
            if (!Closed) throw new InvalidOperationException("Archive is already open.");
            Closed = false;
            ActionCalled = true;
            Mode = mode;
            FilesEnumerable.Reset();
        }
        /// <summary>
        /// Moves the enumerator to the next element in the archive.
        /// </summary>
        /// <returns>Whether there are any more elements in the archive. 
        /// <see langword="true"/> if it is. Otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual bool ReadHeader()
        {
            var a = throwIfDisposed() || throwIfClosed() || throwIfActionNotCalled() || FilesEnumerable.MoveNext();
            if (!a) return a;
            if (Mode == RAR.OpenMode.List) 
                NewFile?.Invoke(this, new NewFileEventArgs(FilesEnumerable.Current));
            ActionCalled = false;
            return true;

        }
        /// <summary>
        /// Checks if disposed or closed. If not, throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual void Skip()
        {
            _ = throwIfDisposed() || throwIfClosed();
            ActionCalled = true;
        }
        /// <summary>
        /// Throws an exception if <see cref="CurrentFile"/> is null or <see cref="Disposed"/> or <see cref="Closed"/> is true.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual void Test()
        {
            _ = throwIfDisposed() && throwIfClosed();
            ArgumentNullException.ThrowIfNull(FilesEnumerable.Current);
            ActionCalled = true;
        }

        /// <summary>
        /// Throws an exception if <see cref="Disposed"/> is true. Always returns false.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        private bool throwIfDisposed() => Disposed ? throw new ObjectDisposedException(nameof(FakeRAR)) : false;
        /// <summary>
        /// Throws an exception if <see cref="Closed"/> is true. Always returns false.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        private bool throwIfClosed() => Closed ? throw new InvalidOperationException("Archive is closed.") : false;

        /// <summary>
        /// Throws an exception if <see cref="ActionCalled"/> is false and the current mode is set to <see cref="RAR.OpenMode.Extract"/>. Always returns false.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        private bool throwIfActionNotCalled() => ActionCalled || Mode != RAR.OpenMode.Extract ? false : 
                                                                                                throw new InvalidOperationException("Archive is corrupt.");

        internal static RARFileInfo CreateFileInfoForEntity(string path) => new()
        {
            UnpackedSize = 1,
            FileName = path,
            IsDirectory = string.IsNullOrEmpty(Path.GetFileName(path))
        };
    }
}
