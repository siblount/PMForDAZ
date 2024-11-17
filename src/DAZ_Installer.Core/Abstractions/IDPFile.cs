using DAZ_Installer.IO;


namespace DAZ_Installer.Core {
    /// <summary>
    /// A file object used for extraction purposes.
    /// </summary>
    /// <remarks>
    /// Usually, this object is used to represent a file in an archive. However, a file object can also represent a file in the system.
    /// (<see cref="IDPFile.Extracted"/>).
    /// <br/>
    /// <b>THIS SHOULD NOT BE USED FOR GENERAL PURPOSE USE! THE ONLY EXCEPTION IS FOR ARCHIVE FILES ON DISK! </b>
    /// </remarks>
    public interface IDPFile : IDPAbstractNode
    {
        /// <summary>
        /// The FileInfo object to use for moving, copying, and deleting files.
        /// </summary>
        public IDPFileInfo? FileInfo { get; set; }
        /// <summary>
        /// A list of tags that are associated with the file.
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// Determines whether the file has been extracted or not.
        /// </summary>
        /// <seealso cref="ExtractedToTarget"/>
        public bool Extracted { get; }
        /// <summary>
        /// Determines whether the file has been extracted to the target path or not.
        /// </summary>
        public bool ExtractedToTarget { get; }
    }
}