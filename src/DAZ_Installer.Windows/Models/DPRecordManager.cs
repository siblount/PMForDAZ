// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using DAZ_Installer.UI;
using DAZ_Installer.Core;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.Database;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DAZ_Installer.IO;
using System.Threading.Tasks;
using System;

namespace DAZ_Installer.Windows.DP {
    /// <summary>
    /// The default implementation for <see cref="IDPRecordManager"/>.
    /// </summary>
    public sealed class DPRecordManager : IDPRecordManager
    {
        /// <summary>
        /// The database to use for modifying the records in the database.
        /// </summary>
        /// <value>By default, the <see cref="Program.Database"/>.</value>
        public IDPDatabase Database { get; init; } 
        /// <summary>
        /// The thumbnail downloader to use for product records.
        /// </summary>
        /// <value>By default, a <see cref="DPThumbnailDownloader.Instance"/></value>
        public IDPThumbnailDownloader ThumbnailDownloader { get; set; } = DPThumbnailDownloader.Instance;
        /// <summary>
        /// The message box provider to use for displaying messages.
        /// </summary>
        /// <value>By default, a <see cref="UI.MessageBoxProvider.Instance"/>.</value>
        public IMessageBoxProvider MessageBoxProvider { get; set; } = UI.MessageBoxProvider.Instance;
        /// <summary>
        /// The file system to use for downloading files and creating the necessary directories.
        /// </summary>
        /// <returns>By default, a <see cref="DPFileSystem"/>.</returns>
        public AbstractFileSystem FileSystem { get; set; } = new DPFileSystem(DPFileScopeSettings.None);
        /// <summary>
        /// The max timeout for fetching a thumbnail.
        /// </summary>
        /// <returns></returns>
        public TimeSpan MaxTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Creates a new instance of <see cref="DPRecordManager"/> with the specified database.
        /// </summary>
        /// <param name="database">The database to use, otherwise, uses the one from <see cref="Program.Database"/></param>
        public DPRecordManager(IDPDatabase? database)
        {
            Database = database ?? Program.Database;
        }
        /// <inheritdoc/>
        public async Task CreateAndAddRecord(DPExtractionReport report, DPSettings userSettings)
        {
            IDPArchive arc = report.Settings.Archive;
            if (arc.Type != ArchiveType.Product) return;
            var imageLocation = string.Empty;

            // Extraction Record successful folder/file paths will now be relative to their content folder (if any).
            var successfulFiles = new List<string>(arc.Contents.Count);
            // Folders where a file was extracted underneath it.
            // Ex: Content/Documents/a.txt was extracted, therefore "Documents" is added.
            var foldersExtracted = new HashSet<string>(arc.Contents.Count);

            // Add the paths relative to the content folder.
            foreach (IDPFile file in report.ExtractedFiles)
            {
                successfulFiles.Add(file.RelativePathToContentFolder!);
                if (!string.IsNullOrWhiteSpace(file.RelativePathToContentFolder))
                    foldersExtracted.Add(Path.GetDirectoryName(file.RelativePathToContentFolder)!);
            }
            var erroredFiles = report.ErroredFiles.Keys.Select(x => x.RelativePathToContentFolder!).ToArray();
            FileSystem.Scope = new DPFileScopeSettings([], [userSettings.ThumbnailsDir], false, false, true);
            var thumbnailsDirInfo = FileSystem.CreateDirectoryInfo(userSettings.ThumbnailsDir);
            var arcName = arc.FileName;
            IDPFileInfo? thumbnailInfo = null;
            if (userSettings!.DownloadImages == SettingOptions.Yes && HasIMNamingFormat(arcName)) {
                thumbnailInfo = await ThumbnailDownloader.DownloadThumbnail(arc.FileName[2..arcName.IndexOf('-')], arcName, thumbnailsDirInfo)
                                                             .WaitAsync(MaxTimeout)
                                                             .ConfigureAwait(false);
            }
            else if (userSettings.DownloadImages == SettingOptions.Prompt && HasIMNamingFormat(arcName))
            {
                // TODO: Use more reliable method! Support files!
                DialogResult result = MessageBoxProvider.Show("Do you wish to download the thumbnail for this product?", "Download Thumbnail Prompt", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) thumbnailInfo = await ThumbnailDownloader.DownloadThumbnail(arc.FileName[2..arcName.IndexOf('-')], arcName, thumbnailsDirInfo)
                                                                                         .WaitAsync(MaxTimeout)
                                                                                         .ConfigureAwait(false);
            }
            
            var author = arc.ProductInfo.Authors.FirstOrDefault(null as string);
            var workingProductRecord = new DPProductRecord(arc.ProductName, [..arc.ProductInfo.Authors], DateTime.Now, imageLocation, arc.FileName, 
                userSettings.DestinationPath, [..arc.ProductInfo.Tags], successfulFiles, 0);
            await Database.AddNewRecordEntry(workingProductRecord).ConfigureAwait(false);
        }

        private static bool HasIMNamingFormat(ReadOnlySpan<char> archiveName) => 
            archiveName.StartsWith("IM") && archiveName.Length > 2 && int.TryParse(archiveName[2..archiveName.IndexOf('-')], out _);
    }
}