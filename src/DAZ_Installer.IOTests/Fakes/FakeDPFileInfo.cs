namespace DAZ_Installer.IO.Fakes
{
    [Obsolete("For testing purposes only")]
    public class FakeDPFileInfo : IDPFileInfo
    {
        private readonly DPFileInfo fileInfo;
        /// <inheritdoc cref="DPFileInfo(IFileInfo, AbstractFileSystem, IDPDirectoryInfo)"/>
        public FakeDPFileInfo(IFileInfo info, FakeFileSystem fs, IDPDirectoryInfo? directory) => fileInfo = new DPFileInfo(info, fs, directory);
        /// <inheritdoc cref="DPFileInfo(IFileInfo, AbstractFileSystem, IDPDirectoryInfo)"/>
        public FakeDPFileInfo(IFileInfo info, AbstractFileSystem fs, IDPDirectoryInfo? directory) => fileInfo = new DPFileInfo(info, fs, directory);


        public virtual IDPDirectoryInfo? Directory => ((IDPFileInfo)fileInfo).Directory;

        public virtual string Name => ((IDPIONode)fileInfo).Name;

        public virtual string Path => ((IDPIONode)fileInfo).Path;

        public virtual bool Exists => ((IDPIONode)fileInfo).Exists;

        public virtual bool Whitelisted => ((IDPIONode)fileInfo).Whitelisted;

        public virtual FileAttributes Attributes { get => ((IDPIONode)fileInfo).Attributes; set => ((IDPIONode)fileInfo).Attributes = value; }

        public virtual AbstractFileSystem FileSystem => ((IDPIONode)fileInfo).FileSystem;

        public virtual IDPFileInfo CopyTo(string path, bool overwrite) => ((IDPFileInfo)fileInfo).CopyTo(path, overwrite);
        public virtual Stream Create() => ((IDPFileInfo)fileInfo).Create();
        public virtual void Delete() => ((IDPFileInfo)fileInfo).Delete();
        public bool SendToRecycleBin() => throw new NotImplementedException();
        public virtual void MoveTo(string path, bool overwrite) => ((IDPFileInfo)fileInfo).MoveTo(path, overwrite);
        public virtual Stream Open(FileMode mode, FileAccess access) => ((IDPFileInfo)fileInfo).Open(mode, access);
        public virtual Stream OpenRead() => ((IDPFileInfo)fileInfo).OpenRead();
        public virtual Stream OpenWrite() => ((IDPFileInfo)fileInfo).OpenWrite();
        public virtual bool PreviewCopyTo(string path, bool overwrite) => ((IDPFileInfo)fileInfo).PreviewCopyTo(path, overwrite);
        public virtual bool PreviewCreate() => ((IDPFileInfo)fileInfo).PreviewCreate();
        public virtual bool PreviewDelete() => ((IDPFileInfo)fileInfo).PreviewDelete();
        public virtual bool PreviewMoveTo(string path, bool overwrite) => ((IDPFileInfo)fileInfo).PreviewMoveTo(path, overwrite);
        public virtual bool PreviewOpen(FileMode mode, FileAccess access) => ((IDPFileInfo)fileInfo).PreviewOpen(mode, access);
        public bool PreviewSendToRecycleBin() => fileInfo.PreviewSendToRecycleBin();
        public virtual bool TryAndFixCopyTo(string path, bool overwrite, out IDPFileInfo? info, out Exception? ex) => ((IDPFileInfo)fileInfo).TryAndFixCopyTo(path, overwrite, out info, out ex);
        public virtual bool TryAndFixDelete(out Exception? ex) => ((IDPFileInfo)fileInfo).TryAndFixDelete(out ex);
        public virtual bool TryAndFixMoveTo(string path, bool overwrite, out Exception? ex) => ((IDPFileInfo)fileInfo).TryAndFixMoveTo(path, overwrite, out ex);
        public virtual bool TryAndFixOpen(FileMode mode, FileAccess access, out Stream? stream, out Exception? ex) => ((IDPFileInfo)fileInfo).TryAndFixOpen(mode, access, out stream, out ex);
        public virtual bool TryAndFixOpenRead(out Stream? stream, out Exception? ex) => ((IDPFileInfo)fileInfo).TryAndFixOpenRead(out stream, out ex);
        public virtual bool TryAndFixOpenWrite(out Stream? stream, out Exception? ex) => ((IDPFileInfo)fileInfo).TryAndFixOpenWrite(out stream, out ex);
        public virtual bool TryCopyTo(string path, bool overwrite, out IDPFileInfo? info) => ((IDPFileInfo)fileInfo).TryCopyTo(path, overwrite, out info);
        public virtual bool TryDelete() => ((IDPFileInfo)fileInfo).TryDelete();
        public virtual bool TryMoveTo(string path, bool overwrite) => ((IDPFileInfo)fileInfo).TryMoveTo(path, overwrite);
        public virtual bool TryOpen(FileMode mode, FileAccess access, out Stream? stream) => ((IDPFileInfo)fileInfo).TryOpen(mode, access, out stream);
        public virtual bool TryOpenRead(out Stream? stream) => ((IDPFileInfo)fileInfo).TryOpenRead(out stream);
        public virtual bool TryOpenWrite(out Stream? stream) => ((IDPFileInfo)fileInfo).TryOpenWrite(out stream);
        public bool TrySendToRecycleBin(out Exception? ex) => fileInfo.TrySendToRecycleBin(out ex);
        public bool TryAndFixSendToRecycleBin(out Exception? ex) => fileInfo.TryAndFixSendToRecycleBin(out ex);
    }
}
