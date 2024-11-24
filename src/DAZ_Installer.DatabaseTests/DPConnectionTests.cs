using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using System.Threading.Tasks;
using Serilog;
using Moq;
using System.Data.Common;
using Moq.Protected;
using System.Data;
using DAZ_Installer.DatabaseTests.Helpers;
using Microsoft.Data.Sqlite;

namespace DAZ_Installer.Database.Tests
{
    [TestClass]
    public class DPConnectionTests
    {
        public static Mock<TestableDbConnection> MockBaseConnection = new();
        public static TestableDbConnection BaseConnection => MockBaseConnection.Object;
        public static DPConnection Connection = new DPConnection(BaseConnection);
        public static SqliteConnectionOpts ConnectionOpts;

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
            MockBaseConnection = new Mock<TestableDbConnection>() { CallBase = true };
            Connection = new DPConnection(BaseConnection);
            ConnectionOpts = new SqliteConnectionOpts(Connection);
        }

        [TestMethod]
        public void BeginTransactionTest_Interface()
        {
            var mockTransaction = new Mock<DbTransaction>();
            // DbConnection.BeginTransaction calls DbConnection.BeginDbTransaction() internally.
            // DbConnection.BeginDbTransaction() is abstract protected, so we need to mock it.
            MockBaseConnection.Setup(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified)).Returns(mockTransaction.Object);

            var transaction = ((IDbConnection) Connection).BeginTransaction();

