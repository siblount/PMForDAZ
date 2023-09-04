namespace DAZ_Installer.IO
{
    public class DPFileSystem : AbstractFileSystem
    {
        public DPFileScopeSettings Scope { get; set; } = DPFileScopeSettings.None;
        public static DPFileSystem Unrestricted => new(DPFileScopeSettings.All);

        public override void DeleteDirectory(string path, bool recursive = false) => Directory.Delete(path, recursive);
        public override void DeleteFile(string path) => File.Delete(path);
        public override IEnumerable<IDPDirectoryInfo> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path).Select(x => new DPDirectoryInfo(x, this));
        public override IEnumerable<IDPFileInfo> EnumerateFiles(string path) => Directory.EnumerateFiles(path).Select(x => new DPFileInfo(x, this));
        public override bool Exists(string? path, bool treatAsDirectory = false) => treatAsDirectory ? Directory.Exists(path) : File.Exists(path);
        public override IDPDriveInfo[] GetDrives() => DriveInfo.GetDrives().Select(x => new DPDriveInfo(x, this)).ToArray();
        public override IDPFileInfo CreateFileInfo(string path) => new DPFileInfo(path, this);
        protected internal override IDPFileInfo CreateFileInfo(string path, IDPDirectoryInfo? directory = null) => new DPFileInfo(new FileInfo(path), this, directory);
        public override IDPDirectoryInfo CreateDirectoryInfo(string path) => new DPDirectoryInfo(new DirectoryInfo(path), this);
        protected internal override IDPDirectoryInfo CreateDirectoryInfo(string path, IDPDirectoryInfo? parent) => new DPDirectoryInfo(new DirectoryInfo(path), this, parent);
        public override IDPDriveInfo CreateDriveInfo(string path) => new DPDriveInfo(path, this);

        /// <summary>
        /// Creates a new instance of <see cref="DPFileSystem"/> with no access to the file system.
        /// </summary>
        public DPFileSystem() { }
        /// <summary>
        /// Creates a new instance of <see cref="DPFileSystem"/> with the specified <paramref name="scope"/>.
        /// </summary>
        /// <param name="scope">The scope to use for all created <see cref="IDPDirectoryInfo"/> and <see cref="IDPFileInfo"/>.</param>
        public DPFileSystem(DPFileScopeSettings scope) => Scope = scope;

        public DPFileSystem(DPFileSystem other) => Scope = new DPFileScopeSettings(other.Scope);
    }
}
