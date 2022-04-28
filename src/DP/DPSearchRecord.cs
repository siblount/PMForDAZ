// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

namespace DAZ_Installer.DP
{
    /// <summary>
    /// A special class (record) for holding information stored from the database. Used for search results.
    /// </summary>
    /// <param name="PID">The product record ID in the database.</param>
    /// <param name="ProductName">The name of the product.</param>
    /// <param name="Tags">A string array of tags.</param>
    /// <param name="Author">The author if any.</param>
    /// <param name="SKU">The SKU if any.</param>
    /// <param name="EID">The extraction record ID in the database.</param>
    /// <param name="ThumbnailPath">The full path to the thumbnail if any.</param>
    public record DPSearchRecord(uint PID, string ProductName, string[] Tags, string Author, uint SKU, uint EID, string ThumbnailPath)
    {

    }
}
