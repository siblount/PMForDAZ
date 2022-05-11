using System.Collections.Generic;

namespace DAZ_Installer.DP {
    internal class DPProductInfo {
        internal string ProductName = string.Empty;
        internal HashSet<string>? Tags = null;
        internal string Author = string.Empty;
        internal string SKU = string.Empty;

        internal DPProductInfo() {}

        internal DPProductInfo(string productName, string? author = null, string? sku = null, HashSet<string>? tags = null) {
            if (tags != null) Tags = tags;
            if (author != null) Author = author;
            if (sku != null) SKU = sku;
            ProductName = productName;
        }
    }
}