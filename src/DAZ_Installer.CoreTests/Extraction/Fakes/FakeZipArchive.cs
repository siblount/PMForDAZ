using System.IO.Compression;

namespace DAZ_Installer.Core.Extraction.Fakes
{
    internal class FakeZipArchive : IZipArchive
    {
        public Dictionary<string, IZipArchiveEntry> PathToEntries = new();
        public FakeZipArchive() { }
        public FakeZipArchive(IEnumerable<IZipArchiveEntry> entries)
        {
            foreach (IZipArchiveEntry entry in entries)
            {
                PathToEntries.Add(entry.FullName, entry);
            }
        }
        public FakeZipArchive(Dictionary<string, IZipArchiveEntry> dict) => PathToEntries = dict;
        public FakeZipArchive(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                var entry = new FakeZipArchiveEntry(this, null)
                {
                    FullName = path,
                };
                PathToEntries.Add(path, entry);
            }
        }

        public virtual IReadOnlyCollection<IZipArchiveEntry> Entries => PathToEntries.Values;

        public virtual ZipArchiveMode Mode { get; set; }

        public virtual IZipArchiveEntry CreateEntry(string entryName) => new FakeZipArchiveEntry(this, null);
        /// <summary>
        /// The name of the archive
        /// </summary>
        /// <param name="entryName">The entry to find.</param>
        /// <returns></returns>
        public virtual IZipArchiveEntry? GetEntry(string entryName) => PathToEntries.TryGetValue(entryName, out IZipArchiveEntry? entry) ? entry : null;
        /// <summary>
        /// Does nothing.
        /// </summary>
        public virtual void Dispose() { }
    }
}
