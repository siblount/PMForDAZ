// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;
using Serilog;
using IOPath = System.IO.Path;
namespace DAZ_Installer.Core
{
    /// <inheritdoc/>
    public class DPFile : DPAbstractNode, IDPFile
    {
        // Public static members
        /// <summary>
        /// A dictionary that maps the string representation of the <see cref="ContentType"/> to the actual <see cref="ContentType"/> enum.
        /// </summary>
        private static Dictionary<string, ContentType> enumPairs { get; } = new Dictionary<string, ContentType>(Enum.GetValues(typeof(ContentType)).Length);
        /// <summary>
        /// A set of DAZ file extensions that are recognized by the program.
        /// </summary>
        public static readonly HashSet<string> DAZFormats = new() { "duf", "dsa", "dse", "daz", "dsf", "dsb", "dson", "ds", "dsb", "djl", "dsx", "dsi", "dcb", "dbm", "dbc", "dbl", "dsd", "dsv" };
        /// <summary>
        /// A set of geometry file extensions that are recognized by the program.
        /// </summary>
        public static readonly HashSet<string> GeometryFormats = new() { "dae", "bvh", "fbx", "obj", "dso", "abc", "mdd", "mi", "u3d" };
        /// <summary>
        /// A set of media file extensions that are recognized by the program.
        /// </summary>
        public static readonly HashSet<string> MediaFormats = new() { "png", "jpg", "hdr", "hdri", "bmp", "gif", "webp", "eps", "raw", "tiff", "tif", "psd", "xcf", "jpeg", "cr2", "svg", "apng", "avif" };
        /// <summary>
        /// A set of document file extensions that are recognized by the program.
        /// </summary>
        public static readonly HashSet<string> DocumentFormats = new() { "txt", "pdf", "doc", "docx", "odt", "html", "ppt", "pptx", "xlsx", "xlsm", "xlsb", "rtf" };
        /// <summary>
        /// A set of other file extensions that are recognized by the program.
        /// </summary>
        public static readonly HashSet<string> OtherFormats = new() { "exe", "lib", "dll", "bat", "cmd" };
        /// <summary>
        /// A set of acceptable import formats that are recognized by the program.
        /// </summary>
        public static readonly HashSet<string> AcceptableImportFormats = new() { "rar", "zip", "7z", "001" };
        /// <summary>
        /// A factory to create folders.
        /// </summary>
        /// <returns>
        /// Inherits the folder factory from <see cref="DPAbstractNode.AssociatedArchive"/> otherwise 
        /// fallbacks to <see cref="DPFolderFactory"/>
        /// </returns>
        public IDPFolderFactory FolderFactory => AssociatedArchive?.FolderFactory ?? DPFolderFactory.Instance;
        /// <summary>
        /// A list of tags that are associated with the file. This is typically initialized with the file name.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>(0);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// Typically, this is a <see cref="DPFileInfo"/> object. This is set by the extractor when the file is extracted.
        /// </remarks>
        public IDPFileInfo? FileInfo { get; set; }
        /// <summary>
        /// The logger to use; typically this is of type <see cref="Log"/>. If you override this, make sure to use <see cref="ILogger.ForContext{TSource}()"/>.
        /// </summary>
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPFile>();
        #region Extraction Properties
        // Properties that are generally used for extraction.

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// This is because the extractor will not set <see cref="FileInfo"/> until the file has been extracted. <br/> This does not necessarily
        /// mean that the file has been extracted to the target path (eg. extracted to temp first). <br/>
        /// If you wish to check if the file has been extracted to the target path,
        /// use <see cref="ExtractedToTarget"/>.
        /// </remarks>
        /// <seealso cref="ExtractedToTarget"/>
        public bool Extracted => FileInfo != null;
        /// <inheritdoc/>
        public bool ExtractedToTarget => FileInfo != null && !string.IsNullOrEmpty(TargetPath) && FileInfo.Path == IOPath.GetFullPath(TargetPath);
        #endregion

