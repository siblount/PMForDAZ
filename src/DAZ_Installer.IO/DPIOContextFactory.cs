namespace DAZ_Installer.IO
{
    public class DPIOContextFactory : IContextFactory
    {
        public DPAbstractIOContext CreateContext() => new DPIOContext();
        public DPAbstractIOContext CreateContext(DPFileScopeSettings scope) => new DPIOContext(scope);
        public DPAbstractIOContext CreateContext(DPAbstractIOContext context) => new DPIOContext(context);
        public DPAbstractIOContext CreateContext(DPFileScopeSettings scope, DriveInfo? info) => new DPIOContext(scope, info);
    }
}
