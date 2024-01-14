using DAZ_Installer.IO;

namespace DAZ_Installer.IO.Fakes
{
    /// <summary>
    /// A default fake file info that can be used for testing. All methods and properties are virtual. Never use this code for production.
    /// </summary>
    [Obsolete("This class is only for testing purposes. Do not use in production code.")]
    public class FakeDirectoryInfo : IDirectoryInfo
    {
        /// <summary>
        /// The name of this directory. Equivalent to <see cref="Path.GetFileName(string)"/>.
        /// </summary>
        public virtual string Name => Path.GetFileName(FullName);
        /// <summary>
        /// The full path of the directory. No modifications are made, it is strictly the path provided to the constructor.
        /// </summary>
        public virtual string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Whether the directory exists or not. Defaults to true.
        /// </summary>
        public virtual bool Exists { get; set; } = true;
        /// <summary>
        /// The attributes of the directory.
        /// </summary>
        public virtual FileAttributes Attributes { get; set; } = FileAttributes.Normal;
        /// <summary>
        /// The parent directory of this directory. Defaults to null.
        /// </summary>
        public virtual IDirectoryInfo? Parent { get; set; } = null;
        /// <summary>
        /// Does nothing.
        /// </summary>
        public virtual void Create() { }
        /// <summary>
        /// Sets <see cref="Exists"/> to false.
        /// </summary>
        public virtual void Delete(bool recursive) => Exists = false;
        /// <summary>
        /// Returns <see cref="IDirectoryInfo"/>s from <see cref="Directories"/> (default is empty)."/>
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IDirectoryInfo> EnumerateDirectories() => Directories;
        /// <inheritdoc cref="EnumerateDirectories()"/>
        public virtual IEnumerable<IDirectoryInfo> EnumerateDirectories(string _, EnumerationOptions __) => Directories;
        /// <summary>
        /// Returns <see cref="IFileInfo"/>s from <see cref="Files"/> (default is empty)."/>
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IFileInfo> EnumerateFiles() => Files;
        /// <inheritdoc cref="EnumerateFiles()"/>
        public virtual IEnumerable<IFileInfo> EnumerateFiles(string _, EnumerationOptions __) => Files;
        /// <summary>
        /// Directories of this directory; default is empty.
        /// </summary>
        public virtual IEnumerable<IDirectoryInfo> Directories { get; set; } = Enumerable.Empty<IDirectoryInfo>();
        /// <summary>
        /// Files of this directory; default is empty.
        /// </summary>
        public virtual IEnumerable<IFileInfo> Files { get; set; } = Enumerable.Empty<IFileInfo>();
        /// <summary>
        /// Sets <see cref="FullName"/> to the provided path.
        /// </summary>
        public virtual void MoveTo(string path) => FullName = path;

        public FakeDirectoryInfo(string path) => FullName = path;
        public FakeDirectoryInfo(string path, IDirectoryInfo? parent) : this(path) => Parent = parent;
    }
}
