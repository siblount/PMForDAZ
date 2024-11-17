using DAZ_Installer.IO;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// A factory for creating new <see cref="IDPFile"/> objects.
    /// </summary>
    public interface IDPParentArchiveFactory
    {
        /// <summary>
        /// Creates a new archive and assigns it to the specified parent folder and archive, if any.
        /// </summary>
        /// <param name="fileInfo">The file info to use for I/O operations.</param>
        /// <returns>A new <see cref="IDPArchive"/> object.</returns>
        public IDPArchive CreateNewParentArchive(IDPFileInfo fileInfo);
    }
}