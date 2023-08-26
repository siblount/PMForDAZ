namespace DAZ_Installer.IO.Fakes
{
    [Obsolete("For testing purposes only")]
    public class FakeDPIOContextFactory : IContextFactory
    {
        public virtual DPAbstractIOContext CreateContext() => new FakeDPIOContext();
        public virtual DPAbstractIOContext CreateContext(DPFileScopeSettings scope) => new FakeDPIOContext(scope);
        public virtual DPAbstractIOContext CreateContext(DPAbstractIOContext context) => new FakeDPIOContext(context);
        public virtual DPAbstractIOContext CreateContext(DPFileScopeSettings scope, DriveInfo? info) => new FakeDPIOContext(scope, info);
    }
}
