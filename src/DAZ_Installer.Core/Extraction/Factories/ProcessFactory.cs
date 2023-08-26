namespace DAZ_Installer.Core.Extraction
{
    internal class ProcessFactory : IProcessFactory
    {
        public IProcess Create() => new ProcessWrapper();
    }
}
