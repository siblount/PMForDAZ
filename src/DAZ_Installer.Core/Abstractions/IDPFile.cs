using DAZ_Installer.Core.Extraction;
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
        /// <summary>
        /// Extracts this file to <see cref="DPAbstractNode.TargetPath"/>.
        /// </summary>
        /// <param name="settings">The extract settings to use.</param>
        /// <returns>Whether the extraction was successful or not.</returns>
        public bool Extract(DPExtractSettings settings);
        /// <summary>
        /// Extracts this file to <paramref name="dest"/> by setting 
        /// <see cref="DPAbstractNode.TargetPath"/> to <paramref name="dest"/> and extracting.
        /// </summary>
        /// <param name="settings">The extract settings to use.</param>
        /// <param name="dest">The location to extract this file to.</param>
        /// <returns>Whether the extraction was a success or not.</returns>
        public bool Extract(DPExtractSettings settings, string dest);
        /// <summary>
        /// Extracts this file to the temporary directory specified in <see cref="DPExtractSettings.TempPath"/>
        /// </summary>
        /// <param name="settings">The extract settings to use.</param>
        /// <returns>Whether the operation was a succses or not</returns>
        public bool ExtractToTemp(DPExtractSettings settings);
    }
}