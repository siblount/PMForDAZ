using System.Threading.Tasks;
using DAZ_Installer.Core;
using DAZ_Installer.IO;

namespace DAZ_Installer.Windows.DP 
{
    /// <summary>
    /// A thumbnail downloader for products.
    /// </summary>
    public interface IDPThumbnailDownloader {
        /// <summary>
        /// Attempts to download a thumbnail for an product archive.
        /// </summary>
        /// <param name="ID">The archive object to download thumbnails for.</param>
        /// <param name="imageName">The file name to save (excluding the extension)</param>
        /// <param name="saveDirectoryInfo">The directory info tho save the file to.</param>
        /// <returns>A Task object with the result of potentially a <see cref="IDPFileInfo"/> if successfully downloaded, otherwise null.</returns>
        /// <remarks>It is your responisible to ensure save directory exists is writable.</remarks>
        Task<IDPFileInfo?> DownloadThumbnail(string ID, string imageName, IDPDirectoryInfo saveDirectoryInfo);
    }
}