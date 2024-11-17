using DAZ_Installer.IO;

namespace DAZ_Installer.Core.Tests.Fakes
{
    /// <summary>
    /// A fake <see cref="IDPFile"/> that can be used for testing with everything virtualized.
    /// </summary>
    /// <remarks>By default, uses <see cref="DPFile"/> code.</remarks>
    public class FakeDPFile : FakeDPAbstractNode, IDPFile
    {
        private readonly IDPFile file = new DPFile();
        /// <inheritdoc cref="IDPFile"/>/>
        public IDPFileInfo? FileInfo { get => file.FileInfo; set => file.FileInfo = value; }
        /// <inheritdoc cref="IDPFile"/>/>
        public List<string> Tags { get => file.Tags; set => file.Tags = value; }
        /// <inheritdoc cref="IDPFile"/>/>
        public bool Extracted => file.Extracted;
        /// <inheritdoc cref="IDPFile"/>/>
        public bool ExtractedToTarget => file.ExtractedToTarget;

        /// <summary>
        /// An empty constructor that does nothing.
        /// </summary>
        public FakeDPFile() { }
        /// <summary>
        /// A constructor that simply sets the parameters with no side-effect.
        /// </summary>
        /// <remarks>
        /// In the real <see cref="DPFile"/>, setting <paramref name="parent"/> would 
        /// add the file to the folder's content among other things.
        /// </remarks>
        /// <param name="path">The path to set.</param>
        /// <param name="parent">The parent to set with no side-effects whatsoever.</param>
        /// <param name="archive">The associated archive to set</param>
        public FakeDPFile(string path, IDPFolder? parent = null, IDPArchive? archive = null)
        {
            Path = path;
            Parent = parent;
            AssociatedArchive = archive;
        }
    }
}
