namespace DAZ_Installer.IO
{
    public interface IContextFactory
    {
        /// <inheritdoc cref="DPAbstractIOContext()"/>
        DPAbstractIOContext CreateContext();
        /// <inheritdoc cref="DPAbstractIOContext(DPFileScopeSettings)"/>
        DPAbstractIOContext CreateContext(DPFileScopeSettings scope);
        /// <inheritdoc cref="DPAbstractIOContext(DPAbstractIOContext)"/>
        DPAbstractIOContext CreateContext(DPAbstractIOContext context);
        /// <inheritdoc cref="DPAbstractIOContext(DPFileScopeSettings, DriveInfo)"/>
        DPAbstractIOContext CreateContext(DPFileScopeSettings scope, DriveInfo? info);
    }
}
