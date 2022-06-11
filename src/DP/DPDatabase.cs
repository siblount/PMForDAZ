// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
        public static event Action<DPProductRecord[]> SearchUpdated;
        public static event Action DatabaseUpdated;
        public static event Action<DataSet> ViewUpdated;
        public static event Action<DPProductRecord[]> LibraryQueryCompleted;
        public static event Action<DPExtractionRecord> RecordQueryCompleted;
        public static event Action MainQueryCompleted;

        public static event Action SearchFailed;
        public static event Action LibraryQueryFailed;
        public static event Action ViewFailed;
        public static event Action DatabaseBroke;
        public static event Action RecordQueryFailed;
        public static event Action MainQueryFailed;
        
        private static string _expectedDatabasePath { get => Path.Join(DPSettings.databasePath, "db.db"); }
        private static string _connectionString { get; set; } = string.Empty;

        // Main task manager...

        private static DPTaskManager _mainTaskManager = new DPTaskManager();

        private static DPTaskManager _priorityTaskManager = new DPTaskManager();

        // Task state.
        private static bool isSearching = false;
        
        private const byte DATABASE_VERSION = 2;
        

        // Cache :D
        // TODO: Limit cache to 5.
        // Might remove to keep low-memory profile.
        private readonly static DPCache<string, string[]> _columnsCache = new();

        
        #region Private methods
        private static void Initialize()
        {
            
            try
            {
                // Check if database exists.
                _expectedDatabasePath = Path.Join(DPSettings.databasePath, "db.db");
                DatabaseExists = File.Exists(_expectedDatabasePath);
                // TODO: Check if const database version is higher than the one in the database.
                if (!DatabaseExists)
                {
                    // Create the database.
                    CreateDatabase();
                    // Update database info.
                    InsertDefaultValuesToTable("DatabaseInfo", CancellationToken.None);
                }
                DatabaseUpdated?.Invoke();
                DPGlobal.AppClosing += OnAppClose;
                Initalized = true;
                tableNames = GetTables(CancellationToken.None);
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred while initializing. REASON: {ex}");
            }
            
        }
        /// <summary>
        /// Creates and returns a connection with the connection string setup.
        /// </summary>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        private static SQLiteConnection? CreateConnection(bool readOnly = false)
        {
            try
            {
                var connection = new SQLiteConnection();
                var builder = new SQLiteConnectionStringBuilder();
                builder.ConnectionString = Path.GetFullPath(_expectedDatabasePath);
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
        /// Attempts to open the connection and returns whether it was successful or not.
        /// Any errors including if connection is null will return false.
        /// </summary>
        /// <param name="connection">The connection to open.</param>
        /// <returns>True if the connection opened successfully, otherwise false.</returns>
        private static bool OpenConnection(SQLiteConnection connection)
        {
            if (connection == null) return false;
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
            var databasePath = Path.Combine(DPSettings.databasePath, "db.db");
            
            if (!Directory.Exists(databasePath))
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));

            SQLiteConnection.CreateFile(databasePath);
            // Create tables, indexes, and triggers.
            CreateTables();
            CreateIndexes();
            CreateTriggers();
            ExecutePragmas();
        }

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
                ""Tag""   TEXT NOT NULL,
                ""Product Record ID""	INTEGER NOT NULL
            )";
            try
            {
                using (var connection = CreateConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    var createCommand = new SQLiteCommand(createProductRecordsCommand, _connection);
                    createCommand.ExecuteNonQuery();
                    createCommand = new SQLiteCommand(createExtractionRecordsCommand, _connection);
                    createCommand.ExecuteNonQuery();
                    createCommand = new SQLiteCommand(createCachedSearchCommand, _connection);
                    createCommand.ExecuteNonQuery();
                    createCommand = new SQLiteCommand(createDatabaseInfoCommand, _connection);
                    createCommand.ExecuteNonQuery();
                    createCommand = new SQLiteCommand(createTagsCommand, _connection);
                    createCommand.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                DPCommon.WriteToLog($"An error occurred while attempting to create database. REASON: {ex}");
                return false;
            }
            return true;
        }

        private static bool CreateIndexes()
        {
            const string createTagToPIDCommand = @"
            CREATE INDEX ""idx_TagToPID"" ON ""Tags"" (
                ""Tag""   ASC,
	            ""Product Record ID""
            );";

            const string createPIDtoTagCommand = @"
            CREATE INDEX ""idx_PIDtoTag"" ON ""Tags"" (
                ""Product Record ID"" ASC,
                ""Tag""	            
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
                using (var connection = CreateConnection())
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
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred creating indexes. REASON: {ex}");
                return false;
            }
            return true;
        }
        
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
                using (var connection = CreateConnection())
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
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred creating triggers. REASON: {ex}");
                return false;
            }
            return true;
            
        }

        private static bool ExecutePragmas()
        {

            const string pramaCommmands = @"PRAGMA journal_mode = WAL;
                                            PRAGMA wal_autocheckpoint=2; 
                                            PRAGMA journal_size_limit=32768;
                                            PRAGMA page_size=512;";
            try {
                using (var connection = CreateConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using (var createCommand = new SQLiteCommand(pramaCommmands, connection))
                    {
                        createCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to execute pragmas. REASON: {ex}");
                return false;
            }
            return true;
        }
        private static bool DeleteTriggers() {

            const string removeTriggersCommand =
                        @"DROP TRIGGER IF EXISTS delete_on_extraction_removal;
                        DROP TRIGGER IF EXISTS delete_on_product_removal;
                        DROP TRIGGER IF EXISTS update_on_extraction_add;
                        DROP TRIGGER IF EXISTS update_product_count;";

            try
            {
                using (var connection = CreateConnection())
                {
                    var success = OpenConnection(connection);
                    if (!success) return false;
                    using var deleteCommand = new SQLiteCommand(removeTriggersCommand, connection);
                    deleteCommand.ExecuteNonQuery();
                }
            } catch (Exception ex)
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
