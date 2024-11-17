namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// A factory class for creating instances of the <see cref="IProcess"/> class.
    /// </summary>
    internal class ProcessFactory : IProcessFactory
    {
        /// <summary>
        /// A singleton instance of the <see cref="ProcessFactory"/> class.
        /// </summary>
        /// <returns>A singleton instance</returns>
        public readonly static ProcessFactory Instance = new ProcessFactory();
        /// <summary>
        /// A constructor to prevent instantiation of the class
        /// </summary>
        private ProcessFactory() { }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>A <see cref="ProcessWrapper"/> object.</returns>
        public IProcess Create() => new ProcessWrapper();
    }
}
