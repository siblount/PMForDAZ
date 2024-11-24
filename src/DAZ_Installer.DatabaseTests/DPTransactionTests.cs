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


namespace DAZ_Installer.Database.Tests
{
    [TestClass]
    public class DPTransactionTests
    {
        public Mock<TestableDbTransaction> MockBaseTransaction = null!;
        public DbTransaction BaseTransaction => MockBaseTransaction.Object;
        public DPTransaction ParentTransaction = null!;
        public DPTransaction ChildTransaction = null!;

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
            MockBaseTransaction = new Mock<TestableDbTransaction>();
            ParentTransaction = new DPTransaction(BaseTransaction, true);
            ChildTransaction = new DPTransaction(ParentTransaction);
        }

        [TestMethod]
        public void DPTransaction()
        {
            var t = new DPTransaction(BaseTransaction, true);
            t.Dispose();

            Assert.AreEqual("", t.Savepoint);
        }

        [TestMethod]
        public void DPTransaction1()
        {
            var t = new DPTransaction(new DPTransaction(BaseTransaction, true));
            t.Dispose();

            Assert.IsFalse(string.IsNullOrEmpty(t.Savepoint));
            MockBaseTransaction.Verify(x => x.Save(t.Savepoint), Times.Once);
        }

        [TestMethod]
        public void CommitTest_Parent()
        {
            ParentTransaction.Commit();

            MockBaseTransaction.Verify(t => t.Commit(), Times.Once);
            Assert.AreEqual(string.Empty, ParentTransaction.Savepoint);
        }

        [TestMethod]
        public void CommitTest_Child()
        {
            ChildTransaction.Commit();

            MockBaseTransaction.Verify(t => t.Commit(), Times.Never);
            MockBaseTransaction.Verify(t => t.Save(It.IsAny<string>()), Times.Once);
            Assert.AreEqual(string.Empty, ParentTransaction.Savepoint);
        }

        [TestMethod]
        public void RollbackTest_Parent()
        {
            ParentTransaction.Rollback();

            MockBaseTransaction.Verify(t => t.Rollback(), Times.Once);
            Assert.AreEqual("", ParentTransaction.Savepoint);
        }

        [TestMethod]
        public void RollbackTest_Child()
        {
            ChildTransaction.Rollback();

            MockBaseTransaction.Verify(t => t.Rollback(), Times.Never);
            Assert.IsFalse(string.IsNullOrEmpty(ChildTransaction.Savepoint));
        }

        [TestMethod]
        public void DisposeTest_Parent()
        {
            ParentTransaction.Dispose();

            MockBaseTransaction.Verify(t => t.DisposePublic(), Times.Once);
        }

        [TestMethod]
        public void DisposeTest_MultipleCalls()
        {
            ParentTransaction.Dispose();
            ParentTransaction.Dispose();

            MockBaseTransaction.Verify(t => t.DisposePublic(), Times.Once);
        }

        [TestMethod]
        public void DisposeTest_ChildNoSavepoint()
        {
            var sp = ChildTransaction.Savepoint;
            ChildTransaction.Commit(); // sets savepoint to empty

            ChildTransaction.Dispose();

            MockBaseTransaction.Verify(t => t.DisposePublic(), Times.Never);
            MockBaseTransaction.Verify(t => t.Rollback(sp), Times.Never);
        }

        [TestMethod]
        public void DisposeTest_ChildSavepoint()
        {
            var sp = ChildTransaction.Savepoint;

            ChildTransaction.Dispose();

            MockBaseTransaction.Verify(t => t.DisposePublic(), Times.Never);
            MockBaseTransaction.Verify(t => t.Rollback(sp), Times.Once);
        }
    }
}