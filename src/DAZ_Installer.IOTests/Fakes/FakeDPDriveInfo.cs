using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.IO.Fakes
{
    [Obsolete("For testing purposes only")]
    public class FakeDPDriveInfo : IDPDriveInfo
    {
        public FakeFileSystem FileSystem { get; init; }

        public virtual long AvailableFreeSpace { get; set; } = long.MaxValue;
        public virtual IDPDirectoryInfo RootDirectory { get; set; } = null!;

        public FakeDPDriveInfo(FakeFileSystem fileSystem, string rootDirName) => (FileSystem, RootDirectory) = (fileSystem, fileSystem.CreateDirectoryInfo(rootDirName));
    }
}
