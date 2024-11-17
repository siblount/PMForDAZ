using Serilog;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// Defines the contract for all elements found in archives, including files and archives themselves.
    /// This interface serves as a base for representing any node within the archive structure.
    /// </summary>
    public interface IDPAbstractNode
    {
        /// <summary>
        /// The logger to use for this node.
        /// </summary>
        ILogger Logger { get; set; }
        /// <summary>
        /// The file name of a file or folder.
        /// </summary>
        string FileName { get; }
        /// <summary>
        /// Gets or sets the full raw path of the node in the archive space.
        /// </summary>
        /// <remarks>
        /// This property holds the exact path given from the archive. It is not recommended for comparing or listing files
        /// as delimiters may vary. Use <see cref="NormalizedPath"/> for such operations.
        /// </remarks>
        /// <seealso cref="NormalizedPath"/>
        string Path { get; set; }
        /// <summary>
        /// Gets the normalized path of the node with consistent delimiters.
        /// </summary>
        /// <remarks>
        /// Use this property for comparing and listing files, folders, and archives.
        /// </remarks>
        string NormalizedPath { get; }
        /// <summary>
        /// Gets the lowercase extension of the file without the dot.
        /// </summary>
        string Ext { get; }
        /// <summary>
        /// The parent folder of this node, if any.
        /// </summary>
        IDPFolder? Parent { get; set; }
        /// <summary>
        /// The associated archive of this node, if any.
        /// </summary>
        IDPArchive? AssociatedArchive { get; set; }
        /// <summary>
        /// Gets or sets the final, absolute path where the file is supposed to be extracted.
        /// </summary>
        string TargetPath { get; set; }
        /// <summary>
        /// Gets or sets the full relative path of the file or folder relative to the determined content folder.
        /// </summary>
        /// <remarks>
        /// If no content folder is detected, this will be <see cref="string.Empty"/>.
        /// </remarks>
        string RelativePathToContentFolder { get; set; }
        /// <summary>
        /// Gets or sets the relative directory path used to determine the file's location in the system.
        /// </summary>
        /// <remarks>
        /// This property is used to determine the target path of a file.
        /// </remarks>
        string RelativeTargetPath { get; set; }
    }
}