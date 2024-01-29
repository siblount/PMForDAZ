namespace DAZ_Installer.IO
{
    public interface IDPFileInfo : IDPIONode
    {
        IDPDirectoryInfo? Directory { get; }
        Stream Create();
        Stream Open(FileMode mode, FileAccess access);
        Stream OpenRead();
        Stream OpenWrite();
        void MoveTo(string path, bool overwrite);
        IDPFileInfo CopyTo(string path, bool overwrite);
        void Delete();
        bool PreviewCreate();
        bool PreviewDelete();
        bool PreviewOpen(FileMode mode, FileAccess access);
        bool PreviewMoveTo(string path, bool overwrite);
        bool PreviewCopyTo(string path, bool overwrite);
        bool TryDelete();
        bool TryOpen(FileMode mode, FileAccess access, out Stream? stream);
        bool TryOpenRead(out Stream? stream);
        bool TryOpenWrite(out Stream? stream);
        bool TryMoveTo(string path, bool overwrite);
        bool TryCopyTo(string path, bool overwrite, out IDPFileInfo? info);
        bool TryAndFixDelete(out Exception? ex);
        bool TryAndFixOpen(FileMode mode, FileAccess access, out Stream? stream, out Exception? ex);
        bool TryAndFixOpenRead(out Stream? stream, out Exception? ex);
        bool TryAndFixOpenWrite(out Stream? stream, out Exception? ex);

        bool TryAndFixMoveTo(string path, bool overwrite, out Exception? ex);
        bool TryAndFixCopyTo(string path, bool overwrite, out IDPFileInfo? info, out Exception? ex);
    }
}
