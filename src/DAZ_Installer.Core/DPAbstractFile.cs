// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System;
using System.Windows.Forms;
using IOPath = System.IO.Path;

namespace DAZ_Installer.Core {
    /// <summary>
    /// Abstract class for all elements found in archives (including archives).
    /// This means that all files, and archives (which are files) should extend
    /// this class.
    /// </summary>
    public abstract class DPAbstractFile {
        /// <summary>
        /// The file name of a file or folder; equivalent to <c>Path.GetFileName()</c>.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// The full path of the file (or folder) in the archive space.
        /// However, if the derived type is a <c>DPArchive</c>, the path can be the path
        /// in the file system space or archive space. If the archive has <c>IsInnerArchive</c> set to
        /// true, then the path is the path of the archive. Otherwise, it is the path in
        /// file system space.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// The full relative path of the file (or folder) relative to the determined content folder (if any). 
        /// If no content folder is detected, relative path will be null.
        /// Currently, relative path is not set for folders.
        /// </summary>
        public string? RelativePath { get; set; }
        /// <summary>
        /// The relative directory path at which will be used to determine which the file will go to in the system. <para/>
        /// This property is used to determine the target path of a file. <para/>
        /// The value will be equal to <see cref="RelativePath"/>
        /// if the <see cref="FileName"/> is not in <see cref="DPSettings.folderRedirects"/>.
        /// </summary>
        public string RelativeTargetPath { get; set; }
        /// <summary>
        /// The full directory path at which the file go to in the file system.
        /// </summary>
        public string TargetPath { get; set; }
        /// <summary>
        /// The extension of the file in lowercase characters and without the dot. ext can be empty.
        /// </summary>
        public string Ext { get; set; }
        /// <summary>
        /// A boolean value to determine if the current file will be extracted.
        /// </summary>
        public bool WillExtract { get; set; }
        /// <summary>
        /// The folder the file (or folder) is a child of. Can be null.
        /// </summary>
        public DPFolder? Parent { get => _parent; set => UpdateParent(value); }
        /// <summary>
        /// The location of the file in the file system after it has been extracted. Can be null.
        /// </summary>
        public string ExtractedPath { get; set; }
        /// <summary>
        /// The unique identifier for the file (or folder).
        /// </summary>
        public uint UID { get; set; }
        /// <summary>
        /// The associated list view item if any.
        /// </summary>
        public ListViewItem? AssociatedListItem { get; set; }
        /// <summary>
        /// The associated tree node if any.
        /// </summary>
        public TreeNode? AssociatedTreeNode { get; set; }
        /// <summary>
        /// A boolean value to determine if the file was successfully extracted.
        /// </summary>
        public bool WasExtracted { get; set; }
        /// <summary>
        /// A boolean value to determine if this file had errored.
        /// </summary>
        public bool errored { get; set; }
        /// <summary>
        /// The archive this file is associated to. Can be null.
        /// </summary>
        public DPAbstractArchive? AssociatedArchive { get; set; }
        
        protected DPFolder? _parent;


        /// <summary>
        /// Updates the parent of the file (or archive). This method is virtual and is overloaded by DPFolder.
        /// </summary>
        /// <param name="newParent">The folder that will be the new parent of the file (or archive). </param>
        public virtual void UpdateParent(DPFolder? newParent) {
            // If we were null, but now we're not...
            if (_parent == null && newParent != null) {
                // Remove ourselves from root content of the working archive.
                try {
                    DPProcessor.workingArchive.RootContents.Remove(this);
                } catch {}

                // Call the folder's addChild() to add ourselves to the children list.
                newParent.addChild(this);
                _parent = newParent;
            } else if (_parent == null && newParent == null) {
                // Try to find a parent.

                var potParent = DPProcessor.workingArchive == null ? null :
                    DPProcessor.workingArchive.FindParent(this);

                // If we found a parent, then update it. This function will be called again.
                if (potParent != null) {
                    Parent = potParent;
                } else {
                    // Otherwise, create a folder for us.
                    potParent = DPFolder.CreateFolderForFile(Path);
                    
                    // If we have successfully created a folder for us, then update it. This function will be called again.
                    if (potParent != null) Parent = potParent;
                    else { // Otherwise, we are supposed to be at root.
                        _parent = null;
                        if (!DPProcessor.workingArchive.RootContents.Contains(this)) {
                            DPProcessor.workingArchive.RootContents.Add(this);
                        }
                    }
                }
            } else if (_parent != null && newParent != null) {
                // Remove ourselves from previous parent children.
                _parent.removeChild(this);

                // Add ourselves to new parent's children.
                newParent.addChild(this);

                _parent = newParent;
            } else if (_parent != null && newParent == null) {
                // Remove ourselves from previous parent's children.
                _parent.removeChild(this);

                // Add ourselves to the archive's root content list.
                DPProcessor.workingArchive.RootContents.Add(this);
                _parent = newParent;
            }
        }

        /// <summary>
        /// Returns the extension of a given name without the dot and lowered to all lowercase.
        /// </summary>
        /// <param name="name">The name to get the extension from.</param>
        public static string GetExtension(ReadOnlySpan<char> name)
        {
            var ext = IOPath.GetExtension(name);
            if (ext.Length > 0) ext = ext.Slice(1);
            Span<char> lowerExt = new char[ext.Length];
            ext.ToLower(lowerExt, System.Globalization.CultureInfo.CurrentCulture);
            return ext.ToString();
        }

        public DPAbstractFile(string _path) {
            UID = DPIDManager.GetNewID();
            Path = _path;
            FileName = IOPath.GetFileName(_path);
        }
        
        
    }
}