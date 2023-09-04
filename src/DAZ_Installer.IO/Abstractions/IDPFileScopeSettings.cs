
namespace DAZ_Installer.IO
{
    public interface IDPFileScopeSettings
    {
        bool ExplicitFilePaths { get; }
        bool ExplicitDirectoryPaths { get; }
        bool NoEnforcement { get; }
        bool ThrowOnPathTransversal { get; }
        public bool IsDirectoryWhitelisted(string directoryPath);
        public bool IsFilePathWhitelisted(string path);
    }
}
