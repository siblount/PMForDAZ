// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

namespace DAZ_Installer.Database
{
    /// <summary>
    /// Represents a product record from the <see cref="DPDatabase"/>.
    /// </summary>
    /// <param name="Name">The identified product name.</param>
    /// <param name="Authors">The identified authors of the product.</param>
    /// <param name="Time">The date/time at which the product was installed.</param>
    /// <param name="ThumbnailPath">The relative (or absolute) path of the thumbnail of the product.</param>
    /// <param name="ArcName">The archive name containing the product.</param>
    /// <param name="Tags">The tags identified for the product.</param>
    /// <param name="Files">The files identified for the product.</param>
    /// <param name="ID">The unique identifier of the product in the database.</param>
    public record DPProductRecord(string Name, IReadOnlyList<string> Authors, DateTime Time, string? ThumbnailPath, 
        string ArcName, string Destination, IReadOnlyList<string> Tags, IReadOnlyList<string> Files, long ID) {
        public string TagsString => string.Join(", ", Tags);
        public string AuthorsString => string.Join(", ", Authors);
        public long TimeStamp => Time.ToFileTimeUtc();
    }
}
