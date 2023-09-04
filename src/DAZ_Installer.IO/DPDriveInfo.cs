namespace DAZ_Installer.IO
{
    public class DPDriveInfo : IDPDriveInfo
    {
        public long AvailableFreeSpace => driveInfo.AvailableFreeSpace;
        public IDPDirectoryInfo RootDirectory => new DPDirectoryInfo(driveInfo.RootDirectory, fs);
        private readonly DriveInfo driveInfo;
        private readonly AbstractFileSystem fs;

        public DPDriveInfo(string path, AbstractFileSystem fs)  => (driveInfo, this.fs) = (new DriveInfo(path), fs);
        internal DPDriveInfo(DriveInfo driveInfo, AbstractFileSystem fs)
        {
            this.driveInfo = driveInfo;
            this.fs = fs;
        }
    }
}
