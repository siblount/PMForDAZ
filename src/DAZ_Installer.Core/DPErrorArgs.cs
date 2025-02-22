namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents that an error occurred with potentialyl additional information.
    /// </summary>
    public class DPErrorArgs : EventArgs
    {
        /// <summary>
        /// The exception thrown, if any.
        /// </summary>
        public Exception? Ex { get; init; }
        /// <summary>
        /// Additional information for the error, if any.
        /// </summary>
        public string Explaination { get; internal set; } = string.Empty;

        public DPErrorArgs(Exception? ex = null, string? explaination = null)
        {
            Ex = ex;
            if (explaination != null)
                Explaination = explaination;
        }
    }
}
