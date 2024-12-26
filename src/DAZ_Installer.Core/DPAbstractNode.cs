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
    public abstract class DPAbstractNode : IDPAbstractNode
    {
        public abstract ILogger Logger { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// By default, returns <c>System.IO.Path.GetFileName(Path)</c>.
        /// </remarks> 
        public virtual string FileName => IOPath.GetFileName(Path);
        /// <summary>
        /// The full path of the file (or folder) in the archive space.
        /// Using this property is not recommended for comparing or listing files as delimiters vary, 
        /// use <see cref="NormalizedPath"/> instead. <para/>
        /// However since this property holds the exact path given from the archive, you can use to compare
        /// to match a <see cref="DPAbstractNode"/> to the archive's native format.
        /// </summary>
        /// <seealso cref="NormalizedPath"/>
        public string Path { get; set; } = string.Empty;
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
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// Setting this parent will call <see cref="UpdateParent(IDPFolder?)"/> to update the parent.
        /// </remarks>
        public IDPFolder? Parent { get => parent; set => UpdateParent(value); }
        /// <inheritdoc/>
        public IDPArchive? AssociatedArchive { get; set; }

        protected abstract void UpdateParent(IDPFolder? parent);

        #region Processing Properties
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// This is the absolute path where the file will be extracted during processing.
        /// </remarks>
        public string TargetPath { get; set; } = string.Empty;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// If no content folder is detected, this will be <see cref="string.Empty"/>.
        /// Currently, <b>relative path is not set for folders.</b>
        /// </remarks>
        public string RelativePathToContentFolder { get; set; } = string.Empty;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// This property is used to determine the target path of a file.
        /// The value will be equal to <see cref="RelativePathToContentFolder"/>
        /// if the <see cref="FileName"/> is not in <see cref="DPProcessSettings.ContentRedirectFolders"/>.
        /// </remarks>
        public string RelativeTargetPath { get; set; } = string.Empty;
        #endregion

        protected IDPFolder? parent;

        /// <summary>
        /// Returns the lowercase extension of a given path without the leading dot.
        /// </summary>
        /// <param name="path">The file path or name from which to extract the extension.</param>
        /// <returns>
        /// The lowercase extension without the leading dot if the path contains an extension;
        /// an empty string if the path is null, empty, or does not contain an extension.
        /// </returns>
        /// <remarks>
        /// This method handles various edge cases:
        /// - If the path is null or empty, it returns an empty string.
        /// - If the path does not contain a dot or ends with a dot, it returns an empty string.
        /// - The extension is converted to lowercase for consistency.
        /// </remarks>
        public static string GetExtension(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            ReadOnlySpan<char> extension = IOPath.GetExtension(path);

            if (extension is { Length: >0 })
            {
                return extension.TrimStart('.').ToString().ToLower();
            }

            return string.Empty;
        }

        /// <summary>
        /// A constructor that does nothing. Only recommended for creating init-archives.
        /// </summary>
        public DPAbstractNode() { }
        /// <summary>
        /// Constructor for creating file, folder, and even archive objects from the archive space.
        /// </summary>
        /// <param name="_path">The path of the file or folder in the archive space.</param>
        /// <param name="associatedArchive">The archive that contains this node, if any.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> or <paramref name="_path"/> is <see langword="null"/>.</exception>
        public DPAbstractNode(string _path, IDPArchive? associatedArchive = null)
        {
            ArgumentNullException.ThrowIfNull(_path);
            Path = _path;
            AssociatedArchive = associatedArchive;
        }

    }
}