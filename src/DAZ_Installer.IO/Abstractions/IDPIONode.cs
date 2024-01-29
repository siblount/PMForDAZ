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
        /// <summary>
        /// Sends the file/directory to the recycle bin.
        /// </summary>
        /// <returns>Whether the operation was successful or not.</returns>
        public bool SendToRecycleBin();
        /// <summary>
        /// Previews whether the operation to send the file/directory to the recycle bin is allowed.
        /// </summary>
        /// <returns>Whether the operation is allowed or not.</returns>
        public bool PreviewSendToRecycleBin();
        /// <summary>
        /// Attempts to send the file/directory to the recycle bin. Will fail if not whitelisted.
        /// </summary>
        /// <param name="ex">The exception that was thrown, if any.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public bool TrySendToRecycleBin(out Exception? ex);
        /// <summary>
        /// Attempts to send the file/directory to the recycle bin and fixes any errors that occur.
        /// </summary>
        /// <param name="ex">The exception that was thrown, if any.</param>
        /// <returns>Whether the operation what successful or not.</returns>
        public bool TryAndFixSendToRecycleBin(out Exception? ex);
    }
}
