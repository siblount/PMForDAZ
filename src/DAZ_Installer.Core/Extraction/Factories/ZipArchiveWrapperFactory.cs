namespace DAZ_Installer.Core.Extraction
{
    internal class ZipArchiveWrapperFactory : IZipArchiveFactory
    {
        public IZipArchive Create(Stream stream) => new ZipArchiveWrapper(stream);
    }
}
