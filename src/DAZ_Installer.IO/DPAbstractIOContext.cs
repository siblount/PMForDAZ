using DAZ_Installer.IO.Wrappers;
using System.Collections.Immutable;

namespace DAZ_Installer.IO
{
    public abstract class DPAbstractIOContext
    {
        public virtual long AvailableFreeSpace => driveInfo.AvailableFreeSpace;
        public virtual long TotalFreeSpace => driveInfo.TotalFreeSpace;
        protected DriveInfo? driveInfo = null;


        protected readonly HashSet<DPIONodeBase> nodes = new(200);
        /// <summary>
        /// Creates a context where no permissions are granted.
        /// </summary>
        public static DPAbstractIOContext None = new DPIOContext(DPFileScopeSettings.None);
        public DPFileScopeSettings Scope { get => scope; private set => ChangeScopeTo(value); }
        private DPFileScopeSettings scope = DPFileScopeSettings.All;

        /// <summary>
        /// Creates a new IO context with all permissions.
        /// </summary>
        public DPAbstractIOContext() { }

        /// <summary>
        /// Creates a new IO context copying the scope from the given <paramref name="context"/>.
        /// </summary>
        /// <param name="context"></param>
        public DPAbstractIOContext(DPAbstractIOContext context) => scope = context.scope;
        /// <summary>
        /// Creates an new IO context with the specified <paramref name="scope"/>.
        /// </summary>
        /// <param name="scope">The scope to set for all <see cref="DPIONodeBase"/> objects 
        /// (<see cref="DPFileInfo"/> and <see cref="DPDirectoryInfo"/>) created.</param>
        public DPAbstractIOContext(DPFileScopeSettings scope) => this.scope = scope;
        /// <summary>
        /// Creates an <see cref="DPIOContext"/> with the specified <paramref name="scope"/> and drive<paramref name="info"/>.
        /// </summary>
        /// <param name="scope">The scope to set for all <see cref="DPIONodeBase"/> objects 
        /// (<see cref="DPFileInfo"/> and <see cref="DPDirectoryInfo"/>) created.</param>
        /// <param name="info"> The drive info to use for this context, if null, then the drive that the current working directory is on will be used. </param>
        public DPAbstractIOContext(DPFileScopeSettings scope, DriveInfo? info = null) => (this.scope, driveInfo) = (scope, info ?? new DriveInfo(Directory.GetCurrentDirectory()));

        public abstract IDPDirectoryInfo CreateDirectoryInfo(string path);
        public abstract IDPFileInfo CreateFileInfo(string path);
        public abstract DPAbstractIOContext CreateTempContext(); 

        /// <summary>
        /// Updates all of the nodes in this context to the specified scope.
        /// </summary>
        /// <param name="scope"></param>
        public virtual void ChangeScopeTo(DPFileScopeSettings scope)
        {
            ArgumentNullException.ThrowIfNull(scope);
            if (None == this) throw new ArgumentException("Cannot change scope of DPIOContext.None");
            this.scope = scope;
            foreach (DPIONodeBase node in nodes)
                node.Invalidate();
        }

        /// <summary>
        /// Clears all of the FileInfos and DirectoryInfos from this context and moves it to <see cref="None"/>.
        /// </summary>
        public void Clear()
        {
            if (None == this) return;
            foreach (var node in nodes) node.Context = None;
            nodes.Clear();
        }
        /// <summary>
        /// Registers the new node to the internal list. This should be required for all nodes, to ensure that nodes
        /// are updated when the scope is changed.
        /// </summary>
        /// <param name="node">The node to register.</param>
        internal void RegisterNode(DPIONodeBase node)
        {
            if (None == this) return;
            nodes.Add(node ?? throw new ArgumentNullException("Attempted to add null DPIONode to DPIOContext."));
        }

        internal ImmutableHashSet<DPIONodeBase> GetNodes() => nodes.ToImmutableHashSet();
    }
}
