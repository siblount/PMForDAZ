namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents arguments for when the <see cref="DPProcessor"/> is about 
    /// to process an archive.
    /// </summary>
    public class DPArchiveEnterArgs : EventArgs
    {
        /// <summary>
        /// Determine whether to cancel operations or not. 
        /// Setting this value to <see langword="true"/> will prevent
        /// the archive from being processed.
        /// </summary>
        public bool Cancel = false;
        /// <summary>
        /// The archive that is about to be processed.
        /// </summary>
        public readonly DPArchive Archive;

        internal DPArchiveEnterArgs(DPArchive archive, bool cancel = false)
        {
            Archive = archive;
            Cancel = cancel;
        }
    }
}
