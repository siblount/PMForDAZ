namespace DAZ_Installer.IO
{
    public interface IDPDriveInfo
    {
        long AvailableFreeSpace { get; }
        IDPDirectoryInfo RootDirectory { get; }
    }
}
