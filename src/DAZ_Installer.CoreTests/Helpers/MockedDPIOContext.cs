using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using NSubstitute;

namespace DAZ_Installer.Core.Tests
{
    [Obsolete("For testing purposes only")]
    internal class MockedDPIOContext : DPAbstractIOContext
    {
        public bool PartialMock = true;
        public override long AvailableFreeSpace => long.MaxValue;
        public override long TotalFreeSpace => long.MaxValue;
        /// <inheritdoc/>
        public MockedDPIOContext() { }
        /// <inheritdoc/>
        public MockedDPIOContext(DPAbstractIOContext context) : base(context) { }
        public MockedDPIOContext(MockedDPIOContext context) : base(context)
        {
            PartialMock = context.PartialMock;
        }
        /// <inheritdoc/>
        public MockedDPIOContext(DPFileScopeSettings scope) : base(scope) { }
        /// <inheritdoc/>
        public MockedDPIOContext(DPFileScopeSettings scope, DriveInfo? info = null) : base(scope, null) { }


        public override FakeDPDirectoryInfo CreateDirectoryInfo(string path) => PartialMock ? Substitute.ForPartsOf<FakeDPDirectoryInfo>(new FakeDirectoryInfo(path), this, null) : Substitute.For<FakeDPDirectoryInfo>(new FakeDirectoryInfo(path), this, null);
        public override FakeDPFileInfo CreateFileInfo(string path) => PartialMock ? Substitute.ForPartsOf<FakeDPFileInfo>(new FakeFileInfo(path), this, null) : Substitute.For<FakeDPFileInfo>(new FakeFileInfo(path), this, null);
        public override DPAbstractIOContext CreateTempContext() => new MockedDPIOContext(this);
    }
}
