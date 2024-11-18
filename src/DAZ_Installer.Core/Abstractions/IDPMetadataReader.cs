namespace DAZ_Installer.Core
{
    public interface IDPMetadataReader
    {
        /// <summary>
        /// Scans the files and calls <see cref="IDPDSXFile.CheckContents(StreamReader)"/> on each of them.
        /// </summary>
        /// <param name="files">The files to read metadata for</param>
        /// <param name="token">The cancellation token for cancelling operations.</param>
        void ReadMetadata(IEnumerable<IDPDSXFile> files, CancellationToken token);
    }
}
