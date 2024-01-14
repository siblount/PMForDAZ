namespace DAZ_Installer.IO
{
    /// <summary>
    /// An abstract class representing a file system. It is a factory class as well, creating <see cref="IDPFileInfo"/>, <see cref="IDPDirectoryInfo"/>, and <see cref="IDPDriveInfo"/> objects.
    /// </summary>
    public abstract class AbstractFileSystem
    {
        /// <summary>
        /// The file scope to use for all IO nodes.
        /// </summary>
        public virtual DPFileScopeSettings Scope { get; set; } = DPFileScopeSettings.None;

        /// <inheritdoc cref="File.Delete(string)"/>
        public abstract void DeleteFile(string path);
        /// <inheritdoc cref="Directory.Delete(string, bool)"/>
        public abstract void DeleteDirectory(string path, bool recursive = false);
        /// <param name="path">The file or directory to check.</param>
        /// <param name="treatAsDirectory">If true, forcefully treats the path as a directory and checks if it exists.</param>
        /// <inheritdoc cref="File.Exists(string?)"/>
        public abstract bool Exists(string? path, bool treatAsDirectory = false);
        /// <inheritdoc cref="Directory.EnumerateDirectories(string)"/>
        public abstract IEnumerable<IDPDirectoryInfo> EnumerateDirectories(string path);
        /// <inheritdoc cref="Directory.EnumerateFiles(string)"/>
        public abstract IEnumerable<IDPFileInfo> EnumerateFiles(string path);
        /// <inheritdoc cref="DriveInfo.GetDrives"/>
        public abstract IDPDriveInfo[] GetDrives();

        public abstract IDPFileInfo CreateFileInfo(string path);
        /// <summary>
        /// Creates a new <see cref="IDPFileInfo"/> instance.
        /// </summary>
        /// <param name="path">The path of the file to assign to the file info.</param>
        /// <param name="directory">The directory of the file, if any. <paramref name="directory"/> must have it's FileSystem set to this object.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="directory"/> is not null and it's FileSystem is not this object.</exception>"
        internal protected abstract IDPFileInfo CreateFileInfo(string path, IDPDirectoryInfo? directory = null);
        /// <summary>
        /// Creates a new <see cref="IDPDirectoryInfo"/> instance.
        /// </summary>
        /// <param name="path">The path of the directory to assign to the directory info.</param>
        public abstract IDPDirectoryInfo CreateDirectoryInfo(string path);
        /// <summary>
        /// Creates a new <see cref="IDPDirectoryInfo"/> instance.
        /// </summary>
        /// <param name="path">The path of the directory to assign to the directory info.</param>
        /// <param name="parent">The directory of this directory, if any. <paramref name="parent"/> must have it's FileSystem set to this object.</param>
        internal protected abstract IDPDirectoryInfo CreateDirectoryInfo(string path, IDPDirectoryInfo? parent);
        /// <summary>
        /// Creates a new <see cref="IDPDriveInfo"/> instance.
        /// </summary>
        /// <param name="path">The path containing the drive name; it doesn't have to only have the drive name.</param>
        public abstract IDPDriveInfo CreateDriveInfo(string path);

        public AbstractFileSystem() { }
        /// <summary>
        /// Sets the <see cref="Scope"/> to <paramref name="scope"/>.
        /// </summary>
        /// <param name="scope">The scope to set.</param>
        public AbstractFileSystem(DPFileScopeSettings scope) => Scope = scope;
    }
}
