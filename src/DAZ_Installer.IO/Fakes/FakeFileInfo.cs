using DAZ_Installer.IO;

namespace DAZ_Installer.IO.Fakes
{
    /// <summary>
    /// A default fake file info that can be used for testing. Never use this code for production.
    /// </summary>
    [Obsolete("This class is only for testing purposes. Do not use in production code.")]
    public class FakeFileInfo : IFileInfo
    {
        /// <summary>
        /// The file name of this file. Equivalent to <see cref="Path.GetFileName(string)"/>.
        /// </summary>
        public virtual string Name => Path.GetFileName(FullName);
        /// <summary>
        /// The full path of the file. No modifications are made, it is strictly the path provided to the constructor.
        /// </summary>
        public virtual string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Whether the file exists or not. Defaults to true.
        /// </summary>
        public virtual bool Exists { get; set; } = true;
        /// <summary>
        /// The parent directory of this file. Defaults to null.
        /// </summary>
        public virtual IDirectoryInfo? Directory { get; set; } = null;
        /// <summary>
        /// The full path of the parent directory of this file. Equivalent to <see cref="IDirectoryInfo.FullName"/>.
        /// </summary>
        public virtual string? DirectoryName => Directory?.FullName ?? null;
        /// <summary>
        /// The attributes of this file. Defaults to <see cref="FileAttributes.Normal"/>.
        /// </summary>
        public virtual FileAttributes Attributes { get; set; } = FileAttributes.Normal;
        /// <summary>
        /// CopyTo accepts any path and returns a new <see cref="FakeFileInfo"/> with the path provided.
        /// </summary>
        /// <returns>A new <see cref="FakeFileInfo"/>.</returns>
        public virtual IFileInfo CopyTo(string path, bool overwrite) => new FakeFileInfo(path);
        /// <summary>
        /// Accepts all parameters and returns <see cref="Stream.Null"/>.
        /// </summary>
        /// <returns><see cref="Stream.Null"/></returns>
        public virtual Stream Create() => Stream.Null;
        /// <summary>
        /// Sets <see cref="Exists"/> to false.
        /// </summary>
        public virtual void Delete() => Exists = false;
        /// <summary>
        /// Sets <see cref="FullName"/> to the provided path.
        /// </summary>
        public virtual void MoveTo(string path, bool overwrite) => FullName = path;
        /// <summary>
        /// Accepts all parameters and returns <see cref="Stream.Null"/>.
        /// </summary>
        /// <returns><see cref="Stream.Null"/></returns>
        public virtual Stream Open(FileMode mode, FileAccess access) => Stream.Null;

        public FakeFileInfo(string path) => FullName = path;
        public FakeFileInfo(string path, IDirectoryInfo? directory) : this(path) => Directory = directory;
    }
}
