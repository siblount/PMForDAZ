namespace DAZ_Installer.IO.Wrappers
{
    /// <summary>
    /// A <see cref="FileInfo"/> wrapper that implements <see cref="IFileInfo"/>.
    /// </summary>
    public class FileInfoWrapper : IFileInfo
    {
        private readonly FileInfo info;
        protected IDirectoryInfo? directory;

        internal FileInfoWrapper(FileInfo info) => this.info = info;

        /// <inheritdoc cref="FileInfo.Name"/>
        public string Name => info.Name;
        /// <inheritdoc cref="FileInfo.Path"/>
        public string FullName => info.FullName;
        /// <inheritdoc cref="FileInfo.Exists"/>
        public bool Exists => info.Exists;
        /// <inheritdoc cref="FileInfo.Directory"/>
        public IDirectoryInfo? Directory => directory ??= tryCreateDirectoryWrapper();
        /// <inheritdoc cref="FileInfo.DirectoryName"/>
        public string? DirectoryName => info.DirectoryName;
        /// <inheritdoc cref="FileInfo.Attributes"/>
        public FileAttributes Attributes { get => info.Attributes; set => info.Attributes = value; }
        /// <inheritdoc cref="FileInfo.Create()"/>
        public Stream Create() => info.Create();
        /// <inheritdoc cref="FileInfo.Delete()"/>
        public void Delete() => info.Delete();
        /// <inheritdoc cref="FileInfo.MoveTo(string, bool)"/>
        public void MoveTo(string path, bool overwrite) => info.MoveTo(path, overwrite);
        /// <inheritdoc cref="FileInfo.CopyTo(string, bool)"/>
        public IFileInfo CopyTo(string path, bool overwrite) => new FileInfoWrapper(info.CopyTo(path, overwrite));
        /// <inheritdoc cref="FileInfo.Open(FileMode, FileAccess)"/>
        public Stream Open(FileMode mode, FileAccess access) => info.Open(mode, access);

        public static implicit operator FileInfoWrapper(FileInfo info) => new FileInfoWrapper(info);

        private IDirectoryInfo? tryCreateDirectoryWrapper() => info.Directory is null ? null : new DirectoryInfoWrapper(info.Directory);
    }
}
