// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System.Collections.Generic;

namespace DAZ_Installer.DP {
    internal struct DPProductInfo {
        internal string ProductName = string.Empty;
        internal HashSet<string>? Tags = null;
        internal HashSet<string> Authors = new HashSet<string>(1);
        internal string SKU = string.Empty;

        public DPProductInfo() {}

        internal DPProductInfo(string productName, HashSet<string> author = null, string? sku = null, HashSet<string>? tags = null) {
            if (tags != null) Tags = tags;
            if (author != null) Authors = author;
            if (sku != null) SKU = sku;
            ProductName = productName;
        }
    }
}