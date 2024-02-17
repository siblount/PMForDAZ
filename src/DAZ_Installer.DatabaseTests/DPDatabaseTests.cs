using DAZ_Installer.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using System.Collections.Concurrent;
using Serilog;
using System.Data;
using Microsoft.Data.Sqlite;
using System.CodeDom.Compiler;

namespace DAZ_Installer.Database.Tests
{
    [TestClass]
    public class DPDatabaseTests
    {
        public static DPDatabase Database { get; set; } = null!;
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
            Database.StopAllDatabaseOperations(true);
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

            bool eventCalled = true;
            Database.ProductRecordAdded += (actual) =>
            {
                eventCalled = true;
                Assert.That.ProductRecordEqual(expected, actual);
            };
            Database.AddNewRecordEntry(expected).Wait();

            var results = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(eventCalled);
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
            int eventCounter = 0;

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

            var eventCalled = false;
            Database.TableUpdated += t =>
            {
                eventCalled = true;
                Assert.AreEqual("TestTable", t);
            };
            Database.RemoveRowQ("TestTable", 1).Wait();

            using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath, true);
            using var cmd2 = c2.CreateCommand();
            cmd2.CommandText = "SELECT * FROM TestTable";
            using var reader = cmd2.ExecuteReader();
            Assert.IsFalse(reader.Read());
            Assert.IsTrue(eventCalled);
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

            var eventCalled = true;
            Database.TableUpdated += t =>
            {
                eventCalled = true;
                Assert.AreEqual("TestTable", t);
            };
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

            var callback = (long i) => Assert.AreEqual(1, i);

            Database.AddNewRecordEntry(record).Wait();
            record = record with { Authors = Array.Empty<string>() };

            bool eventCalled = false, event2Called = false;
            bool productsFound = false;
            Database.TableUpdated += (t) =>
            {
                if (t == "Products")
                    productsFound = eventCalled = true;
            };
            Database.ProductRecordModified += (r, i) =>
            {
                event2Called = true;
                Assert.That.ProductRecordEqual(record, r);
                Assert.AreEqual(1, i);
            };
            Database.UpdateRecordQ(1, record, callback).Wait();

