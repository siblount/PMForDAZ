using DAZ_Installer.IO;


namespace DAZ_Installer.Core.Tests
{
    [Obsolete("For testing purposes only")]
    public class MockedFakeDPIOContextFactory : IContextFactory
    {
        public bool PartiallyMock = true;
        public virtual DPAbstractIOContext CreateContext() => new MockedDPIOContext() { PartialMock = PartiallyMock };
        public virtual DPAbstractIOContext CreateContext(DPFileScopeSettings scope) => new MockedDPIOContext(scope) { PartialMock = PartiallyMock };
        public virtual DPAbstractIOContext CreateContext(DPAbstractIOContext context) => new MockedDPIOContext(context) { PartialMock = PartiallyMock };

        public virtual DPAbstractIOContext CreateContext(DPFileScopeSettings scope, DriveInfo? info) => new MockedDPIOContext(scope, info) { PartialMock = PartiallyMock };
    }
}