        // TO DO : Add get tags func.
        static DPFile()
        {
            foreach (var eName in Enum.GetNames(typeof(ContentType)))
            {
                var lowercasedName = eName.ToLower();
                enumPairs[lowercasedName] = (ContentType)Enum.Parse(typeof(ContentType), eName);
            }
        }
        /// <summary>
        /// A constructor that does nothing.
        /// </summary>
        public DPFile() { }

        /// <summary>
        /// A public constructor required for setting up this file that is connected to a <see cref="IDPArchive"/>.
        /// </summary>
        /// <param name="_path">The path to set for this file.</param>
        /// <param name="arc">The archive to associate to, if any.</param>
        /// <param name="__parent">The parent folder for this file, if any.</param>
        /// <exception cref="InvalidOperationException">File already exists in <paramref name="arc"/>.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="_path"/> is null</exception>"
        public DPFile(string _path, IDPArchive? arc, IDPFolder? __parent) : base(_path, arc)
        {
            ArgumentNullException.ThrowIfNull(_path, nameof(_path));
            if (GetType() == typeof(DPFile)) Logger.Debug("Creating new DPFile for {0}", Path);
            Parent = __parent;
            InitializeTagsList();

            if (arc is null) return;
            AssociatedArchive = arc;
            if (!arc.Contents.TryAdd(NormalizedPath, this))
                throw new InvalidOperationException("File already exists in this archive.");
        }

        /// <summary>
        /// A testing constructor intended for testing purposes only. This calls <see cref="DPFile(string, IDPArchive?, IDPFolder?)"/> 
        /// constructor which means that the file will be added to the archive if <paramref name="arc"/> is not null, create missing folders,
        /// initialize tags, set the parent, etc.
        /// </summary>
        /// <param name="_path">The path to set for this file.</param>
        /// <param name="arc">The associated archive to set for this file, if any.</param>
        /// <param name="__parent">The parent folder for this file, if any.</param>
        /// <param name="fileInfo">The related system FileInfo object, if any.</param>
        /// <param name="logger">The logger to use.</param>
        public DPFile(string _path, IDPArchive? arc, IDPFolder? __parent, IDPFileInfo? fileInfo, ILogger logger) : this(_path, arc, __parent)
        {
            FileInfo = fileInfo;
            Logger = logger;
        }

        /// <summary>
        /// Updates the parent of the file (or archive).
        /// </summary>
        /// <param name="newParent">The folder that will be the new parent of the file (or archive). </param>
        protected override void UpdateParent(IDPFolder? newParent)
        {
            // If we were null, but now we're not...
            if (parent == null && newParent != null)
            {
                // Remove ourselves from root content of the working archive.
                // AssociatedArchive shouldn't be null at the point.
                AssociatedArchive?.RootContents.Remove(this);

                // Call the folder's addChild() to add ourselves to the children list.
                newParent.AddChild(this);
                parent = newParent;
            }
            else if (parent == null && newParent == null)
            {
                // If associated archive is null, then there are no parents to look for. 
                // This should only happen when the file is an archive to be processed/extracted.
                // Any other DPFile should have an associated archive.
                if (AssociatedArchive is null)
                {
                    parent = null;
                    return;
                }
                // Try to find a parent.
                IDPFolder? potParent = AssociatedArchive.FindParent(this);

                // If we found a parent, then update it. This function will be called again.
                if (potParent != null) Parent = potParent;
                else
                {
                    // Create a folder for us.
                    potParent = FolderFactory.CreateFolders(Path, AssociatedArchive);

                    // If we have successfully created a folder for us, then update it. This function will be called again.
                    if (potParent != null) Parent = potParent;
                    else // Otherwise, we are supposed to be at root.
                    {
                        parent = null;
                        if (!AssociatedArchive.RootContents.Contains(this))
                            AssociatedArchive.RootContents.Add(this);
                    }
                }
            }
            else if (parent != null && newParent != null)
            {
                // Remove ourselves from previous parent children.
                parent.RemoveChild(this);

                // Add ourselves to new parent's children.
                newParent.AddChild(this);

                parent = newParent;
            }
            else if (parent != null && newParent == null)
            {
                // Remove ourselves from previous parent's children.
                parent.RemoveChild(this);

                // Add ourselves to the archive's root content list.
                // AssoiciatedArchive should never be null at this point.
                AssociatedArchive!.RootContents.Add(this);
                parent = newParent;
            }
        }

