using DAZ_Installer.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core.Tests.Fakes
{
    /// <summary>
    /// A fake <see cref="IDPAbstractNode"/> that can be used for testing with everything virtualized.
    /// </summary>
    /// <remarks>
    /// By default, uses the <see cref="DPAbstractNode"/> code excluding the <see cref="Parent"/> 
    /// property which simply sets the value as the parent <b>without modifying the folder's content list.</b>
    /// </remarks>
    public abstract class FakeDPAbstractNode : IDPAbstractNode
    {
        class AbstractNodeImpl : DPAbstractNode
        {
            public AbstractNodeImpl() : base() { }

            public override ILogger Logger { get; set; } = Log.Logger.ForContext<FakeDPAbstractNode>();

            protected override void UpdateParent(IDPFolder? parent) => this.parent = parent;
        }

        protected IDPAbstractNode node { get; set; } = new AbstractNodeImpl();

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual ILogger Logger { get => node.Logger; set => node.Logger = value; }

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual string FileName => node.FileName;

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual string Path { get => node.Path; set => node.Path = value; }

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual string NormalizedPath => node.NormalizedPath;

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual string Ext => node.Ext;

        /// <inheritdoc cref="IDPAbstractNode"/>
        /// <remarks>Simply sets value to parent; does not add to folder's content by default.</remarks>
        public virtual IDPFolder? Parent { get => node.Parent; set => node.Parent = value; }

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual IDPArchive? AssociatedArchive { get => node.AssociatedArchive; set => node.AssociatedArchive = value; }

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual string TargetPath { get => node.TargetPath; set => node.TargetPath = value; }

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual string RelativePathToContentFolder { get => node.RelativePathToContentFolder; set => node.RelativePathToContentFolder = value; }

        /// <inheritdoc cref="IDPAbstractNode"/>
        public virtual string RelativeTargetPath { get => node.RelativeTargetPath; set => node.RelativeTargetPath = value; }

        protected FakeDPAbstractNode(IDPAbstractNode node) => this.node = node;
        public FakeDPAbstractNode() { }
    }
}
