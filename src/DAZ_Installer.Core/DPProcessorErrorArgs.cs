namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents error arguments for errors in DPProcessor outside out extraction errors.
    /// </summary>
    public class DPProcessorErrorArgs : EventArgs
    {
        /// <summary>
        /// The exception thrown, if any.
        /// </summary>
        public Exception? Ex { get; init; }
        /// <summary>
        /// The processor that threw the error.
        /// </summary>
        public DPProcessor Processor { get; internal set; } = null!;
        /// <summary>
        /// Additional information for the error, if any.
        /// </summary>
        public string Explaination { get; internal set; } = string.Empty;
        /// <summary>
        /// Represents whether the operation can be continued or not. Default is false.
        /// </summary>
        public bool Continuable { get; internal set; } = false;
        /// <summary>
        /// <inheritdoc cref="DPProcessorErrorArgs"/>
        /// </summary>
        /// <param name="ex">The exception thrown by the error, if any.</param>
        /// <param name="explaination">The additional explaination for the error/situation.</param>
        /// <param name="archive">The corresponding archive the <see cref="DPProcessor"/ was processing. ></param>
        internal DPProcessorErrorArgs(Exception? ex = null, string? explaination = null) : base()
        {
            Ex = ex;
            if (explaination != null)
                Explaination = explaination;
        }
    }
}