            Assert.IsTrue(new[] { eventCalled, event2Called, productsFound }.All(x => x));
            var a = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
            var b = DPDatabaseTestHelpers.GetAllProductRecordLites(DatabasePath);
            Assert.That.ProductRecordEqual(record, a[0], true);
            var liteRecord = new DPProductRecordLite(record.Name, record.ThumbnailPath, record.Tags, record.ID);
            Assert.That.ProductRecordLiteEqual(liteRecord, b[0], true);
        }

        [TestMethod]
        public void ContainsArchiveTest()
        {
            var record = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.UtcNow, "a.png", "arc.zip", "J:/",
                new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);

            Database.AddNewRecordEntry(record);
            record = record with { Authors = Array.Empty<string>() };
            Database.AddNewRecordEntry(record with { ArcName = "arc2.zip" }).Wait();

            Assert.IsTrue(Database.ContainsArchive("arc.zip").Result);
            Assert.IsTrue(Database.ContainsArchive("arc2.zip").Result);
        }

        [TestMethod]
        public void RemoveAllRecordsQTest()
        {
            var record = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.UtcNow, "a.png", "arc.zip", "J:/",
                new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);

            Database.AddNewRecordEntry(record).Wait();
            record = record with { Authors = Array.Empty<string>() };
            Database.AddNewRecordEntry(record with { ArcName = "arc2.zip" });

            bool tableUpdatedEventThrown = false;
            bool recordsClearedEventThrown = false;
            bool databaseUpdatedEventThrown = false;
            Database.TableUpdated += (t) =>
            {
                Assert.AreEqual("Products", t);
                tableUpdatedEventThrown = true;
            };
            Database.RecordsCleared += () => recordsClearedEventThrown = true;
            Database.DatabaseUpdated += () => databaseUpdatedEventThrown = true;
            Database.RemoveAllRecordsQ().Wait();


            Assert.AreEqual(0, DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath).Count);
            Assert.IsTrue(tableUpdatedEventThrown);
            Assert.IsTrue(recordsClearedEventThrown);
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

        [TestMethod]
        public void RemoveProductRecordQTest_Lite()
        {
            var record = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.UtcNow, "a.png", "arc.zip", "J:/",
                new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);
            var recordLite = new DPProductRecordLite(record.Name, record.ThumbnailPath, record.Tags, record.ID);

            Database.AddNewRecordEntry(record).Wait();
            Database.RemoveProductRecordQ(recordLite).Wait();

            Assert.AreEqual(0, DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath).Count);
        }

        [TestMethod]
        public void UpdateDatabaseTest()
        {
            Database.StopMainDatabaseOperations();
            SqliteConnection.ClearAllPools();
            File.Delete(DatabasePath);
            File.Create(DatabasePath).Close();
            using var connection = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            DPDatabaseTestHelpers.CreateV2Database(connection);
            using var cmd = connection.CreateCommand();
            var txt = @"
                INSERT INTO ProductRecords (""Product Name"", ""Tags"", ""Author"", ""SKU"", ""Date Created"", ""Extraction Record ID"", ""Thumbnail Full Path"")
                VALUES ('Test Product', 'tag1, tag2', 'TheRealSolly', 'SKU', 0, 1, 'a.png'),
                       ('Test Product 2', 'tag1, tag2', 'TheRealSolly', 'SKU', 0, 2, 'a.png');
                INSERT INTO ExtractionRecords (""Files"", ""Folders"", ""Destination Path"", ""Errored Files"", ""Error Messages"", ""Archive Name"", ""Product Record ID"")
                VALUES ('file1, file2', 'folder1, folder2', 'J:/', '', '', 'arc.zip', 1),
                       ('file1, file2', 'folder1, folder2', 'J:/', '', '', 'arc2.zip', 2);
                INSERT INTO DatabaseInfo (""Version"", ""Product Record Count"", ""Extraction Record Count"")
                VALUES (2, 2, 2);
";
            cmd.CommandText = txt;
            cmd.ExecuteNonQuery();
            DPDatabaseTestHelpers.FinishV2Database(connection);
            connection.Dispose();
            Database = new DPDatabase(DatabasePath);
            var cts = new CancellationTokenSource();
            //cts.CancelAfter(5000);
            Database.UpdateDatabase(cts.Token).Wait();

            using var connection1 = DPDatabaseTestHelpers.CreateConnection(DatabasePath, true);
            DPDatabaseTestHelpers.AssertOldSchemaRemoved(connection1);

            // Assert that the old records exist in the new database.
            var records = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
            Assert.AreEqual(2, records.Count);
            var expectedRecord = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                                              new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);
            var expectedRecord1 = expectedRecord with { Name = "Test Product 2", ArcName = "arc2.zip" };
            Assert.That.ProductRecordEqual(expectedRecord, records[0]);
            Assert.That.ProductRecordEqual(expectedRecord1, records[1]);


        }

        [TestMethod]
        public void DatabaseFailsOnCorruptedTest()
        {
            Database.StopMainDatabaseOperations();
            SqliteConnection.ClearAllPools();
            File.Delete(DatabasePath);
            File.Create(DatabasePath).Close();
            using var connection = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            DPDatabaseTestHelpers.CreateV2Database(connection);
            using var cmd = connection.CreateCommand();
            DPDatabaseTestHelpers.FinishV2Database(connection);
            connection.Dispose();
            Database = new DPDatabase(DatabasePath);
            var dummyRecord = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                                                             new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);

            Database.AddNewRecordEntry(dummyRecord).Wait();
            Database.StopMainDatabaseOperations();
            SqliteConnection.ClearAllPools();

            Assert.IsTrue(Database.Corrupted);
        }

        [TestMethod]
        public void DatabaseFailsOnUpdateRequiredTest()
        {
            using var connection = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
            using var t = connection.BeginTransaction();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"pragma user_version=0;";
            cmd.ExecuteNonQuery();
            t.Commit();
            connection.Dispose();
            var dummyRecord = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                                                             new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);

            Database = new DPDatabase(DatabasePath);
            Database.AddNewRecordEntry(dummyRecord).Wait();

            Assert.IsTrue(Database.UpdateRequired);
            using var connection2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath, true);
            using var cmd2 = connection2.CreateCommand();
            cmd2.CommandText = "SELECT COUNT(*) FROM Products";
            int result = Convert.ToInt32(cmd2.ExecuteScalar());
            Assert.AreEqual(0, result);

        }

        [TestMethod]
        public void RefreshDatabaseQTest()
        {
            Database.GetType().GetProperty("Flags").SetValue(Database, DPArchiveFlags.UpdateRequired | DPArchiveFlags.Corrupted);
            Database.RefreshDatabaseQ().Wait();
            Assert.AreEqual(Database.Flags, DPArchiveFlags.Initialized);
        }

        [TestMethod]
        public void GetFullProductRecordTest()
        {
            var expected = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);
            Database.AddNewRecordEntry(expected).Wait();

            DPProductRecord? callbackRecord = null;
            var result = Database.GetFullProductRecord(1, r => callbackRecord = r).Result;

            Assert.That.ProductRecordEqual(expected, result, true);
            Assert.That.ProductRecordEqual(expected, callbackRecord, true);
        }

        [TestMethod]
        public void SearchQTest()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"Test Product {i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.SearchQ("Test Product", DPSortMethod.None, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(n, callbackResults.Count);
            Assert.AreEqual(n, result.Count);

            for (var i = 0; i < n; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void SearchQTest_AlphabeticalSort()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"{'A' + i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.SearchQ("tag1", DPSortMethod.Alphabetical, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(n, callbackResults.Count);
            Assert.AreEqual(n, result.Count);

            for (var i = 0; i < n; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void SearchQTest_DateSort()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"{'A' + i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(i), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.SearchQ("tag1", DPSortMethod.Date, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(n, callbackResults.Count);
            Assert.AreEqual(n, result.Count);

            for (var i = 0; i < n; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void SearchQTest_RelevanceSort()
        {
            var perfectMatchRecord = new DPProductRecord("Perfect Match", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                                              new[] { "Perfect", "Match", "with", "other", "tags" }, new[] { "file1", "file2" }, 1);
            var goodMatchRecord = perfectMatchRecord with { Name = "Good Match Perfect", Tags = new[] { "Good", "Match", "Perfect" }, ID = 2 };
            var badMatchRecord = perfectMatchRecord with { Name = "Bad Match", Tags = new[] { "Bad", "Match" }, ID = 3 };

            DPProductRecord[] expected = new[] { perfectMatchRecord, goodMatchRecord };
            Array.ForEach(expected, r => Database.AddNewRecordEntry(r));
            Database.AddNewRecordEntry(badMatchRecord).Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.SearchQ("Perfect Match", DPSortMethod.Relevance, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(expected.Length, callbackResults.Count);
            Assert.AreEqual(expected.Length, result.Count);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i].ToLite(), callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i].ToLite(), result[i]);
            }
        }

        [TestMethod]
        public void GetProductRecordsQTest()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"Test Product {i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.GetProductRecordsQ(DPSortMethod.None, 1, n, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(n, callbackResults.Count);
            Assert.AreEqual(n, result.Count);

            for (var i = 0; i < n; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void GetProductRecordsQTest_AlphabeticalSort()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"{'A' + i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.GetProductRecordsQ(DPSortMethod.Alphabetical, 1, n, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(n, callbackResults.Count);
            Assert.AreEqual(n, result.Count);

            for (var i = 0; i < n; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void GetProductRecordsQTest_DateSort()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"{'A' + i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(i), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.GetProductRecordsQ(DPSortMethod.Date, 1, n, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(n, callbackResults.Count);
            Assert.AreEqual(n, result.Count);

            for (var i = 0; i < n; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void GetProductRecordsQTest_FirstPage()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"Test Product {i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            expected = expected.Take(10).ToList();
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.GetProductRecordsQ(DPSortMethod.None, 1, 10, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(expected.Count, callbackResults.Count);
            Assert.AreEqual(expected.Count, result.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void GetProductRecordsQTest_MiddlePage()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"Test Product {i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            expected = expected.Take(new Range(5, 10)).ToList();
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.GetProductRecordsQ(DPSortMethod.None, 2, 5, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(expected.Count, callbackResults.Count);
            Assert.AreEqual(expected.Count, result.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }
        [TestMethod]
        public void GetProductRecordsQTest_LastPage()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"Test Product {i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            expected = expected.Skip(20).ToList();
            lastTask!.Wait();

            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.GetProductRecordsQ(DPSortMethod.None, 3, 10, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(expected.Count, callbackResults.Count);
            Assert.AreEqual(expected.Count, result.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void GetProductRecordsQTest_DateSortMiddlePage()
        {
            // Create some dummy, but distingiushable records
            const int n = 25;
            Task? lastTask = null;
            List<DPProductRecordLite> expected = new(n);
            for (var i = 0; i < n; i++)
            {
                var record = new DPProductRecord($"{'A' + i}", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(i), "a.png", "arc.zip", "J:/",
                               new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, (uint)i);
                expected.Add(record.ToLite());
                lastTask = Database.AddNewRecordEntry(record);
            }
            lastTask!.Wait();
            expected = expected.Take(new Range(5, 10)).ToList();
            List<DPProductRecordLite>? callbackResults = null;
            var result = Database.GetProductRecordsQ(DPSortMethod.Date, 2, 5, 0, r => callbackResults = r).Result;

            Assert.IsNotNull(callbackResults);
            Assert.AreEqual(expected.Count, callbackResults.Count);
            Assert.AreEqual(expected.Count, result.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.That.ProductRecordLiteEqual(expected[i], callbackResults[i]);
                Assert.That.ProductRecordLiteEqual(expected[i], result[i]);
            }
        }

        [TestMethod]
        public void RestoreDatabaseQTest()
        {
            // Database file is created and SQLiteConnection pool is available at this point.
            var expectedRecord = new DPProductRecord("Test Product", new[] { "TheRealSolly" }, DateTime.FromFileTimeUtc(0), "a.png", "arc.zip", "J:/",
                                                             new[] { "tag1", "tag2" }, new[] { "file1", "file2" }, 1);
            Database.AddNewRecordEntry(expectedRecord).Wait();
            Database.StopAllDatabaseOperations(true);
            SqliteConnection.ClearAllPools();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            // At this point, all pools should be removed. This means there should not be any handle on the database file.
            var newPath = Path.Combine(Path.GetDirectoryName(DatabasePath), Path.GetFileNameWithoutExtension(DatabasePath) + "_backup.db");
            File.Move(DatabasePath, newPath);
            // Database file is created again since the file does not exist.
            Database = new DPDatabase(DatabasePath);
            var callbackResult = false;

            var result = Database.RestoreDatabaseQ(newPath, r => callbackResult = r).Result;
            Assert.IsTrue(callbackResult);
            Assert.IsTrue(result);
            Assert.AreEqual(DPArchiveFlags.Initialized, Database.Flags);
            var record = Database.GetFullProductRecord(1).Result;
            Assert.That.ProductRecordEqual(expectedRecord, record, true);
        }
    }
}