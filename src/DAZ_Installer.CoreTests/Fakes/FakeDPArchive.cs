using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core.Tests.Fakes
{
    /// <summary>
    /// A fake <see cref="IDPArchive"/> for testing purposes.
    /// </summary>
    /// <remarks>Utilizes <see cref="DPArchive"/> for operations. All properties and methods are virtual.</remarks>
    public class FakeDPArchive : FakeDPFile, IDPArchive
    {
        private readonly DPArchive archive = new();

        /// <inheritdoc cref="DPArchive.ProductName"/>
        public virtual string ProductName => ((IDPArchive)archive).ProductName;

        /// <inheritdoc cref="DPArchive.ArchiveFormat"/>
        public virtual ArchiveFormat ArchiveFormat => ((IDPArchive)archive).ArchiveFormat;

        /// <inheritdoc cref="DPArchive.ExtractorFactory"/>
        public virtual IDPExtractorFactory ExtractorFactory { get => ((IDPArchive)archive).ExtractorFactory; set => ((IDPArchive)archive).ExtractorFactory = value; }

        /// <inheritdoc cref="DPArchive.FolderFactory"/>
        public virtual IDPFolderFactory FolderFactory { get => ((IDPArchive)archive).FolderFactory; set => ((IDPArchive)archive).FolderFactory = value; }

        /// <inheritdoc cref="DPArchive.FileFactory"/>
        public virtual IDPFileFactory FileFactory { get => ((IDPArchive)archive).FileFactory; set => ((IDPArchive)archive).FileFactory = value; }

        /// <inheritdoc cref="DPArchive.Extractor"/>
        public virtual DPAbstractExtractor? Extractor { get => ((IDPArchive)archive).Extractor; set => ((IDPArchive)archive).Extractor = value; }

        /// <inheritdoc cref="DPArchive.FileSystem"/>
        public virtual AbstractFileSystem FileSystem => ((IDPArchive)archive).FileSystem;

        /// <inheritdoc cref="DPArchive.ListName"/>
        public virtual string ListName => ((IDPArchive)archive).ListName;

        /// <inheritdoc cref="DPArchive.Subarchives"/>
        public virtual List<IDPArchive> Subarchives => ((IDPArchive)archive).Subarchives;

        /// <inheritdoc cref="DPArchive.ManifestFiles"/>
        public virtual List<IDPDSXFile> ManifestFiles => ((IDPArchive)archive).ManifestFiles;

        /// <inheritdoc cref="DPArchive.SupplementFiles"/>
        public virtual List<IDPDSXFile> SupplementFiles => ((IDPArchive)archive).SupplementFiles;

        /// <inheritdoc cref="DPArchive.IsInnerArchive"/>
        public virtual bool IsInnerArchive => ((IDPArchive)archive).IsInnerArchive;

        /// <inheritdoc cref="DPArchive.Type"/>
        public virtual ArchiveType Type { get => ((IDPArchive)archive).Type; set => ((IDPArchive)archive).Type = value; }

        /// <inheritdoc cref="DPArchive.ProductInfo"/>
        public virtual DPProductInfo ProductInfo { get => ((IDPArchive)archive).ProductInfo; set => ((IDPArchive)archive).ProductInfo = value; }

        /// <inheritdoc cref="DPArchive.Folders"/>
        public virtual Dictionary<string, IDPFolder> Folders => ((IDPArchive)archive).Folders;

        /// <inheritdoc cref="DPArchive.RootFolders"/>
        public virtual List<IDPFolder> RootFolders => ((IDPArchive)archive).RootFolders;

        /// <inheritdoc cref="DPArchive.Contents"/>
        public virtual Dictionary<string, IDPFile> Contents => ((IDPArchive)archive).Contents;

        /// <inheritdoc cref="DPArchive.RootContents"/>
        public virtual List<IDPFile> RootContents => ((IDPArchive)archive).RootContents;

        /// <inheritdoc cref="DPArchive.DSXFiles"/>
        public virtual List<IDPDSXFile> DSXFiles => ((IDPArchive)archive).DSXFiles;

        /// <inheritdoc cref="DPArchive.DazFiles"/>
        public virtual List<IDPDazFile> DazFiles => ((IDPArchive)archive).DazFiles;

        /// <inheritdoc cref="DPArchive.TrueArchiveSize"/>
        public virtual ulong TrueArchiveSize { get => ((IDPArchive)archive).TrueArchiveSize; set => ((IDPArchive)archive).TrueArchiveSize = value; }

        /// <inheritdoc cref="DPArchive.ExpectedTagCount"/>
        public virtual uint ExpectedTagCount { get => ((IDPArchive)archive).ExpectedTagCount; set => ((IDPArchive)archive).ExpectedTagCount = value; }

        /// <inheritdoc cref="DPArchive.DetermineArchiveType"/>
        public virtual ArchiveType DetermineArchiveType() => ((IDPArchive)archive).DetermineArchiveType();

        /// <inheritdoc cref="DPArchive.ExtractAllContents"/>
        public virtual DPExtractionReport ExtractAllContents(string tempLocation, bool overwrite = true) => ((IDPArchive)archive).ExtractAllContents(tempLocation, overwrite);

        /// <inheritdoc cref="DPArchive.ExtractContent"/>
        public virtual bool ExtractContent(IDPFile file, string tempLocation, bool overwrite = true) => ((IDPArchive)archive).ExtractContent(file, tempLocation, overwrite);

        /// <inheritdoc cref="DPArchive.ExtractContents"/>
        public virtual DPExtractionReport ExtractContents(DPExtractSettings settings) => ((IDPArchive)archive).ExtractContents(settings);

        /// <inheritdoc cref="DPArchive.ExtractContentsToTemp"/>
        public virtual DPExtractionReport ExtractContentsToTemp(DPExtractSettings settings) => ((IDPArchive)archive).ExtractContentsToTemp(settings);

        /// <inheritdoc cref="DPArchive.FindFileViaNameContains"/>
        public virtual IDPFile? FindFileViaNameContains(string name) => ((IDPArchive)archive).FindFileViaNameContains(name);

        /// <inheritdoc cref="DPArchive.FindFolder"/>
        public virtual bool FindFolder(string relativePath, out IDPFolder? folder) => ((IDPArchive)archive).FindFolder(relativePath, out folder);

        /// <inheritdoc cref="DPArchive.FindParent"/>
        public virtual IDPFolder? FindParent(IDPAbstractNode obj) => ((IDPArchive)archive).FindParent(obj);

        /// <inheritdoc cref="DPArchive.FolderExists"/>
        public virtual bool FolderExists(string fPath) => ((IDPArchive)archive).FolderExists(fPath);

        /// <inheritdoc cref="DPArchive.GetEstimateTagCount"/>
        public virtual int GetEstimateTagCount() => ((IDPArchive)archive).GetEstimateTagCount();

        /// <inheritdoc cref="DPArchive.PeekContents"/>
        public virtual void PeekContents(string? temp = null) => ((IDPArchive)archive).PeekContents(temp);

        /// <summary>
        /// An empty constructor that does nothing.
        /// </summary>
        public FakeDPArchive() { }

        /// <inheritdoc cref="DPArchive.DPArchive(IDPFileInfo)"/>
        public FakeDPArchive(IDPFileInfo fileInfo) => archive = new DPArchive(fileInfo);

        /// <summary>
        /// Creates a new <see cref="FakeDPArchive"/> with the given path, archive, and parent.
        /// </summary>
        /// <remarks>Utilizies <see cref="DPArchive"/> for operations.</remarks>
        /// <param name="path">The path to set.</param>
        /// <param name="archive">The parent archive of this archive, if any.</param>
        /// <param name="parent">The parent of this archive, if any.</param>
        public FakeDPArchive(string path, IDPArchive? archive = null, IDPFolder? parent = null)
        {
            archive = new DPArchive(path, archive, parent);
        }
    }
}
