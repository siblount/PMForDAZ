
namespace DAZ_Installer.IO.Fakes
{
    public class FakeDPDirectoryInfo : IDPDirectoryInfo
    {
        private readonly DPDirectoryInfo info;

        /// <inheritdoc cref="DPDirectoryInfo(IDirectoryInfo, DPIOContext, IDPDirectoryInfo)"/>
        public FakeDPDirectoryInfo(IDirectoryInfo info, AbstractFileSystem ctx, IDPDirectoryInfo? parent) => this.info = new DPDirectoryInfo(info, ctx, parent);

        public virtual IDPDirectoryInfo? Parent => ((IDPDirectoryInfo)info).Parent;

        public virtual string Name => ((IDPIONode)info).Name;

        public virtual string Path => ((IDPIONode)info).Path;

        public virtual bool Exists => ((IDPIONode)info).Exists;

        public virtual bool Whitelisted => ((IDPIONode)info).Whitelisted;

        public virtual FileAttributes Attributes { get => ((IDPIONode)info).Attributes; set => ((IDPIONode)info).Attributes = value; }

        public virtual AbstractFileSystem FileSystem => ((IDPIONode)info).FileSystem;

        public virtual void Create() => ((IDPDirectoryInfo)info).Create();
        public virtual void Delete(bool recursive) => ((IDPDirectoryInfo)info).Delete(recursive);
        public virtual bool SendToRecycleBin() => info.SendToRecycleBin();
        public virtual void MoveTo(string path) => ((IDPDirectoryInfo)info).MoveTo(path);
        public virtual bool PreviewCreate() => ((IDPDirectoryInfo)info).PreviewCreate();
        public virtual bool PreviewDelete(bool recursive) => ((IDPDirectoryInfo)info).PreviewDelete(recursive);
        public virtual bool PreviewMoveTo(string path) => ((IDPDirectoryInfo)info).PreviewMoveTo(path);
        public virtual bool PreviewSendToRecycleBin() => info.PreviewSendToRecycleBin();
        public virtual bool TryCreate() => ((IDPDirectoryInfo)info).TryCreate();
        public virtual bool TrySendToRecycleBin(out Exception? ex) => info.TrySendToRecycleBin(out ex);
        public virtual bool TryAndFixSendToRecycleBin(out Exception? ex) => info.TryAndFixSendToRecycleBin(out ex);
    }
}
