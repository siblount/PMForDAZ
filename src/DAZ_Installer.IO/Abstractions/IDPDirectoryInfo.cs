namespace DAZ_Installer.IO
{
    public interface IDPDirectoryInfo : IDPIONode
    {
        /// <summary>
        /// The parent directory of this directory.
        /// </summary>
        public IDPDirectoryInfo? Parent { get; }
        /// <summary>
        /// Create the directory on disk and necessary subdirectories.
        /// </summary>
        public void Create();
        /// <summary>
        /// Deletes the directory on disk. If there are files and subdirectories, this will fail and throw an error unless <paramref name="recursive"/> is true. 
        /// </summary>
        /// <param name="recursive">Setting this to <see langword="true"/> will delete files and subdirectories along with this directory.</param>
        public void Delete(bool recursive);
        /// <summary>
        /// Moves the directory and it's contents to <paramref name="path"/>. <paramref name="path"/> must exist on disk.
        /// </summary>
        /// <param name="path">The path to move the directory and it's contents to.</param>
        public void MoveTo(string path);
        /// <summary>
        /// Indicates whether this operation is allowed.
        /// </summary>
        public bool PreviewCreate();
        /// <summary>
        /// Indicates whether this operation is allowed.
        /// </summary>
        public bool PreviewDelete(bool recursive);
        /// <summary>
        /// Indicates whether this operation is allowed.
        /// </summary>
        public bool PreviewMoveTo(string path);
        /// <summary>
        /// Attempts to create the directory and necessary subdirectories. If the path is not whitelisted, it will fail.
        /// </summary>
        /// <returns>Whether the operation was successful or not.</returns>
        bool TryCreate();
    }
}
