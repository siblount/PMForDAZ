namespace DAZ_Installer.IO
{
    public interface IDirectoryInfo
    {
        // Properties
        string Name { get; }
        string FullName { get; }
        bool Exists { get; }
        FileAttributes Attributes { get; set; }
        IDirectoryInfo? Parent { get; }

        // Methods
        void Create();
        void Delete(bool recursive);
        void MoveTo(string path);
        IEnumerable<IDirectoryInfo> EnumerateDirectories();
        IEnumerable<IDirectoryInfo> EnumerateDirectories(string pattern, EnumerationOptions options);
        IEnumerable<IFileInfo> EnumerateFiles();
        IEnumerable<IFileInfo> EnumerateFiles(string pattern, EnumerationOptions options);

    }
}
