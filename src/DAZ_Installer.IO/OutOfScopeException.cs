namespace DAZ_Installer.IO
{
    /// <summary>
    /// Represents an exception that is thrown when a path is out of scope from its <see cref="IDPFileScopeSettings"/> and an action attempted to be performed on it.
    /// </summary>
    public class OutOfScopeException : Exception
    {
        /// <summary>
        /// The offending file or directory path that was out of scope.
        ///
        public string OffendingPath { get; init; }
        public OutOfScopeException(string path) : base($"Attempted to do an operation on a path that is out of scope: {path}") => OffendingPath = path;
        public OutOfScopeException(string path, string? msg, bool template = true) : 
            base(template ? $"Attempted to do an operation on a path that is out of scope: {path}" : (msg ?? $"Attempted to do an operation on a path that is out of scope: {path}")) => OffendingPath = path;
    }
}
