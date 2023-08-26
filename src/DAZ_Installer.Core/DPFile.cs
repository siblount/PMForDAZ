// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;
using Serilog;
using IOPath = System.IO.Path;
namespace DAZ_Installer.Core
{
    /// <summary>
    /// A <see cref="DPFile"/> is a regular file that can be found in a <see cref="DPArchive"/> or a <see cref="DPFolder"/>.
    /// </summary>
    public class DPFile : DPAbstractNode
    {
        // Public static members
        private static Dictionary<string, ContentType> enumPairs { get; } = new Dictionary<string, ContentType>(Enum.GetValues(typeof(ContentType)).Length);
        public static readonly HashSet<string> DAZFormats = new() { "duf", "dsa", "dse", "daz", "dsf", "dsb", "dson", "ds", "dsb", "djl", "dsx", "dsi", "dcb", "dbm", "dbc", "dbl", "dso", "dsd", "dsv" };
        public static readonly HashSet<string> GeometryFormats = new() { "dae", "bvh", "fbx", "obj", "dso", "abc", "mdd", "mi", "u3d" };
        public static readonly HashSet<string> MediaFormats = new() { "png", "jpg", "hdr", "hdri", "bmp", "gif", "webp", "eps", "raw", "tiff", "tif", "psd", "xcf", "jpeg", "cr2", "svg", "apng", "avif" };
        public static readonly HashSet<string> DocumentFormats = new() { "txt", "pdf", "doc", "docx", "odt", "html", "ppt", "pptx", "xlsx", "xlsm", "xlsb", "rtf" };
        public static readonly HashSet<string> OtherFormats = new() { "exe", "lib", "dll", "bat", "cmd" };
        public static readonly HashSet<string> AcceptableImportFormats = new() { "rar", "zip", "7z", "001" };
        public List<string> Tags { get; set; } = new List<string>(0);
        /// <summary>
        /// The FileInfo object to use for moving, copying, and deleting files. Typically this is a <see cref="DPFileInfo"/>. 
        /// </summary>
        public IDPFileInfo? FileInfo { get; set; }
        /// <summary>
        /// The logger to use; typically this is of type <see cref="Log"/>. If you override this, make sure to use <see cref="ILogger.ForContext{TSource}()"/>.
        /// </summary>
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPFile>();
        #region Extraction Properties
        // Properties that are generally used for extraction.

        /// <summary>
        /// Determines whether the file has been extracted or not; this simply checks if <see cref="FileInfo"/> is null or not. <br/>
        /// This is because the extractor will not set <see cref="FileInfo"/> until the file has been extracted. <br/> This does not necessarily
        /// mean that the file has been extracted to the target path (eg. extracted to temp first). <br/>
        /// If you wish to check if the file has been extracted to the target path,
        /// use <see cref="ExtractedToTarget"/>.
        /// </summary>
        /// <seealso cref="ExtractedToTarget"/>
        public bool Extracted => FileInfo != null;
        /// <summary>
        /// Determines whether the file has been extracted to the target path or not. <para/>
        /// </summary>
        public bool ExtractedToTarget => FileInfo != null && FileInfo.Path == IOPath.GetFullPath(TargetPath);
        /// <summary>
        /// The FileInfo object that represents the file that is on disk. This should only be set during initialization
        /// or by the extractor.
        /// </summary>
        #endregion

        // TO DO : Add get tags func.
        // TO DO: Add static function to search for a property.
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

        public DPFile(string _path, DPArchive? arc, DPFolder? __parent) : base(_path, arc)
        {
            ArgumentNullException.ThrowIfNull(_path, nameof(_path));
            if (GetType() == typeof(DPFile)) Logger.Debug("Creating new DPFile for {0}", Path);
            Parent = __parent;
            InitializeTagsList();

            if (arc is null) return;
            AssociatedArchive = arc;
            if (!arc.Contents.TryAdd(NormalizedPath, this))
                throw new Exception("File already exists in this archive.");
        }

        public DPFile(string _path, DPArchive? arc, DPFolder? __parent, IDPFileInfo? fileInfo, ILogger logger) : this(_path, arc, __parent)
        {
            FileInfo = fileInfo;
            Logger = logger;
        }

        public static DPFile CreateNewFile(string path, DPArchive? arc, DPFolder? parent)
        {
            var ext = GetExtension(path);
            if (ext == "dsf" || ext == "duf")
            {
                return new DPDazFile(path, arc!, parent);
            }
            else if (ext == "dsx")
            {
                return new DPDSXFile(path, arc!, parent);
            }
            else if (AcceptableImportFormats.Contains(ext))
                return new DPArchive(path, arc, parent);
            return new DPFile(path, arc, parent);
        }

        /// <summary>
        /// Attempts to move the file to the given path. This simply calls <see cref="FileInfo.MoveTo(string, bool)"/> to move
        /// the file <b>in file system space</b> (not in archive space). Throws exceptions.
        /// </summary>
        /// <param name="path">The path to move to (must exist and have access to it).</param>
        public void MoveTo(string path) => FileInfo?.MoveTo(path, true);
        /// <summary>
        /// Attempts to delete the file <b>in file system space</b> (not archive space). This simply calls <see cref="FileInfo.Delete"/> to delete the file. Throws exceptions.
        /// </summary>
        public void Delete() => FileInfo?.Delete();

        /// <summary>
        /// Updates the parent of the file (or archive).
        /// </summary>
        /// <param name="newParent">The folder that will be the new parent of the file (or archive). </param>
        protected override void UpdateParent(DPFolder? newParent)
        {
            // If we were null, but now we're not...
            if (parent == null && newParent != null)
            {
                // Remove ourselves from root content of the working archive.
                // AssociatedArchive shouldn't be null at the point.
                AssociatedArchive!.RootContents.Remove(this);

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
                DPFolder? potParent = AssociatedArchive.FindParent(this);

                // If we found a parent, then update it. This function will be called again.
                if (potParent != null) Parent = potParent;
                else
                {
                    // Create a folder for us.
                    potParent = DPFolder.CreateFoldersForFile(Path, AssociatedArchive);

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
        /// <returns></returns>
        public bool ExtractToTemp(DPExtractSettings settings)
        {
            if (AssociatedArchive is null) return false;
            return AssociatedArchive.ExtractContentsToTemp(new DPExtractSettings(settings.TempPath, new[] { this }, archive: AssociatedArchive)).SuccessPercentage == 1;
        }

        public static ContentType GetContentType(string type, DPFile dP)
        {
            if (!string.IsNullOrEmpty(type) && enumPairs.TryGetValue(type, out ContentType contentType))
                return contentType;
            if (dP is null) return ContentType.DAZ_File;
            if (GeometryFormats.Contains(dP.Ext))
                return ContentType.Geometry;
            else if (MediaFormats.Contains(dP.Ext))
                return ContentType.Media;
            else if (DocumentFormats.Contains(dP.Ext))
                return ContentType.Document;
            else if (OtherFormats.Contains(dP.Ext))
                return ContentType.Program;
            else if (DAZFormats.Contains(dP.Ext))
                return ContentType.DAZ_File;

            // The most obvious comment ever - implied else :\
            return ContentType.Unknown;
        }

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
