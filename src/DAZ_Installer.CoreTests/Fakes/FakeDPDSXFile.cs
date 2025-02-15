
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;

namespace DAZ_Installer.Core.Tests.Fakes
{
    /// <summary>
    /// A fake <see cref="IDPFile"/> that can be used for testing with everything virtualized.
    /// </summary>
    /// <remarks>By default, uses <see cref="DPFile"/> code.</remarks>
    public class FakeDPDSXFile : FakeDPAbstractNode, IDPDSXFile
    {
        private DPFile file => (DPFile)node;
        /// <inheritdoc cref="IDPFile"/>/>
        public virtual IDPFileInfo? FileInfo { get => file.FileInfo; set => file.FileInfo = value; }
        /// <inheritdoc cref="IDPFile"/>/>
        public virtual List<string> Tags { get => file.Tags; set => file.Tags = value; }
        /// <inheritdoc cref="IDPFile"/>/>
        public virtual bool Extracted => file.Extracted;
        /// <inheritdoc cref="IDPFile"/>/>
        public virtual bool ExtractedToTarget => file.ExtractedToTarget;

        public virtual DPContentInfo ContentInfo { get; set; }

        /// <summary>
        /// An empty constructor that does nothing.
        /// </summary>
        public FakeDPDSXFile() : base(new DPFile()){ }
        /// <summary>
        /// A regular constructor that sets the path, parent, and archive with no side-effects. Optionally, adds the file to the archive.
        /// </summary>
        /// <remarks>
        /// In the real <see cref="DPFile"/>, setting <paramref name="parent"/> would 
        /// add the file to the folder's content among other things.
        /// </remarks>
        /// <param name="path">The path to set.</param>
        /// <param name="parent">The parent to set with no side-effects whatsoever.</param>
        /// <param name="archive">The associated archive to set</param>
        /// <param name="addToArchive">Calls <see cref="AddToArchive"/> if true.</param>
        public FakeDPDSXFile(string path, IDPFolder? parent = null, IDPArchive? archive = null, bool addToArchive = true) : this()
        {
            Path = path;
            Parent = parent;
            AssociatedArchive = archive;
            if (addToArchive) AddToArchive();
        }

        public virtual void CheckContents(StreamReader stream) {}
        public virtual Dictionary<string, string> GetManifestDestinations() => throw new NotImplementedException();
        /// <summary>
        /// Adds the file to the associated archive if it exists.
        /// </summary>
        /// <remarks>
        /// If the file is named "Manifest.dsx", it will also be added to the archive's manifest files.
        /// Nothing happens if archive is null.
        /// </remarks>
        public void AddToArchive() {
            AssociatedArchive?.DSXFiles.Add(this);
            if (FileName == "Manifest.dsx") AssociatedArchive?.ManifestFiles.Add(this);
        }

        public bool Extract(DPExtractSettings settings) => file.Extract(settings);

        public bool Extract(DPExtractSettings settings, string dest) => file.Extract(settings, dest);

        public bool ExtractToTemp(DPExtractSettings settings) => file.ExtractToTemp(settings);
    }
}
