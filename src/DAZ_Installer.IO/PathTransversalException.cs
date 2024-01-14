namespace DAZ_Installer.IO
{
    public class PathTransversalException : Exception
    {
        public PathTransversalException() : base($"Path transversal detected") { }
        public PathTransversalException(string msg) : base(msg) { }
        public static void ThrowIfTransversalDetected(string path)
        {
            if (PathHelper.CheckForTranversal(path))
                throw new PathTransversalException();
        }

        public static void ThrowIfTransversalDetected(string path, string? messageIfDetected)
        {
            messageIfDetected ??= $"Path tranversal detected for {path}";
            if (PathHelper.CheckForTranversal(path))
                throw new PathTransversalException(messageIfDetected);
        }
    }
}
