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
        SevenZ, PKZip, RAR, Unknown
    }

    /// <summary>
    /// Abstract class for all supported archive files. 
    /// Currently the supported archive files are RAR, WinZip, and 7z (partially).
    /// </summary>
    public partial class DPArchive : DPFile, IDPArchive
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPArchive>();
        public override string FileName
        {
            get
            {
                if (!IsInnerArchive && FileInfo is not null) return FileInfo.Name;
                else if (!IsInnerArchive) Logger.Warning("Expected FileInfo to be not null when IsInnerArchive is true. Falling back to base.");
                return base.FileName;
            }
        }
        public override string Ext
        {
            get
            {
                if (!IsInnerArchive && FileInfo is not null) return GetExtension(FileInfo.Name ?? string.Empty);
                else if (IsInnerArchive) Logger.Warning("Expected FileInfo to be not null when IsInnerArchive is true. Falling back to base.");
                return base.Ext;
            }
        }
        /// <summary>
        /// The product name of the archive. If the archive has not been successfully processed, the product name will be equivalent to <see cref="FileName"/>.
        /// Otherwise, it is either the product name of the archive determined via the manifest file, a regex-filtered file name, or simply <see cref="FileName"/>.
        /// </summary>
        public virtual string ProductName => getProductName();
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks> If the archive has not been successfully processed, the archive format will be equivalent to <see cref="ArchiveFormat.Unknown"/>.</remarks>
        public ArchiveFormat ArchiveFormat { get; protected set; } = ArchiveFormat.Unknown;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// If this archive was NOT constructed by <see cref="DPArchive.DPArchive(IDPFileInfo)"/>, it will be 
        /// inherited by the parent archive's <see cref="ExtractorFactory"/> if both the ExtractorFactory 
        /// and the <see cref="DPAbstractNode.AssociatedArchive"/> are not null.
        /// </remarks>
        /// <returns>
        /// By default, a <see cref="DPExtractorFactory"/> is returned; otherwise, <see cref="IDPExtractorFactory"/>.
        /// Furthermore, children of this archive will inherit this factory at construction time.
        /// </returns>
        public IDPExtractorFactory ExtractorFactory { get; set; } = DPExtractorFactory.Singleton;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks> 
        /// By default, a <see cref="DPFolderFactory"/> is returned; otherwise, <see cref="IDPFolderFactory"/>.
        /// Furthermore, children of this archive will inherit this factory at construction time.
        /// </remarks>
        public override IDPFolderFactory FolderFactory { get; set; } = DPFolderFactory.Instance;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks> 
        /// By default, a <see cref="DPFileFactory"/> is returned; otherwise, <see cref="IDPFileFactory"/>.
        /// Furthermore, children of this archive will inherit this factory at construction time.
        /// </remarks>
        public IDPFileFactory FileFactory { get; set; } = DPFileFactory.Instance;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// This could be null if the <see cref="ArchiveFormat"/> is <see cref="ArchiveFormat.Unknown"/>. 
        /// But after construction of this object, it is usually not null.
        /// </remarks>
        public DPAbstractExtractor? Extractor { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// This is derived from the <see cref="IDPIONode.FileSystem"/> which is from <see cref="IDPFile.FileInfo"/>.
        /// If the <see cref="DPFile.FileInfo"/> is null, a new readonly <see cref="DPFileSystem"/> is returned.
        /// </remarks>
        public AbstractFileSystem FileSystem => FileInfo?.FileSystem ?? new DPFileSystem();
        /// <inheritdoc/>
        public List<IDPArchive> Subarchives { get; init; } = new();
        /// <inheritdoc/>
        public List<IDPDSXFile> ManifestFiles { get; protected set; } = new(2);
        /// <inheritdoc/>
        public List<IDPDSXFile> SupplementFiles { get; protected set; } = new(1);
        /// <inheritdoc/>
        public bool IsInnerArchive => AssociatedArchive is not null;
        /// <summary>
        /// The type of this archive. Default is <see cref="ArchiveType.Unknown"/>.
        /// </summary>
        public ArchiveType Type { get; set; } = ArchiveType.Unknown;
        /// <inheritdoc/>
        public DPProductInfo ProductInfo { get; set; } = new();

        /// <inheritdoc/>
        public Dictionary<string, IDPFolder> Folders { get; } = new();

        /// <inheritdoc/>
        public List<IDPFolder> RootFolders { get; } = new();

        /// <inheritdoc/>
        public Dictionary<string, IDPFile> Contents { get; } = new();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="IDPFile">The file content in this archive.</typeparam>
        public List<IDPFile> RootContents { get; } = new();
        /// <inheritdoc/>
        public List<IDPDSXFile> DSXFiles { get; } = new();
        /// <inheritdoc/>
        public List<IDPDazFile> DazFiles { get; } = new();
        /// <inheritdoc/>
        public ulong TrueArchiveSize { get; set; } = 0;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// This value is updated when an applicable file has discovered new tags.
        /// </remarks>
        public uint ExpectedTagCount { get; set; } = 0;

        /// <summary>
        /// The regex expression used for creating a product name.
        /// </summary>
        public static Regex ProductNameRegex { get; protected set; } = new(@"([^+|\-|_|\s]+)", RegexOptions.Compiled);


        [GeneratedRegex(@"\.part(\d)+$", RegexOptions.IgnoreCase)]
        private static partial Regex PartNumberRARRegex();

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
            Extractor = ExtractorFactory.CreateExtractor(ArchiveFormat);
        }
        /// <summary>
        /// The constructor used for creating an archive that may be a child of another archive.
        /// </summary>
        /// <remarks>
        /// The <see cref="ExtractorFactory"/> will be inherited from the parent archive if both the ExtractorFactory
        /// and the <see cref="DPAbstractNode.AssociatedArchive"/> are not null.
        /// </remarks>
        /// <param name="_path">The path of the archive.</param>
        /// <param name="parent">The parent archive of this archive.</param>
        /// <param name="parentFolder">The parent folder of this archive.</param>
        /// <inheritdoc/>
        public DPArchive(string _path, IDPArchive? parent = null, IDPFolder? parentFolder = null) : base(_path, parent, parentFolder)
        {
            RelativePathToContentFolder = FileName;
            ProductInfo = new DPProductInfo(IOPath.GetFileNameWithoutExtension(Path));
            parent?.Subarchives.Add(this);

            // Try to determine the archive format NOW using most reliable method if possible.
            ArchiveFormat = GetArchiveFormat();
            if (parent is { ExtractorFactory: not null }) ExtractorFactory = parent.ExtractorFactory;
            Extractor = ExtractorFactory.CreateExtractor(ArchiveFormat);
        }

        /// <summary>
        /// Constructor for testing purposes.
        /// </summary>
        internal DPArchive(string _path, ILogger logger, IDPFileInfo info, DPAbstractExtractor extractor, IDPArchive? parent = null, IDPFolder? parentFolder = null) : base(_path, parent, parentFolder, info, logger)
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
        /// <inheritdoc/>
        public DPExtractionReport ExtractAllContents(string tempLocation, bool overwrite = true) =>
            ExtractContents(new DPExtractSettings(tempLocation, Contents.Values, overwrite));

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">If <see cref="Extractor"/> is null.</exception>
        public DPExtractionReport ExtractContents(DPExtractSettings settings)
        {
            if (FileInfo is null or { Exists: false } && !ExtractToTemp(settings))
                throw new IOException("Archive was not on disk and could not be extracted.");
            if (Extractor is null)
                throw new InvalidOperationException("Extractor is null. Cannot extract archive contents.");
            return Extractor.Extract(settings);
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">If <see cref="Extractor"/> is null.</exception>
        public DPExtractionReport ExtractContentsToTemp(DPExtractSettings settings)
        {
            if (FileInfo is null or { Exists: false } && !ExtractToTemp(settings))
                throw new IOException("Archive was not on disk and could not be extracted.");
            if (Extractor is null)
                throw new InvalidOperationException("Extractor is null. Cannot extract archive contents.");
            return Extractor.ExtractToTemp(settings);
        }

        /// <summary>
        /// Previews the archive by discovering files in this archive. 
        /// </summary>
        /// <remarks>
        /// If the archive is not on disk, then it will be first extracted to <paramref name="temp"/>.
        /// If <paramref name="temp"/> is null, then it will be extracted to the temp directory.
        /// </remarks>
        /// <param name="temp">The temp path to extract if the archive is not on disk, otherwise it will extract to <see cref="IOPath.GetTempPath"/></param>
        public void PeekContents(string? temp = null)
        {
            // Just extract to temp.
            var settings = new DPExtractSettings(temp ?? IOPath.GetTempPath(), Array.Empty<IDPFile>(), archive: this);
            if (FileInfo is null or { Exists: false } && !ExtractToTemp(settings))
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
        /// <exception cref="ArgumentException">If <paramref name="file"/> is not a child of this archive.</exception>
        public bool ExtractContent(IDPFile file, string tempLocation, bool overwrite = true) {
            if (!Contents.ContainsKey(file.NormalizedPath))
                throw new ArgumentException($"Cannot extract file {file.Path} that is not a child of archive {FileName}", nameof(file));
            return ExtractContents(new DPExtractSettings(tempLocation, [file], overwrite)).SuccessPercentage == 1;
        }

        /// <summary>
        /// Checks whether or not the given ext is what is expected. Checks file headers. Does not throw exceptions.
        /// </summary>
        /// <param name="stream">The stream to use.</param>
        /// <param name="closeWhenFinished">Determines whether to close the stream when finished.</param>
        /// <returns>Returns an extension of the appropriate archive extraction method. Otherwise, null.</returns>
        public static ArchiveFormat DetermineArchiveFormatPrecise(Stream stream, bool closeWhenFinished)
        {
            Span<byte> zipFileHeaders = [0x50, 0x4B];
            Span<byte> RAR5FileHeaders = [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00];
            Span<byte> RAR4FileHeaders = [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00];
            Span<byte> sevenZFileHeaders = [0x37, 0x7A, 0xBC, 0xAF];

            try
            {
                Span<byte> bytes = stackalloc byte[8];
                stream.Read(bytes);

                // ZIP File Header (PKZip, not WinZip)
                // 	50 4B
                if (bytes[..2].SequenceEqual(zipFileHeaders[..2]))
                    return ArchiveFormat.PKZip;
                if (bytes[..4].SequenceEqual(sevenZFileHeaders))
                    return ArchiveFormat.SevenZ;
                // RAR 5 consists of 8 bytes.  0x52 0x61 0x72 0x21 0x1A 0x07 0x01 0x00
                // RAR 4.x consists of 7.      0x52 0x61 0x72 0x21 0x1A 0x07 0x00
                // Rar!
                if (bytes.SequenceEqual(RAR5FileHeaders) || bytes[..(RAR4FileHeaders.Length)].SequenceEqual(RAR4FileHeaders))
                    return ArchiveFormat.RAR;
                return ArchiveFormat.Unknown;
            }
            catch (Exception e) { 
                Log.Error(e, "Failed to accurately determine archive format");  
                return ArchiveFormat.Unknown; 
            }
            finally
            {
                if (closeWhenFinished) stream.Close();
            }
        }

        /// <summary>
        /// Returns an enum describing the archive's format based on the file extension.
        /// This is used for determining archive files inside of an archive.
        /// </summary>
        /// <param name="path">The path of the archive.</param>
        /// <returns>A ArchiveFormat enum determining the archive format.</returns>
        public static ArchiveFormat DetermineArchiveFormat(string ext)
        {
            return ext.ToLower() switch
            {
                "7z" or "001" => ArchiveFormat.SevenZ,
                "rar" => ArchiveFormat.RAR,
                "zip" => ArchiveFormat.PKZip,
                _ => ArchiveFormat.Unknown,
            };
        }

        /// <summary>
        /// Quickly checks if an given archive file can be processed.
        /// </summary>
        /// <remarks>
        /// An slightly more extensive check than simply gathering the archive format. This is
        /// should be primarily used as a validation check whether inputted files will process.
        /// </remarks>
        /// <example>
        /// A rar file with the extension `test.part2.rar` would be given an <see cref="ArchiveFormat.RAR"/>
        /// format. However, this would cause errors since <see cref="DPRARExtractor"/> only processes the 
        /// first part of the archive.
        /// </example>
        /// <param name="fileInfo">The file info of the archive on disk to validate</param>
        /// <returns>Whether the archive is supported by DP extractors.</returns>
        public static bool IsValidSupportedArchive(IDPFileInfo fileInfo) {
            var ext = GetExtension(fileInfo.Name);
            var nameWithoutExtension = IOPath.GetFileNameWithoutExtension(fileInfo.Name);
            var hasDotInNameWithoutExtension = nameWithoutExtension.LastIndexOf('.') != -1;
            var format = DetermineArchiveFormat(ext);

            if (format == ArchiveFormat.RAR) {
                return !hasDotInNameWithoutExtension || IsFirstPartRarFile(nameWithoutExtension);
            } else if (format != ArchiveFormat.Unknown) return true;

            // If the format is unknown based on extension, check the file header
            using var stream = fileInfo.OpenRead();
            format = DetermineArchiveFormatPrecise(stream, true);
            return format != ArchiveFormat.Unknown;
        }

        /// <summary>
        /// Determines if a given name indicates the first part of a multi-part RAR file.
        /// </summary>
        /// <remarks>
        /// Call <see cref="IOPath.GetFileNameWithoutExtension(string?)"/> first and pass it as an argument to
        /// <paramref name="nameWithoutExtension"/>.
        /// </remarks>
        /// <param name="nameWithoutExtension">The name without the extension.</param>
        private static bool IsFirstPartRarFile(string nameWithoutExtension) {
            var match = PartNumberRARRegex().Match(nameWithoutExtension);
            if (!match.Success) return true;
            if (int.TryParse(match.Groups[1].Value, out int partNumber)) {
                return partNumber == 1;
            }
            return false;
        }

        /// <summary>
        /// Determines if the given extension (without the dot) represents a multi-part 7z file.
        /// </summary>
        /// <param name="extension">The file extension without the leading dot.</param>
        /// <returns>True if the extension represents a multi-part 7z file, otherwise false.</returns>
        private static bool IsMultiPart7zExtension(string extension)
        {
            // Check if the extension is exactly 3 digits
            if (extension.Length != 3 || !int.TryParse(extension, out int partNumber))
            {
                return false;
            }

            // Check if the number is between 001 and 999
            return partNumber >= 1 && partNumber <= 999;
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
        /// Determines the archive type of this archive.
        /// </summary>
        /// <remarks>
        /// This function should be called after all the files have been extracted.
        /// If there are content folders detected, then <see cref="ArchiveType.Product"/>
        /// is returned. If not, then if there is an archive file, then it will be treated
        /// as a <see cref="ArchiveType.Bundle"/>. Otherwise, <see cref="ArchiveType.Unknown"/>
        /// is returned.
        /// </remarks>
        public ArchiveType DetermineArchiveType()
        {
            foreach (IDPFolder folder in Folders.Values)
            {
                if (folder.IsContentFolder)
                {
                    return ArchiveType.Product;
                }
            }
            foreach (IDPFile content in Contents.Values)
            {
                if (content is IDPArchive) return ArchiveType.Bundle;
            }
            return ArchiveType.Unknown;

        }

        public int GetEstimateTagCount()
        {
            var count = 0;
            foreach (IDPFile content in Contents.Values)
            {
                count += content.Tags.Count;
            }
            count += ProductInfo.Authors.Count;
            return count;
        }

        public IDPFolder? FindParent(IDPAbstractNode obj)
        {
            ReadOnlySpan<char> path = obj.Path;
            
            // Remove trailing separator if one exists
            if (path.Length > 0 && (path[^1] == '\\' || path[^1] == '/'))
                path = path[..^1];

            var fileName = PathHelper.GetFileName(path).AsSpan();
            if (fileName.IsEmpty)
                return null; // If fileName is empty after trimming, we can't find a parent

            var fileNameIndex = path.LastIndexOf(fileName);
            if (fileNameIndex <= 0)
                return null; // If we can't find the fileName in the path, we can't find a parent

            ReadOnlySpan<char> relativePathSpan = path[..fileNameIndex];
            string relativePathOnly;

            try
            {
                relativePathOnly = PathHelper.CleanDirPath(relativePathSpan);
            }
            catch
            {
                return null; // If we can't clean the path, we can't find a parent
            }

            if (FindFolder(relativePathOnly, out IDPFolder? folder))
            {
                return folder;
            }

            return null;
        }
        
        /// <summary>
        /// Determines if this archive contains a folder with the exact normalized path specified.
        /// </summary>
        /// <remarks>
        /// This is equivalent to <c>Folders.ContainsKey(normalizedFolderPath)</c>.
        /// </remarks>
        /// <param name="normalizedFolderPath">The normalized folder path with 
        /// <see cref="PathHelper.NormalizePath(string)"/>.
        /// </param>
        /// <returns>True if the <see cref="Folders"/> dictionary contains the key or not.</returns>
        public bool FolderExists(string normalizedFolderPath) => Folders.ContainsKey(normalizedFolderPath);

        /// <summary>
        /// Simply finds the folder given a path.
        /// </summary>
        /// <remarks>
        /// The path is normalized internally, so you can pass non-normalized paths safely.
        /// </remarks>
        /// <param name="path">A path of a potential folder (can be a normalized path)</param>
        /// <param name="folder">The folder if found, otherwise null.</param>
        /// <returns>True if the folder was found, otherwise false.</returns>
        public bool FindFolder(string path, [NotNullWhen(true)] out IDPFolder? folder)
        {
            var normalizedPath = PathHelper.NormalizePath(path);
            return Folders.TryGetValue(normalizedPath, out folder);
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