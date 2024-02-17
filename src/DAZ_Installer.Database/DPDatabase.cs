// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using Serilog;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using DAZ_Installer.IO;

namespace DAZ_Installer.Database
{
    /// <summary>
    /// This class will handle all database operations such as initializing the database, creating tables, rows, deleting, etc.
    /// Database will be run on a different thread aside from the main thread.
    /// </summary>
    public partial class DPDatabase
    // SELECT * FROM ProductRecords WHERE ID IN(SELECT "Product Record ID" FROM TAGS WHERE Tag IN ("Run"))
    // Internal methods with suffix 'Q' are methods that can be queued to the TaskScheduler. 
    // Some can be executed immediately, such as RefreshDatabase.

    // Private methods with suffix 'S' are methods that are used for priority search calls.
    // Any method without this suffix will stop at the beginning of the method call and wait until the search has 
    // been completed before completing task.

    // All applicable methods that has the CancellationToken as the last parameter will stop before, during an expensive operation.
    // When this occurs, these methods should return false, empty array of type, empty string, -1.

    // DO NOT THROW ERRORS! IT IS SIGNIFICANTLY SLOW!
    // RETURN A BOOL DETERMINING IF IT WAS SUCCESSFUL OR NOT. AND IF YOU NEED TO RETURN A VALUE USE OUT PARAM OR REF PARAM!

