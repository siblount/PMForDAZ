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
        /// The processor that threw  the error.
        /// </summary>
        public DPProcessor Processor { get; internal set; } = null!;
        public CancellationToken CancellationToken => Processor.CancellationToken;
        /// <summary>
        /// Additional information for the error, if any.
        /// </summary>
        public string Explaination { get; internal set; } = string.Empty;
        /// <summary>
        /// Determine whether the processor should cancel the operation or not.
        /// <para/>
        /// Change this value to <see langword="true"/> if you wish to cancel processing the 
        /// archive. <para/>
        /// This will only be honored if <see cref="Cancellable"/> is <see langword="true"/>.
        /// </summary>
        public bool CancelOperation { get; set; } = true;
        /// <summary>
        /// Represents whether the operation can be continued or not.
        /// </summary>
        public bool Continuable { get; internal set; } = false;
        /// <summary>
        /// Represents whether the operation can be cancelled or not. <br/>
        /// If <see langword="false"/>, <see cref="CancelOperation"/> will not be honored.
        /// </summary>
        public bool Cancellable { get; internal set; } = false;
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