        /// <summary>
        /// Extracts the current file to <see cref="DPAbstractNode.TargetPath"/>. If the <see cref="DPAbstractNode.AssociatedArchive"/> is not on disk, 
        /// then it will be extracted first.
        /// </summary>
        /// <param name="settings">The extract settings to use.</param>
        /// <returns>Whether the extraction was successful or not.</returns>
        public bool Extract(DPExtractSettings settings)
        {
            if (AssociatedArchive is null) return false;
            return AssociatedArchive!.ExtractContent(this, settings.TempPath, settings.OverwriteFiles);
        }

        /// <summary>
        /// Extracts the current file to <paramref name="dest"/> by setting <see cref="DPAbstractNode.TargetPath"/> to <paramref name="dest"/> and extracting.
        /// </summary>
        /// <param name="settings">The extract settings to use.</param>
        /// <param name="dest">Whether the extraction was successful or not.</param>
        /// <returns></returns>
        public bool Extract(DPExtractSettings settings, string dest)
        {
            TargetPath = dest;
            return Extract(settings);
        }

        /// <summary>
        /// Extracts the current file. If the file is not extracted, then it will be extracted. Otherwise, nothing will happen.
        /// </summary>
        /// <param name="settings">The extract settings to use; only <see cref="DPExtractSettings.TempPath"/> will be honored.</param>
        /// <returns>Whether the operation was a succses or not</returns>
        public bool ExtractToTemp(DPExtractSettings settings)
        {
            if (AssociatedArchive is null) return false;
            return AssociatedArchive.ExtractContentsToTemp(new DPExtractSettings(settings.TempPath, new[] { this }, archive: AssociatedArchive)).SuccessPercentage == 1;
        }

        /// <summary>
        /// Determines the content type of a file given the extension and the <paramref name="type"/> defined from the
        /// content info in the DAZ file.
        /// </summary>
        /// <param name="type">The content type defined in the <see cref="DPDazFile"/> content info.</param>
        /// <param name="file">The file to use.</param>
        /// <returns>The content type based on the parameters.</returns>
        public static ContentType GetContentType(string? type, IDPFile file)
        {
            if (!string.IsNullOrEmpty(type) && enumPairs.TryGetValue(type, out ContentType contentType))
                return contentType;
            if (file is null) return ContentType.DAZ_File;
            if (GeometryFormats.Contains(file.Ext))
                return ContentType.Geometry;
            else if (MediaFormats.Contains(file.Ext))
                return ContentType.Media;
            else if (DocumentFormats.Contains(file.Ext))
                return ContentType.Document;
            else if (OtherFormats.Contains(file.Ext))
                return ContentType.Program;
            else if (DAZFormats.Contains(file.Ext))
                return ContentType.DAZ_File;

            // The most obvious comment ever - implied else :\
            return ContentType.Unknown;
        }

        /// <summary>
        /// Determines whether the extension is a valid import extension or not.
        /// </summary>
        /// <remarks>
        /// For instance, if <paramref name="ext"/> is <c>zip</c>, then it will return true.
        /// If <paramref name="ext"/> is <c>jpg</c>, then it will return false.
        /// </remarks>
        /// <param name="ext">The extension to check</param>
        /// <returns>Whether the extension refers to a valid archive such as (zip, rar, 7z)</returns>
        public static bool ValidImportExtension(string ext) => AcceptableImportFormats.Contains(ext);

        /// <summary>
        /// Adds the file name to the tags name.
        /// </summary>
        protected void InitializeTagsList()
        {
            var fileName = IOPath.GetFileName(Path);
            var tokens = fileName.Split(' ');
            Tags = new List<string>(tokens.Length);
            Tags.AddRange(tokens);
        }

    }

}
