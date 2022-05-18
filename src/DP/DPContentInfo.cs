using System.Collections.Generic;

namespace DAZ_Installer.DP {
    internal struct DPContentInfo {
        internal ContentType ContentType {get; set;} = ContentType.Unknown;
        internal List<string> Authors = new List<string>();
        internal string Website {get; set;} = string.Empty;
        internal string Email {get; set; } = string.Empty;
        internal string ID {get; set; } = string.Empty;

        public DPContentInfo() {} // microsoftttttttt (╯‵□′)╯︵┻━┻┻━┻ ︵ヽ(`Д´)ﾉ︵ ┻━┻

    }
}