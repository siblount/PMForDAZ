namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// A factory class for creating instances of the <see cref="IProcess"/> class.
    /// </summary>
    public interface IProcessFactory
    {
        /// <summary>
        /// Creates a new instance of the <see cref="IProcess"/> class.
        /// </summary>
        IProcess Create();
    }
}
