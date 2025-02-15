using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Serilog;

namespace DAZ_Installer.Database {
    /// <summary>
    /// A class that manages the connection to the database.
    /// </summary>
    /// <param name="database">The database to manage</param>
    /// <param name="initFunc">The database's initialize function in the event the database is not in an initialized state.</param>
    /// <remarks>
    /// We did not want to directly expose the <see cref="DPDatabase.Initialize"/> method, so we use a function to initialize the database.
    /// </remarks>
    public class DPConnectionManager(IDPDatabase database, Func<bool> initFunc) : IDPConnectionManager
    {
        public ILogger Logger { get; set; } = Log.ForContext<DPConnectionManager>();
        public IDPDatabase Database { get; init; } = database;
        public Func<bool> InitializeFunction { get; init; } = initFunc;

        /// <inheritdoc/>
        public DPConnection? CreateAndOpenConnection(ref DPConnectionOpts opts, bool readOnly = false)
        {
            CreateConnection(ref opts, readOnly);
            var success = OpenConnection(opts.Connection);
            return success ? opts.Connection : null;
        }

        private void CreateConnection(ref DPConnectionOpts opts, bool readOnly = false)
        {
            // If opts.Connection is not null, that connection will still work
            // since it was fine before. Commands will stop working if the database is locked.
            if (Database.DatabaseNotReady) return;
            if (opts.Connection is not null)
            {
                // SqliteConnectionOpts side effect will wrap the current connection with a new DPConnection
                // so that on Dispose, it will not dispose the underlying connection.
                opts.Connection = null;
                return;
            }
            if (!Database.Initialized && InitializeFunction()) return;
            try
            {
                SqliteConnection connection = new();
                SqliteConnectionStringBuilder builder = new();
                builder.DataSource = Path.GetFullPath(database.Path);
                builder.Pooling = true;
                builder.Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite;
                connection.ConnectionString = builder.ConnectionString;

                // SqliteConnectionOpts has a side effect with the Connection property.
                // It will set the connection as expected ONLY when connection is null.
                // Otherwise, any set operation will be ignore the value and set it to new DPConnection(connection).
                // Hence, why we use null.
                opts.Connection = new DPConnection(connection, true);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to create connection");
            }
        }

        /// <inheritdoc/>
        public DPConnection? CreateInitialConnection(ref DPConnectionOpts opts)
        {
            try
            {
                SqliteConnection connection = new();
                SqliteConnectionStringBuilder builder = new();
                builder.DataSource = Path.GetFullPath(Database.Path);
                builder.Pooling = true;
                connection.ConnectionString = builder.ConnectionString;
                
                if (opts.Connection is null)
                    opts.Connection = new DPConnection(connection, true);
                else 
                    opts.Connection = null;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to create initial connection");
            }
            return opts.Connection;
        }

        /// <inheritdoc/>
        public bool OpenConnection([NotNullWhen(true)] IDbConnection? connection)
        {
            if (connection == null) return false;
            if (connection.State != ConnectionState.Closed) return true;
            try
            {
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to open connection");
            }
            return false;
        }
    }
}