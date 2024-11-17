namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// An interface for creating instances of the <see cref="IRAR"/> class.
    /// </summary>
    public interface IRARFactory
    {
        /// <summary>
        /// Creates a new instance of the <see cref="IRAR"/> class.
        /// </summary>
        /// <param name="arcPath">The path to the RAR archive.</param>
        /// <returns>An IRAR object</returns>
        IRAR Create(string arcPath);
    }
}
