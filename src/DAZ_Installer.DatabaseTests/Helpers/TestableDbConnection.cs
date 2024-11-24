using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.DatabaseTests.Helpers
{
    public abstract class TestableDbConnection : DbConnection
    {
        public bool BeginTransactionCalled = false;
        public bool CreateCommandCalled = false;
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            BeginTransactionCalled = true;
            return BeginDbTransactionPublic(isolationLevel);
        }
        protected override DbCommand CreateDbCommand()
        {
            CreateCommandCalled = true;
            return CreateDbCommandPublic();
        }
        public abstract DbTransaction BeginDbTransactionPublic(IsolationLevel isolationLvl);
        public abstract DbCommand CreateDbCommandPublic();
        protected override void Dispose(bool disposing)
        {
            DisposePublic();
            base.Dispose(disposing);
        }
        public abstract void DisposePublic();
    }
}
