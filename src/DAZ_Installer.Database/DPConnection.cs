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
    /// A wrapper of <see cref="DbConnection"/> that implements <see cref="IDbConnection"/>. This class allows you to use the using pattern by only disposing
    /// on the initial creation of the object. 
    /// </summary>
    public class DPConnection : IDbConnection
    {
        public readonly DbConnection Connection;
        protected readonly DPConnection? parentConnection;
        protected DPTransaction? transaction;
        private bool dispose;
        private bool Disposed = false;
        public string ConnectionString { get => Connection.ConnectionString; set => Connection.ConnectionString = value; }
        public int ConnectionTimeout => Connection.ConnectionTimeout;
        public string Database => Connection.Database;
        public ConnectionState State => Connection.State;

        internal DPConnection(DbConnection connection) : this(connection, true) { }
        internal DPConnection(DbConnection connection, bool dispose = true)
        {
            this.Connection = connection;
            this.dispose = dispose;
        }
        internal DPConnection(DPConnection c) : this(c.Connection, false)
        {
            transaction = c.transaction;
            parentConnection = c;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>A <see cref="DPTransaction"/> wrapping a <see cref="DbTransaction"/>.</returns>
        public DPTransaction BeginTransaction(ref SqliteConnectionOpts opts)
        {
            if (transaction is not null) 
                return opts.Transaction = new DPTransaction(transaction);
            if (parentConnection is null || parentConnection.transaction is null) 
                transaction = new DPTransaction(Connection.BeginTransaction(), true);
            else transaction = new DPTransaction(parentConnection.transaction);
            return transaction = opts.Transaction = transaction;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>A <see cref="DPTransaction"/> wrapping a <see cref="DbTransaction"/>.</returns>
        public DPTransaction BeginTransaction(IsolationLevel il, ref SqliteConnectionOpts opts)
        {
            if (transaction is not null) return transaction;
            if (parentConnection is null || parentConnection.transaction is null)
                transaction = new DPTransaction(Connection.BeginTransaction(il), true);
            else transaction = new DPTransaction(parentConnection.transaction);
            return transaction;
        }

        /// <summary>
        /// This is undefined behavior. Use the concrete type instead.
        /// </summary>
        [Obsolete("Do not use")]
        public void ChangeDatabase(string databaseName) => Connection.ChangeDatabase(databaseName);

        public void Close() => Connection.Close();

        // DbCommand.Transaction is automatically set under the hood.
        public DbCommand CreateCommand() => Connection.CreateCommand();
        /// <summary>
        /// Creates and returns a <see cref="IDbCommand"/> with the given command text and associates it with this connection.
        /// </summary>
        /// <param name="cmd">The command text to set for this command.</param>
        /// <returns>A <see cref="DbCommand"/> object associated with this connection 
        /// with command text equal to <paramref name="cmd"/>.
        /// </returns>
        public DbCommand CreateCommand(string cmd)
        {
            var command = Connection.CreateCommand();
            command.CommandText = cmd;
            return command;
        }

        /// <inheritdoc cref="SqliteConnection.BackupDatabase(SqliteConnection, string, string)"/>
        /// <exception cref="InvalidCastException">Occurs when the Connection is not an SqliteConnection (ie testing).</exception>
        public void BackupDatabase(SqliteConnection destination, string destinationName, string sourceName)
        {
            ((SqliteConnection)Connection).BackupDatabase(destination, destinationName, sourceName);
        }
        public void Dispose()
        {
            if (Disposed) return;
            GC.SuppressFinalize(this);
            if (dispose) Connection.Dispose();
            Disposed = true;
        }

        ~DPConnection()
        {
            Dispose();
        }
        public void Open() => Connection.Open();
        IDbTransaction IDbConnection.BeginTransaction()
        {
            var t = new SqliteConnectionOpts();
            return BeginTransaction(ref t);
        }
        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
        {
            var t = new SqliteConnectionOpts();
            return BeginTransaction(il, ref t);
        }
        IDbCommand IDbConnection.CreateCommand() => CreateCommand();
    }
}
