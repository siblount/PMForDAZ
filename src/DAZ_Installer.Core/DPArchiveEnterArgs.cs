namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents arguments for when the <see cref="DPProcessor"/> is about 
    /// to process an archive.
    /// </summary>
    public class DPArchiveEnterArgs : EventArgs
    {
        /// <summary>
        /// The archive that is about to be processed.
        /// </summary>
        public readonly DPArchive Archive;

        internal DPArchiveEnterArgs(DPArchive archive) => Archive = archive;
    }
}
