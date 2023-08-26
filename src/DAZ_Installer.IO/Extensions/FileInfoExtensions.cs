namespace DAZ_Installer.IO 
{
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Converts the FileInfo into a <see cref="DPFileInfo"/> with no enforcement.
        /// </summary>
        public static DPFileInfo ToDPFileInfo(this FileInfo file)
        {
            ArgumentNullException.ThrowIfNull(file);
            return new DPFileInfo(file, DPIOContext.None);
        }
        /// <summary>
        /// Converts the FileInfo into a <see cref="DPFileInfo"/> with scope specified.
        /// </summary>
        /// <param name="scope">The scope to use.</param>
        public static DPFileInfo ToDPFileInfo(this FileInfo file, DPFileScopeSettings scope)
        {
            ArgumentNullException.ThrowIfNull(file);
            ArgumentNullException.ThrowIfNull(scope);
            return new DPFileInfo(file, new DPIOContext(scope));
        }
        /// <summary>
        /// Converts the FileInfo into a <see cref="DPFileInfo"/> by using an <see cref="DPIOContext"/> for scope context.
        /// </summary>
        /// <param name="scope">The scope to use.</param>
        public static DPFileInfo ToDPFileInfo(this FileInfo file, DPIOContext ctx)
        {
            ArgumentNullException.ThrowIfNull(ctx);
            return new DPFileInfo(file, ctx);
        }
    }
}
