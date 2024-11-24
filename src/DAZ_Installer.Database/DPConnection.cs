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
        /// <summary>
        /// The underlying <see cref="DbConnection"/> object.
        /// </summary>
        public readonly DbConnection Connection;

        /// <summary>
        /// The parent connection, if any. 
        /// </summary>
        protected readonly DPConnection? parentConnection;

        /// <summary>
        /// The transaction object, if any.
        /// </summary>
        protected DPTransaction? transaction;

        /// <summary>
        /// Determines whether this connection should be disposed or not.
        /// </summary>
        private bool dispose;

        /// <summary>
        /// Determines whether this connection has been disposed or not.
        /// </summary>
        private bool Disposed = false;

        /// <summary>
        /// The connection string for this connection.
        /// </summary>
        public string ConnectionString { get => Connection.ConnectionString; set => Connection.ConnectionString = value; }

        /// <summary>
        /// The connection timeout for this connection.
        /// </summary>
        public int ConnectionTimeout => Connection.ConnectionTimeout;

        /// <summary>
        /// The database for this connection.
        /// </summary>
        public string Database => Connection.Database;

        /// <summary>
        /// The state for this connection.
        /// </summary>
        public ConnectionState State => Connection.State;

        /// <summary>
        /// A <see cref="DPConnection"/> object that wraps a <see cref="DbConnection"/> object that will dispose this object when it is disposed.
        /// </summary>
        /// <param name="connection">The connection to use</param>

        internal DPConnection(DbConnection connection) : this(connection, true) { }

        /// <summary>
        /// A constructor that wraps a <see cref="DbConnection"/> object and allows you to determine whether this object should be disposed or not.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="dispose">Determines whether this object should be disposed when <see cref="Dispose"/> is called.</param>
        internal DPConnection(DbConnection connection, bool dispose = true)
        {
            this.Connection = connection;
            this.dispose = dispose;
        }

        /// <summary>
        /// A special constructor that makes a new nested connection, making <paramref name="c"/> the parent connection.
        /// This object will not be disposed when <see cref="Dispose"/> is called.
        /// </summary>
        /// <param name="c"></param>
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
            return opts.Transaction = transaction;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>A <see cref="DPTransaction"/> wrapping a <see cref="DbTransaction"/>.</returns>
        public DPTransaction BeginTransaction(IsolationLevel il, ref SqliteConnectionOpts opts)
        {
            if (transaction is not null)
                return opts.Transaction = new DPTransaction(transaction);
            if (parentConnection is null || parentConnection.transaction is null)
                transaction = new DPTransaction(Connection.BeginTransaction(il), true);
            else transaction = new DPTransaction(parentConnection.transaction);
            return opts.Transaction = transaction;
        }

        /// <summary>
        /// This is undefined behavior. Use the concrete type instead.
        /// </summary>
        [Obsolete("Do not use")]
        public void ChangeDatabase(string databaseName) => Connection.ChangeDatabase(databaseName);

        /// <summary>
        /// Closes the connection. It is highly not recommended to use this method unless this is the parent connection.
        /// </summary>
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

        /// <summary>
        /// This method will dispose this object ONLY when it is the parent connection.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            GC.SuppressFinalize(this);
            if (dispose) Connection.Dispose();
            Disposed = true;
        }
        /// <summary>
        /// Calls Dispose.
        /// </summary>
        ~DPConnection()
        {
            Dispose();
        }

        /// <summary>
        /// Opens the connection. It is highly not recommended to use this method unless this is the parent connection.
        /// </summary>
        public void Open() => Connection.Open();
        /// <summary>
        /// Implements the required BeginTransaction, although it is not recommended to use this method.
        /// </summary>
        /// <returns>A transaction.</returns>
        IDbTransaction IDbConnection.BeginTransaction()
        {
            var t = new SqliteConnectionOpts();
            return BeginTransaction(ref t);
        }
        /// <summary>
        /// Implements the required BeginTransaction, although it is not recommended to use this method.
        /// </summary>
        /// <returns>A transaction.</returns>
        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
        {
            var t = new SqliteConnectionOpts();
            return BeginTransaction(il, ref t);
        }
        /// <summary>
        /// Implements the required CreateCommand, although it is not recommended to use this method.
        /// </summary>
        /// <returns>A command</returns>
        IDbCommand IDbConnection.CreateCommand() => CreateCommand();
    }
}
