using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using DAZ_Installer.Database;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
using System.Data;
using Microsoft.Data.Sqlite;

namespace DAZ_Installer.Database.Tests
{
    [TestClass]
    public class DPDatabaseTests
    {
        public static DPDatabase Database { get; set; }
        public static string DatabasePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                File.Delete(DatabasePath);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to clean up test database (Class Cleanup).", ex);
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Database = new DPDatabase(DatabasePath);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Database.StopAllDatabaseOperations();
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                File.Delete(DatabasePath);
                Log.Information("Successfully deleted test database.");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to delete test database.", ex);
            }
        }

        [TestMethod]
        public void AddNewRecordEntryTest()
        {
            var expected = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.UtcNow, "J:/a.png",
                "ArcName.zip", "J:/Destination", new[] { "Test1", "Test2" }, new[] { "a.file", "b.file" }, 0);

            Database.ProductRecordAdded += (actual) => Assert.That.ProductRecordEqual(expected, actual);
            Database.AddNewRecordEntry(expected).Wait();

            var results = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
            Assert.AreEqual(1, results.Count);
            Assert.That.ProductRecordEqual(expected, results[0]);
        }

        [TestMethod]
        public void AddNewRecordEntryTest_Multithreaded()
        {
            const int count = 25;
            List<DPProductRecord> inputs = new(count);
            object lockObj = new();

            int counter = -1;
            var counterFunc = new Func<int>(() => Interlocked.Increment(ref counter));
            var tasks = DPDatabaseTestHelpers.ExecuteTasksSequentially(count, () =>
            {
                var i = counterFunc();
                var record = new DPProductRecord(i.ToString(), Enumerable.Repeat("TheRealSolly", i).ToArray(),
                    DateTime.UtcNow, "abc.png", "TestProduct.rar", "J:/Destination",
                    Enumerable.Range(0, i).Select(x => x.ToString()).ToArray(),
                    Enumerable.Range(0, i).Select(x => x.ToString() + ".file").ToArray(),
                    i);
                inputs.Add(record);
                Database.AddNewRecordEntry(record).Wait();
            });
            Task.WaitAll(tasks.ToArray());

            var results = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
            var results2 = DPDatabaseTestHelpers.GetAllProductRecordLites(DatabasePath);

            Assert.AreEqual(count, results.Count);
            Assert.AreEqual(count, results.Count);
            for (int i = 0; i < count; i++)
            {
                DPProductRecord precord = inputs[i];
                var expectedLite = new DPProductRecordLite(precord.Name, precord.ThumbnailPath, precord.Tags, precord.ID);
                Assert.That.ProductRecordEqual(precord, results[i]);
                Assert.That.ProductRecordLiteEqual(expectedLite, results2[i]);
            }
        }

        [TestMethod]
        public void AddNewRecordEntryTest_Parallel()
        {
            const int count = 50;
            ConcurrentBag<DPProductRecord> inputs = new();
            object lockObj = new();

            var tasks = DPDatabaseTestHelpers.ExecuteInParallel(count, 5, (i) =>
            {
                var record = new DPProductRecord(i.ToString(), Enumerable.Repeat("TheRealSolly", i).ToArray(),
                    DateTime.UtcNow, "abc.png", "TestProduct.rar", "J:/Destination",
                    Enumerable.Range(0, i).Select(x => x.ToString()).ToArray(),
                    Enumerable.Range(0, i).Select(x => x.ToString() + ".file").ToArray(),
                    i);
                inputs.Add(record);
                Database.AddNewRecordEntry(record).Wait();
            });
            tasks.Wait();

            var results = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
            var results2 = DPDatabaseTestHelpers.GetAllProductRecordLites(DatabasePath);

            results.Sort((a, b) => a.Name.CompareTo(b.Name));
            results2.Sort((a, b) => a.Name.CompareTo(b.Name));

            Assert.AreEqual(count, results.Count);
            Assert.AreEqual(count, results.Count);
            var inputsList = inputs.ToList();
            inputsList.Sort((a, b) => a.Name.CompareTo(b.Name));
            for (int i = 0; i < count; i++)
            {
                DPProductRecord precord = inputsList[i];
                var expectedLite = new DPProductRecordLite(precord.Name, precord.ThumbnailPath, precord.Tags, precord.ID);
                Assert.That.ProductRecordEqual(precord, results[i]);
                Assert.That.ProductRecordLiteEqual(expectedLite, results2[i]);
            }
        }

        [TestMethod]
        public void ViewTableQTest()
        {
            using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO TestTable (Name) VALUES ('Test')";
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            bool callbackCalled = false, eventCalled = false;
            var assertLogicFunc = new Action<DataSet>(r =>
            {
                Assert.AreEqual(r.Tables[0].DataSet.DataSetName, "TestTable");
                Assert.AreEqual(r.Tables[0].Rows.Count, 1);
                Assert.AreEqual(r.Tables[0].Rows[0]["Name"], "Test");
            });
            var assertFunc = new Action<DataSet>(r =>
            {
                Assert.IsFalse(callbackCalled);
                assertLogicFunc(r);
                callbackCalled = true;
            });
            Database.ViewUpdated += (r, id) =>
            {
                Assert.IsFalse(eventCalled);
                Assert.AreEqual((uint)1, id);
                assertLogicFunc(r);
                eventCalled = true;
            };

            var result = Database.ViewTableQ("TestTable", 1, callback: assertFunc).Result;

            assertLogicFunc(result);
        }

        [TestMethod]
        public void InsertNewRowQTest()
        {
            using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
            cmd.ExecuteNonQuery();
            c.Dispose();
            Database.TableUpdated += t => Assert.AreEqual("TestTable", t);

            Database.InsertNewRowQ("TestTable", new object[] { "Test" }, new string[] { "Name" }).Wait();

            using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd2 = c2.CreateCommand();
            cmd2.CommandText = "SELECT * FROM TestTable";
            using var reader = cmd2.ExecuteReader();
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(reader["Name"], "Test");
        }

        [TestMethod]
        public void InsertNewRowQTest_Multithreaded()
        {
            using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
            cmd.ExecuteNonQuery();
            c.Dispose();
            // Create a counter function that yields an integer and always increases by 1 (use interlocked or any sync method).
            int counter = -1;
            var counterFunc = new Func<int>(() => Interlocked.Increment(ref counter));
            var tasks = DPDatabaseTestHelpers.ExecuteTasksSequentially(25,
                () => Database.InsertNewRowQ("TestTable", new object[] { counterFunc() }, new string[] { "Name" }).Wait());

            Task.WaitAll(tasks.ToArray());

            using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd2 = c2.CreateCommand();
            cmd2.CommandText = "SELECT * FROM TestTable";
            using var reader = cmd2.ExecuteReader();
            for (int i = 0; i < 25; i++)
            {
                Console.WriteLine(i);
                Assert.IsTrue(reader.Read());
                Assert.IsTrue(int.Parse((string)reader["Name"]) == i);
            }
        }

        [TestMethod]
        public void InsertNewRowQTest_Parallel()
        {
            const int COUNT = 25;
            using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
            cmd.ExecuteNonQuery();
            c.Dispose();

            var task = DPDatabaseTestHelpers.ExecuteInParallel(COUNT, 5,
                (i) => Database.InsertNewRowQ("TestTable", new[] { i.ToString(), i.ToString() }, new string[] { "Name", "Placeholder" }).Wait());

            task.Wait();

            using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd2 = c2.CreateCommand();
            cmd2.CommandText = "SELECT * FROM TestTable";
            using var reader = cmd2.ExecuteReader();

            // Just test that the data inserted isn't jumbled and all the records completed successfully.
            // ID cannot be checked (well...i mean it can but im tired rn).

            HashSet<int> ids = new HashSet<int>(Enumerable.Range(0, COUNT));
            for (int i = 0; i < COUNT; i++)
            {
                Console.WriteLine(i);
                Assert.IsTrue(reader.Read());
                var id = Convert.ToInt32(reader["Name"]);
                Assert.AreEqual(reader["Name"], reader["Placeholder"]);
                ids.Remove(id);
            }
            Assert.AreEqual(0, ids.Count);
        }

        [TestMethod]
        public void RemoveRowQTest()
        {
            using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO TestTable (Name) VALUES ('Test')";
            cmd.ExecuteNonQuery();
            c.Dispose();

            Database.RemoveRowQ("TestTable", 1).Wait();

            using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath, true);
            using var cmd2 = c2.CreateCommand();
            cmd2.CommandText = "SELECT * FROM TestTable";
            using var reader = cmd2.ExecuteReader();
            Assert.IsFalse(reader.Read());
        }

        [TestMethod]
        public void ClearTableQTest()
        {
            using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO TestTable (Name) VALUES ('Test')";
            cmd.ExecuteNonQuery();
            c.Dispose();

            Database.ClearTableQ("TestTable").Wait();

            using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath, true);
            using var cmd2 = c2.CreateCommand();
            cmd2.CommandText = "SELECT * FROM TestTable";
            using var reader = cmd2.ExecuteReader();
            Assert.IsFalse(reader.Read());
        }

        [TestMethod]
        public void UpdateRecordQTest()
        {
            var record = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.UtcNow, "a.png", "arc.zip", "J:/",
                new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);

            Database.AddNewRecordEntry(record).Wait();
            record = record with { Authors = Array.Empty<string>() };

            Database.UpdateRecordQ(1, record).Wait();

            var a = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
            var b = DPDatabaseTestHelpers.GetAllProductRecordLites(DatabasePath);
            Assert.That.ProductRecordEqual(record, a[0], true);
            var liteRecord = new DPProductRecordLite(record.Name, record.ThumbnailPath, record.Tags, record.ID);
            Assert.That.ProductRecordLiteEqual(liteRecord, b[0], true);
        }

        [TestMethod]
        public void GetInstalledArchiveNamesQTest()
        {
            var record = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.UtcNow, "a.png", "arc.zip", "J:/",
                new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);

            Database.AddNewRecordEntry(record).Wait();
            record = record with { Authors = Array.Empty<string>() };
            Database.AddNewRecordEntry(record with { ArcName = "arc2.zip" });

            var a = Database.GetInstalledArchiveNamesQ();
            var result = a.Result.ToArray();
            CollectionAssert.Contains(result, "arc.zip");
            CollectionAssert.Contains(result, "arc2.zip");
        }

        [TestMethod]
        public void RemoveAllRecordsQTest()
        {
            var record = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.UtcNow, "a.png", "arc.zip", "J:/",
                new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);

            Database.AddNewRecordEntry(record).Wait();
            record = record with { Authors = Array.Empty<string>() };
            Database.AddNewRecordEntry(record with { ArcName = "arc2.zip" });

            Database.RemoveAllRecordsQ().Wait();

            Assert.AreEqual(0, DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath).Count);
        }

        [TestMethod]
        public void RemoveProductRecordQTest()
        {
            var record = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.UtcNow, "a.png", "arc.zip", "J:/",
                new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);

            Database.AddNewRecordEntry(record).Wait();
            Database.RemoveProductRecordQ(record).Wait();

            Assert.AreEqual(0, DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath).Count);
        }
    }
}