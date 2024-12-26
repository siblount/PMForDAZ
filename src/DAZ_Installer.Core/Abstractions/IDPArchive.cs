using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// An interface for all archive types.
    /// </summary>
    public interface IDPArchive : IDPFile
    {
        /// <summary>
        /// The product name of the archive.
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// The archive format of the archive.
        /// </summary>
        ArchiveFormat ArchiveFormat { get; }

        /// <summary>
        /// The default extractor factory to use for creating extractors.
        /// </summary>
        IDPExtractorFactory ExtractorFactory { get; set; }

        /// <summary>
        /// The default folder factory to use for creating folders.
        /// </summary>
        IDPFolderFactory FolderFactory { get; set; }

        /// <summary>
        /// The default file factory to use for creating file.
        /// </summary>
        IDPFileFactory FileFactory { get; set; }

        /// <summary>
        /// The extractor that the archive will use.
        /// </summary>
        DPAbstractExtractor? Extractor { get; set; }

        /// <summary>
        /// The file system to use.
        /// </summary>
        AbstractFileSystem FileSystem { get; }

        /// <summary>
        /// A list of archives that are children of this archive. 
        /// Or, in other words, archives that are contained within this archive.
        /// </summary>
        List<IDPArchive> Subarchives { get; }

        /// <summary>
        /// A list of Manifest files.
        /// </summary>
        List<IDPDSXFile> ManifestFiles { get; }

        /// <summary>
        /// A list of Supplement files.
        /// </summary>
        List<IDPDSXFile> SupplementFiles { get; }

        /// <summary>
        /// A boolean value to describe if this archive is a child of another archive.
        /// </summary>
        bool IsInnerArchive { get; }

        /// <summary>
        /// The type of this archive. Default is <see cref="ArchiveType.Unknown"/>.
        /// </summary>
        ArchiveType Type { get; set; }

        /// <summary>
        /// The product info connected to this archive.
        /// </summary>
        DPProductInfo ProductInfo { get; set; }

        /// <summary>
        /// A map of all of the folders parented to this archive.
        /// </summary>
        /// <typeparam name="string">The <see cref="IDPAbstractNode.NormalizedPath"/> of the folder.</typeparam>
        /// <typeparam name="IDPFolder">The folder</typeparam>
        Dictionary<string, IDPFolder> Folders { get; }

        /// <summary>
        /// A list of folders at the root level of this archive.
        /// </summary>
        List<IDPFolder> RootFolders { get; }

        /// <summary>
        /// A dictionary of all of the contents and their normalized paths (<see cref="DPAbstractNode.NormalizedPath"/>) in the archive.
        /// </summary>
        /// <typeparam name="DPFile">The file content in this archive.</typeparam>
        /// <typeparam name="string">The <see cref="DPAbstractNode.NormalizedPath"/> of a file.</typeparam>
        Dictionary<string, IDPFile> Contents { get; }

        /// <summary>
        /// A list of the root contents/ the contents at root level (DPAbstractFiles) of this archive.
        /// </summary>
        /// <typeparam name="IDPFile">The file in this archive.</typeparam>

        List<IDPFile> RootContents { get; }

        /// <summary>
        /// A list of all .dsx files in this archive.
        /// </summary>
        /// <typeparam name="DPDSXFile">A file that is either a manifest, supplementary, or support file (.dsx).</typeparam>
        List<IDPDSXFile> DSXFiles { get; }

        /// <summary>
        /// A list of all readable daz files in this archive. This consists of types with extension: .duf, .dsf.
        /// </summary>
        /// <typeparam name="DPDazFile">A file with the extension .duf OR .dsf.</typeparam>
        List<IDPDazFile> DazFiles { get; }

        /// <summary>
        /// The true uncompressed size of the archive contents in bytes.
        /// </summary>
        /// <remarks>
        /// <b>This value should ONLY be updated by an instance of a <see cref="DPAbstractExtractor"/>.</b>
        /// Otherwise, do not edit this value.
        /// </remarks> 
        ulong TrueArchiveSize { get; set; }

        /// <summary>
        /// The expected tag count for this archive. This value is updated when an applicable file has discovered new tags.
        /// </summary>
        uint ExpectedTagCount { get; set; }

        /// <summary>
        /// Peeks the archive contents if possible and will extract ALL archive contents to <paramref name="destLocation"/>.
        /// </summary>
        /// <param name="destLocation">The destination path to extract the archive contents to.</param>
        /// <param name="tempLocation">The temporary path to extract the archive contents to.</param>
        /// <param name="overwrite">Determines whether to overwrite the files on disk if they exist.</param>
        /// <exception cref="ArgumentException"><paramref name="tempLocation"/> or <paramref name="destLocation"/> does not exist or do not have access to it.</exception>
        DPExtractionReport ExtractAllContents(string tempLocation, bool overwrite = true);

        /// <summary>
        /// Extracts contents from the archive (using <see cref="Extractor"/>), then peeks the archive contents if possible and 
        /// will attempt to extract files specifed in <see cref="DPExtractSettings.FilesToExtract"/> to <see cref="DPExtractSettings.TempPath"/>.
        /// </summary>
        /// <param name="settings">The settings to use for extraction.</param>
        /// <exception cref="IOException">Archive needed to be extracted first, but it failed to be extracted. </exception>
        DPExtractionReport ExtractContents(DPExtractSettings settings);

        /// <summary>
        /// Extracts contents from the archive (using <see cref="Extractor"/>), then peeks the archive contents if possible and 
        /// will attempt to extract files specifed in <see cref="DPExtractSettings.FilesToExtract"/> to <see cref="DPExtractSettings.TempPath"/>.
        /// </summary>
        /// <param name="settings">The settings to use for extraction. <see cref="DPExtractSettings.DestinationPath"/> will be ignored.</param>
        /// <exception cref="IOException">Archive needed to be extracted first, but it failed to be extracted. </exception>
        DPExtractionReport ExtractContentsToTemp(DPExtractSettings settings);

        /// <summary>
        /// Previews the archive by discovering files in this archive.
        /// </summary>
        /// <param name="temp">The temp path to extract if the archive is not on disk.</param>
        void PeekContents(string? temp = null);

        /// <summary>
        /// Extracts the <paramref name="file"/> from the archive to the file's TargetPath.
        /// </summary>
        /// <param name="file">The file to extract.</param>
        /// <param name="tempLocation">The temp path to use if needed.</param>
        /// <param name="overwrite">Determines whether to overwrite the files on disk if they exist.</param>
        bool ExtractContent(IDPFile file, string tempLocation, bool overwrite = true);

        /// <summary>
        /// Returns the archive type of this archive given various factors.
        /// </summary>
        ArchiveType DetermineArchiveType();

        /// <summary>
        /// Gets an estimate of the tag count for this archive.
        /// </summary>
        /// <returns>The estimated tag count.</returns>
        int GetEstimateTagCount();

        /// <summary>
        /// Finds the parent folder of the given object.
        /// </summary>
        /// <param name="obj">The object to find the parent for.</param>
        /// <returns>The parent folder, or null if not found.</returns>
        IDPFolder? FindParent(IDPAbstractNode obj);

        /// <summary>
        /// Checks if a folder exists at the given path.
        /// </summary>
        /// <param name="fPath">The path to check.</param>
        /// <returns>True if the folder exists, false otherwise.</returns>
        bool FolderExists(string fPath);

        /// <summary>
        /// Simply finds the folder given a relative path.
        /// </summary>
        /// <param name="relativePath">The relative path to search for.</param>
        /// <param name="folder">The found folder, if any.</param>
        /// <returns>True if the folder was found, false otherwise.</returns>
        bool FindFolder(string relativePath, out IDPFolder? folder);
    }
}