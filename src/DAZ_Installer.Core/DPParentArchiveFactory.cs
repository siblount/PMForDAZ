using DAZ_Installer.IO;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// A factory for creating new <see cref="IDPFile"/> objects.
    /// </summary>
    public sealed class DPParentArchiveFactory : IDPParentArchiveFactory
    {
        /// <summary>
        /// A singleton instance of the <see cref="DPParentArchiveFactory"/> class.
        /// </summary>
        public static readonly DPParentArchiveFactory Instance = new();

        private DPParentArchiveFactory() { }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fileInfo"><inheritdoc/></param>
        /// <returns>A <see cref="DPArchive"/> object.</returns>
        public IDPArchive CreateNewParentArchive(IDPFileInfo fileInfo) => new DPArchive(fileInfo);
    }
}

