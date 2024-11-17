using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// A special class that marks the DPFile as .dsx file which typically is a Supplement file or a Manifest file.
    /// </summary>
    public interface IDPDSXFile : IDPFile
    {
        /// <summary>
        /// The content information of the DSX file.
        /// </summary>
        public DPContentInfo ContentInfo { get; set; }
        /// <summary>
        /// Reads the contents of this file and updates the <see cref="ContentInfo"/> struct. 
        /// </summary>
        public void CheckContents(StreamReader stream);
        /// <summary>
        /// A map with the keys being the full path of the file and the value being the path without "Content\" included.
        /// </summary>
        /// <returns>
        /// Returns a dictionary containing files to extract and their destination. 
        /// Key is the file path in the archive, and value is the path relative to Content folder.
        /// </returns>
        public Dictionary<string, string> GetManifestDestinations();
    }
}
