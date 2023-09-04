namespace DAZ_Installer.IO
{
    public interface IFileInfo
    {
        string Name { get; }
        string FullName { get; }
        bool Exists { get; }
        IDirectoryInfo? Directory { get; }
        string? DirectoryName { get; }
        FileAttributes Attributes { get; set; }
        Stream Create();
        Stream Open(FileMode mode, FileAccess access);
        void Delete();
        void MoveTo(string path, bool overwrite);
        IFileInfo CopyTo(string path, bool overwrite);
    }
}
