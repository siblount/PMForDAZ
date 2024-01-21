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
                SQLiteConnection.Shutdown(true, false);
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
            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                File.Delete(DatabasePath);
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
        //[TestMethod]
        //public void AddNewRecordEntryTest_Multithreaded()
        //{
        //    List<Tuple<DPProductRecord, DPProductRecordLite>> inputs = new(25);
        //    object lockObj = new();

        //    int counter = -1;
        //    var counterFunc = new Func<int>(() => Interlocked.Increment(ref counter));
        //    var tasks = DPDatabaseTestHelpers.ExecuteTasksSequentially(25, () =>
        //    {
        //        var i = (uint) counterFunc();
        //        var precord = new DPProductRecord(i.ToString(), new[] { "Test" }, "TheRealSolly", "", DateTime.UtcNow, "abc.png", i + 1, i + 1);
        //        var erecord = new DPProductRecordLite("TestProduct.rar", "A:/b.rar", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), i);
        //        inputs.Add(new Tuple<DPProductRecord, DPProductRecordLite>(precord, erecord));
        //        Database.AddNewRecordEntry(precord, erecord).Wait();
        //    });
        //    Task.WaitAll(tasks.ToArray());

        //    var results = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
        //    var results2 = DPDatabaseTestHelpers.GetAllProductRecordLites(DatabasePath);

        //    Assert.AreEqual(results.Count, 25);
        //    Assert.AreEqual(results2.Count, 25);
        //    for (int i = 0; i < 25; i++)
        //    {
        //        (DPProductRecord precord, DPProductRecordLite erecord) = inputs[i];
        //        Assert.That.ProductRecordEqual(precord, results[i]);
        //        Assert.That.ProductRecordLiteEqual(erecord, results2[i]);
        //    }

        //}

        //[TestMethod]
        //public void ViewTableQTest()
        //{
        //    using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
        //    using var cmd = c.CreateCommand();
        //    cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT)";
        //    cmd.ExecuteNonQuery();
        //    cmd.CommandText = "INSERT INTO TestTable (Name) VALUES ('Test')";
        //    cmd.ExecuteNonQuery();
        //    cmd.Dispose();
        //    bool callbackCalled = false, eventCalled = false;
        //    var assertLogicFunc = new Action<DataSet>(r =>
        //    {
        //        Assert.AreEqual(r.Tables[0].DataSet.DataSetName, "TestTable");
        //        Assert.AreEqual(r.Tables[0].Rows.Count, 1);
        //        Assert.AreEqual(r.Tables[0].Rows[0]["Name"], "Test");
        //    });
        //    var assertFunc = new Action<DataSet>(r =>
        //    {
        //        Assert.IsFalse(callbackCalled);
        //        assertLogicFunc(r);
        //        callbackCalled = true;
        //    });
        //    Database.ViewUpdated += (r, id) =>
        //    {
        //        Assert.IsFalse(eventCalled);
        //        Assert.AreEqual((uint) 1, id);
        //        assertLogicFunc(r);
        //        eventCalled = true;
        //    };

        //    var result = Database.ViewTableQ("TestTable", 1, callback: assertFunc).Result;

        //    assertLogicFunc(result);
        //}

        //[TestMethod]
        //public void InsertNewRowQTest()
        //{
        //    using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
        //    using var cmd = c.CreateCommand();
        //    cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
        //    cmd.ExecuteNonQuery();
        //    c.Dispose();
        //    Database.TableUpdated += t => Assert.AreEqual("TestTable", t);

        //    Database.InsertNewRowQ("TestTable", new object[] { "Test" }, new string[] { "Name" }).Wait();

        //    using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
        //    using var cmd2 = c2.CreateCommand();
        //    cmd2.CommandText = "SELECT * FROM TestTable";
        //    using var reader = cmd2.ExecuteReader();
        //    Assert.IsTrue(reader.Read());
        //    Assert.AreEqual(reader["Name"], "Test");
        //}
        //[TestMethod]
        //public void InsertNewRowQTest_Multithreaded()
        //{
        //    using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
        //    using var cmd = c.CreateCommand();
        //    cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
        //    cmd.ExecuteNonQuery();
        //    c.Dispose();
        //    // Create a counter function that yields an integer and always increases by 1 (use interlocked or any sync method).
        //    int counter = -1;
        //    var counterFunc = new Func<int>(() => Interlocked.Increment(ref counter));
        //    var tasks = DPDatabaseTestHelpers.ExecuteTasksSequentially(25, 
        //        () => Database.InsertNewRowQ("TestTable", new object[] { counterFunc() }, new string[] { "Name" }).Wait());

        //    Task.WaitAll(tasks.ToArray());

        //    using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
        //    using var cmd2 = c2.CreateCommand();
        //    cmd2.CommandText = "SELECT * FROM TestTable";
        //    using var reader = cmd2.ExecuteReader();
        //    for (int i = 0; i < 25; i++)
        //    {
        //        Console.WriteLine(i);
        //        Assert.IsTrue(reader.Read());
        //        Assert.IsTrue(int.Parse((string) reader["Name"]) == i);
        //    }
        //}

        //[TestMethod]
        //public void RemoveRowQTest()
        //{
        //    using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
        //    using var cmd = c.CreateCommand();
        //    cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
        //    cmd.ExecuteNonQuery();
        //    cmd.CommandText = "INSERT INTO TestTable (Name) VALUES ('Test')";
        //    cmd.ExecuteNonQuery();
        //    c.Dispose();

        //    Database.RemoveRowQ("TestTable", 1).Wait();

        //    using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath, true);
        //    using var cmd2 = c2.CreateCommand();
        //    cmd2.CommandText = "SELECT * FROM TestTable";
        //    using var reader = cmd2.ExecuteReader();
        //    Assert.IsFalse(reader.Read());
        //}

        //[TestMethod]
        //public void ClearTableQTest()
        //{
        //    using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
        //    using var cmd = c.CreateCommand();
        //    cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
        //    cmd.ExecuteNonQuery();
        //    cmd.CommandText = "INSERT INTO TestTable (Name) VALUES ('Test')";
        //    cmd.ExecuteNonQuery();
        //    c.Dispose();

        //    Database.ClearTableQ("TestTable").Wait();

        //    using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath, true);
        //    using var cmd2 = c2.CreateCommand();
        //    cmd2.CommandText = "SELECT * FROM TestTable";
        //    using var reader = cmd2.ExecuteReader();
        //    Assert.IsFalse(reader.Read());
        //}

        //[TestMethod]
        //public void UpdateValuesQTest()
        //{
        //    // Not fully implemented yet.
        //    Assert.Inconclusive();
        //    using var c = DPDatabaseTestHelpers.CreateConnection(DatabasePath);
        //    using var cmd = c.CreateCommand();
        //    cmd.CommandText = "CREATE TABLE TestTable (ID INTEGER PRIMARY KEY, Name TEXT, Placeholder TEXT)";
        //    cmd.ExecuteNonQuery();
        //    cmd.CommandText = "INSERT INTO TestTable (Name) VALUES ('Test')";
        //    cmd.ExecuteNonQuery();
        //    c.Dispose();

        //    Database.UpdateValuesQ("TestTable", new string[] { "Test2" }, new string[] { "Name" }, 1).Wait();

        //    using var c2 = DPDatabaseTestHelpers.CreateConnection(DatabasePath, true);
        //    using var cmd2 = c2.CreateCommand();
        //    cmd2.CommandText = "SELECT * FROM TestTable";
        //    using var reader = cmd2.ExecuteReader();
        //    Assert.IsTrue(reader.Read());
        //    Assert.AreEqual(reader["Name"], "Test2");
        //}

        //[TestMethod]
        //public void UpdateRecordQTest()
        //{
        //    var dummyRecord = new DPProductRecord("Test Product", new[] { "Test" }, "TheRealSolly", "", DateTime.UtcNow, "abc.png", 1, 1);
        //    var dummyERecord = new DPProductRecordLite("TestProduct.rar", "A:/b.rar", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), 0);
        //    Database.AddNewRecordEntry(dummyRecord, dummyERecord).Wait();
        //    dummyRecord = dummyRecord with { Author = "" };

        //    Database.UpdateRecordQ(1, dummyRecord, dummyERecord).Wait();

        //    var a = DPDatabaseTestHelpers.GetAllProductRecords(DatabasePath);
        //    var b = DPDatabaseTestHelpers.GetAllProductRecordLites(DatabasePath);
        //    Assert.That.ProductRecordEqual(dummyRecord, a[0]);
        //    Assert.That.ProductRecordLiteEqual(dummyERecord, b[0]);
        //}
    }
}