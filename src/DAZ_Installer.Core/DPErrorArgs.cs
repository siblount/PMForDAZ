namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents an exception and a file or 
    /// </summary>
    public class DPErrorArgs : EventArgs
    {
        public readonly DPAbstractNode File;
        public readonly Exception Ex;

        internal DPErrorArgs(DPAbstractNode file, Exception ex) : base()
        {
            File = file;
            Ex = ex;
        }
    }
}