            Assert.IsTrue(BaseConnection.BeginTransactionCalled);
            Assert.AreNotSame(transaction, ConnectionOpts.Transaction);
            MockBaseConnection.Verify(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified), Times.Once());
        }

        [TestMethod]
        public void BeginTransactionTest1_Interface()
        {
            var mockTransaction = new Mock<DbTransaction>();
            // DbConnection.BeginTransaction calls DbConnection.BeginDbTransaction() internally.
            // DbConnection.BeginDbTransaction() is abstract protected, so we need to mock it.
            MockBaseConnection.Setup(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified)).Returns(mockTransaction.Object);

            var transaction = ((IDbConnection)Connection).BeginTransaction(IsolationLevel.Chaos);

            Assert.IsTrue(BaseConnection.BeginTransactionCalled);
            Assert.AreNotSame(transaction, ConnectionOpts.Transaction);
            MockBaseConnection.Verify(x => x.BeginDbTransactionPublic(IsolationLevel.Chaos), Times.Once());
        }

        [TestMethod]
        public void BeginTransactionTest_NoParentHasNoTransaction()
        {
            var mockTransaction = new Mock<DbTransaction>();
            // DbConnection.BeginTransaction calls DbConnection.BeginDbTransaction() internally.
            // DbConnection.BeginDbTransaction() is abstract protected, so we need to mock it.
            MockBaseConnection.Setup(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified)).Returns(mockTransaction.Object);

            var transaction = Connection.BeginTransaction(ref ConnectionOpts);

            Assert.IsTrue(BaseConnection.BeginTransactionCalled);
            Assert.AreSame(transaction, ConnectionOpts.Transaction);
            MockBaseConnection.Verify(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified), Times.Once());
        }
        [TestMethod]
        public void BeginTransactionTest_ParentHasNoTransaction()
        {
            var mockTransaction = new Mock<DbTransaction>();
            // DbConnection.BeginTransaction calls DbConnection.BeginDbTransaction() internally.
            // DbConnection.BeginDbTransaction() is abstract protected, so we need to mock it.
            MockBaseConnection.Setup(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified)).Returns(mockTransaction.Object);

            DPTransaction? transaction = null;
            // Mimic a transaction in a function.
            {
                var childConnection = new DPConnection(Connection);
                SqliteConnectionOpts opts = new SqliteConnectionOpts(childConnection);
                transaction = Connection.BeginTransaction(ref opts);
                Assert.AreSame(transaction, opts.Transaction);
            }

            Assert.IsTrue(BaseConnection.BeginTransactionCalled);
            MockBaseConnection.Verify(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified), Times.Once());
        }
        [TestMethod]
        public void BeginTransactionTest_ParentHasTransaction()
        {
            var mockTransaction = new Mock<DbTransaction>();
            // DbConnection.BeginTransaction calls DbConnection.BeginDbTransaction() internally.
            // DbConnection.BeginDbTransaction() is abstract protected, so we need to mock it.
            MockBaseConnection.Setup(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified)).Returns(mockTransaction.Object);
            var t = Connection.BeginTransaction(ref ConnectionOpts);

            DPTransaction? childTransaction = null;
            // Mimic a transaction in a function.
            {
                var childConnection = new DPConnection(Connection);
                SqliteConnectionOpts opts = new SqliteConnectionOpts(childConnection);
                childTransaction = Connection.BeginTransaction(ref opts);
                Assert.AreSame(childTransaction, opts.Transaction);
            }

            Assert.IsTrue(BaseConnection.BeginTransactionCalled);
            Assert.AreSame(t, ConnectionOpts.Transaction);
            MockBaseConnection.Verify(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified), Times.Once());
        }
        [TestMethod]
        public void BeginTransactionTest1_NoParentHasNoTransaction()
        {
            var mockTransaction = new Mock<DbTransaction>();
            // DbConnection.BeginTransaction calls DbConnection.BeginDbTransaction() internally.
            // DbConnection.BeginDbTransaction() is abstract protected, so we need to mock it.
            MockBaseConnection.Setup(x => x.BeginDbTransactionPublic(IsolationLevel.Chaos)).Returns(mockTransaction.Object);

            var transaction = Connection.BeginTransaction(IsolationLevel.Chaos, ref ConnectionOpts);

            Assert.IsTrue(BaseConnection.BeginTransactionCalled);
            Assert.AreSame(transaction, ConnectionOpts.Transaction);
        }
        [TestMethod]
        public void BeginTransactionTest1_ParentHasNoTransaction()
        {
            var mockTransaction = new Mock<DbTransaction>();
            // DbConnection.BeginTransaction calls DbConnection.BeginDbTransaction() internally.
            // DbConnection.BeginDbTransaction() is abstract protected, so we need to mock it.
            MockBaseConnection.Setup(x => x.BeginDbTransactionPublic(IsolationLevel.Chaos)).Returns(mockTransaction.Object);

            DPTransaction? transaction = null;
            // Mimic a transaction in a function.
            {
                var childConnection = new DPConnection(Connection);
                SqliteConnectionOpts opts = new SqliteConnectionOpts(childConnection);
                transaction = Connection.BeginTransaction(IsolationLevel.Chaos, ref opts);

                Assert.AreSame(transaction, opts.Transaction);
            }

            Assert.IsTrue(BaseConnection.BeginTransactionCalled);
            MockBaseConnection.Verify(x => x.BeginDbTransactionPublic(IsolationLevel.Chaos), Times.Once());
        }
        [TestMethod]
        public void BeginTransactionTest1_ParentHasTransaction()
        {
            var mockTransaction = new Mock<DbTransaction>();
            // DbConnection.BeginTransaction calls DbConnection.BeginDbTransaction() internally.
            // DbConnection.BeginDbTransaction() is abstract protected, so we need to mock it.
            MockBaseConnection.Setup(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified)).Returns(mockTransaction.Object);
            var t = Connection.BeginTransaction(ref ConnectionOpts);

            DPTransaction? childTransaction = null;
            // Mimic a transaction in a function.
            {
                var childConnection = new DPConnection(Connection);
                SqliteConnectionOpts opts = new SqliteConnectionOpts(childConnection);
                childTransaction = Connection.BeginTransaction(IsolationLevel.Chaos, ref opts);
                Assert.AreSame(childTransaction, opts.Transaction);
            }

            Assert.IsTrue(BaseConnection.BeginTransactionCalled);
            Assert.AreSame(t, ConnectionOpts.Transaction);
            MockBaseConnection.Verify(x => x.BeginDbTransactionPublic(IsolationLevel.Unspecified), Times.Once());
        }

        [TestMethod]
        public void ChangeDatabaseTest()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Connection.ChangeDatabase("test");
