using System.Configuration;
using System.Data;
using System.Data.Common;

namespace DAZ_Installer.Database
{
    public struct SqliteConnectionOpts
    {
        public DPConnection? Connection
        {
            get => connection;
            set
            {
                if (connection is null) connection = value;
                else connection = new DPConnection(connection);
            }
        }
        public DPTransaction? Transaction { get; set; } = null;
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public bool IsCancellationRequested => CancellationToken.IsCancellationRequested;
        private DPConnection? connection = null;


        public SqliteConnectionOpts() { }
        public SqliteConnectionOpts(DPConnection? connection) => this.connection = connection;

        public SqliteConnectionOpts(DPConnection? connection, DPTransaction? transaction = null)
        {
            this.connection = connection;
            Transaction = transaction;
        }

        public SqliteConnectionOpts(DPConnection? connection, DPTransaction? transaction, CancellationToken t)
        {
            this.connection = connection;
            Transaction = transaction;
            CancellationToken = t;
        }

        /// <summary>
        /// Begins the transaction assuming <see cref="Connection"/> is not null. 
        /// </summary>
        /// <returns>The transaction already established on the <see cref="Connection"/> or an entirely new one.</returns>
        public DPTransaction BeginTransaction()
        {
            ArgumentNullException.ThrowIfNull(Connection, nameof(Connection));
            Transaction = Connection.BeginTransaction(ref this);
            return Transaction;
        }

        /// <summary>
        /// Begins the transaction assuming <see cref="Connection"/> is not null.
        /// </summary>
        /// <returns>An <see cref="IDbCommand"/> object associated with this connection.</returns
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="Exception"/>
        public IDbCommand CreateCommand()
        {
            ArgumentNullException.ThrowIfNull(Connection, nameof(Connection));
            return Connection.CreateCommand();
        }

        /// <summary>
        /// Begins the transaction with the command text assuming <see cref="Connection"/> is not null.
        /// </summary>
        /// <param name="cmd">The command text to set for this command.</param>
        /// <returns>An <see cref="IDbCommand"/> object associated with this connection.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="Exception"/>
        public DbCommand CreateCommand(string cmd)
        {
            ArgumentNullException.ThrowIfNull(Connection, nameof(Connection));
            return Connection.CreateCommand(cmd);
        }

    }
}
