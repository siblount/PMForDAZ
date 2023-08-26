namespace DAZ_Installer.IO
{
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Converts the FileInfo into a <see cref="DPDirectoryInfo"/> with no enforcement.
        /// </summary>
        public static DPDirectoryInfo ToDPDirectoryInfo(this DirectoryInfo directoryInfo)
        {
            ArgumentNullException.ThrowIfNull(directoryInfo);
            return new DPDirectoryInfo(directoryInfo, new DPIOContext());
        }
        /// <summary>
        /// Converts the FileInfo into a <see cref="DPDirectoryInfo"/> with scope specified.
        /// </summary>
        /// <param name="scope">The scope to use.</param>
        public static DPDirectoryInfo ToDPDirectoryInfo(this DirectoryInfo directoryInfo, DPFileScopeSettings scope)
        {
            ArgumentNullException.ThrowIfNull(directoryInfo);
            ArgumentNullException.ThrowIfNull(scope);
            return new DPDirectoryInfo(directoryInfo, new DPIOContext(scope));
        }

        /// <summary>
        /// Converts the FileInfo into a <see cref="DPDirectoryInfo"/> by using an <see cref="DPIOContext"/> for scope context.
        /// </summary>
        /// <param name="scope">The scope to use.</param>
        public static DPDirectoryInfo ToDPDirectoryInfo(this DirectoryInfo dir, DPIOContext ctx)
        {
            ArgumentNullException.ThrowIfNull(ctx);
            return new DPDirectoryInfo(dir, ctx);
        }
    }
}
