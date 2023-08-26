namespace DAZ_Installer.IO.Fakes
{
    [Obsolete("For testing purposes only")]
    public class FakeDPIOContext : DPAbstractIOContext
    {
        public override long AvailableFreeSpace => long.MaxValue;
        public override long TotalFreeSpace => long.MaxValue;
        /// <inheritdoc/>
        public FakeDPIOContext() { }
        /// <inheritdoc/>
        public FakeDPIOContext(DPAbstractIOContext context) : base(context) { }
        /// <inheritdoc/>
        public FakeDPIOContext(DPFileScopeSettings scope) : base(scope) { }
        /// <inheritdoc/>
        public FakeDPIOContext(DPFileScopeSettings scope, DriveInfo? info = null) : base(scope, null) { }


        public override FakeDPDirectoryInfo CreateDirectoryInfo(string path) => new FakeDPDirectoryInfo(new FakeDirectoryInfo(path), this, null);
        public override FakeDPFileInfo CreateFileInfo(string path) => new FakeDPFileInfo(new FakeFileInfo(path), this, null);
        public override DPAbstractIOContext CreateTempContext() => new FakeDPIOContext(this);

    }
}
