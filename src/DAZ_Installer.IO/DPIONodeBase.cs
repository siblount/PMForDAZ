namespace DAZ_Installer.IO
{
    public abstract class DPIONodeBase : IDPIONode
    {
        public abstract DPAbstractIOContext Context { get; internal set; }
        public abstract string Name { get; }
        public abstract string Path { get; }
        public abstract bool Exists { get; }
        public abstract bool Whitelisted { get; }
        public abstract FileAttributes Attributes { get; set; }
        internal abstract void Invalidate();
    }
}
