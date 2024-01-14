using System.IO.Compression;

namespace DAZ_Installer.Core.Extraction
{
    /// <inheritdoc cref="ZipArchiveEntry"/>
    internal interface IZipArchiveEntry
    {
        /// <inheritdoc cref="ZipArchiveEntry.Archive"/>
        IZipArchive Archive { get; }
        /// <inheritdoc cref="ZipArchiveEntry.Name"/>
        string Name { get; }
        /// <inheritdoc cref="ZipArchiveEntry.FullName"/>
        string FullName { get; }
        /// <inheritdoc cref="ZipArchiveEntry.Length"/>
        long Length { get; }
        /// <inheritdoc cref="ZipArchiveEntry.CompressedLength"/>
        long CompressedLength { get; }
        /// <inheritdoc cref="ZipArchiveEntry.LastWriteTime"/>
        DateTimeOffset LastWriteTime { get; set; }
        /// <inheritdoc cref="ZipArchiveEntry.Delete()"/>
        void Delete();
        /// <inheritdoc cref="ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string)"/>
        void ExtractToFile(string destinationFileName);
        /// <inheritdoc cref="ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string, bool)"/>
        void ExtractToFile(string destinationFileName, bool overwrite);
        /// <inheritdoc cref="ZipArchiveEntry.Open()"/>
        Stream Open();
    }
}