#pragma warning restore CS0618 // Type or member is obsolete

            MockBaseConnection.Verify(x => x.ChangeDatabase("test"), Times.Once());
        }

        [TestMethod]
        public void CloseTest()
        {
            Connection.Close();

            MockBaseConnection.Verify(x => x.Close(), Times.Once());
        }

        [TestMethod]
        public void CreateCommandTest()
        {
            var mockCommand = new Mock<DbCommand>();
            MockBaseConnection.Setup(x => x.CreateDbCommandPublic()).Returns(mockCommand.Object);

            Connection.CreateCommand();

            Assert.IsTrue(BaseConnection.CreateCommandCalled);
            MockBaseConnection.Verify(x => x.CreateDbCommandPublic(), Times.Once());
        }

        [TestMethod]
        public void CreateCommandTest_Interface()
        {
            var mockCommand = new Mock<DbCommand>();
            MockBaseConnection.Setup(x => x.CreateDbCommandPublic()).Returns(mockCommand.Object);

            ((IDbConnection) Connection).CreateCommand();

            Assert.IsTrue(BaseConnection.CreateCommandCalled);
            MockBaseConnection.Verify(x => x.CreateDbCommandPublic(), Times.Once());
        }


        [TestMethod()]
        public void CreateCommandTest1()
        {
            var mockCommand = new Mock<DbCommand>();
            MockBaseConnection.Setup(x => x.CreateDbCommandPublic()).Returns(mockCommand.Object);

            Connection.CreateCommand("foo");

            Assert.IsTrue(BaseConnection.CreateCommandCalled);
            mockCommand.VerifySet(x => x.CommandText = "foo", Times.Once());
            MockBaseConnection.Verify(x => x.CreateDbCommandPublic(), Times.Once());
        }

        [TestMethod]
        public void BackupDatabaseTest_ThrowsCastException()
        {
            var dummyConnection = new SqliteConnection();
            Assert.ThrowsException<InvalidCastException>(() => Connection.BackupDatabase(dummyConnection, "test", "test"));
        }

        [TestMethod]
        public void DisposeTest()
        {
            Connection.Dispose();
            
            MockBaseConnection.Verify(x => x.DisposePublic(), Times.Once());
        }

        [TestMethod]
        public void DisposeTest_AlreadyDisposed()
        {
            Connection.Dispose();
            Connection.Dispose();

            MockBaseConnection.Verify(x => x.DisposePublic(), Times.Once());
        }

        [TestMethod]
        public void DisposeTest_NotDisposedForChildren()
        {
            var childConnection = new DPConnection(Connection);
            childConnection.Dispose();
            Connection.Dispose();

            MockBaseConnection.Verify(x => x.DisposePublic(), Times.Once());
        }

        [TestMethod]
        public void OpenTest()
        {
            Connection.Open();

            MockBaseConnection.Verify(x => x.Open(), Times.Once());
        }

        [TestMethod]
        public void ConnectionStringTest()
        {
            MockBaseConnection.SetupAllProperties();
            Connection.ConnectionString = "test";
            MockBaseConnection.VerifySet(x => x.ConnectionString = "test", Times.Once());
            Assert.AreEqual("test", Connection.ConnectionString);
        }

        [TestMethod]
        public void ConnectionTimeoutTest()
        {
            MockBaseConnection.SetupGet(x => x.ConnectionTimeout).Returns(10);
            Assert.AreEqual(10, Connection.ConnectionTimeout);
        }

        [TestMethod]
        public void DatabaseTest()
        {
            MockBaseConnection.SetupGet(x => x.Database).Returns("test");
            Assert.AreEqual("test", Connection.Database);
        }

        [TestMethod]
        public void StateTest()
        {
            MockBaseConnection.SetupGet(x => x.State).Returns(ConnectionState.Executing);
            Assert.AreEqual(ConnectionState.Executing, Connection.State);
        }
    }
}