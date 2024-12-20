using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using System.Threading.Tasks;
using Serilog;
using System.Data.Common;
using System.Data;
using DAZ_Installer.DatabaseTests.Helpers;
using Microsoft.Data.Sqlite;

namespace DAZ_Installer.Database.Integration.Tests
{
    [TestClass]
    public class DPConnectionTests
    {
        public readonly static string TempDir = Path.Combine(Path.GetTempPath(), "DAZ_Installer", "DatabaseTests", "Integration", "DPConnectionTests");
        public readonly static string DatabasePath = Path.Combine(TempDir, "test.db");
        public static SqliteConnection BaseConnection = new SqliteConnection();
        public static SqliteConnectionStringBuilder ConnectionStringBuilder = new SqliteConnectionStringBuilder();
        public static DPConnection Connection = new DPConnection(BaseConnection);
        public static DPConnectionOpts ConnectionOpts;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                .MinimumLevel.Debug()
                .CreateLogger();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            InitializeDatabaseFile();
            ConnectionStringBuilder = new() { DataSource = DatabasePath, Pooling = false };
            BaseConnection = new SqliteConnection(ConnectionStringBuilder.ConnectionString);
            Connection = new DPConnection(BaseConnection);
            ConnectionOpts = new DPConnectionOpts(Connection);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                Connection.Dispose();
                BaseConnection.Dispose();
                Directory.Delete(TempDir, true);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to delete temporary directory.");
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static void InitializeDatabaseFile()
        {
            if (File.Exists(DatabasePath)) File.Delete(DatabasePath);
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        }

        [TestMethod]
        public void BeginTransactionTest()
        {
            Connection.Open();

            var t = Connection.BeginTransaction(ref ConnectionOpts);

            Assert.IsInstanceOfType<SqliteConnection>(t.Connection);
            Assert.AreEqual(ConnectionStringBuilder.ConnectionString, t.Connection!.ConnectionString);
        }

        [TestMethod]
        public void CloseTest()
        {
            Connection.Open();

            Connection.Close();
            Assert.AreEqual(Connection.State, ConnectionState.Closed);
        }

        [TestMethod]
        public void CreateCommandTest()
        {
            var cmd = Connection.CreateCommand();

            Assert.IsInstanceOfType<SqliteCommand>(cmd);
        }

        [TestMethod]
        public void CreateCommandTest1()
        {
            const string txt = "AND HIS NAME IS JOHN CENA!!!";
            var cmd = Connection.CreateCommand(txt);

            Assert.IsInstanceOfType<SqliteCommand>(cmd);
            Assert.AreEqual(txt, cmd.CommandText);
        }

        [TestMethod]
        public void BackupDatabaseTest()
        {
            var dcsb = new SqliteConnectionStringBuilder() { Pooling = false, DataSource = ":memory:", Mode=SqliteOpenMode.Memory, Cache = SqliteCacheMode.Shared };
            using var dest = new SqliteConnection(dcsb.ConnectionString);
            const string createSQL = """
                CREATE TABLE IF NOT EXISTS DummyTable (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name VARCHAR(50),
                    Age INT
                );

                INSERT INTO DummyTable (Name, Age)
                VALUES ('John Doe', 30),
                       ('Jane Smith', 25);
            """;
            const string readSQL = "SELECT * FROM DummyTable";
            dest.Open();
            Connection.Open();
            {
                using var t = Connection.BeginTransaction(ref ConnectionOpts);
                using var cmd = Connection.CreateCommand(createSQL);
                cmd.ExecuteNonQuery();
                t.Commit();
            }

            Connection.BackupDatabase(dest, "main", "main");

            using var readCmd = dest.CreateCommand();
            readCmd.CommandText = readSQL;
            using var reader = readCmd.ExecuteReader();
            var results = new Dictionary<string, int>(2);
            while (reader.Read())
            {
                results.Add(reader.GetString(1), reader.GetInt32(2));
            }

            CollectionAssert.AreEquivalent(new Dictionary<string, int> { { "John Doe", 30 }, { "Jane Smith", 25 } }, results);
        }

        [TestMethod]
        public void OpenTest()
        {
            Connection.Open();

            Assert.AreEqual(Connection.State, ConnectionState.Open);
        }

        [TestMethod]
        public void ConnectionStringTest()
        {
            Assert.AreEqual(BaseConnection.ConnectionString, Connection.ConnectionString);
        }

        [TestMethod]
        public void ConnectionTimeoutTest()
        {
            Assert.AreEqual(BaseConnection.ConnectionTimeout, Connection.ConnectionTimeout);
        }

        [TestMethod]
        public void DatabaseTest()
        {
            Assert.AreEqual(BaseConnection.Database, Connection.Database);
        }

        [TestMethod]
        public void StateTest()
        {
            Assert.AreEqual(BaseConnection.State, Connection.State);
        }
    }
}