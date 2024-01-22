using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Database
{
    public class DPTransaction : IDbTransaction
    {
        public bool Disposed { get; protected set; } = false;
        protected DbTransaction transaction;
        protected DPTransaction? parentTransaction;
        protected bool dispose;
        public string savepoint = string.Empty;

        /// <summary>
        /// Creates a new DPTransaction.
        /// </summary>
        /// <param name="t">The transaction to wrap.</param>
        /// <param name="dispose">Whether this transaction will be disposed.</param>
        internal DPTransaction(DbTransaction t, bool dispose)
        {
            transaction = t;
            this.dispose = dispose;
        }
        
        internal DPTransaction(DPTransaction t) : this(t.transaction, false)
        {
            parentTransaction = t;
            savepoint = Guid.NewGuid().ToString();
            transaction.Save(savepoint);
        }

        public IDbConnection? Connection => transaction.Connection;

        public IsolationLevel IsolationLevel => transaction.IsolationLevel;

        public void Commit()
        {
            if (parentTransaction is null) transaction.Commit();
            savepoint = string.Empty;
        }
        public void Rollback()
        {
            if (parentTransaction is null) transaction.Rollback();
            else transaction.Rollback(savepoint);
        }
        public void Dispose() {
            if (Disposed) return;
            GC.SuppressFinalize(this);
            if (dispose)
            {
                transaction.Dispose();
                Disposed = true;
                return;
            }
            if (!string.IsNullOrEmpty(savepoint))
            {
                transaction.Rollback(savepoint);
            };
        }
    }
}
