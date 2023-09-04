namespace DAZ_Installer.IO 
{
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Converts the FileInfo into a <see cref="DPFileInfo"/> by using an <see cref="AbstractFileSystem"/> for file system context.
        /// </summary>
        /// <param name="fs">The file system to use.</param>
        public static DPFileInfo ToDPFileInfo(this FileInfo file, AbstractFileSystem fs)
        {
            ArgumentNullException.ThrowIfNull(fs);
            return new DPFileInfo(file, fs);
        }
    }
}
