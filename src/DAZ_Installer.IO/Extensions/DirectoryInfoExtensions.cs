namespace DAZ_Installer.IO
{
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Converts the FileInfo into a <see cref="DPDirectoryInfo"/> by using an <see cref="DPIOContext"/> for scope context.
        /// </summary>
        /// <param name="scope">The scope to use.</param>
        public static DPDirectoryInfo ToDPDirectoryInfo(this DirectoryInfo dir, AbstractFileSystem fs)
        {
            ArgumentNullException.ThrowIfNull(fs);
            return new DPDirectoryInfo(dir, fs);
        }
    }
}