    // TODO: Backup Database.
    // TODO: Tool to use backup database.
    {   // TODO : Hold last transcations.
        // Public
        public ILogger Logger { get; set; } = Log.Logger.ForContext<DPDatabase>();
        public DPArchiveFlags Flags { get; private set; } = DPArchiveFlags.None;
        public bool Initialized => Flags.HasFlag(DPArchiveFlags.Initialized);
        public bool Locked => Flags.HasFlag(DPArchiveFlags.Locked);
        public bool UpdateRequired => Flags.HasFlag(DPArchiveFlags.UpdateRequired);
        public bool Corrupted => Flags.HasFlag(DPArchiveFlags.Corrupted);
        public bool Missing => Flags.HasFlag(DPArchiveFlags.Missing);
        public bool DatabaseNotReady => (Flags ^ DPArchiveFlags.Initialized) != DPArchiveFlags.None;
        public string[]? TableNames { get; set; } 
        public ulong ProductRecordCount { get; private set; } = 0;

        public const string ProductTable = "Products";
        public const string DestinationTable = "Destinations";
        public const string FilesTable = "Files";
        public const string ProductFTS5Table = "ProductFTS5";
        public const string DatabaseInfoTable = "DatabaseInfo";
        public const string ProductFullView = "ProductFull";
        public const string ProductLiteView = "ProductLite";
        public const string ProductLiteAlphabeticalView = "ProductLite_Alphabetical";
        public const string ProductLiteDateView = "ProductLite_Date";
        public const string ArchivesView = "Archives";
        public const string IDX_Name_Products = "idx_Name_Products";
        public const string IDX_Date_Products = "idx_Date_Products";
        public const string IDX_Arc_Products = "idx_Arc_Products";
        public const string DeleteOnProductTrigger = "delete_on_removal";
        public const string UpdateProductCountTrigger = "update_count";
        public const string AddToFTSTrigger = "add_to_fts5";

        public readonly ImmutableHashSet<string> DatabaseObjects = new HashSet<string>() { 
            ProductTable, DestinationTable, FilesTable, ProductFTS5Table, DatabaseInfoTable, ProductFullView,
            ProductLiteView, ProductLiteAlphabeticalView, ProductLiteDateView, ArchivesView, IDX_Name_Products, IDX_Date_Products, 
            IDX_Arc_Products, DeleteOnProductTrigger, UpdateProductCountTrigger, AddToFTSTrigger }.ToImmutableHashSet();

        // Events
        /// <summary>
        /// This event is invoked whenever a Search function has completed searching whether any results were found or not.
        /// </summary>
        public event Action<List<DPProductRecordLite>, long>? SearchUpdated;
        /// <summary>
        /// This event is invoked whenever the database schema, database connection status, or other database configurations have been changed.
        /// </summary>
        public event Action? DatabaseUpdated;
        /// <summary>
        /// This event is invoked whenever a table has been updated; updated being having rows, columns removed, modified, or added.
        /// </summary>
        public event Action<string>? TableUpdated;
        /// <summary>
        /// This event is invoked whenever a request to view the table of the database has been called and successfully finished the request.
        /// </summary>
        public event Action<DataSet, uint>? ViewUpdated;
        /// <summary>
        /// This event is currently not being used.
        /// </summary>
        public event Action<List<DPProductRecordLite>, long>? LibraryQueryCompleted;
        /// <summary>
        /// This event is invoked whenever requesting for an extraction record has been successfully completed.
        /// </summary>
        public event Action<DPProductRecordLite, long>? RecordQueryCompleted;
        /// <summary>
        /// This event is invoked whenever a library query has been completed regardless if it yields any product records or not.
        /// </summary>
        public event Action<long>? MainQueryCompleted;

        // Product Record events
        /// <summary>
        /// This event is invoked whenever a product record has been removed (aside from when the table has been cleared).
        /// </summary>
        public event Action<long>? ProductRecordRemoved;
        /// <summary>
        /// This event is invoked whenever a product record has been modified.
        /// </summary>
        public event Action<DPProductRecord, long>? ProductRecordModified;
        /// <summary>
        /// This event is invoked whenever a new product record has been added.
        /// </summary>
        public event Action<DPProductRecord>? ProductRecordAdded;
        /// <summary>
        /// This event is invoked whenever all of the records have been removed from the database.
        /// </summary>
        public event Action? RecordsCleared;
        /// <summary>
        /// The path of the database to use. Default is: <c>%TEMP%\db.db</c>.
        /// </summary>
        public string Path { get; protected set; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "db.db");

        //private string _expectedDatabasePath => Path.Join(DPSettings.databasePath, "db.db");

        // Main task manager...

        private DPTaskManager _mainTaskManager = new();

        private DPTaskManager _priorityTaskManager = new();

        // Task state.
        private const byte DATABASE_VERSION = 3;
        private bool _initializing = false;
        ~DPDatabase()
        {
            StopAllDatabaseOperations(true);
        }
        public DPDatabase(string path)
        {
            if (!path.EndsWith(".db")) throw new ArgumentException("Database path must end with .db");
            Path = path;
            Initialize();
        }

        #region Private methods
        /// <summary>
        /// Initalize creates a new database if one is not found at the expected database path.
        /// An issue may occur where multiple threads may attempt to initalize the database. If 
        /// there are any errors that occur, Initalize() will return false indicating it failed
        /// to initalize. Otherwise, it will return true indicating it initalized successfully.
        /// </summary>
        /// <returns>True if initalization was successful, otherwise false.</returns>
        private bool Initialize()
        {
            // If another thread is initalizing, wait for it to initalize or wait 10 secs max.
            try
            {
                if (_initializing)
                    SpinWait.SpinUntil(() => _initializing = false, 10000);
                // Timeout throws an error, if we timed out intialized failed.
            }
            catch { return false; }

            if (Flags.HasFlag(DPArchiveFlags.Initialized)) return true;
            _initializing = true;
            Flags = DPArchiveFlags.None;
            try
            {
                // Check if database exists.
                if (File.Exists(Path)) Flags &= ~DPArchiveFlags.Missing;
                else Flags |= DPArchiveFlags.Missing;
                // TODO: Check if const database version is higher than the one in the database.
                var opts = new SqliteConnectionOpts();
                if (Flags.HasFlag(DPArchiveFlags.Missing))
                {
                    // Create the database.
                    CreateDatabase(opts);
                    // Update database info.
                    using var connection = CreateInitialConnection(ref opts);
                    if (connection == null) return false;
                    InsertDefaultValuesToTable(DatabaseInfoTable, opts);
                }
                // Set the corrupted flag if applicable.
                CheckCorrupted(opts);
                // Set the update required flag if applicable.
                CheckUpdateRequired(opts);
                DatabaseUpdated?.Invoke();
                Flags |= DPArchiveFlags.Initialized;
                TableNames = GetTables(opts);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while initializing");
                _initializing = false;
                return false;
            }
            _initializing = false;
            return true;
        }
        /// <summary>
        /// Creates and returns a connection with the connection string setup.
        /// </summary>
        /// <param name="readOnly">Determines if the connection should be a read-only
        /// connection or not.</param>
        /// <returns>An SqliteConnection if successfully created otherwise null.</returns>
        private DPConnection? CreateConnection(ref SqliteConnectionOpts opts, bool readOnly = false)
        {
            // If opts.Connection is not null, that connection will still work
            // since it was fine before. Commands will stop working if the database is locked.
            if (DatabaseNotReady || opts.Connection is not null) return null;
            if (!Initialized)
            {
                var success = Initialize();
                if (!success) return null;
            }
            try
            {
                SqliteConnection connection = new();
                SqliteConnectionStringBuilder builder = new();
                builder.DataSource = System.IO.Path.GetFullPath(Path);
                builder.Pooling = true;
                builder.Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite;
                connection.ConnectionString = builder.ConnectionString;
                return new DPConnection(connection, true);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to create connection");
            }
            return null;
        }
        /// <summary>
        /// Creates and returns a connection with the connection string setup. Should only be used for the Initialization function.
        /// </summary>
        /// <returns>An SqliteConnection if successfully created, otherwise null.</returns>
        private DPConnection? CreateInitialConnection(ref SqliteConnectionOpts opts)
        {
            try
            {
                SqliteConnection connection = new();
                SqliteConnectionStringBuilder builder = new();
                builder.DataSource = System.IO.Path.GetFullPath(Path);
                builder.Pooling = true;
                connection.ConnectionString = builder.ConnectionString;
                opts.Connection = new DPConnection(connection, true);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to create initial connection");
            }
            return opts.Connection;
        }

        /// <summary>
        /// Creates, opens, and returns a SQLite Connection. If connection is null, a
        /// connection will be created for you. If the connection fails to open or be
        /// created, it will return null.
        /// </summary>
        /// <param name="connection">An existing connection to open.</param>
        /// <param name="readOnly">Determine if the new connection should be read only.</param>
        /// <returns>The connection passed if it isn't null and was successfully opened. 
        /// Otherwise, a new connection is passed if it was successfully opened. Otherwise,
        /// null is returned.</returns>
        private DPConnection? CreateAndOpenConnection(ref SqliteConnectionOpts opts, bool readOnly = false)
        {
            opts.Connection = CreateConnection(ref opts, readOnly);
            var success = OpenConnection(opts.Connection);
            return success ? opts.Connection : null;
        }

        /// <summary>
        /// Attempts to open the connection and returns whether it was successful or not.
        /// Any errors including if connection is null will return false.
        /// </summary>
        /// <param name="connection">The connection to open.</param>
        /// <returns>True if the connection opened successfully, otherwise false.</returns>
        private bool OpenConnection([NotNullWhen(true)] IDbConnection? connection)
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

        /// <summary>
        /// Creates a new database file and sets it up for use.
        /// </summary>
        private void CreateDatabase(SqliteConnectionOpts opts)
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);

