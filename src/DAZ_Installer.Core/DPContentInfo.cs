// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System.Collections.Generic;

namespace DAZ_Installer.Core {
    internal struct DPContentInfo {
        /// <summary>
        /// The content type of the DSON User File. Default is ContentType.Unknown.
        /// </summary>
        internal ContentType ContentType {get; set;} = ContentType.Unknown;
        /// <summary>
        /// The list of authors found in the DSON user file. Default is an empty list.
        /// </summary>
        internal List<string> Authors = new List<string>();
        /// <summary>
        /// The website found in the DSON user file. Default is an empty string.
        /// </summary>
        internal string Website {get; set;} = string.Empty;
        /// <summary>
        /// The email found in the DSON user file. Default is an empty string.
        /// </summary>
        internal string Email {get; set; } = string.Empty;
        /// <summary>
        /// The ID found in the DSX user file. Default is an empty string.
        /// </summary>
        internal string ID {get; set; } = string.Empty;

        public DPContentInfo() {} // microsoftttttttt (╯‵□′)╯︵┻━┻┻━┻ ︵ヽ(`Д´)ﾉ︵ ┻━┻

    }
}