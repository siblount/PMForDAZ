using System.Configuration;
using System.Data;
using System.Data.Common;

namespace DAZ_Installer.Database
{
    /// <summary>
    /// A struct that contains the connection object, transaction object, and cancellation token for a connection.
    /// </summary>
    public struct DPConnectionOpts
    {
        /// <summary>
        /// The connection object to use for this connection.
        /// <para/>
        /// WARNING: This object has side effects. If you attempt to set this property when it is null, it will be set to the <see langword="value"/> provided. <para/>
        /// If you set this property when it is not null, <see cref="Connection"/> will be a new <see cref="DPConnection"/> wrapping over the current <see cref="Connection"/>.
        /// </summary>
        public DPConnection? Connection
        {
            readonly get => connection;
            set
            {
                if (connection is null) connection = value;
                else connection = new DPConnection(connection);
            }
        }
        /// <summary>
        /// The transaction object to use for this connection. By default, this is null.
        /// </summary>
        public DPTransaction? Transaction { get; set; } = null;
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public readonly bool IsCancellationRequested => CancellationToken.IsCancellationRequested;
        private DPConnection? connection = null;

        /// <summary>
        /// A new instance of <see cref="DPConnectionOpts"/> with <see cref="Connection"/> set to null.
        /// </summary>
        public DPConnectionOpts() { }
        /// <summary>
        /// A new instance of <see cref="DPConnectionOpts"/> with <see cref="Connection"/> set to the <see langword="value"/> provided.
        /// </summary>
        /// <param name="connection">The connection to use, if any.</param>
        public DPConnectionOpts(DPConnection? connection) => this.connection = connection;
        /// <summary>
        /// A new instance of <see cref="DPConnectionOpts"/> with <see cref="Connection"/> set to the <see langword="value"/> provided and <see cref="Transaction"/> set to the <see langword="value"/> provided.
        /// </summary>
        /// <param name="connection">The connection to use, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>

        public DPConnectionOpts(DPConnection? connection, DPTransaction? transaction = null)
        {
            this.connection = connection;
            Transaction = transaction;
        }
        /// <summary>
        /// A new instance of <see cref="DPConnectionOpts"/> with <see cref="Connection"/> set to the <see langword="value"/> provided and <see cref="Transaction"/> set to the <see langword="value"/> provided.
        /// </summary>
        /// <param name="connection">The connection to use, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="t">The cancellation to use.</param>
        public DPConnectionOpts(DPConnection? connection, DPTransaction? transaction, CancellationToken t)
        {
            this.connection = connection;
            Transaction = transaction;
            CancellationToken = t;
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
