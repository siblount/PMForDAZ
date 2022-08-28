// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using DAZ_Installer.External;

namespace DAZ_Installer.DP
{
    /// <summary>
    /// This class will handle all database operations such as initializing the database, creating tables, rows, deleting, etc.
    /// Database will be run on a different thread aside from the main thread.
    /// </summary>
    public static partial class DPDatabase
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
        public static bool DatabaseExists { get; private set; } = false;
        public static bool Initalized { get; private set; } = false;
        public static string[] tableNames;

        public static uint ProductRecordCount { get; private set; } = 0;
        public static uint ExtractionRecordCount { get; private set; } = 0;
        public static HashSet<string> ArchiveFileNames { get; private set; } = new HashSet<string>();

        // Events
        /// <summary>
        /// This event is invoked whenever a Search function has completed searching whether any results were found or not.
        /// </summary>
        public static event Action<DPProductRecord[], uint> SearchUpdated;
        /// <summary>
        /// This event is invoked whenever the database schema, database connection status, or other database configurations have been changed.
        /// </summary>
        public static event Action DatabaseUpdated;
        /// <summary>
        /// This event is invoked whenever a table has been updated; updated being having rows, columns removed, modified, or added.
        /// </summary>
        public static event Action<string> TableUpdated;
        /// <summary>
        /// This event is invoked whenever a request to view the table of the database has been called and successfully finished the request.
        /// </summary>
        public static event Action<DataSet, uint> ViewUpdated;
        /// <summary>
        /// This event is currently not being used.
        /// </summary>
        public static event Action<DPProductRecord[], uint> LibraryQueryCompleted;
        /// <summary>
        /// This event is invoked whenever requesting for an extraction record has been successfully completed.
        /// </summary>
        public static event Action<DPExtractionRecord, uint> RecordQueryCompleted;
        /// <summary>
        /// This event is invoked whenever a library query has been completed regardless if it yields any product records or not.
        /// </summary>
        public static event Action<uint> MainQueryCompleted;

        // Product Record events
        /// <summary>
        /// This event is invoked whenever a product record has been removed (aside from when the table has been cleared).
        /// </summary>
        public static event Action<uint> ProductRecordRemoved;
        /// <summary>
        /// This event is invoked whenever a product record has been modified.
        /// </summary>
        public static event Action<DPProductRecord, uint> ProductRecordModified;
        /// <summary>
        /// This event is invoked whenever a new product record has been added.
        /// </summary>
        public static event Action<DPProductRecord, uint> ProductRecordAdded;

        /// <summary>
        /// This event is invoked whenever an extraction record has been removed (aside from when the table has been cleared).
        /// </summary>
        public static event Action<uint> ExtractionRecordRemoved;
        /// <summary>
        /// This event is invoked whenever a extraction record has been modified.
        /// </summary>
        public static event Action<uint, DPExtractionRecord?> ExtractionRecordModified;
        /// <summary>
        /// This event is invoked whenever a new extraction record has been added.
        /// </summary>
        public static event Action<DPExtractionRecord> ExtractionRecordAdded;
        /// <summary>
        /// This event is invoked whenever all of the records have been removed from the database.
        /// </summary>
        public static event Action RecordsCleared;

        private static string _expectedDatabasePath { get => Path.Join(DPSettings.databasePath, "db.db"); }

        // Main task manager...

        private static DPTaskManager _mainTaskManager = new DPTaskManager();

        private static DPTaskManager _priorityTaskManager = new DPTaskManager();

        // Task state.
        private const byte DATABASE_VERSION = 2;
        private static bool _initializing = false;

        // Cache :D
        // TODO: Limit cache to 5.
        // Might remove to keep low-memory profile.
        private readonly static DPCache<string, string[]> _columnsCache = new();

        static DPDatabase() => Initialize();

