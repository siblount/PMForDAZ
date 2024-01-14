using System.IO.Compression;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// Wrapper for <see cref="ZipArchive"/>
    /// </summary>
    internal class ZipArchiveWrapper : IZipArchive
    {
        readonly ZipArchive archive;

        internal ZipArchiveWrapper(ZipArchive archive) => this.archive = archive;

        public ZipArchiveWrapper(Stream stream) => archive = new(stream);
        public IReadOnlyCollection<IZipArchiveEntry> Entries => archive.Entries.Select(x => new ZipArchiveEntryWrapper(x)).ToList();

        public ZipArchiveMode Mode => archive.Mode;

        public IZipArchiveEntry CreateEntry(string entryName) => new ZipArchiveEntryWrapper(archive.CreateEntry(entryName));
        public void Dispose()
        {
            archive.Dispose();
            GC.SuppressFinalize(this);
        }

        public IZipArchiveEntry? GetEntry(string entryName)
        {
            var entry = archive.GetEntry(entryName);
            return entry is null ? null : new ZipArchiveEntryWrapper(entry);
        }
    }
}
