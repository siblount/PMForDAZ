using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using DAZ_Installer.Database;

namespace DAZ_Installer.DatabaseTests
{
    [TestClass]
    public class DPConnectionOptsTests
    {
        public Mock<TestableDbConnection> MockBaseConnection = null!;
        public Mock<TestableDbTransaction> MockBaseTransaction = null!;
        public DPConnection Connection = null!;
        public DPTransaction Transaction = null!;
        public TestableDbConnection BaseConnection => MockBaseConnection.Object;
        public TestableDbTransaction BaseTransaction => MockBaseTransaction.Object;


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
            MockBaseConnection = new Mock<TestableDbConnection>();
            MockBaseTransaction = new Mock<TestableDbTransaction>();
            Connection = new DPConnection(BaseConnection);
            Transaction = new DPTransaction(BaseTransaction, true);
        }

        [TestMethod]
        public void DPConnectionOptsTest()
        {
            var c = new DPConnectionOpts();

            Assert.IsNull(c.Connection);
            Assert.IsNull(c.Transaction);
        }

        [TestMethod]
        public void DPConnectionOptsTest1()
        {
            var c = new DPConnectionOpts(Connection);

            Assert.AreSame(Connection, c.Connection);
            Assert.IsNull(c.Transaction);
        }

        [TestMethod]
        public void DPConnectionOptsTest2()
        {
            var c = new DPConnectionOpts(Connection, Transaction);

            Assert.AreSame(Connection, c.Connection);
            Assert.AreSame(Transaction, c.Transaction);
        }

        [TestMethod]
        public void DPConnectionOptsTest3()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var c = new DPConnectionOpts(Connection, Transaction, cts.Token);

            Assert.AreSame(Connection, c.Connection);
            Assert.AreSame(Transaction, c.Transaction);
            Assert.IsTrue(c.CancellationToken.IsCancellationRequested);
        }

        [TestMethod]
        public void CreateCommandTest()
        {
            var mockCommand = new Mock<DbCommand>();
            mockCommand.SetupAllProperties();
            MockBaseConnection.Setup(x => x.CreateDbCommandPublic()).Returns(mockCommand.Object);
            var opts = new DPConnectionOpts(Connection);

            var cmd = opts.CreateCommand("SELECT * FROM table");
            
            Assert.AreEqual("SELECT * FROM table", mockCommand.Object.CommandText);
        }

        [TestMethod]
        public void CreateCommandTest_NullConnection()
        {
            var opts = new DPConnectionOpts();
            Assert.ThrowsException<ArgumentNullException>(() => opts.CreateCommand(""));
        }

        [TestMethod]
        public void ConnectionTest_Getter()
        {
            var co = new DPConnectionOpts(Connection);

            Assert.AreSame(Connection, co.Connection);
        }

        [TestMethod]
        public void ConnectionTest_Setter_NullConnection()
        {
            var co = new DPConnectionOpts();
            co.Connection = Connection;

            Assert.AreSame(Connection, co.Connection);
        }

        [TestMethod]
        public void ConnectionTest_Setter_Connection()
        {
            var co = new DPConnectionOpts(Connection);
            co.Connection = null;
            co.Connection?.Close();

            Assert.IsNotNull(co.Connection);
            Assert.AreNotSame(Connection, co.Connection);
            MockBaseConnection.Verify(x => x.Close(), Times.Once);
        }

        [TestMethod]
        public void TransactionTest()
        {
            var co = new DPConnectionOpts(Connection);
            co.Transaction = Transaction;

            Assert.AreSame(Transaction, co.Transaction);
        }

        [TestMethod]
        public void CancellationTokenTest()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var co = new DPConnectionOpts(Connection, Transaction, cts.Token);
            Assert.IsTrue(co.CancellationToken.IsCancellationRequested);
        }

        [TestMethod]
        public void IsCancellationRequestedTest()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var co = new DPConnectionOpts(Connection, Transaction, cts.Token);
            Assert.IsTrue(co.IsCancellationRequested);
        }
    }
}