        #region Private methods
        /// <summary>
        /// Initalize creates a new database if one is not found at the expected database path.
        /// An issue may occur where multiple threads may attempt to initalize the database. If 
        /// there are any errors that occur, Initalize() will return false indicating it failed
        /// to initalize. Otherwise, it will return true indicating it initalized successfully.
        /// </summary>
        /// <returns>True if initalization was successful, otherwise false.</returns>
        private static bool Initialize()
        {
            // If another thread is initalizing, wait for it to initalize or wait 10 secs max.
            try
            {
                if (_initializing)
                    SpinWait.SpinUntil(() => _initializing = false, 10000);
                // Timeout throws an error, if we timed out intialized failed.
            } catch { return false; }

            if (Initalized) return true;
            _initializing = true;
            try
            {
                // Check if database exists.
                DatabaseExists = File.Exists(_expectedDatabasePath);
                // TODO: Check if const database version is higher than the one in the database.
                if (!DatabaseExists)
                {
                    // Create the database.
                    CreateDatabase();
                    // Update database info.
                    using var connection = CreateInitialConnection();
                    if (connection == null) return false;
                    InsertDefaultValuesToTable("DatabaseInfo", connection, CancellationToken.None);
                }
                DatabaseUpdated?.Invoke();
                DPGlobal.AppClosing += OnAppClose;
                Initalized = true;
                tableNames = GetTables(null, CancellationToken.None);
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred while initializing. REASON: {ex}");
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
        private static SQLiteConnection? CreateConnection(bool readOnly = false)
        {
            if (!Initalized)
            {
                var success = Initialize();
                if (!success) return null;
            }
            try
            {
                var connection = new SQLiteConnection();
                var builder = new SQLiteConnectionStringBuilder();
                builder.DataSource = Path.GetFullPath(_expectedDatabasePath);
                builder.Pooling = true;
                builder.ReadOnly = readOnly;
                connection.ConnectionString = builder.ConnectionString;
                return connection; 
            } catch (Exception e)
            {
                DPCommon.WriteToLog($"Failed to create connection. REASON: {e}");
            }
            return null;
        }
        /// <summary>
        /// Creates and returns a connection with the connection string setup. Should only be used for the Initialization function.
        /// </summary>
        /// <returns>An SQLiteConnection if successfully created, otherwise null.</returns>
        private static SQLiteConnection? CreateInitialConnection()
        {
            try
            {
                var connection = new SQLiteConnection();
                var builder = new SQLiteConnectionStringBuilder();
                builder.DataSource = Path.GetFullPath(_expectedDatabasePath);
                builder.Pooling = true;
                connection.ConnectionString = builder.ConnectionString;
                return connection;
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"Failed to create connection. REASON: {e}");
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
        private static SQLiteConnection? CreateAndOpenConnection(SQLiteConnection? connection, bool readOnly = false)
        {
            var c = connection ?? CreateConnection(readOnly);
            var success = OpenConnection(c);
            return success ? c : null;
        }

        /// <summary>
        /// Attempts to open the connection and returns whether it was successful or not.
        /// Any errors including if connection is null will return false.
        /// </summary>
        /// <param name="connection">The connection to open.</param>
        /// <returns>True if the connection opened successfully, otherwise false.</returns>
        private static bool OpenConnection(SQLiteConnection connection)
        {
            if (connection == null) return false;
            if (connection.State != ConnectionState.Closed) return true;
            try
            {
                connection.Open();
                return true;
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to open connection. REASON: {ex}");
            }
            return false;
        }

        /// <summary>
        /// Creates a new database file and sets it up for use.
        /// </summary>
        private static void CreateDatabase()
        {
            
            if (!Directory.Exists(_expectedDatabasePath))
                Directory.CreateDirectory(Path.GetDirectoryName(_expectedDatabasePath));

            SQLiteConnection.CreateFile(_expectedDatabasePath);
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
        private static bool CreateTables()
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
            string createDatabaseInfoCommand = $@"
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
                using (var connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    var createCommand = new SQLiteCommand(createProductRecordsCommand, connection);
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
            } catch (Exception ex) {
                DPCommon.WriteToLog($"An error occurred while attempting to create database. REASON: {ex}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Adds indexes to the database to improve searching and sorting performance.
        /// Does not check if they exist. May throw an error if the tables already exist.
        /// </summary>
        /// <returns>Whether creating indexes was a success.</returns>
        private static bool CreateIndexes()
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
                using (var connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using (var cmdObj = new SQLiteCommand(createTagToPIDCommand, connection))
                    {
                        cmdObj.ExecuteNonQuery();
                        cmdObj.CommandText = createPIDtoTagCommand;
                        cmdObj.ExecuteNonQuery();
                        cmdObj.CommandText = createProductNameToPIDCommand;
                        cmdObj.ExecuteNonQuery();
                        cmdObj.CommandText = createDateCreatedToPIDCommand;
                        cmdObj.ExecuteNonQuery();
                    }
                }
                DatabaseUpdated?.Invoke();
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred creating indexes. REASON: {ex}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Adds the triggers required for application to properly execute into the database.
        /// Does not check if they exist. May throw an error if the tables already exist.
        /// </summary>
        /// <returns>Whether creating triggers was a success.</returns>
        private static bool CreateTriggers() {
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
                using (var connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using (var createCommand = new SQLiteCommand(deleteOnProductRemoveTriggerCommand, connection))
                    {
                        createCommand.ExecuteNonQuery();
                        createCommand.CommandText = deleteOnExtractionRemoveTriggerCommand;
                        createCommand.ExecuteNonQuery();
                        createCommand.CommandText = updateOnExtractionInsertionTriggerCommand;
                        createCommand.ExecuteNonQuery();
                        createCommand.CommandText = updateProductCountTriggerCommand;
                        createCommand.ExecuteNonQuery();
                    }
                }
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred creating triggers. REASON: {ex}");
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
        private static bool ExecutePragmas()
        {

            const string pramaCommmands = @"PRAGMA journal_mode = WAL;
                                            PRAGMA wal_autocheckpoint=2; 
                                            PRAGMA journal_size_limit=32768;
                                            PRAGMA page_size=512;";
            try {
                using (var connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using (var createCommand = new SQLiteCommand(pramaCommmands, connection))
                    {
                        createCommand.ExecuteNonQuery();
                    }
                }
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to execute pragmas. REASON: {ex}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Removes the triggers from the database.
        /// Does not check if they exist. May throw an error if don't the tables already exist.
        /// </summary>
        /// <returns>Whether deleting triggers was a success.</returns>
        private static bool DeleteTriggers() {

            const string removeTriggersCommand =
                        @"DROP TRIGGER IF EXISTS delete_on_extraction_removal;
                        DROP TRIGGER IF EXISTS delete_on_product_removal;
                        DROP TRIGGER IF EXISTS update_on_extraction_add;
                        DROP TRIGGER IF EXISTS update_product_count;";

            try
            {
                using (var connection = CreateInitialConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using var deleteCommand = new SQLiteCommand(removeTriggersCommand, connection);
                    deleteCommand.ExecuteNonQuery();
                }
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred removing triggers. REASON: {ex}");
                return false;
            }
            return true;
        }

        // TO DO: Refresh database code.
        private static void RefreshDatabase(CancellationToken t) {
            if (t.IsCancellationRequested) return;

            try {
                var task = Task.Run(_columnsCache.Clear);
                _mainTaskManager.StopAndWait();
                _priorityTaskManager.StopAndWait();
                Initalized = false;
                Initialize();
                task.Wait();
            } catch (Exception e) {
                DPCommon.WriteToLog($"An unexpected error occured while attempting to refresh the database. REASON: {e}");
            }
            
        }
        
        private static void BackupDatabase(CancellationToken t) {
            return;
        }

        private static void RestoreDatabase(CancellationToken t) {
            return;
        }

        private static void RebuildDatabase(CancellationToken t)
        {

        }

        // Prep for app closure.
        private static void OnAppClose(object e)
        {
            _mainTaskManager.Stop();
            _priorityTaskManager.Stop();
            TruncateJournal();
        }
        
        #endregion
    }
}
