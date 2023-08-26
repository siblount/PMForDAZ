using DAZ_Installer.External;

namespace DAZ_Installer.Core.Extraction
{
    internal class RARFactory : IRARFactory
    {
        public IRAR Create(string arcPath) => new RAR(arcPath);
    }
}
