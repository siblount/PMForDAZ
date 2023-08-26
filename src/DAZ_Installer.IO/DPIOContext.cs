namespace DAZ_Installer.IO
{
    public class DPIOContext : DPAbstractIOContext
    {
        /// <summary>
        /// Creates an <see cref="DPIOContext"/> with the default scope set to <see cref="DPFileScopeSettings.All"/> (no enforcement).
        /// </summary>
        public DPIOContext() { }
        /// <summary>
        /// Creates a new IO context copying the scope from the given <paramref name="context"/>.
        /// </summary>
        /// <param name="context"></param>
        public DPIOContext(DPAbstractIOContext context) : base(context) { }
        /// <inheritdoc cref="DPAbstractIOContext(DPFileScopeSettings, DriveInfo)"/>
        public DPIOContext(DPFileScopeSettings scope, DriveInfo? info = null) : base(scope, info) { }
        /// <summary>
        /// Creates a new <see cref="DPDirectoryInfo"/> object with the given <paramref name="path"/> and this context.
        /// </summary>
        /// <param name="path">The path to use.</param>
        public override DPDirectoryInfo CreateDirectoryInfo(string path) => new DPDirectoryInfo(path, this);
        /// <summary>
        /// Creates a new <see cref="DPFileInfo"/> object with the given <paramref name="path"/> and this context.
        /// </summary>
        /// <param name="path">The path to use.</param>
        public override DPFileInfo CreateFileInfo(string path) => new DPFileInfo(path, this);
        /// <summary>
        /// Returns a new <see cref="DPIOContext"/> with the same scope as this one but detached tracking from this context.
        /// </summary>
        public override DPIOContext CreateTempContext() => new DPIOContext(this);

    }
}
