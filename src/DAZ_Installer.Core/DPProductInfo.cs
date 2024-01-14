// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

namespace DAZ_Installer.Core
{
    public struct DPProductInfo
    {
        /// <summary>
        /// The name of the product.
        /// </summary>
        public string ProductName = string.Empty;
        /// <summary>
        /// The tags auto-generated from the archive.
        /// </summary>
        public HashSet<string> Tags = new(3);
        /// <summary>
        /// The authors identified from the archive.
        /// </summary>
        public HashSet<string> Authors = new(1);
        /// <summary>
        /// The SKU of the product.
        /// </summary>
        public string SKU = string.Empty;

        /// <summary>
        /// Creates a new instance of <see cref="DPProductInfo"/>.
        /// </summary>
        public DPProductInfo() { }
        /// <summary>
        /// Creates a new instance of <see cref="DPProductInfo"/>.
        /// </summary>
        /// <param name="productName">The product name observed from the archive.</param>
        /// <param name="authors">The authors identified from the archive.</param>
        /// <param name="sku">The SKU of the product.</param>
        /// <param name="tags">Tags generated based on parsed archive contents.</param>
        public DPProductInfo(string productName, HashSet<string>? authors = null, string? sku = null, HashSet<string>? tags = null)
        {
            if (tags != null) Tags = tags;
            if (authors != null) Authors = authors;
            if (sku != null) SKU = sku;
            ProductName = productName;
        }
    }
}