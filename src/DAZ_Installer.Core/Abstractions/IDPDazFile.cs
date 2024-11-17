using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// A DAZ content file that may contain metadata.
    /// </summary>
    public interface IDPDazFile : IDPFile
    {
        /// <summary>
        /// The content information of the DAZ file.
        /// </summary>
        public DPContentInfo ContentInfo { get; set; }
        /// <summary>
        /// Reads and updates <see cref="ContentInfo"/>.
        /// </summary>
        /// <param name="stream">The file stream to read from, expecting a JSON stream.</param>
        public void ReadContents(StreamReader stream);
    }
}
