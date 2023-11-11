using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using System.Data.SQLite;
using System.ComponentModel;
using System.Configuration;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;

namespace DAZ_Installer.Database.Tests
{
    internal static class DPDatabaseTestHelpers
    {
        internal static SQLiteConnection? CreateConnection(string path, bool readOnly = false)
        {
            try
            {
                SQLiteConnection connection = new();
                SQLiteConnectionStringBuilder builder = new();
                builder.DataSource = Path.GetFullPath(path);
                builder.Pooling = true;
                builder.ReadOnly = readOnly;
                connection.ConnectionString = builder.ConnectionString;
                return connection.OpenAndReturn();
            }
            catch (Exception e)
            {
                Log.Error("Failed to create connection.", e);
            }
            return null;
        }

        internal static List<DPProductRecord> GetAllProductRecords(string path)
        {
            using var connection = CreateConnection(path, true);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ProductRecords";
            using var reader = command.ExecuteReader();
            List<DPProductRecord> searchResults = new(4);
            string productName, author, thumbnailPath, sku;
            string[] tags;
            DateTime dateCreated;
            uint extractionID, pid;
            while (reader.Read())
            {
                // Construct product records
                // NULL values return type DB.NULL.
                productName = (string)reader["Product Name"];
                // TODO: Tags have returned null; investigate why.
                var rawTags = reader["Tags"] as string ?? string.Empty;
                tags = rawTags.Trim().Split(", ");
                author = reader["Author"] as string; // May return NULL
                thumbnailPath = reader["Thumbnail Full Path"] as string; // May return NULL
                extractionID = Convert.ToUInt32(reader["Extraction Record ID"] is DBNull ?
                    0 : reader["Extraction Record ID"]);
                dateCreated = DateTime.FromFileTimeUtc((long)reader["Date Created"]);
                sku = reader["SKU"] as string; // May return NULL
                pid = Convert.ToUInt32(reader["ID"]);
                searchResults.Add(
                    new DPProductRecord(productName, tags, author, sku, dateCreated, thumbnailPath, extractionID, pid));
            }
            return searchResults;
        }

        internal static List<DPExtractionRecord> GetAllExtractionRecords(string path)
        {
            using var connection = CreateConnection(path, true);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ExtractionRecords";
            using var reader = command.ExecuteReader();
            List<DPExtractionRecord> searchResults = new(4);
            while (reader.Read())
            {
                string[] files, folders, erroredFiles, errorMessages;
                var archiveFileName = reader["Archive Name"] as string;
                var filesStr = reader["Files"] as string;
                var foldersStr = reader["Folders"] as string;
                var destinationPath = reader["Destination Path"] as string;
                var erroredFilesStr = reader["Errored Files"] as string;
                var errorMessagesStr = reader["Error Messages"] as string;
                var pid = Convert.ToUInt32(reader["Product Record ID"]);

                files = filesStr?.Split(", ") ?? Array.Empty<string>();
                folders = foldersStr?.Split(", ") ?? Array.Empty<string>();
                erroredFiles = erroredFilesStr?.Split(", ") ?? Array.Empty<string>();
                errorMessages = errorMessagesStr?.Split(", ") ?? Array.Empty<string>();
                searchResults.Add(
                    new DPExtractionRecord(archiveFileName, destinationPath, files, erroredFiles, errorMessages, folders, pid));
            }
            return searchResults;
        }

        /// <summary>
        /// A custom assertion for comparing two <see cref="DPProductRecord"/>s.
        /// </summary>
        /// <param name="expected">The expected DPProductRecord.</param>
        /// <param name="actual">The actual DPProductRecord.</param>
        /// <param name="assertIDs">Choose whether to assert ID and EIDs or not.</param>
        internal static void ProductRecordEqual(this Assert a, DPProductRecord expected, DPProductRecord actual, bool assertIDs = false)
        {
            Assert.AreEqual(expected.ProductName, actual.ProductName);
            CollectionAssert.AreEquivalent(expected.Tags, actual.Tags);
            Assert.AreEqual(expected.Author, actual.Author);
            Assert.AreEqual(expected.ThumbnailPath, actual.ThumbnailPath);
            Assert.AreEqual(expected.SKU, actual.SKU);
            Assert.AreEqual(expected.Time, actual.Time);
            if (!assertIDs) return;
            Assert.AreEqual(expected.ID, actual.ID);
            Assert.AreEqual(expected.EID, actual.EID);
        }

        /// <summary>
        /// A custom assertion for comparing two <see cref="DPExtractionRecord"/>s.
        /// </summary>
        /// <param name="expected">The expected <see cref="DPExtractionRecord"/>.</param>
        /// <param name="actual">The actual <see cref="DPExtractionRecord"/>.</param>
        /// <param name="assertIDs">Choose whether to assert IDs or not.</param>
        internal static void ExtractionRecordEqual(this Assert a, DPExtractionRecord expected, DPExtractionRecord actual, bool assertIDs = false)
        {
            Assert.AreEqual(expected.ArchiveFileName, actual.ArchiveFileName);
            Assert.AreEqual(expected.DestinationPath, actual.DestinationPath);
            CollectionAssert.AreEquivalent(expected.Files, actual.Files);
            CollectionAssert.AreEquivalent(expected.Folders, actual.Folders);
            CollectionAssert.AreEquivalent(expected.ErroredFiles, actual.ErroredFiles);
            CollectionAssert.AreEquivalent(expected.ErrorMessages, actual.ErrorMessages);
            if (!assertIDs) return;
            Assert.AreEqual(expected.PID, actual.PID);
        }

        /// <summary>
        /// Executes the task sequentially on the thread pool <paramref name="n"/> times executing <paramref name="action"/> each time.
        /// </summary>
        /// <param name="n">The amount of times to execute the action sequentially.</param>
        /// <param name="action">The action to perform.</param>
        /// <returns>A list of tasks.</returns>
        internal static List<Task> ExecuteTasksSequentially(uint n, Action action)
        {
            List<Task> tasks = new((int) n);
            Task? lastTask = null;
            for (uint j = 0; j < n; j++)
            {
                uint i = j;
                if (lastTask is not null) tasks.Add(lastTask = lastTask.ContinueWith(_ =>
                {
                    Console.WriteLine(i);
                    action();
                }));
                else tasks.Add(lastTask = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine(i);
                    action();
                }));
            }
            return tasks;
        }
    }
}
