// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using Serilog;
using System.Data;
using System.Data.SQLite;

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
        public string[] tableNames;

        public uint ProductRecordCount { get; private set; } = 0;
        public uint ExtractionRecordCount { get; private set; } = 0;
        public HashSet<string> ArchiveFileNames { get; private set; } = new HashSet<string>();

        // Events
        /// <summary>
        /// This event is invoked whenever a Search function has completed searching whether any results were found or not.
        /// </summary>
        public event Action<DPProductRecord[], uint>? SearchUpdated;
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
        public event Action<DPProductRecord[], uint>? LibraryQueryCompleted;
        /// <summary>
        /// This event is invoked whenever requesting for an extraction record has been successfully completed.
        /// </summary>
        public event Action<DPExtractionRecord, uint>? RecordQueryCompleted;
        /// <summary>
        /// This event is invoked whenever a library query has been completed regardless if it yields any product records or not.
        /// </summary>
        public event Action<uint>? MainQueryCompleted;

        // Product Record events
        /// <summary>
        /// This event is invoked whenever a product record has been removed (aside from when the table has been cleared).
        /// </summary>
        public event Action<uint>? ProductRecordRemoved;
        /// <summary>
        /// This event is invoked whenever a product record has been modified.
        /// </summary>
        public event Action<DPProductRecord, uint>? ProductRecordModified;
        /// <summary>
        /// This event is invoked whenever a new product record has been added.
        /// </summary>
        public event Action<DPProductRecord>? ProductRecordAdded;

        /// <summary>
        /// This event is invoked whenever an extraction record has been removed (aside from when the table has been cleared).
        /// </summary>
        public event Action<uint>? ExtractionRecordRemoved;
        /// <summary>
        /// This event is invoked whenever a extraction record has been modified.
        /// </summary>
        public event Action<DPExtractionRecord?, uint>? ExtractionRecordModified;
        /// <summary>
        /// This event is invoked whenever a new extraction record has been added.
        /// </summary>
        public event Action<DPExtractionRecord>? ExtractionRecordAdded;
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
        private const byte DATABASE_VERSION = 2;
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
                    InsertDefaultValuesToTable("DatabaseInfo", connection, CancellationToken.None);
                }
                DatabaseUpdated?.Invoke();
                Initalized = true;
                tableNames = GetTables(null, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An error occurred while initializing. REASON: {ex}");
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
                // DPCommon.WriteToLog($"Failed to create connection. REASON: {e}");
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
                // DPCommon.WriteToLog($"Failed to create connection. REASON: {e}");
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
                // DPCommon.WriteToLog($"Failed to open connection. REASON: {ex}");
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
            // Create tables, indexes, and triggers.
            CreateTables();
            CreateIndexes();
            CreateTriggers();
            ExecutePragmas();
        }

        /// <summary>
        /// Adds the tables required for application to properly execute into the database.
        /// Does not check if they exist. May throw an error if the tables already exist.
        /// </summary>
        /// <returns>Whether creating tables was a success.</returns>
        private bool CreateTables()
        {

            const string createProductRecordsCommand = @"
            CREATE TABLE ""ProductRecords"" (

                ""ID""    INTEGER NOT NULL UNIQUE,

                ""Product Name""  TEXT NOT NULL,
	            ""Tags""  TEXT,
	            ""Author""    TEXT,
	            ""SKU""   TEXT,
                ""Date Created"" INTEGER,
                ""Extraction Record ID""  INTEGER UNIQUE,
                ""Thumbnail Full Path""	TEXT,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            ); ";
            const string createExtractionRecordsCommand = @"
            CREATE TABLE ""ExtractionRecords"" (

                ""ID""    INTEGER NOT NULL UNIQUE,

	            ""Files"" TEXT,
                ""Folders"" TEXT,
	            ""Destination Path""  TEXT,
                ""Errored Files""   TEXT,
	            ""Error Messages""    TEXT,
	            ""Archive Name""  TEXT,
	            ""Product Record ID"" INTEGER UNIQUE,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            ); ";
            const string createCachedSearchCommand = @"
            CREATE TABLE ""CachedSearches"" (

                ""Search String"" TEXT NOT NULL UNIQUE,
                ""Result Product IDs""    TEXT,
	            PRIMARY KEY(""Search String"")
            ); ";
            var createDatabaseInfoCommand = $@"
            CREATE TABLE ""DatabaseInfo"" (

                ""Version""   INTEGER NOT NULL DEFAULT {DATABASE_VERSION},
	            ""Product Record Count""  INTEGER NOT NULL DEFAULT 0,
	            ""Extraction Record Count""   INTEGER NOT NULL DEFAULT 0
            );";
            const string createTagsCommand = @"
                CREATE TABLE ""Tags""(
                ""Tag""   TEXT NOT NULL COLLATE NOCASE,
                ""Product Record ID""	INTEGER NOT NULL
            )";
            try
            {
                using SQLiteConnection? connection = CreateInitialConnection();
                var success = OpenConnection(connection);
                if (!success) return false;
                SQLiteCommand createCommand = new(createProductRecordsCommand, connection);
                createCommand.ExecuteNonQuery();
                createCommand.CommandText = createExtractionRecordsCommand;
                createCommand.ExecuteNonQuery();
                createCommand.CommandText = createCachedSearchCommand;
                createCommand.ExecuteNonQuery();
                createCommand.CommandText = createDatabaseInfoCommand;
                createCommand.ExecuteNonQuery();
                createCommand.CommandText = createTagsCommand;
                createCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An error occurred while attempting to create database. REASON: {ex}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Adds indexes to the database to improve searching and sorting performance.
        /// Does not check if they exist. May throw an error if the tables already exist.
        /// </summary>
        /// <returns>Whether creating indexes was a success.</returns>
        private bool CreateIndexes()
        {
            const string createTagToPIDCommand = @"
            CREATE INDEX ""idx_TagToPID"" ON ""Tags"" (
                ""Tag""   COLLATE NOCASE ASC,
	            ""Product Record ID""
            );";

            const string createPIDtoTagCommand = @"
            CREATE INDEX ""idx_PIDtoTag"" ON ""Tags"" (
                ""Product Record ID"" ASC,
                ""Tag""	COLLATE NOCASE
            );";

            const string createProductNameToPIDCommand = @"
            CREATE INDEX ""idx_ProductNameToPID"" ON ""ProductRecords"" (
                ""Product Name"" ASC,
                ""ID""	            
            );";

            const string createDateCreatedToPIDCommand = @"
            CREATE INDEX ""idx_DateCreatedToPID"" ON ""ProductRecords"" (
                ""Date Created"" ASC,
                ""ID""	            
            );";

            try
            {
                using (SQLiteConnection? connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using SQLiteCommand cmdObj = new(createTagToPIDCommand, connection);
                    cmdObj.ExecuteNonQuery();
                    cmdObj.CommandText = createPIDtoTagCommand;
                    cmdObj.ExecuteNonQuery();
                    cmdObj.CommandText = createProductNameToPIDCommand;
                    cmdObj.ExecuteNonQuery();
                    cmdObj.CommandText = createDateCreatedToPIDCommand;
                    cmdObj.ExecuteNonQuery();
                }
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An error occurred creating indexes. REASON: {ex}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Adds the triggers required for application to properly execute into the database.
        /// </summary>
        /// <returns>Whether creating triggers was a success.</returns>
        private bool CreateTriggers()
        {
            const string deleteOnProductRemoveTriggerCommand =
                        @"CREATE TRIGGER IF NOT EXISTS delete_on_product_removal
                            AFTER DELETE ON ProductRecords FOR EACH ROW
                        BEGIN
                            UPDATE DatabaseInfo SET ""Product Record Count"" = (SELECT COUNT(*) FROM ProductRecords);
                            DELETE FROM ExtractionRecords WHERE ID = old.""Extraction Record ID"";
                            DELETE FROM TAGS WHERE ""Product Record ID"" = old.ID;
                        END;";
            const string deleteOnExtractionRemoveTriggerCommand =
                        @"CREATE TRIGGER IF NOT EXISTS delete_on_extraction_removal
                            AFTER DELETE ON ExtractionRecords FOR EACH ROW
                        BEGIN
                            UPDATE DatabaseInfo SET ""Extraction Record Count"" = (SELECT COUNT(*) FROM ExtractionRecords);
                            UPDATE ProductRecords SET ""Extraction Record ID"" = NULL WHERE ""Extraction Record ID"" = old.ID;
                        END;";

            const string updateOnExtractionInsertionTriggerCommand = @"
                        CREATE TRIGGER update_on_extraction_add
	                        AFTER INSERT ON ExtractionRecords FOR EACH ROW
                        BEGIN
	                        UPDATE DatabaseInfo SET ""Extraction Record Count"" = (SELECT COUNT(*) FROM ExtractionRecords);
                            UPDATE ExtractionRecords SET ""Product Record ID"" = (SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1) WHERE ID = NEW.ID;
                            UPDATE ProductRecords SET ""Extraction Record ID"" = NEW.ID WHERE ID IN (SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1);
                        END;";

            const string updateProductCountTriggerCommand = @"
                        CREATE TRIGGER update_product_count
	                        AFTER INSERT ON ProductRecords
                        BEGIN
	                        UPDATE DatabaseInfo SET ""Product Record Count"" = (SELECT COUNT(*) FROM ProductRecords);
                        END";

            try
            {
                using (SQLiteConnection? connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using SQLiteCommand createCommand = new(deleteOnProductRemoveTriggerCommand, connection);
                    createCommand.ExecuteNonQuery();
                    createCommand.CommandText = deleteOnExtractionRemoveTriggerCommand;
                    createCommand.ExecuteNonQuery();
                    createCommand.CommandText = updateOnExtractionInsertionTriggerCommand;
                    createCommand.ExecuteNonQuery();
                    createCommand.CommandText = updateProductCountTriggerCommand;
                    createCommand.ExecuteNonQuery();
                }
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An error occurred creating triggers. REASON: {ex}");
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
                // DPCommon.WriteToLog($"Failed to execute pragmas. REASON: {ex}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Removes the triggers from the database.
        /// Does not check if they exist. May throw an error if don't the tables already exist.
        /// </summary>
        /// <returns>Whether deleting triggers was a success.</returns>
        private bool DeleteTriggers()
        {

            const string removeTriggersCommand =
                        @"DROP TRIGGER IF EXISTS delete_on_extraction_removal;
                        DROP TRIGGER IF EXISTS delete_on_product_removal;
                        DROP TRIGGER IF EXISTS update_on_extraction_add;
                        DROP TRIGGER IF EXISTS update_product_count;";

            try
            {
                using (SQLiteConnection? connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using SQLiteCommand deleteCommand = new(removeTriggersCommand, connection);
                    deleteCommand.ExecuteNonQuery();
                }
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An error occurred removing triggers. REASON: {ex}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Temporarily deletes triggers from the database. 
        /// Use this for all other code excluding initialization. <para/>
        /// To restore triggers, use <c>RestoreTriggers()</c> instead of <c>CreateTriggers()</c>.
        /// </summary>
        /// <param name="c">The SQLiteConnection to use, if any. Recommended to use a connection, otherwise use <c>DeleteTriggers()</c> instead.</param>
        /// <param name="token">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether deleting triggers was a success.</returns>
        private bool TempDeleteTriggers(SQLiteConnection c, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;
            const string removeTriggersCommand =
                        @"DROP TRIGGER IF EXISTS delete_on_extraction_removal;
                        DROP TRIGGER IF EXISTS delete_on_product_removal;
                        DROP TRIGGER IF EXISTS update_on_extraction_add;
                        DROP TRIGGER IF EXISTS update_product_count;";
            SQLiteConnection? connection = null;
            SQLiteCommand deleteCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                deleteCommand = new SQLiteCommand(removeTriggersCommand, connection);
                deleteCommand.ExecuteNonQuery();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An error occurred removing triggers. REASON: {ex}");
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
        /// Restores the triggers previously removed by <c>TempDeleteTriggers()</c>.
        /// </summary>
        /// <param name="c">The SQLiteConnection to use, if any. Recommended to use a connection, otherwise use <c>DeleteTriggers()</c> instead.</param>
        /// <param name="token">Cancel token. Required, cannot be null. Use CancellationToken.None instead and if you wish to restore triggers that 
        /// cannot be cancelled.</param>
        /// <returns>Whether restoring the triggers was a success.</returns>
        private bool RestoreTriggers(SQLiteConnection c, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;
            const string deleteOnProductRemoveTriggerCommand =
                        @"CREATE TRIGGER IF NOT EXISTS delete_on_product_removal
                            AFTER DELETE ON ProductRecords FOR EACH ROW
                        BEGIN
                            UPDATE DatabaseInfo SET ""Product Record Count"" = (SELECT COUNT(*) FROM ProductRecords);
                            DELETE FROM ExtractionRecords WHERE ID = old.""Extraction Record ID"";
                            DELETE FROM TAGS WHERE ""Product Record ID"" = old.ID;
                        END;";
            const string deleteOnExtractionRemoveTriggerCommand =
                        @"CREATE TRIGGER IF NOT EXISTS delete_on_extraction_removal
                            AFTER DELETE ON ExtractionRecords FOR EACH ROW
                        BEGIN
                            UPDATE DatabaseInfo SET ""Extraction Record Count"" = (SELECT COUNT(*) FROM ExtractionRecords);
                            UPDATE ProductRecords SET ""Extraction Record ID"" = NULL WHERE ""Extraction Record ID"" = old.ID;
                        END;";

            const string updateOnExtractionInsertionTriggerCommand = @"
                        CREATE TRIGGER IF NOT EXISTS update_on_extraction_add
	                        AFTER INSERT ON ExtractionRecords FOR EACH ROW
                        BEGIN
	                        UPDATE DatabaseInfo SET ""Extraction Record Count"" = (SELECT COUNT(*) FROM ExtractionRecords);
                            UPDATE ExtractionRecords SET ""Product Record ID"" = (SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1) WHERE ID = NEW.ID;
                            UPDATE ProductRecords SET ""Extraction Record ID"" = NEW.ID WHERE ID IN (SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1);
                        END;";

            const string updateProductCountTriggerCommand = @"
                        CREATE TRIGGER IF NOT EXISTS update_product_count
	                        AFTER INSERT ON ProductRecords
                        BEGIN
	                        UPDATE DatabaseInfo SET ""Product Record Count"" = (SELECT COUNT(*) FROM ProductRecords);
                        END";

            SQLiteConnection? connection = null;
            SQLiteCommand createCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                createCommand = new SQLiteCommand(deleteOnProductRemoveTriggerCommand, connection);
                createCommand.ExecuteNonQuery();
                createCommand.CommandText = deleteOnExtractionRemoveTriggerCommand;
                createCommand.ExecuteNonQuery();
                createCommand.CommandText = updateOnExtractionInsertionTriggerCommand;
                createCommand.ExecuteNonQuery();
                createCommand.CommandText = updateProductCountTriggerCommand;
                createCommand.ExecuteNonQuery();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An error occurred creating triggers. REASON: {ex}");
                return false;
            }
            finally
            {
                if (c == null)
                {
                    connection?.Dispose();
                    createCommand?.Dispose();
                }
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
                // DPCommon.WriteToLog($"An unexpected error occured while attempting to refresh the database. REASON: {e}");
            }

        }

        private void BackupDatabase(CancellationToken t)
        {
            using SQLiteConnection? c = CreateAndOpenConnection(null, true);
            using SQLiteConnection d = new();
            SQLiteConnectionStringBuilder builder = new();

            var newFileName = System.IO.Path.GetFileNameWithoutExtension(Path) + "_backup.db";
            builder.DataSource = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, newFileName));
            d.ConnectionString = builder.ConnectionString;
            try
            {
                c.BackupDatabase(d, "main", "main", -1, null, 5000);
            }
            catch { } // TODO: Log this.

        }

        private void RestoreDatabase(CancellationToken t)
        {
            return;
        }

        private void RebuildDatabase(CancellationToken t)
        {

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
