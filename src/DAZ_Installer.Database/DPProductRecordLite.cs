// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

namespace DAZ_Installer.Database
{
    /// <summary>
    /// A lite version of <see cref="DPProductRecord"/>. This is used for the search results.
    /// </summary>

    public record struct DPProductRecordLite(string Name, string Thumbnail, IReadOnlyList<string> Tags, long ID) { }
}
