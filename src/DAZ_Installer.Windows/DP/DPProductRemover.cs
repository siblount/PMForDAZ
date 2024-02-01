using DAZ_Installer.Database;
using DAZ_Installer.IO;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Microsoft.VisualBasic.FileIO;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// A static class that removes a product from the database and/or the file system with respect to the <see cref="DPSettings"/>.
    /// </summary>
    internal static class DPProductRemover
    {
        /// <summary>
        /// A record that contains the result of a removal operation.
        /// </summary>
        /// <param name="Success">Whether the operation was COMPLETELY SUCCESSFUL; as in no operation failed.</param>
        /// <param name="FailedFiles">A list containing all the files that failed to be deleted, if any.</param>
        internal record struct RemovalResult(bool Success, List<string> FailedFiles);
        public static ILogger Logger = Log.Logger.ForContext(typeof(DPProductRemover));

        /// <inheritdoc cref="RemoveRecordAsync(DPProductRecord, IDPDatabase)"/>
        internal static async Task<bool> RemoveRecordAsync(DPProductRecordLite record, IDPDatabase database)
        {
            var returnResult = false;
            var callback = new Action<long>((id) => returnResult = record.ID == id);
            await database.RemoveProductRecordQ(record);
            return returnResult;
        }

        /// <summary>
        /// Removes a product record from the database. This does NOT remove the files from the file system.
        /// If you wish to delete both the record and the files, use <see cref="RemoveProductAsync(DPProductRecord, IDPDatabase, DPSettings, DPFileSystem)"/>.
        /// </summary>
        /// <param name="record">The record to delete from the database.</param>
        /// <param name="database">The database to remove the product record from.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        internal static async Task<bool> RemoveRecordAsync(DPProductRecord record, IDPDatabase database)
        {
            var returnResult = false;
            var callback = new Action<long>((id) => returnResult = record.ID == id);
            await database.RemoveProductRecordQ(record);
            return returnResult;
        }

        /// <inheritdoc cref="RemoveProductAsync(DPProductRecord, IDPDatabase, DPSettings, DPFileSystem)"/>
        internal static async Task<RemovalResult> RemoveProductAsync(DPProductRecordLite record, IDPDatabase database, DPSettings settings, DPFileSystem system)
        {
            var fullRecord = await database.GetFullProductRecord(record.ID).ConfigureAwait(false);
            if (fullRecord is null) return new(false, new());
            return await RemoveProductAsync(fullRecord, database, settings, system).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a product from the database and file system with respect to the <see cref="DPSettings"/>. 
        /// It will first attempt to remove the files from the file system and then remove the record from the database.
        /// If the file system removal fails, the record will not be removed from the database.
        /// This function respects the <see cref="DPSettings.DeleteAction"/> property.
        /// </summary>
        /// <param name="record">The record to delete from the database.</param>
        /// <param name="database">The database to remove the product record from.</param>
        /// <param name="settings">The settings to use to find </param>
        /// <param name="system">The file system to use for deletion.</param>
        /// <returns>A removal result that enholds information such as whether the operation completely succeeded and files that failed to be deleted.</returns>
        internal static async Task<RemovalResult> RemoveProductAsync(DPProductRecord record, IDPDatabase database, DPSettings settings, DPFileSystem system)
        {
            var returnResult = false;
            var failedFiles = new List<string>(0);
            // Detect the directories and see if we should delete the directories with all the files or just the files if some files in the directory do not belong to the product.
            try
            {
                foreach (var file in record.Files)
                {
                    var fileInfo = system.CreateFileInfo(Path.Combine(record.Destination, file));
                    if (!fileInfo.Exists) continue;
                    var result = false;
                    Exception? ex = null;
                    if (settings.DeleteAction == RecycleOption.DeletePermanently)
                        result = fileInfo.TryAndFixDelete(out ex);
                    else result = fileInfo.TryAndFixSendToRecycleBin(out ex);
                    if (!result)
                    {
                        Logger.Error(ex, "Failed to remove file {file}", file);
                        failedFiles.Add(file);
                        returnResult = false;
                    }
                }

                var callback = new Action<long>((id) => returnResult &= record.ID == id);
                await database.RemoveProductRecordQ(record, callback).ConfigureAwait(false);
            } catch (Exception ex)
            {
                Logger.Error(ex, "Failed to remove product {product}", record.Name);
                return new(false, failedFiles);
            }

            return new(returnResult, failedFiles);

        }
    }
}
