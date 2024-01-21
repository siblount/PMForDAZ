// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using Serilog;
using System.Data;
using System.Data.SQLite;
using static System.Net.Mime.MediaTypeNames;

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
        public ILogger Logger { get; set; } = Log.Logger;
        public bool DatabaseExists { get; private set; } = false;
        public bool Initalized { get; private set; } = false;
        public bool Locked { get; private set; } = false;
        public string[] tableNames;
        public uint ProductRecordCount { get; private set; } = 0;
        public uint ExtractionRecordCount { get; private set; } = 0;

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
        public HashSet<string> ArchiveFileNames { get; private set; } = new HashSet<string>();

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
        public string Path { get; init; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "db.db");

        //private string _expectedDatabasePath => Path.Join(DPSettings.databasePath, "db.db");

        // Main task manager...

        private DPTaskManager _mainTaskManager = new();

        private DPTaskManager _priorityTaskManager = new();

        // Task state.
        private const byte DATABASE_VERSION = 3;
        private bool _initializing = false;
        ~DPDatabase()
        {
            StopAllDatabaseOperations();
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

            if (Initalized) return true;
            _initializing = true;
            try
            {
                // Check if database exists.
                DatabaseExists = File.Exists(Path);
                // TODO: Check if const database version is higher than the one in the database.
                if (!DatabaseExists)
                {
                    // Create the database.
                    CreateDatabase();
                    // Update database info.
                    using SQLiteConnection? connection = CreateInitialConnection();
                    if (connection == null) return false;
                    InsertDefaultValuesToTable(DatabaseInfoTable, connection, CancellationToken.None);
                }
                DatabaseUpdated?.Invoke();
                Initalized = true;
                tableNames = GetTables(null, CancellationToken.None);
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
        /// <returns>An SQLiteConnection if successfully created otherwise null.</returns>
        private SQLiteConnection? CreateConnection(bool readOnly = false)
        {
            if (!Initalized)
            {
                var success = Initialize();
                if (!success) return null;
            }
            try
            {
                SQLiteConnection connection = new();
                SQLiteConnectionStringBuilder builder = new();
                builder.DataSource = System.IO.Path.GetFullPath(Path);
                builder.Pooling = true;
                builder.ReadOnly = readOnly;
                connection.ConnectionString = builder.ConnectionString;
                return connection;
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
        /// <returns>An SQLiteConnection if successfully created, otherwise null.</returns>
        private SQLiteConnection? CreateInitialConnection()
        {
            try
            {
                SQLiteConnection connection = new();
                SQLiteConnectionStringBuilder builder = new();
                builder.DataSource = System.IO.Path.GetFullPath(Path);
                builder.Pooling = true;
                connection.ConnectionString = builder.ConnectionString;
                return connection;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to create initial connection");
            }
            return null;
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
        private SQLiteConnection? CreateAndOpenConnection(SQLiteConnection? connection, bool readOnly = false)
        {
            SQLiteConnection? c = connection ?? CreateConnection(readOnly);
            var success = OpenConnection(c);
            return success ? c : null;
        }

        /// <summary>
        /// Attempts to open the connection and returns whether it was successful or not.
        /// Any errors including if connection is null will return false.
        /// </summary>
        /// <param name="connection">The connection to open.</param>
        /// <returns>True if the connection opened successfully, otherwise false.</returns>
        private bool OpenConnection(SQLiteConnection? connection)
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
        private void CreateDatabase()
        {

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);

            SQLiteConnection.CreateFile(Path);
            using var connection = CreateInitialConnection();
            if (connection is null) return;
            // Create tables, views, indexes, and triggers.
            CreateTables(connection, CancellationToken.None);
            CreateIndexes(connection, CancellationToken.None);
            CreateTriggers(connection, CancellationToken.None);
            CreateViews(connection, CancellationToken.None);
            ExecutePragmas();
        }

        /// <summary>
        /// Adds the tables required for application to properly execute into the database.
        /// Does not check if they exist. May throw an error if the tables already exist.
        /// </summary>
        /// <param name="c">The SQLiteConnection to use, if any. Recommended to use a connection, otherwise use <c>CreateTables()</c> instead.</param>
        /// <param name="t">The cancellation token to use, if any. Use <see cref="CancellationToken.None"/> if it should never cancel.</param>
        /// <returns>Whether creating tables was a success.</returns>
        private bool CreateTables(SQLiteConnection? c, CancellationToken t)
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
	            ""Product Record Count""  INTEGER NOT NULL DEFAULT 0,
            ); ";
            SQLiteConnection? connection = c;
            try
            {
                connection ??= CreateInitialConnection();
                if (!OpenConnection(connection) || t.IsCancellationRequested) return false;
                SQLiteCommand createCommand = new(cmd, connection);
                createCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create tables");
                return false;
            } finally
            {
                if (c is null) connection?.Dispose();
            }
            return true;
        }
        /// <summary>
        /// Adds indexes to the database to improve searching and sorting performance.
        /// Does not check if they exist.
        /// </summary>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">The cancellation token to use, if any. Use <see cref="CancellationToken.None"/> if it should never cancel.</param>
        /// <returns>Whether creating indexes was a success.</returns>
        private bool CreateIndexes(SQLiteConnection? c, CancellationToken t)
        {
            const string createProductNameToPIDCommand = @$"
            CREATE INDEX ""idx_Name_Products"" ON {ProductTable} (
                ""Name"" ASC
            );

            CREATE INDEX ""idx_Date_Products"" ON {ProductTable} (
	            ""Date""	ASC
            );


            CREATE INDEX ""idx_Arc_Products"" ON {ProductTable} (
	            ""ArcName""
            );
";
            SQLiteConnection? connection = c;
            try
            {
                connection ??= CreateInitialConnection();
                var success = OpenConnection(connection);
                if (!success || t.IsCancellationRequested) return false;
                using SQLiteCommand cmdObj = new(createProductNameToPIDCommand, connection);
                cmdObj.ExecuteNonQuery();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create indexes");
                return false;
            } finally
            {
                if (c is null) connection?.Dispose();
            }
            return true;
        }
        /// <summary>
        /// Adds the triggers required for application to properly execute into the database.
        /// </summary>
        /// <param name="c">The SQLiteConnection to use, if any. Recommended to use a connection, otherwise use <c>CreateTables()</c> instead.</param>
        /// <param name="t">The cancellation token to use, if any. Use <see cref="CancellationToken.None"/> if it should never cancel.</param>
        /// <returns>Whether creating triggers was a success.</returns>
        private bool CreateTriggers(SQLiteConnection? c, CancellationToken t)
        {
            const string triggerSQL = @$"
                        CREATE TRIGGER IF NOT EXISTS delete_on_product_removal
                            AFTER DELETE ON {ProductTable} FOR EACH ROW
                        BEGIN
                            UPDATE {DatabaseInfoTable} SET ""Product Record Count"" = (SELECT ""Product Record Count"" FROM {DatabaseInfoTable}) - 1;
	                        DELETE FROM {ProductFTS5Table} WHERE ID = old.ID;
                        END;

                        CREATE TRIGGER IF NOT EXISTS update_product_count
	                        AFTER INSERT ON {ProductTable} FOR EACH ROW
                        BEGIN
	                        UPDATE {DatabaseInfoTable} SET ""Product Record Count"" = (SELECT ""Product Record Count"" FROM {DatabaseInfoTable}) + 1;
                        END"";
                        CREATE TRIGGER IF NOT EXISTS add_to_fts5
	                        AFTER INSERT ON {ProductTable}
	                    BEGIN
		                    INSERT INTO {ProductFTS5Table} (ID, Name, Tags) VALUES (new.ROWID, new.Name, new.Tags);
	                    END;

";
                        
            SQLiteConnection? connection = c;
            try
            {
                connection = CreateInitialConnection();
                var success = OpenConnection(connection);
                if (!success || t.IsCancellationRequested) return false;
                using SQLiteCommand createCommand = new(triggerSQL, connection);
                createCommand.ExecuteNonQuery();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create triggers");
                return false;
            }
            finally
            {
                if (c is null) connection?.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Adds the triggers required for application to properly execute into the database.
        /// </summary>
        /// <param name="c">The SQLiteConnection to use, if any. Recommended to use a connection, otherwise use <c>CreateTables()</c> instead.</param>
        /// <param name="t">The cancellation token to use, if any. Use <see cref="CancellationToken.None"/> if it should never cancel.</param>
        /// <returns>Whether creating triggers was a success.</returns>
        private bool CreateViews(SQLiteConnection? c, CancellationToken t)
        {
            const string viewSQL = @$"
                        CREATE VIEW {ProductLiteView} AS SELECT Name, Thumbnail, Tags, ROWID FROM {ProductTable};
                        CREATE VIEW {ProductLiteAlphabeticalView} AS SELECT Name, Thumbnail, Tags, ROWID FROM {ProductTable} ORDER BY Name;
                        CREATE VIEW {ProductLiteDateView} AS SELECT Name, Thumbnail, Tags, ROWID FROM {ProductTable} ORDER BY Date;
                        CREATE VIEW {ArchivesView} AS SELECT ArcName FROM {ProductTable}
                        CREATE VIEW {ProductFullView} AS 
                        SELECT P.ID, P.Name, P.Author, P.Date, P.Thumbnail, P.ArcName, P.Tags, 
                               F.File AS Files, D.Destination
                        FROM {ProductTable} P
                        JOIN {FilesTable} F ON P.ID = F.PID
                        JOIN {DestinationTable} D ON P.DestID = D.ID;";

            SQLiteConnection? connection = c;
            try
            {
                connection = CreateInitialConnection();
                var success = OpenConnection(connection);
                if (!success || t.IsCancellationRequested) return false;
                using SQLiteCommand createCommand = new(viewSQL, connection);
                createCommand.ExecuteNonQuery();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create triggers");
                return false;
            }
            finally
            {
                if (c is null) connection?.Dispose();
            }
            return true;

        }
        /// <summary>
        /// Changes a few settings for how the database should act. This is required
        /// to make sure that the database allows multiple connections and use 
        /// less journal sizes.
        /// </summary>
        /// <returns>Whether the execution was a success.</returns>
        private bool ExecutePragmas()
        {

            const string pramaCommmands = @"PRAGMA journal_mode = WAL;
                                            PRAGMA wal_autocheckpoint=2; 
                                            PRAGMA journal_size_limit=32768;
                                            PRAGMA page_size=512;";
            try
            {
                using (SQLiteConnection? connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using SQLiteCommand createCommand = new(pramaCommmands, connection);
                    createCommand.ExecuteNonQuery();
                }
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
        /// <param name="c">The SQLiteConnection to use, if any. Recommended to use a connection, otherwise use <c>DeleteTriggers()</c> instead.</param>
        /// <param name="token">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether deleting triggers was a success.</returns>
        private bool TempDeleteTriggers(SQLiteConnection c, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;
            const string removeTriggersCommand = @"DROP TRIGGER IF EXISTS delete_on_product_removal;
                                                   DROP TRIGGER IF EXISTS update_product_count;
                                                   DROP TRIGGER IF EXISTS add_to_fts5;";
            SQLiteConnection? connection = null;
            SQLiteCommand? deleteCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                deleteCommand = new SQLiteCommand(removeTriggersCommand, connection);
                if (token.IsCancellationRequested) return false;
                deleteCommand.ExecuteNonQuery();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to temp delete triggers");
                return false;
            }
            finally
            {
                if (c == null)
                {
                    connection?.Dispose();
                    deleteCommand?.Dispose();
                }
            }
            return true;
        }

        /// <summary>
        /// Resets the database by drops every table, view, index, and trigger. Then recreates them.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool ResetDatabase(SQLiteConnection? c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            SQLiteConnection? connection = c;
            SQLiteCommand? command = null;
            SQLiteTransaction? transaction = null;
            try
            {
                if (c is null) connection = CreateInitialConnection();
                if (!OpenConnection(connection) || t.IsCancellationRequested) return false;
                transaction = connection!.BeginTransaction();
                if (!TempDeleteTriggers(connection, t)) return false;
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
                command = new SQLiteCommand(txt, connection, transaction);
                command.ExecuteNonQuery();

                if (!CreateTables(connection, t) || !CreateIndexes(connection, t) || 
                    !CreateTriggers(connection, t) || !CreateViews(connection, t)) return false;

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
                transaction?.Rollback();
                return false;
            }
            finally
            {
                if (c is null) connection?.Dispose();
            }
            return true;
        }

        // TO DO: Refresh database code.
        private void RefreshDatabase(CancellationToken t)
        {
            if (t.IsCancellationRequested) return;

            try
            {
                _mainTaskManager.StopAndWait();
                _priorityTaskManager.StopAndWait();
                Initalized = false;
                Initialize();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to refresh database");
            }

        }

        private bool BackupDatabase(CancellationToken t)
        {
            using SQLiteConnection? c = CreateAndOpenConnection(null, true);
            using SQLiteConnection d = new();
            SQLiteConnectionStringBuilder builder = new();

            var newFileName = System.IO.Path.GetFileNameWithoutExtension(Path) + "_backup.db";
            builder.DataSource = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, newFileName));
            d.ConnectionString = builder.ConnectionString;
            if (c is null || !OpenConnection(d) || t.IsCancellationRequested) return false;
            try
            {
                c.BackupDatabase(d, "main", "main", -1, null, 5000);
            }
            catch (Exception ex) { 
                Logger.Error(ex, "Failed to backup database");
                return false;
            }
            return true;
        }

        [Obsolete("Not implemented yet")]
        private void RestoreDatabase(CancellationToken t)
        {
            return;
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
