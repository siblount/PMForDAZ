namespace DAZ_Installer.Core.Tests.Fakes
{
    /// <summary>
    /// A fake implementation of <see cref="IDPFolder"/> for testing purposes.
    /// </summary>
    /// <remarks>All functions and properties are virtual.</remarks>
    public class FakeDPFolder : FakeDPAbstractNode, IDPFolder
    {
        /// <summary>
        /// By default, a <see cref="FakeDPFolder"/> will use a <see cref="FakeDPFolderFactory"/> to create subfolders."/>
        /// </summary>
        public virtual IDPFolderFactory FolderFactory { get; set; } = new FakeDPFolderFactory();

        public virtual List<IDPFolder> Subfolders { get; set; } = new(0);

        public virtual HashSet<IDPFile> Contents { get; set; } = new(0);

        public virtual bool IsContentFolder { get; set; } = false;

        public virtual bool IsPartOfContentFolder { get; set; } = false;

        public virtual void AddChild(IDPAbstractNode child)
        {
            if (child is not IDPFolder && child is not IDPFile)
                throw new ArgumentException("Child must be a IDPFolder or IDPFile.", nameof(child));

            if (child is IDPFolder folder)
                Subfolders.Add(folder);
            else if (child is IDPFile file && !Contents.Contains(file))
                Contents.Add(file);
        }
        public virtual void RemoveChild(IDPAbstractNode child)
        {
            if (child is not IDPFolder && child is not IDPFile)
                throw new ArgumentException("Child must be a IDPFolder or IDPFile.", nameof(child));

            if (child is IDPFolder folder)
                Subfolders.Remove(folder);
            else if (child is IDPFile file)
                Contents.Remove(file);
        }
        public virtual string CalculateChildRelativePath(IDPAbstractNode child) => throw new NotImplementedException();
        public virtual string CalculateChildRelativeTargetPath(IDPAbstractNode child, DPProcessSettings settings) => throw new NotImplementedException();
        public virtual IDPFolder? GetContentFolder() => throw new NotImplementedException();
        public virtual void UpdateChildrenRelativePaths(DPProcessSettings settings) => throw new NotImplementedException();
    }
}
