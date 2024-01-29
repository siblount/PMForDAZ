namespace DAZ_Installer.IO
{
    public abstract class DPIONodeBase : IDPIONode
    {
        public abstract AbstractFileSystem FileSystem { get; internal set; }
        public abstract string Name { get; }
        public abstract string Path { get; }
        public abstract bool Exists { get; }
        public abstract bool Whitelisted { get; }
        public abstract FileAttributes Attributes { get; set; }
        internal abstract void Invalidate();
        public virtual bool PreviewSendToRecycleBin() => Whitelisted;
        public virtual bool SendToRecycleBin()
        {
            if (!Whitelisted) return false;
            return DPRecycleBin.SendToRecycleBin(this);
        }
        public virtual bool TrySendToRecycleBin(out Exception? ex)
        {
            ex = null;
            try
            {
                if (!Whitelisted) return false;
                return DPRecycleBin.SendToRecycleBin(this);
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }
        public abstract bool TryAndFixSendToRecycleBin(out Exception? ex);
    }
}
