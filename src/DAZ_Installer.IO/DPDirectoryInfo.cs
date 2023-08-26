using DAZ_Installer.IO.Wrappers;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DAZ_Installer.IO
{
    /// <summary>
    /// A safer directory info class to with security to ensure that modifications on the disk are permitted via <see cref="DPFileScopeSettings"/>.
    /// </summary>
    public class DPDirectoryInfo : DPIONodeBase, IDPDirectoryInfo
    {
        /// <inheritdoc cref="DirectoryInfo.Name"/>
        public override string Name => directoryInfo.Name;

        /// <inheritdoc cref="DirectoryInfo.FullName"/>
        public override string Path => directoryInfo.FullName;

        /// <inheritdoc cref="DirectoryInfo.Exists"/>
        public override bool Exists => directoryInfo.Exists;
        public override FileAttributes Attributes { get => directoryInfo.Attributes; set => directoryInfo.Attributes = value; }

        /// <summary>
        /// Determines whether the directory is whitelisted.
        /// </summary>
        public override bool Whitelisted => whitelisted;
        /// <summary>
        /// The ctx that is currently used for this directory.
        /// </summary>
        public IDPFileScopeSettings Scope => Context.Scope;
        public override DPAbstractIOContext Context
        {
            get => context; 
            internal set
            {
                ArgumentNullException.ThrowIfNull(value);
                context = value;
                Invalidate();
            }
        }
        /// <inheritdoc cref="DirectoryInfo.Parent"/>
        public IDPDirectoryInfo? Parent => parent ??= tryCreateDirectoryInfoParent(directoryInfo, context);

        protected IDirectoryInfo directoryInfo;
        protected IDPDirectoryInfo? parent;
        protected bool whitelisted;
        private DPAbstractIOContext context;

        internal DPDirectoryInfo(DirectoryInfo info, DPAbstractIOContext ctx) : this(info, ctx, tryCreateDirectoryInfoParent(info, ctx)) { }
        internal DPDirectoryInfo(IDirectoryInfo info, DPAbstractIOContext ctx) : this(info, ctx, tryCreateDirectoryInfoParent(info, ctx)) { }
        internal DPDirectoryInfo(string path, DPAbstractIOContext ctx) : this(new DirectoryInfo(path), ctx) { }
        internal DPDirectoryInfo(DirectoryInfo info, DPAbstractIOContext ctx, IDPDirectoryInfo? parent) : this(new DirectoryInfoWrapper(info), ctx, parent) { }
        /// <summary>
        /// Constructor used for testing and internally.
        /// </summary>
        /// <param name="info">The directory info to use.</param>
        /// <param name="ctx">The ctx settings to use.</param>
        /// <param name="parent">The parent directory of this directory.</param>
        internal DPDirectoryInfo(IDirectoryInfo info, DPAbstractIOContext ctx, IDPDirectoryInfo? parent)
        {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(ctx);
            directoryInfo = info;
            context = ctx;
            ctx.RegisterNode(this);
            whitelisted = ctx.Scope.IsDirectoryWhitelisted(info.FullName);
            this.parent = parent;
        }

        /// <inheritdoc cref="DirectoryInfo.Create()"/>
        /// <exception cref="OutOfScopeException"></exception>
        public void Create()
        {
            // We don't necessarily care if subdirectories required are created as well. But, we can change this later. For now, it stays.
            throwIfNotWhitelisted();
            directoryInfo.Create();
        }

        /// <inheritdoc cref="DirectoryInfo.Delete(bool)"/>
        /// <exception cref="OutOfScopeException"></exception>
        public void Delete(bool recursive)
        {
            throwIfNotWhitelisted();
            // If recursive is true and we are in strict directories mode, we need to check all of the subdirectories and see if they are whitelisted.
            throwIfChildrenNotWhitelisted();
            directoryInfo.Delete(recursive);
        }
        /// <inheritdoc cref="DirectoryInfo.MoveTo(string)"/>
        /// <exception cref="OutOfScopeException"></exception>
        public void MoveTo(string path)
        {
            // Check if we have permission to 'modify' the current path.
            // Since we are going to (technically) be removing it, so we need to check if we have permission to do so.
            throwIfNotWhitelisted();
            throwIfChildrenNotWhitelisted();

            // Now, we are checking if we have permission to modify the new path.
            throwIfNotWhitelisted(path);
            throwIfChildrenNotWhitelisted(path);
            directoryInfo.MoveTo(path);
        }
        public override string ToString() => "DPDirectoryInfo: " + Path;

        #region Preview methods
        public bool PreviewCreate() => Whitelisted;
        public bool PreviewDelete(bool recursive)
        {
            if (!recursive) return Whitelisted;
            try
            {
                throwIfChildrenNotWhitelisted();
            }
            catch { return false; }
            return true;
        }
        public bool PreviewMoveTo(string path)
        {
            if (!Whitelisted && !Scope.IsDirectoryWhitelisted(path)) return false;
            try
            {
                throwIfChildrenNotWhitelisted();
                throwIfChildrenNotWhitelisted(path);
            }
            catch { return false;  }
            return true;
        }
        #endregion
        #region Try methods
        public bool TryCreate()
        {
            if (!Whitelisted) return false;
            try
            {
                Create();
                return true;
            }
            catch { return false; }
        }

        public bool TryDelete(bool recursive)
        {
            if (!recursive && !Whitelisted) return false;
            try
            {
                Delete(recursive);
                return true;
            } catch { return false; }
        }

        public bool TryMoveTo(string path)
        {
            if (!Whitelisted && !Scope.IsDirectoryWhitelisted(path)) return false;
            try
            {
                MoveTo(path);
                return true;
            } catch { return false; }
        }
        #endregion

        #region TryAndFix Methods
        /// <summary>
        /// Attempts to move the file, and if an exception is thrown (aside from <see cref="OutOfScopeException"/>), it will attempt to fix the problem.
        /// If it couldn't, it will be returned.
        /// </summary>
        /// <returns> Whether the operation successfully executed (whitelisted and no exception after attempted recovery).</returns>
        public bool TryAndFixMoveTo(string path, out Exception? exception)
        {
            exception = null;
            if (!Whitelisted || !directoryInfo.Exists) return false;
            var targetInfo = Context.CreateTempContext().CreateFileInfo(path);
            if (targetInfo.Exists && (targetInfo.Attributes.HasFlag(FileAttributes.ReadOnly) || targetInfo.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                try
                {
                    targetInfo.Attributes = FileAttributes.Normal;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return false;
                }
            }
            if (attemptWithRecovery(() => MoveTo(path), this, out exception)) return true;
            return false;
        }
        /// <summary>
        /// Attempts to move the file, and if an exception is thrown (aside from <see cref="OutOfScopeException"/>), it will attempt to fix the problem.
        /// If it couldn't, it will be returned.
        /// </summary>
        /// <returns> Whether the operation successfully executed (whitelisted and no exception after attempted recovery).</returns>
        public bool TryAndFixDelete(bool recursive, [NotNullWhen(false)] out Exception? exception)
        {
            exception = null;
            if (!Whitelisted || !directoryInfo.Exists) return false;
            if (attemptWithRecovery(() => Delete(recursive), this, out exception)) return true;
            return false;
        }
        #endregion
        #region Private methods
        private static IDPDirectoryInfo? tryCreateDirectoryInfoParent(DirectoryInfo info, DPAbstractIOContext ctx) => info.Parent == null ? null : new DPDirectoryInfo(info.Parent, ctx, null);
        private static IDPDirectoryInfo? tryCreateDirectoryInfoParent(IDirectoryInfo info, DPAbstractIOContext ctx) => info.Parent == null ? null : new DPDirectoryInfo(info.Parent, ctx, null);
        private void throwIfNotWhitelisted()
        {
            if (!Whitelisted) throw new OutOfScopeException(Path);
        }

        private void throwIfNotWhitelisted(string path)
        {
            if (!Scope.IsDirectoryWhitelisted(path)) throw new OutOfScopeException(path);
        }
        
        // TODO: This needs to be cached. Maybe
        private void throwIfChildrenNotWhitelisted()
        {
            var enumOptions = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
            };

            // Only do this one if explicit directory is set. Otherwise, do explict file paths, which will do the same thing.
            // This will prevent enumerating twice.
            if (Scope.ExplicitDirectoryPaths && !Scope.ExplicitFilePaths)
                foreach (var directory in directoryInfo.EnumerateDirectories("*", enumOptions))
                {
                    if (!Scope.IsDirectoryWhitelisted(directory.FullName))
                        throw new OutOfScopeException(directory.FullName, "Subdirectory is not whitelisted");
                }
            else if (Scope.ExplicitFilePaths)
                foreach (var file in directoryInfo.EnumerateFiles("*", enumOptions))
                {
                    if (!Scope.IsFilePathWhitelisted(file.FullName)) 
                        throw new OutOfScopeException(file.FullName, "File is not whitelisted");
                }
        }
        private void throwIfChildrenNotWhitelisted(string path) => new DPDirectoryInfo(path, context).throwIfChildrenNotWhitelisted();
        internal override void Invalidate()
        {
            whitelisted = Scope.IsFilePathWhitelisted(Path);
        }
        private static bool attemptWithRecovery(Action a, IDPDirectoryInfo info, [NotNullWhen(false)] out Exception? ex)
        {
            ex = null;
            try
            {
                if (!info.Exists) return false;
                a();
                return true;
            }
            // There are two possible reasons for this exception:
            // 1) The directory is readonly or hidden.
            // 2) A file in the directory is readonly or hidden.
            // 3) 1 and 2 but applied to subdirectories recursively.
            // For now, we will deal with 1.
            catch (UnauthorizedAccessException e)
            {
                try
                {
                    info.Attributes = FileAttributes.Normal;
                }
                catch (Exception e2)
                {
                    ex = new AggregateException(e, e2);
                    return false;
                }
                // Try again.
                try
                {
                    a(); return true;
                }
                catch (Exception e2)
                {
                    ex = e2;
                    return false;
                }
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        #endregion


    }
}
