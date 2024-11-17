namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// A factory for creating instances of the <see cref="IZipArchive"/> class.
    /// </summary>
    public interface IZipArchiveFactory
    {
        /// <summary>
        /// Creates a new instance of the <see cref="IZipArchive"/> class.
        /// </summary>
        /// <param name="stream">The stream to read ZIP archive data.</param>
        /// <returns>A zip archive.</returns>
        IZipArchive Create(Stream stream);
    }
}
