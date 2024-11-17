using DAZ_Installer.External;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// A factory class to create instances of the <see cref="RAR"/> class
    /// </summary>
    internal class RARFactory : IRARFactory
    {
        /// <summary>
        /// A singleton instance of the class
        /// </summary>
        public readonly static RARFactory Instance = new RARFactory();
        /// <summary>
        /// A constructor to prevent instantiation of the class
        /// </summary>
        private RARFactory() {}
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>An <see cref="RAR"/> class instance</returns>
        public IRAR Create(string arcPath) => new RAR(arcPath);
    }
}
