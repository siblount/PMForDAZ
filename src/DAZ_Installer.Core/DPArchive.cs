// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using IOPath = System.IO.Path;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// Defines the archive type of an archive.
    /// </summary>
    // TODO: Add a type for Multi-Product and Multi-Bundle to help determine whether
    // an archive should be added to the database/library
    public enum ArchiveType
    {
        Product, Bundle, Unknown
    }

    /// <summary>
    /// Defines the archive format of an archive.
    /// </summary>
    public enum ArchiveFormat
    {
        SevenZ, WinZip, RAR, Unknown
    }
    /// <summary>
    /// Abstract class for all supported archive files. 
    /// Currently the supported archive files are RAR, WinZip, and 7z (partially).
    /// </summary>
    public class DPArchive : DPFile
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPArchive>();
        public override string FileName => !IsInnerArchive ? FileInfo!.Name : IOPath.GetFileName(NormalizedPath);
        public override string Ext => IsInnerArchive ? base.Ext : GetExtension(FileInfo?.Name ?? string.Empty);
        /// <summary>
        /// The product name of the archive. If the archive has not been successfully processed, the product name will be equivalent to <see cref="FileName"/>.
        /// Otherwise, it is either the product name of the archive determined via the manifest file, a regex-filtered file name, or simply <see cref="FileName"/>.
        /// </summary>
        public virtual string ProductName => getProductName();
        /// <summary>
        /// The archive format of the archive. If the archive has not been successfully processed, the archive format will be equivalent to <see cref="ArchiveFormat.Unknown"/>.
        /// </summary>
        public ArchiveFormat ArchiveFormat { get; protected set; } = ArchiveFormat.Unknown;
        /// <summary>
        /// The extractor that the archive will use. This could be null if the <see cref="ArchiveFormat"/> is <see cref="ArchiveFormat.Unknown"/>. But after construction of this object, it is usually not null.
        /// </summary>
        public DPAbstractExtractor? Extractor { get; set; }
        /// <summary>
        /// The file system to use, this is derived from the <see cref="IDPIONode.FileSystem"/> which is from <see cref="DPFile.FileInfo"/>.
        /// </summary>
        public AbstractFileSystem FileSystem => FileInfo!.FileSystem;
        /// <summary>
        /// The name that will be used for the list view. The list name is the working archive's <c>FileName</c> + $"\\{Path}".
        /// </summary>
        /// <value>The working archive's FileName + $"\\{Path}".</value>
        public string ListName => AssociatedArchive is null ? string.Empty : AssociatedArchive.FileName + '\\' + Path;
        /// <summary>
        /// A list of archives that are children of this archive. 
        /// Or, in other words, archives that are contained within this archive.
        /// </summary>
        public List<DPArchive> Subarchives { get; init; } = new();
        /// <summary>
        /// A file that has been detected as a manifest file.
        /// </summary>
        // TODO: Make this a list of manifest files and fetch from here.
        public List<DPDSXFile> ManifestFiles { get; protected set; } = new(2);
        /// <summary>
        /// A file that has been detected as a supplement file.
        /// </summary>
        // TODO: Make this a list of supplement files and fetch from here.
        public List<DPDSXFile> SupplementFiles { get; protected set; } = new(1);
        /// <summary>
        /// A boolean value to describe if this archive is a child of another archive. Default is false.
        /// </summary>
        public bool IsInnerArchive => AssociatedArchive is not null;
        /// <summary>
        /// The type of this archive. Default is <see cref="ArchiveType.Unknown"/>.
        /// </summary>
        public ArchiveType Type { get; set; } = ArchiveType.Unknown;
        /// <summary>
        /// The product info connected to this archive.
        /// </summary>
        public DPProductInfo ProductInfo = new();

        /// <summary>
        /// A map of all of the folders parented to this archive.
        /// </summary>
        /// <typeparam name="string">The NormalizedPath of the Folder.</typeparam>
        /// <typeparam name="DPFolder">The folder.</typeparam>

        public Dictionary<string, DPFolder> Folders { get; } = new();

        /// <summary>
        /// A list of folders at the root level of this archive.
        /// </summary>
        public List<DPFolder> RootFolders { get; } = new();
        /// <summary>
        /// A dictionary of all of the contents and their normalized paths (<see cref="DPAbstractNode.NormalizedPath"/>) in the archive.
        /// </summary>
        /// <typeparam name="DPFile">The file content in this archive.</typeparam>
        /// <typeparam name="string">The <see cref="DPAbstractNode.NormalizedPath"/> of a file.</typeparam>
        public Dictionary<string, DPFile> Contents { get; } = new();

        /// <summary>
        /// A list of the root contents/ the contents at root level (DPAbstractFiles) of this archive.
        /// </summary>
        /// <typeparam name="DPAbstractFile">The file content in this archive.</typeparam>
        public List<DPFile> RootContents { get; } = new();
        /// <summary>
        /// A list of all .dsx files in this archive.
        /// </summary>
        /// <typeparam name="DPDSXFile">A file that is either a manifest, supplementary, or support file (.dsx).</typeparam>
        public List<DPDSXFile> DSXFiles { get; } = new();
        /// <summary>
        /// A list of all readable daz files in this archive. This consists of types with extension: .duf, .dsf.
        /// </summary>
        /// <typeparam name="DPDazFile">A file with the extension .duf OR .dsf.</typeparam>
        /// <returns></returns>
        public List<DPDazFile> DazFiles { get; } = new();
        /// <summary>
        /// The true uncompressed size of the archive contents in bytes.
        /// </summary>
        public ulong TrueArchiveSize { get; internal set; } = 0;
        /// <summary>
        /// The expected tag count for this archive. This value is updated when an applicable file has discovered new tags.
        /// </summary>
        public uint ExpectedTagCount { get; set; } = 0;

        /// <summary>
        /// The regex expression used for creating a product name.
        /// </summary>
        public static Regex ProductNameRegex { get; protected set; } = new(@"([^+|\-|_|\s]+)", RegexOptions.Compiled);

        public DPArchive() { }
        /// <summary>
        /// Create an archive that is on the disk.
        /// </summary>
        /// <param name="info">A FileInfo object that represents an archive on the file system.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is <see langword="null"/>.</exception>
        public DPArchive(IDPFileInfo info) : base(string.Empty, null, null)
        {
            FileInfo = info;
            // Try to determine the archive format NOW using most reliable method if possible.
            ArchiveFormat = GetArchiveFormat();
            Extractor = GetDefaultExtractorForArchiveFormat(ArchiveFormat);
        }
        public DPArchive(string _path, DPArchive? parent = null, DPFolder? parentFolder = null) : base(_path, parent, parentFolder)
        {
            // Make a file but we don't want to check anything.
            //if (IsInnerArchive) Parent = null;
            //else base.parent = null;
            RelativePathToContentFolder = FileName;
            ProductInfo = new DPProductInfo(IOPath.GetFileNameWithoutExtension(Path));
            parent?.Subarchives.Add(this);

            // Try to determine the archive format NOW using most reliable method if possible.
            ArchiveFormat = GetArchiveFormat();
            Extractor = GetDefaultExtractorForArchiveFormat(ArchiveFormat);
        }

        /// <summary>
        /// Constructor for testing purposes.
        /// 
        internal DPArchive(string _path, ILogger logger, IDPFileInfo info, DPAbstractExtractor extractor, DPArchive? parent = null, DPFolder? parentFolder = null) : base(_path, parent, parentFolder, info, logger)
        {
            // Make a file but we don't want to check anything.
            //if (IsInnerArchive) Parent = null;
            //else base.parent = null;
            RelativePathToContentFolder = FileName;
            ProductInfo = new DPProductInfo(IOPath.GetFileNameWithoutExtension(Path));
            parent?.Subarchives.Add(this);
            ArchiveFormat = GetArchiveFormat();
            Extractor = extractor;
        }

        // This is a great use for an AI solution.
        protected virtual string getProductName()
        {
            // If we found the product name from the manifest, then use that since it is the most reliable.
            if (!string.IsNullOrWhiteSpace(ProductInfo.ProductName)) return ProductInfo.ProductName;
            // otherwise, try to get the product name from the archive name.
            // Get the product name from the archive file name without extension.
            // Product name excludes any +, -, _, or whitespaces.
            var path = IOPath.GetFileNameWithoutExtension(IsInnerArchive ? FileName : FileInfo!.Name);
            var matches = ProductNameRegex.Matches(path);
            if (matches.Count == 0) return path;
            else return string.Join(' ', matches.Select(x => x.Value));
        }
        #region Public Methods
        /// <summary>
        /// Creates a new archive that lives on the disk and has no parent.
        /// </summary>
        /// <param name="info">The file info to use for I/O operations.</param>
        /// <returns>A new <see cref="DPArchive"/>.</returns>
        public static DPArchive CreateNewParentArchive(IDPFileInfo info) => new(info);
        /// <summary>
        /// Peeks the archive contents if possible and will extract ALL archive contents to <paramref name="destLocation"/>.
        /// </summary>
        /// <param name="destLocation">The destination path to extract the archive contents to.</param>
        /// <param name="tempLocation">The temporary path to extract the archive contents to.</param>
        /// <param name="overwrite">Determines whether to overwrite the files on disk if they exist.</param>
        /// <exception cref="ArgumentException"><paramref name="tempLocation"/> or <paramref name="destLocation"/> does not exist or do not have access to it.</exception>
        public DPExtractionReport ExtractAllContents(string tempLocation, bool overwrite = true) =>
            ExtractContents(new DPExtractSettings(tempLocation, Contents.Values, overwrite));
        /// <summary>
        /// Extracts contents from the archive (using <see cref="Extractor"/>), then peeks the archive contents if possible and 
        /// will attempt to extract files specifed in <see cref="DPExtractSettings.FilesToExtract"/> to <see cref="DPExtractSettings.TempPath"/>.
        /// </summary>
        /// <param name="settings">The settings to use for extraction.</param>
        /// <exception cref="IOException">Archive needed to be extracted first, but it failed to be extracted. </exception>
        public DPExtractionReport ExtractContents(DPExtractSettings settings)
        {
            if (!Extracted && !ExtractToTemp(settings))
                throw new IOException("Archive was not on disk and could not be extracted.");
            if (Extractor is null)
                throw new InvalidOperationException("Extractor is null. Cannot extract archive contents.");
            return Extractor.Extract(settings);
        }
        /// <summary>
        /// Extracts contents from the archive (using <see cref="Extractor"/>), then peeks the archive contents if possible and 
        /// will attempt to extract files specifed in <see cref="DPExtractSettings.FilesToExtract"/> to <see cref="DPExtractSettings.TempPath"/>.
        /// </summary>
        /// <param name="settings">The settings to use for extraction. <see cref="DPExtractSettings.DestinationPath"/> will be ignored.</param>
        /// <exception cref="IOException">Archive needed to be extracted first, but it failed to be extracted. </exception>
        public DPExtractionReport ExtractContentsToTemp(DPExtractSettings settings)
        {
            if (!Extracted && !ExtractToTemp(settings))
                throw new IOException("Archive was not on disk and could not be extracted.");
            if (Extractor is null)
                throw new InvalidOperationException("Extractor is null. Cannot extract archive contents.");
            return Extractor.ExtractToTemp(settings);
        }

        /// <summary>
        /// Previews the archive by discovering files in this archive. If the archive is not on disk, then it will be first extracted to <paramref name="temp"/>.
        /// If <paramref name="temp"/> is null, then it will be extracted to the temp directory.
        /// </summary>
        /// <param name="temp">The temp path to extract if the archive is not on disk, otherwise it will extract to <see cref="IOPath.GetTempPath"/></param>
        public void PeekContents(string? temp = null)
        {
            // Just extract to temp.
            var settings = new DPExtractSettings(temp ?? IOPath.GetTempPath(), Array.Empty<DPFile>(), archive: this);
            if (!Extracted && !ExtractToTemp(settings))
                throw new IOException("Archive was not on disk and could not be extracted.");
            if (Extractor is null)
                throw new InvalidOperationException("Extractor is null. Cannot peek archive contents.");
            Extractor.Peek(this);
        }

        /// <summary>
        /// Extracts the <paramref name="file"/> from the archive to the file's TargetPath. If this archive needs to be extracted first,
        /// then the archive will extract this archive first then extract the requested <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to extract.</param>
        /// <param name="tempLocation">The temp path to use if needed.</param>
        /// <param name="overwrite">Determines whether to overwrite the files on disk if they exist.</param>
        public bool ExtractContent(DPFile file, string tempLocation, bool overwrite = true) => ExtractContents(new DPExtractSettings(tempLocation, new DPFile[] { file }, overwrite)).SuccessPercentage == 1;

        /// <summary>
        ///  Checks whether or not the given ext is what is expected. Checks file headers. Does not throw exceptions.
        /// </summary>
        /// <returns>Returns an extension of the appropriate archive extraction method. 
        /// Returns <see cref="ArchiveFormat.Unknown"/> on errors and when it doesn't match archive magic strings.</returns>

        public static ArchiveFormat DetermineArchiveFormatPrecise(string location)
        {
            try
            {
                using FileStream stream = File.OpenRead(location);
                var bytes = new byte[8];
                stream.Read(bytes, 0, 8);
                stream.Close();
                // ZIP File Header
                // 	50 4B OR 	57 69
                if ((bytes[0] == 80 || bytes[0] == 87) && (bytes[1] == 75 || bytes[2] == 105))
                    return ArchiveFormat.WinZip;
                // RAR 5 consists of 8 bytes.  0x52 0x61 0x72 0x21 0x1A 0x07 0x01 0x00
                // RAR 4.x consists of 7. 0x52 0x61 0x72 0x21 0x1A 0x07 0x00
                // Rar!
                if (bytes[0] == 82 && bytes[1] == 97 && bytes[2] == 114 && bytes[3] == 33)
                    return ArchiveFormat.RAR;

                if (bytes[0] == 55 && bytes[1] == 122 && bytes[2] == 188 && bytes[3] == 175)
                    return ArchiveFormat.SevenZ;

                return ArchiveFormat.Unknown;
            }
            catch { return ArchiveFormat.Unknown; }
        }
        /// <summary>
        /// <inheritdoc cref="DetermineArchiveFormatPrecise(string)"/>
        /// </summary>
        /// <param name="stream">The stream to use.</param>
        /// <param name="closeWhenFinished">Determines whether to close the stream when finished.</param>
        /// <returns>Returns an extension of the appropriate archive extraction method. Otherwise, null.</returns>
        public static ArchiveFormat DetermineArchiveFormatPrecise(Stream stream, bool closeWhenFinished)
        {
            try
            {
                var bytes = new byte[8];
                stream.Read(bytes, 0, 8);
                if (closeWhenFinished) stream.Close();
                // ZIP File Header
                // 	50 4B OR 	57 69
                if ((bytes[0] == 80 || bytes[0] == 87) && (bytes[1] == 75 || bytes[2] == 105))
                    return ArchiveFormat.WinZip;
                // RAR 5 consists of 8 bytes.  0x52 0x61 0x72 0x21 0x1A 0x07 0x01 0x00
                // RAR 4.x consists of 7. 0x52 0x61 0x72 0x21 0x1A 0x07 0x00
                // Rar!
                if (bytes[0] == 82 && bytes[1] == 97 && bytes[2] == 114 && bytes[3] == 33)
                    return ArchiveFormat.RAR;

                if (bytes[0] == 55 && bytes[1] == 122 && bytes[2] == 188 && bytes[3] == 175)
                    return ArchiveFormat.SevenZ;

                return ArchiveFormat.Unknown;
            }
            catch { return ArchiveFormat.Unknown; }
        }

        /// <summary>
        /// Returns an enum describing the archive's format based on the file extension.
        /// This is used for determining archive files inside of an archive.
        /// </summary>
        /// <param name="path">The path of the archive.</param>
        /// <returns>A ArchiveFormat enum determining the archive format.</returns>
        public static ArchiveFormat DetermineArchiveFormat(string ext)
        {
            // ADDITONAL NOTE: This is called for determing archive files inside of an
            // archive file.
            ext = ext.ToLower();
            switch (ext)
            {
                case "7z":
                    return ArchiveFormat.SevenZ;
                case "rar":
                    return ArchiveFormat.RAR;
                case "zip":
                    return ArchiveFormat.WinZip;
                default:
                    if (uint.TryParse(ext, out var _)) return ArchiveFormat.SevenZ;
                    return ArchiveFormat.Unknown;
            }
        }
        /// <summary>
        /// Calls <see cref="DetermineArchiveFormat(string)"/> on the <see cref="Ext"/> property and 
        /// potentially calls <see cref="DetermineArchiveFormatPrecise(string)"/> if <see cref="Extracted"/> is true.
        /// </summary>
        /// <returns></returns>
        protected ArchiveFormat GetArchiveFormat()
        {
            ArchiveFormat result1 = DetermineArchiveFormat(Ext);
            if (!Extracted) return result1;
            ArchiveFormat result2 = DetermineArchiveFormatPrecise(FileInfo!.OpenRead(), true);
            return result2;
        }

        /// <summary>
        /// Updates the <see cref="Extractor"/> property. If <paramref name="extractor"/> is null, it will be set to the
        /// extractor that matches the <see cref="ArchiveFormat"/> property. <para/>
        /// If <see cref="ArchiveFormat"/> is <see cref="ArchiveFormat.Unknown"/>, <see cref="Extractor"/> will be set to null. <para/>
        /// If <paramref name="extractor"/> is not null, <see cref="Extractor"/> will be set to <paramref name="extractor"/> and no checks will be made.
        /// </summary>
        /// <param name="extractor"></param>
        public void SetExtractor(DPAbstractExtractor? extractor = null)
        {
            if (extractor != null)
            {
                Extractor = extractor;
                return;
            }
            ArchiveFormat = GetArchiveFormat();
            if (ArchiveFormat == ArchiveFormat.Unknown)
            {
                Extractor = null;
                return;
            }
            Extractor = GetDefaultExtractorForArchiveFormat(ArchiveFormat);
        }

        /// <summary>
        /// Returns the default extractor given an archive format. 
        /// </summary>
        /// <param name="format">The format to get default extractor for.</param>
        /// <returns>The default extractor, null if <paramref name="format"/> is <see cref="ArchiveFormat.Unknown"/></returns>
        public static DPAbstractExtractor? GetDefaultExtractorForArchiveFormat(ArchiveFormat format)
        {
            return format switch
            {
                ArchiveFormat.SevenZ => new DP7zExtractor(),
                ArchiveFormat.RAR => new DPRARExtractor(),
                ArchiveFormat.WinZip => new DPZipExtractor(),
                _ => null,
            };
        }

        /// <summary>
        /// Searches for all files that contains the name specified by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>The first file that contains <paramref name="name"/>; null if not found. </returns>
        public DPFile? FindFileViaNameContains(string name) => Contents.Values.First(x => x.FileName.Contains(name));

        private static List<string> ConvertDPFoldersToStringArr(Dictionary<string, DPFolder> folders)
        {
            var a = new List<string>(folders.Count);
            foreach (DPFolder v in folders.Values) a.Add(v.Path);
            return a;
        }

        /// <summary>
        /// This function should be called after all the files have been extracted. If no content folders have been found, this is a bundle.
        /// </summary>
        public ArchiveType DetermineArchiveType()
        {
            foreach (DPFolder folder in Folders.Values)
            {
                if (folder.IsContentFolder)
                {
                    return ArchiveType.Product;
                }
            }
            foreach (DPFile content in Contents.Values)
            {
                if (content is DPArchive) return ArchiveType.Bundle;
            }
            return ArchiveType.Unknown;

        }

        public int GetEstimateTagCount()
        {
            var count = 0;
            foreach (DPFile content in Contents.Values)
            {
                count += content.Tags.Count;
            }
            count += ProductInfo.Authors.Count;
            return count;
        }

        public DPFolder? FindParent(DPAbstractNode obj)
        {
            var fileName = PathHelper.GetFileName(obj.Path);
            // This means obj.Path contains trailing seperator, so do it again but without the seperator.
            if (fileName == string.Empty) fileName = IOPath.GetFileName(obj.Path.TrimEnd(PathHelper.GetSeperator(obj.Path)));
            var relativePathOnly = "";
            try
            {
                relativePathOnly = PathHelper.CleanDirPath(obj.Path.Remove(obj.Path.LastIndexOf(fileName)));
            }
            catch { }
            if (FindFolder(relativePathOnly, out DPFolder? folder))
            {
                return folder;
            }
            return null;
        }

        public bool FolderExists(string fPath) => Folders.ContainsKey(fPath);
        /// <summary>
        /// Simply finds the folder given a relative path.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public bool FindFolder(string relativePath, [NotNullWhen(true)] out DPFolder? folder)
        {
            var normalizedRelativePath = PathHelper.NormalizePath(relativePath);
            foreach (DPFolder _folder in Folders.Values)
            {
                if (_folder.NormalizedPath == normalizedRelativePath)
                {
                    folder = _folder;
                    return true;
                }
            }
            folder = null;
            return false;
        }

        /// <summary>
        /// Uses the <see cref="ProductNameRegex"/> to split the product name into tokens.
        /// </summary>
        /// <param name="name">The name of a file to split.</param>
        /// <returns>The tokens of <paramref name="name"/>.</returns>
        public static IEnumerable<string> RegexSplitName(string name) =>
            ProductNameRegex.Matches(name).Select(x => x.Value);

        #endregion
    }
}