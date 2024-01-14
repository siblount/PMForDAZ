namespace DAZ_Installer.Core.Extraction
{
    internal interface IZipArchiveFactory
    {
        IZipArchive Create(Stream stream);
    }
}
