using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using System.Data;
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
            command.CommandText = $"SELECT * FROM {DPDatabase.ProductTable}";
            using var reader = command.ExecuteReader();
            List<DPProductRecord> searchResults = new(4);
            while (reader.Read())
            {
                var record = new DPProductRecord(
                    Name: reader.GetString("Product Name"),
                    Authors: reader.GetString("Authors").Split(", "),
                    Time: DateTime.FromFileTimeUtc(reader.GetInt64("Time")),
                    ThumbnailPath: reader.GetString("Thumbnail"),
                    ArcName: reader.GetString("ArcName"),
                    Destination: reader.GetString("Destination"),
                    Tags: reader.GetString("Tags").Split(", "),
                    Files: reader.GetString("Files").Split(", "),
                    ID: reader.GetInt64("ROWID")
                );
                searchResults.Add(record);
            }
            return searchResults;
        }

        internal static List<DPProductRecordLite> GetAllProductRecordLites(string path)
        {
            using var connection = CreateConnection(path, true);
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {DPDatabase.ProductLiteView}";
            using var reader = command.ExecuteReader();
            List<DPProductRecordLite> searchResults = new(4);
            while (reader.Read())
            {
                var record = new DPProductRecordLite(
                    Name: reader.GetString("Product Name"),
                    Thumbnail: reader.GetString("Thumbnail"),
                    Tags: reader.GetString("Tags").Split(", "),
                    ID: reader.GetInt64("ID")
                );
                searchResults.Add(record);
            }
            return searchResults;
        }

        /// <summary>
        /// A custom assertion for comparing two <see cref="DPProductRecord"/>s.
        /// </summary>
        /// <param name="expected">The expected DPProductRecord.</param>
        /// <param name="actual">The actual DPProductRecord.</param>
        /// <param name="assertIDs">Choose whether to assert ID and EIDs or not.</param>
        internal static void ProductRecordEqual(this Assert _, DPProductRecord expected, DPProductRecord actual, bool assertIDs = false)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.TagsString, actual.TagsString);
            Assert.AreEqual(expected.AuthorsString, actual.AuthorsString);
            Assert.AreEqual(expected.ThumbnailPath, actual.ThumbnailPath);
            Assert.AreEqual(expected.Time, actual.Time);
            Assert.AreEqual(expected.ArcName, actual.ArcName);
            Assert.AreEqual(expected.Destination, actual.Destination);
            CollectionAssert.AreEquivalent(expected.Files.ToArray(), actual.Files.ToArray());
            if (assertIDs) Assert.AreEqual(expected.ID, actual.ID);
        }

        /// <summary>
        /// A custom assertion for comparing two <see cref="DPProductRecordLite"/>s.
        /// </summary>
        /// <param name="expected">The expected <see cref="DPProductRecordLite"/>.</param>
        /// <param name="actual">The actual <see cref="DPProductRecordLite"/>.</param>
        /// <param name="assertIDs">Choose whether to assert IDs or not.</param>
        internal static void ProductRecordLiteEqual(this Assert a, DPProductRecordLite expected, DPProductRecordLite actual, bool assertIDs = false)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Thumbnail, actual.Thumbnail);
            CollectionAssert.AreEqual(expected.Tags.ToArray(), actual.Tags.ToArray());
            if (assertIDs) Assert.AreEqual(expected.ID, actual.ID);
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
