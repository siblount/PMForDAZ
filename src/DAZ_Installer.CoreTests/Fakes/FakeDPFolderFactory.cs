using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core.Tests.Fakes
{
    /// <summary>
    /// A fake implementation of <see cref="IDPFolderFactory"/> that can be used for testing.
    /// </summary>
    public class FakeDPFolderFactory : IDPFolderFactory
    {
        public virtual IDPFolder CreateFolder(string path, IDPArchive parentArchive, IDPFolder? parentFolder) => throw new NotImplementedException();
        public virtual IDPFolder CreateFolders(string dpFilePath, IDPArchive associatedArchive) => throw new NotImplementedException();

    }
}
