// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System.Windows.Forms;

namespace DAZ_Installer
{
    public interface IDPWorkingFile
    {
        public string path { get; set; }
        public string relativePath { get; set; }
        /// <summary>
        /// The directory path at which the file will be go to.
        /// </summary>
        public string destinationPath { get; set; }
        public string ext { get; set; }
        /// <summary>
        /// A boolean value to determine if the current file will be extracted.
        /// </summary>
        public bool extract { get; set; }
        public DPFolder parent { get; set; }
        public string extractedPath { get; set; }
        public uint uid { get; set; }
        public ListViewItem associatedListItem { get; set; }
        public TreeNode associatedTreeNode { get; set; }
        public bool wasExtracted { get; set; }
    }
}
