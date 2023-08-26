namespace DAZ_Installer.Core.Extraction.Fakes
{
    internal class FakeZipArchiveEntry : IZipArchiveEntry
    {
        Stream stream = Stream.Null;
        public virtual IZipArchive Archive { get; set; }

        public virtual string Name => Path.GetFileName(FullName);

        public virtual string FullName { get; set; } = string.Empty;

        public virtual long Length { get; set; } = 0;

        public virtual long CompressedLength { get; set; } = 0;

        public virtual DateTimeOffset LastWriteTime { get; set; } = new DateTimeOffset();
        internal FakeZipArchiveEntry(IZipArchive Archive, Stream? stream)
        {
            this.Archive = Archive;
            this.stream = stream ?? Stream.Null;
        }
        public virtual void Delete() { }
        public virtual void ExtractToFile(string destinationFileName) { }
        public virtual void ExtractToFile(string destinationFileName, bool overwrite) { }
        public virtual Stream Open() => stream;
    }
}
