using Moq;
namespace DAZ_Installer.IO.Fakes
{
    [Obsolete("For testing purposes only")]
    public class FakeFileSystem : AbstractFileSystem
    {
        public bool PartialMock = true;
        /// <inheritdoc/>
        public FakeFileSystem() { }
        /// <inheritdoc/>
        public FakeFileSystem(DPFileScopeSettings scope) : base(scope) { }

        public override FakeDPDirectoryInfo CreateDirectoryInfo(string path) => new Mock<FakeDPDirectoryInfo>(new Mock<FakeDirectoryInfo>(path) { CallBase = PartialMock }.Object, this, null) { CallBase = PartialMock }.Object;
        public override FakeDPFileInfo CreateFileInfo(string path) => new Mock<FakeDPFileInfo>(new Mock<FakeFileInfo>(path) { CallBase = PartialMock }.Object, this, null) { CallBase = PartialMock }.Object;
        public override FakeDPDriveInfo CreateDriveInfo(string path) => new Mock<FakeDPDriveInfo>(this, Path.GetPathRoot(path)!) { CallBase = PartialMock }.Object;
        public override void DeleteDirectory(string path, bool recursive = false) => throw new NotImplementedException();
        public override void DeleteFile(string path) => throw new NotImplementedException();
        public override IEnumerable<IDPDirectoryInfo> EnumerateDirectories(string path) => throw new NotImplementedException();
        public override IEnumerable<IDPFileInfo> EnumerateFiles(string path) => throw new NotImplementedException();
        public override bool Exists(string? path, bool treatAsDirectory = false) => throw new NotImplementedException();
        public override IDPDriveInfo[] GetDrives() => new[] { new FakeDPDriveInfo(this, "A:/") };
        protected internal override IDPDirectoryInfo CreateDirectoryInfo(string path, IDPDirectoryInfo? parent) => CreateDirectoryInfo(path, parent);
        protected internal override IDPFileInfo CreateFileInfo(string path, IDPDirectoryInfo? directory = null) => CreateFileInfo(path, directory);
    }
}
