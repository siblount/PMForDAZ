namespace DAZ_Installer.IO
{
    /// <summary>
    /// Represents an DP IO node that extends the real <see cref="System.IO.FileInfo"/>, or <see cref="System.IO.DirectoryInfo"/> 
    /// classes with <see cref="DPDirectoryInfo"/> and <see cref="DPFileInfo"/>.
    /// </summary>
    public interface IDPIONode
    {
        /// <summary>
        /// The filename of the object.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string Path { get; }
        /// <summary>
        /// Determines whether this exists on disk.
        /// </summary>
        public bool Exists { get; }
        /// <summary>
        /// Determines whether the Path is whitelisted.
        /// </summary>
        public bool Whitelisted { get; }
        /// <summary>
        /// The attributes of the object.
        /// </summary>
        FileAttributes Attributes { get; set; }
        /// <summary>
        /// The context to use for this object.
        /// </summary>
        public AbstractFileSystem FileSystem { get; }
    }
}
