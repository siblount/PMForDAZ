// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using DAZ_Installer.IO;
using Serilog;
using IOPath = System.IO.Path;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// Abstract class for all elements found in archives (including archives).
    /// This means that all files, and archives (which are files) should extend
    /// this class.
    /// </summary>
    public abstract class DPAbstractNode
    {
        public abstract ILogger Logger { get; set; }
        /// <summary>
        /// The file name of a file or folder.
        /// </summary>
        public virtual string FileName => IOPath.GetFileName(Path);
        /// <summary>
        /// The full path of the file (or folder) in the archive space. Using this property is not recommended
        /// for comparing or listing files as delimiters vary, use <see cref="NormalizedPath"/> instead. <para/>
        /// However since this property holds the exact path given from the archive, you can use to compare
        /// to match a <see cref="DPAbstractNode"/> to the archive's native format. For example,
        /// <code>
        /// RARFileInfo info = e.fileInfo;
        /// info.FileName == Path // returns true
        /// </code>
        /// </summary>
        public string Path = string.Empty;
        /// <summary>
        /// The path with all forward slashes replaced with backslashes. Use this property for comparing
        /// and listing files, folders, and archives.
        /// </summary>
        public virtual string NormalizedPath => PathHelper.NormalizePath(Path);
        /// <summary>
        /// The extension of the file in lowercase characters and without the dot. ext can be empty.
        /// </summary>
        public virtual string Ext => GetExtension(Path);
        /// <summary>
        /// The folder the file (or folder) is a child of. Can be null.
        /// </summary>
        public DPFolder? Parent { get => parent; set => UpdateParent(value); }
        /// <summary>
        /// The archive this file is associated to. Can be null. <para/>
        /// Should only be null if this file is an <see cref="DPArchive"/> <b>AND</b>
        /// the initial archive called by <see cref="DPProcessor.ProcessArchive(string, DPProcessSettings)"/>
        /// </summary>
        public DPArchive? AssociatedArchive { get; set; }

        protected abstract void UpdateParent(DPFolder? parent);

        #region Processing Properties
        /// <summary>
        /// The final, absolute path that the file is supposed to be extracted to. 
        /// </summary>
        public string TargetPath { get; internal set; } = string.Empty;
        /// <summary>
        /// The full relative path of the file (or folder) relative to the determined content folder (if any). 
        /// If no content folder is detected, relative path will be <see cref="string.Empty"/>.
        /// Currently, <b>relative path is not set for folders.</b>
        /// </summary>
        public string RelativePathToContentFolder { get; set; } = string.Empty;
        /// <summary>
        /// The relative directory path at which will be used to determine which the file will go to in the system. <para/>
        /// This property is used to determine the target path of a file. <para/>
        /// The value will be equal to <see cref="RelativePathToContentFolder"/>
        /// if the <see cref="FileName"/> is not in <see cref="DPProcessSettings.ContentRedirectFolders"/>.
        /// </summary>
        public string RelativeTargetPath { get; set; } = string.Empty;
        #endregion

        protected DPFolder? parent;

        /// <summary>
        /// Returns the extension of a given name without the dot and lowered to all lowercase.
        /// </summary>
        public static string GetExtension(string path) => IOPath.GetExtension(path).Substring(path.Length > 0 ? 1 : 0).ToLower();
        /// <summary>
        /// A constructor that does nothing. Only recommended for creating init-archives.
        /// </summary>
        public DPAbstractNode() { }
        /// <summary>
        /// Constructor for creating file, folder, and even archive objects from the archive space.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> or <paramref name="_path"/> is <see langword="null"/>.</exception>
        public DPAbstractNode(string _path, DPArchive? associatedArchive = null)
        {
            ArgumentNullException.ThrowIfNull(_path);
            Path = _path;
            AssociatedArchive = associatedArchive;
        }

    }
}