using System.IO.Compression;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// Wrapper for <see cref="ZipArchiveEntry"/>
    /// </summary>
    internal class ZipArchiveEntryWrapper : IZipArchiveEntry
    {
        private readonly ZipArchiveEntry _entry;

        internal ZipArchiveEntryWrapper(ZipArchiveEntry entry) => _entry = entry;
        /// <inheritdoc cref="ZipArchiveEntry.Name"/>
        public string Name => _entry.Name;
        /// <inheritdoc cref="ZipArchiveEntry.FullName"/>
        public string FullName => _entry.FullName;
        /// <inheritdoc cref="ZipArchiveEntry.Length"/>
        public long Length => _entry.Length;
        /// <inheritdoc cref="ZipArchiveEntry.LastWriteTime"/>
        public DateTimeOffset LastWriteTime { get => _entry.LastWriteTime; set => _entry.LastWriteTime = value; }
        /// <inheritdoc cref="ZipArchiveEntry.Archive"/>
        public IZipArchive Archive => new ZipArchiveWrapper(_entry.Archive);
        /// <inheritdoc cref="ZipArchiveEntry.CompressedLength"/>
        public long CompressedLength => _entry.CompressedLength;
        /// <inheritdoc cref="ZipArchiveEntry.Delete"/>
        public void Delete() => _entry.Delete();
        /// <inheritdoc cref="ZipArchiveEntry.ExtractToFile(string)"/>
        public void ExtractToFile(string destinationFileName) => _entry.ExtractToFile(destinationFileName);
        /// <inheritdoc cref="ZipArchiveEntry.ExtractToFile(string, bool)"/>
        public void ExtractToFile(string destinationFileName, bool overwrite) => _entry.ExtractToFile(destinationFileName, overwrite);
        /// <inheritdoc cref="ZipArchiveEntry.Open"/>
        public Stream Open() => _entry.Open();
        /// <inheritdoc cref="ZipArchiveEntry.ToString"/>
        public override string ToString() => _entry.ToString();
    }
}
