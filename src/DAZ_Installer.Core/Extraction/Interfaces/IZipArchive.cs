using System.IO.Compression;

namespace DAZ_Installer.Core.Extraction
{
    /// <inheritdoc cref="ZipArchive"/>
    internal interface IZipArchive : IDisposable
    {
        /// <inheritdoc cref="ZipArchive.Entries"/>
        IReadOnlyCollection<IZipArchiveEntry> Entries { get; }
        /// <inheritdoc cref="ZipArchive.Mode"/>
        ZipArchiveMode Mode { get; }
        /// <inheritdoc cref="ZipArchive.CreateEntry(string)"/>
        IZipArchiveEntry CreateEntry(string entryName);
        /// <inheritdoc cref="ZipArchive.GetEntry(string)"/>
        IZipArchiveEntry? GetEntry(string entryName);
    }
}