            File.Create(Path).Close();
            Flags &= ~DPArchiveFlags.Missing;

            using var connection = CreateInitialConnection(ref opts);

            if (!OpenConnection(connection)) return;
            using var transaction = connection.BeginTransaction(ref opts);

            // Create tables, views, indexes, and triggers.
            if (!CreateTables(opts) || !CreateIndexes(opts) || !CreateTriggers(opts) || !CreateViews(opts) || !ExecutePragmas(opts))
                throw new Exception("Failed to create database");
            
            transaction.Commit();
        }

        /// <summary>
        /// Adds the tables required for application to properly execute into the database.
        /// Does not check if they exist. May throw an error if the tables already exist.
        /// </summary>
        /// <param name="c">The SqliteConnection to use, if any. Recommended to use a connection, otherwise use <c>CreateTables()</c> instead.</param>
        /// <param name="t">The cancellation token to use, if any. Use <see cref="CancellationToken.None"/> if it should never cancel.</param>
        /// <returns>Whether creating tables was a success.</returns>
        private bool CreateTables(SqliteConnectionOpts opts)
        {
            var cmd = $@"
            CREATE TABLE IF NOT EXISTS {ProductTable} (
                Name TEXT NOT NULL,
                Authors TEXT,
                Date INTEGER NOT NULL,
                Thumbnail TEXT,
                ArcName TEXT NOT NULL,
                DestID INTEGER,
                Tags TEXT
            ); 

            CREATE VIRTUAL TABLE IF NOT EXISTS {ProductFTS5Table}
                    USING fts5(ID UNINDEXED, Name, Tags);

            CREATE TABLE IF NOT EXISTS {FilesTable} (
                PID INTEGER,
                File TEXT,
                PRIMARY KEY(PID)
            ) WITHOUT ROWID;

            CREATE TABLE IF NOT EXISTS {DestinationTable} (
                ID INTEGER NOT NULL,
                Destination TEXT UNIQUE NOT NULL,
                PRIMARY KEY(ID AUTOINCREMENT)
            );

            CREATE TABLE IF NOT EXISTS {DatabaseInfoTable} (
                Version   INTEGER NOT NULL DEFAULT {DATABASE_VERSION},
	            ""Product Record Count""  INTEGER NOT NULL DEFAULT 0
            ); ";
            try
            {
                using var connection = CreateInitialConnection(ref opts);
                if (!OpenConnection(connection) || opts.IsCancellationRequested) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                using var createCommand = opts.CreateCommand(cmd);
                createCommand.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create tables");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Adds indexes to the database to improve searching and sorting performance.
        /// Does not check if they exist.
        /// </summary>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">The cancellation token to use, if any. Use <see cref="CancellationToken.None"/> if it should never cancel.</param>
        /// <returns>Whether creating indexes was a success.</returns>
        private bool CreateIndexes(SqliteConnectionOpts opts)
        {
            const string createProductNameToPIDCommand = @$"
            CREATE INDEX {IDX_Name_Products} ON {ProductTable} (
                ""Name"" COLLATE NOCASE ASC
            );

            CREATE INDEX {IDX_Date_Products} ON {ProductTable} (
	            ""Date""	ASC
            );

            CREATE INDEX {IDX_Arc_Products} ON {ProductTable} (
	            ""ArcName""
            );
";
            try
            {
                using var connection = CreateInitialConnection(ref opts);
                if (!OpenConnection(connection) || opts.IsCancellationRequested) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                using var command = opts.CreateCommand(createProductNameToPIDCommand);
                command.ExecuteNonQuery();
                transaction.Commit();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create indexes");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Adds the triggers required for application to properly execute into the database.
        /// </summary>
        /// <param name="c">The SqliteConnection to use, if any. Recommended to use a connection, otherwise use <c>CreateTables()</c> instead.</param>
        /// <param name="t">The cancellation token to use, if any. Use <see cref="CancellationToken.None"/> if it should never cancel.</param>
        /// <returns>Whether creating triggers was a success.</returns>
        private bool CreateTriggers(SqliteConnectionOpts opts)
        {
            const string triggerSQL = @$"
                        CREATE TRIGGER IF NOT EXISTS {DeleteOnProductTrigger}
                            AFTER DELETE ON {ProductTable} FOR EACH ROW
                        BEGIN
                            UPDATE {DatabaseInfoTable} SET ""Product Record Count"" = (SELECT ""Product Record Count"" FROM {DatabaseInfoTable}) - 1;
	                        DELETE FROM {ProductFTS5Table} WHERE ID = old.ROWID;
                        END;

                        CREATE TRIGGER IF NOT EXISTS {UpdateProductCountTrigger}
	                        AFTER INSERT ON {ProductTable} FOR EACH ROW
                        BEGIN
	                        UPDATE {DatabaseInfoTable} SET ""Product Record Count"" = (SELECT ""Product Record Count"" FROM {DatabaseInfoTable}) + 1;
                        END;
                        CREATE TRIGGER IF NOT EXISTS {AddToFTSTrigger}
	                        AFTER INSERT ON {ProductTable}
	                    BEGIN
		                    INSERT INTO {ProductFTS5Table} (ID, Name, Tags) VALUES (new.ROWID, new.Name, new.Tags);
	                    END;
";
            try
            {
                using var connection = CreateInitialConnection(ref opts);
                if (!OpenConnection(connection) || opts.IsCancellationRequested) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                using var createCommand = opts.CreateCommand(triggerSQL);
                createCommand.ExecuteNonQuery();
                transaction.Commit();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create triggers");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Adds the triggers required for application to properly execute into the database.
        /// </summary>
        /// <param name="c">The SqliteConnection to use, if any. Recommended to use a connection, otherwise use <c>CreateTables()</c> instead.</param>
        /// <param name="t">The cancellation token to use, if any. Use <see cref="CancellationToken.None"/> if it should never cancel.</param>
        /// <returns>Whether creating triggers was a success.</returns>
        private bool CreateViews(SqliteConnectionOpts opts)
        {
            const string viewSQL = @$"
                        CREATE VIEW {ProductLiteView} (Name, Thumbnail, Tags, PID) 
                            AS SELECT Name, Thumbnail, Tags, ROWID FROM {ProductTable};
                        CREATE VIEW {ProductLiteAlphabeticalView} (Name, Thumbnail, Tags, PID) 
                            AS SELECT Name, Thumbnail, Tags, ROWID FROM {ProductTable} ORDER BY Name COLLATE NOCASE;
                        CREATE VIEW {ProductLiteDateView} (Name, Thumbnail, Tags, PID) 
                            AS SELECT Name, Thumbnail, Tags, ROWID FROM {ProductTable} ORDER BY Date;
                        CREATE VIEW {ArchivesView} AS SELECT ArcName FROM {ProductTable};
                        CREATE VIEW {ProductFullView} (PID, Name, Authors, Date, Thumbnail, ArcName, Tags, Files, Destination) AS 
                        SELECT P.ROWID, P.Name, P.Authors, P.Date, P.Thumbnail, P.ArcName, P.Tags, 
                               F.File AS Files, D.Destination
                        FROM {ProductTable} P
                        LEFT JOIN {FilesTable} F ON P.ROWID = F.PID
                        LEFT JOIN {DestinationTable} D ON P.DestID = D.ID;";

            try
            {
                using var connection = CreateInitialConnection(ref opts);
                if (!OpenConnection(connection) || opts.IsCancellationRequested) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                using var createCommand = opts.CreateCommand(viewSQL);
                createCommand.ExecuteNonQuery();
                transaction.Commit();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create views");
                return false;
            }
            return true;

        }
        /// <summary>
        /// Changes a few settings for how the database should act. This is required
        /// to make sure that the database allows multiple connections and use 
        /// less journal sizes.
        /// </summary>
        /// <returns>Whether the execution was a success.</returns>
        private bool ExecutePragmas(SqliteConnectionOpts opts)
        {

            var pramaCommmands = @$"PRAGMA journal_mode = WAL;
                                    PRAGMA wal_autocheckpoint=2; 
                                    PRAGMA journal_size_limit=32768;
                                    PRAGMA page_size=512;
                                    PRAGMA user_version={DATABASE_VERSION}";
            try
            {
                using var connection = CreateInitialConnection(ref opts);
                if (!OpenConnection(connection)) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                using var createCommand = opts.CreateCommand(pramaCommmands);
                createCommand.ExecuteNonQuery();
                transaction.Commit();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to execute pragmas");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Temporarily deletes triggers from the database. 
        /// Use this for all other code excluding initialization. <para/>
        /// </summary>
        /// <param name="c">The SqliteConnection to use, if any. Recommended to use a connection, otherwise use <c>DeleteTriggers()</c> instead.</param>
        /// <param name="token">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether deleting triggers was a success.</returns>
        private bool TempDeleteTriggers(SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return false;
            const string removeTriggersCommand = @$"DROP TRIGGER IF EXISTS {DeleteOnProductTrigger};
                                                   DROP TRIGGER IF EXISTS {UpdateProductCountTrigger};
                                                   DROP TRIGGER IF EXISTS {AddToFTSTrigger};";
            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection is null) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                using var deleteCommand = opts.CreateCommand(removeTriggersCommand);
                if (opts.IsCancellationRequested) return false;
                deleteCommand.ExecuteNonQuery();
                transaction.Commit();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to temp delete triggers");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Resets the database by drops every table, view, index, and trigger. Then recreates them.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool ResetDatabase(SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return false;
            try
            {
                using var connection = CreateInitialConnection(ref opts);
                if (!OpenConnection(connection) || opts.IsCancellationRequested) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                if (!TempDeleteTriggers(opts)) return false;
                var txt = $@"DROP TABLE IF EXISTS {ProductTable};
                            DROP TABLE IF EXISTS {DestinationTable};
                            DROP TABLE IF EXISTS {FilesTable};
                            DROP TABLE IF EXISTS {ProductFTS5Table};
                            DROP TABLE IF EXISTS {DatabaseInfoTable};
                            DROP VIEW IF EXISTS {ProductFullView};
                            DROP VIEW IF EXISTS {ProductLiteView};
                            DROP VIEW IF EXISTS {ProductLiteAlphabeticalView};
                            DROP VIEW IF EXISTS {ProductLiteDateView};
                            DROP VIEW IF EXISTS {ArchivesView};
                ";
                using var command = opts.CreateCommand(txt);
                command.ExecuteNonQuery();

                if (!CreateTables(opts) || !CreateIndexes(opts) || 
                    !CreateTriggers(opts) || !CreateViews(opts)) return false;

                transaction.Commit();
                RecordsCleared?.Invoke();
                TableUpdated?.Invoke(ProductTable);
                TableUpdated?.Invoke(FilesTable);
                TableUpdated?.Invoke(DestinationTable);
                TableUpdated?.Invoke(ProductFTS5Table);
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to reset database");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sets the <see cref="Flags"/> to <see cref="DPArchiveFlags.None"/> and calls <see cref="Initialize"/>.
        /// </summary>
        /// <param name="opts"></param>
        private void RefreshDatabase(SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return;
            try
            {
                Flags = DPArchiveFlags.None;
                Initialize();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to refresh database");
            }
        }

        /// <summary>
        /// Checks if the database is corrupted. Sets the flags if it is.
        /// </summary>
        /// <param name="opts"></param>
        /// <returns>Whether the operation was successful or not.</returns>
        private bool CheckCorrupted(SqliteConnectionOpts opts)
        {
            using var c = CreateInitialConnection(ref opts);
            if (c is null || !OpenConnection(c) || opts.IsCancellationRequested) return false;
            try
            {
                using var cmd = opts.CreateCommand($"PRAGMA quick_check");
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var result = reader.GetString(0);
                    if (result != "ok")
                    {
                        Logger.Error("Database is corrupted: {0}", result);
                        Flags |= DPArchiveFlags.Corrupted;
                        return true;
                    }
                }
                var tables = GetTables(opts).ToHashSet();
                tables.ExceptWith(DatabaseObjects);
                tables.RemoveWhere(x => x.StartsWith("sqlite_"));
                tables.RemoveWhere(x => x.StartsWith(ProductFTS5Table + '_'));

                if (tables.Count > 0)
                {
                    Logger.Error("Database is corrupted: Missing tables");
                    Flags |= DPArchiveFlags.Corrupted;
                    return true;
                }
                Flags &= ~DPArchiveFlags.Corrupted;
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to check if database is corrupted");
                return false;
            }
        }

        /// <summary>
        /// Checks if the database is outdated. Sets the flags if it is.
        /// </summary>
        /// <param name="opts"></param>
        /// <returns>Whether the command executed without error or not.</returns>
        private bool CheckUpdateRequired(SqliteConnectionOpts opts)
        {
            using var c = CreateInitialConnection(ref opts);
            if (c is null || !OpenConnection(c) || opts.IsCancellationRequested) return false;
            try
            {
                using var cmd = opts.CreateCommand($"PRAGMA user_version;");
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var result = reader.GetInt32(0);
                    if (result == DATABASE_VERSION) return true;
                    Logger.Error("Database is outdated: {0}", result);
                    Flags |= DPArchiveFlags.UpdateRequired;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to check if database is outdated");
            }
            return false;
        }

        private bool BackupDatabase(SqliteConnectionOpts opts)
        {
            using var c = CreateAndOpenConnection(ref opts, true);
            using var d = new SqliteConnection();

            
            SqliteConnectionStringBuilder builder = new();

            var newFileName = System.IO.Path.GetFileNameWithoutExtension(Path) + "_backup.db";
            builder.DataSource = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, newFileName));
            d.ConnectionString = builder.ConnectionString;
            if (File.Exists(d.DataSource)) File.Delete(d.DataSource);
            if (c is null || !OpenConnection(d) || opts.IsCancellationRequested) return false;
            try
            {
                c.BackupDatabase(d, "main", "main");
            }
            catch (Exception ex) { 
                Logger.Error(ex, "Failed to backup database");
                return false;
            }
            return true;
        }

        private bool RestoreDatabase(string? location, SqliteConnectionOpts opts)
        {
            location ??= System.IO.Path.GetFileNameWithoutExtension(Path) + "_backup.db";
            if (!File.Exists(location)) return false;
;
            try
            {
                DPFileSystem system = new(new DPFileScopeSettings(new[] { location, Path }, Array.Empty<string>(), false, true));

                //using var connection = CreateInitialConnection(ref opts);
                //if (!OpenConnection(connection) || opts.IsCancellationRequested) return false;
                //connection.Dispose();

                //SqliteConnection.ClearPool((SqliteConnection) connection.Connection);
                SqliteConnection.ClearAllPools();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var file = system.CreateFileInfo(location);
                if (!file.TryAndFixMoveTo(Path, true, out var ex))
                {
                    Logger.Error(ex, "Failed to restore database");
                    return false;
                }
                RefreshDatabase(opts);
                return true;
            } catch (Exception ex)
            {
                Logger.Error(ex, "Failed to restore database");
            } finally
            {
                Flags &= ~DPArchiveFlags.Locked;
            }
            return false;
        }

        private bool VacuumDatabase(SqliteConnectionOpts opts)
        {
            using var c = CreateAndOpenConnection(ref opts, true);
            if (!OpenConnection(c) || opts.IsCancellationRequested) return false;
            try
            {
                using var cmd = opts.CreateCommand("pragma VACUUM");
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to vacuum database");
            }
            return false;
        }

        // Prep for app closure.
        private void OnAppClose(object e)
        {
            _mainTaskManager.Stop();
            _priorityTaskManager.Stop();
            TruncateJournal();
        }

        #endregion
    }
}
