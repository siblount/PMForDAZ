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
    /// <summary>
    /// DPTransaction is a wrapper of <see cref="DbTransaction"/> that implements <see cref="IDbTransaction"/>. 
    /// This class allows you to use the using pattern by only disposing on the initial creation of the object.
    /// It also allows for nested transactions. This class handles whether to commit or rollback transactions or to
    /// rollback to a savepoint.
    /// 
    /// To create a transaction that disposes and calls Commit and Rollback, use the constructor <see cref="DPTransaction(DbTransaction, bool)"/>.
    /// Otherwise, use the constructor <see cref="DPTransaction(DPTransaction)"/> to create a transaction that does not dispose and uses savepoints.
    /// </summary>
    public class DPTransaction : IDbTransaction
    {
        /// <summary>
        /// Determines whether this transaction has been disposed.
        /// </summary>
        protected bool Disposed = false;
        /// <summary>
        /// The transaction object to wrap.
        /// </summary>
        protected DbTransaction transaction;
        /// <summary>
        /// The parent transaction, if any.
        /// </summary>
        protected DPTransaction? parentTransaction;
        /// <summary>
        /// Determines whether this transaction should dispose when Dispose is called.
        /// </summary>
        protected bool dispose;
        /// <summary>
        /// The savepoint for this transaction, if any. 
        /// </summary>
        /// <remarks>
        /// This is only used/set when this is not the parent transaction (when <see cref="parentTransaction"/> is null).
        /// </remarks>
        /// <value><see cref="string.Empty"/> if there is no savepoint, otherwise a GUID string.</value>
        public string Savepoint { get; protected set; } = string.Empty;

        /// <summary>
        /// Creates a new DPTransaction.
        /// </summary>
        /// <seealso cref="DPTransaction(DbTransaction, bool)"/>
        /// <param name="t">The transaction to wrap.</param>
        /// <param name="dispose">Whether this transaction will be disposed.</param>
        internal DPTransaction(DbTransaction t, bool dispose)
        {
            transaction = t;
            this.dispose = dispose;
        }

        /// <summary>
        /// A special constructor that will make <paramref name="t"/> as the parent transaction.
        /// This will ensure that the transaction is not disposed when <see cref="Dispose"/> is called.
        /// It will also ensure Commit() and Rollback() use appropriate savepoints.
        /// </summary>
        /// <param name="t">The parent transaction</param>
        internal DPTransaction(DPTransaction t) : this(t.transaction, false)
        {
            parentTransaction = t;
            Savepoint = Guid.NewGuid().ToString();
            transaction.Save(Savepoint);
        }

        /// <summary>
        /// The connection associated with this transaction.
        /// </summary>
        public IDbConnection? Connection => transaction.Connection;

        /// <summary>
        /// The isolation level of this transaction.
        /// </summary>
        public IsolationLevel IsolationLevel => transaction.IsolationLevel;

        /// <summary>
        /// Commits the transaction if there is no parent transaction (if <see cref="parentTransaction"/> is null).
        /// </summary>
        public void Commit()
        {
            if (parentTransaction is null) transaction.Commit();
            Savepoint = string.Empty;
        }

        /// <summary>
        /// Rollbacks the transaction if there is no parent transaction (if <see cref="parentTransaction"/> is null).
        /// Otherwise, it will rollback to the internal savepoint.
        /// </summary>
        public void Rollback()
        {
            if (parentTransaction is null) transaction.Rollback();
            else transaction.Rollback(Savepoint);
        }

        /// <summary>
        /// This may dispose if <see cref="dispose"/> is true and not already disposed. 
        /// Otherwise, it will rollback the transaction at the savepoint (if there is any).
        /// </summary>
        public void Dispose() {
            if (Disposed) return;
            GC.SuppressFinalize(this);
            if (dispose)
            {
                transaction.Dispose();
                Disposed = true;
                return;
            }
            if (!string.IsNullOrEmpty(Savepoint))
            {
                transaction.Rollback(Savepoint);
            };
        }
    }
}
