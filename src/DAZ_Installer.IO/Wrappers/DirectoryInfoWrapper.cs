namespace DAZ_Installer.IO.Wrappers
{
    /// <summary>
    /// A <see cref="DirectoryInfo"/> wrapper that implements <see cref="IDirectoryInfo"/>.
    /// </summary>
    public class DirectoryInfoWrapper : IDirectoryInfo
    {
        private readonly DirectoryInfo info;
        private IDirectoryInfo? parent;

        internal DirectoryInfoWrapper(DirectoryInfo info) => this.info = info;
        /// <inheritdoc cref="DirectoryInfo.Name"/>
        public string Name => info.Name;
        /// <inheritdoc cref="DirectoryInfo.FullName"/>
        public string FullName => info.FullName;
        /// <inheritdoc cref="DirectoryInfo.Exists"/>
        public bool Exists => info.Exists;
        /// <inheritdoc cref="FileSystemInfo.Attributes"/>
        public FileAttributes Attributes { get => info.Attributes; set => info.Attributes = value; }
        /// <inheritdoc cref="DirectoryInfo.Parent"/>
        public IDirectoryInfo? Parent => parent ??= tryCreateDirectoryInfoWrapper();
        /// <inheritdoc cref="DirectoryInfo.Create()"/>
        public void Create() => info.Create();
        /// <inheritdoc cref="DirectoryInfo.Delete(bool)"/>
        public void Delete(bool recursive) => info.Delete(recursive);
        /// <inheritdoc cref="DirectoryInfo.EnumerateDirectories()"/>
        public IEnumerable<IDirectoryInfo> EnumerateDirectories() => info.EnumerateDirectories().Select(d => new DirectoryInfoWrapper(d));
        /// <inheritdoc cref="DirectoryInfo.EnumerateDirectories(string, EnumerationOptions)"/>
        public IEnumerable<IDirectoryInfo> EnumerateDirectories(string pattern, EnumerationOptions options) => info.EnumerateDirectories(pattern, options).Select(d => new DirectoryInfoWrapper(d));
        /// <inheritdoc cref="DirectoryInfo.EnumerateFiles()"/>
        public IEnumerable<IFileInfo> EnumerateFiles() => info.EnumerateFiles().Select(f => new FileInfoWrapper(f));
        /// <inheritdoc cref="DirectoryInfo.EnumerateFiles(string, EnumerationOptions)"/>
        public IEnumerable<IFileInfo> EnumerateFiles(string pattern, EnumerationOptions options) => info.EnumerateFiles(pattern, options).Select(f => new FileInfoWrapper(f));
        /// <inheritdoc cref="DirectoryInfo.MoveTo(string)"/>
        public void MoveTo(string path) => info.MoveTo(path);

        public static implicit operator DirectoryInfoWrapper(DirectoryInfo info) => new DirectoryInfoWrapper(info);
        private IDirectoryInfo? tryCreateDirectoryInfoWrapper() => info.Parent is null ? null : new DirectoryInfoWrapper(info.Parent);
    }
}
