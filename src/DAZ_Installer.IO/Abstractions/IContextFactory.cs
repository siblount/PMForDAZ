namespace DAZ_Installer.IO
{
    public interface IContextFactory
    {
        /// <inheritdoc cref="AbstractFileSystem()"/>
        AbstractFileSystem CreateContext();
        /// <inheritdoc cref="AbstractFileSystem(DPFileScopeSettings)"/>
        AbstractFileSystem CreateContext(DPFileScopeSettings scope);
        /// <inheritdoc cref="AbstractFileSystem(AbstractFileSystem)"/>
        AbstractFileSystem CreateContext(AbstractFileSystem context);
        /// <inheritdoc cref="AbstractFileSystem(DPFileScopeSettings, DriveInfo)"/>
        AbstractFileSystem CreateContext(DPFileScopeSettings scope, DriveInfo? info);
    }
}
