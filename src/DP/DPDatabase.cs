// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace DAZ_Installer.DP
{
    /// <summary>
    /// This class will handle all database operations such as initializing the database, creating tables, rows, deleting, etc.
    /// Database will be run on a different thread aside from the main thread.
    /// </summary>
    public static class DPDatabase
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
        public static bool Initalizing = false;
        public static string[] tableNames;

        // Database State
        public static bool IsReadyToExecute => _connection.State == ConnectionState.Open;
        public static bool IsBroken => _connection.State == ConnectionState.Broken;
        public static bool CanBeOpened
        {
            get
            {
                if (_connection == null) return true;
                switch (_connection.State)
                {
                    case ConnectionState.Executing:
                    case ConnectionState.Connecting:
                    case ConnectionState.Fetching:
                    case ConnectionState.Broken:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public static uint ProductRecordCount { get; private set; } = 0;
        public static uint ExtractionRecordCount { get; private set; } = 0;

        // Events
        public static event Action<DPProductRecord[]> SearchUpdated;
        public static event Action DatabaseUpdated;
        public static event Action<DataSet> ViewUpdated;
        public static event Action<DPProductRecord[]> LibraryQueryCompleted;
        public static event Action<DPExtractionRecord> RecordQueryCompleted;

        public static event Action SearchFailed;
        public static event Action LibraryQueryFailed;
        public static event Action ViewFailed;
        public static event Action DatabaseBroke;
        public static event Action RecordQueryFailed;


        // Private
        private static SQLiteConnection _connection = new SQLiteConnection();
        
        private static string _expectedDatabasePath { get; set; } = Path.Join(DPSettings.databasePath, "db.db");
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

        #region Public methods
        // TO DO: Improve. This can be so much more efficient.
        // I lack the brain capacity to do this at the moment.
        public static void Search(string searchQuery, DPSortMethod sortMethod = DPSortMethod.None)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue(DoSearchS, searchQuery, sortMethod);
        }

        public static void RegexSearch(string regex, DPSortMethod sortMethod = DPSortMethod.None)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue(DoRegexSearchS, regex, sortMethod);
        }

        public static void GetProductRecords(DPSortMethod sortMethod, uint page = 1, uint limit = 0)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue(DoLibraryQuery, page, limit, sortMethod);
        }

        public static void StopMainDatabaseOperations()
        {
            _mainTaskManager.Stop();
        }
        

        public static void StopAllDatabaseOperations() {
            _mainTaskManager.Stop();
            _priorityTaskManager.Stop();
        }

        #endregion

        #region Queryable methods

        public static void InitializeQ()
        {
            if (!Initalized)
                _mainTaskManager.AddToQueue(Initialize);
        }

        /// <summary>
        /// If `forceClose` is false, the close action will be queued. Otherwise, the action queue will be cleared and database will be closed immediately.
        /// </summary>
        /// <param name="forceClose">Closes the database connection immediately if True, otherwise database closure is queued.</param>
        public static void CloseConnectionQ(bool forceClose = false)
        {
            if (!forceClose)
                _mainTaskManager.AddToQueue(CloseConnection); 
            else
                CloseConnection(CancellationToken.None);
        }
        /// <summary>
        /// If `forceRefresh` is false, the refresh action will be queued. Otherwise, the action queue will be cleared and database will be refreshed immediately.
        /// </summary>
        /// <param name="forceRefresh">Refreshes immediately if True, otherwise it is queued.</param>
        public static void RefreshDatabaseQ(bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                _mainTaskManager.Stop();
                _priorityTaskManager.Stop();
                Initalized = false;
                Initialize();
                _columnsCache.Clear();
            } else
            {
                _mainTaskManager.AddToQueue(RefreshDatabase);
            }
        }

        public static void ViewTableQ(string tableName)
        {
            _mainTaskManager.AddToQueue(GetAllValuesFromTable, tableName);
        }

        public static void AddNewRecordEntry(DPProductRecord pRecord)
        {
            _mainTaskManager.AddToQueue(InsertRecords, pRecord, DPExtractionRecord.NULL_RECORD);
        }

        public static void AddNewRecordEntry(DPProductRecord pRecord, DPExtractionRecord eRecord)
        {
            _mainTaskManager.AddToQueue(InsertRecords, pRecord, eRecord);
        }

        public static void InsertNewRowQ(string tableName, object[] values, string[] columns)
        {
            _mainTaskManager.AddToQueue(InsertValuesToTable, tableName, columns, values);
        }

        public static void RemoveRowQ(string tableName, int id) {
            var arg = new Tuple<string, object>[1] { new Tuple<string, object>("ID", id) };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, arg, false);
        }

        public static void ClearTableQ(string tableName) {
            _mainTaskManager.AddToQueue(RemoveAllFromTable, tableName);
        }

        public static void UpdateValuesQ(string tableName, object[] values, string[] columns)
        {
            _mainTaskManager.AddToQueue(UpdateValues, tableName, columns, values);
        }

        public static void RemoveProductRecordsViaTagsQ(string[] tags) {
            _mainTaskManager.AddToQueue(RemoveProductRecordsViaTag, tags);
        }

        public static void RemoveProductRecordsQ(Tuple<string, object> condition) {
            var t = new Tuple<string, object>[] { condition };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, "ProductRecords", t, false);
        }
        public static void RemoveProductRecordsQ(Tuple<string, object>[] conditions) {
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, "ProductRecords", conditions, false);
        }

        public static void RemoveRowWithConditionQ(string tableName, Tuple<string, object> condition) {
            var t = new Tuple<string, object>[] { condition };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, t, false);
        }

        public static void RemoveRowWithConditionsQ(string tableName, Tuple<string, object>[] conditions) {
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, conditions, false);
        }

        public static void RemoveAllRecordsQ() {
            _mainTaskManager.AddToQueue(RemoveAllRecords);
        }

        public static void RemoveTagsQ(uint pid) {
            _mainTaskManager.AddToQueue(DeleteTags, pid);
        }

        public static void GetExtractionRecordQ(uint eid) {
            _priorityTaskManager.AddToQueue(GetExtractionRecord, eid);
        }
        #endregion
        
        #region Private methods
        private static void Initialize()
        {
            if (Initalizing)
            {
                SpinWait.SpinUntil(() => Initalized, 30 * 1000);
                return;
            }
            Initalizing = true;
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
                else
                {
                    UpdateConnectionString();
                }
                CloseConnection(CancellationToken.None);
                DatabaseUpdated?.Invoke();
                DP.DPGlobal.AppClosing += OnAppClose;
                Initalized = true;
                tableNames = GetTables(CancellationToken.None);
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred while initializing. REASON: {ex}");
            } finally
            {
                Initalizing = false;
            }
            
        }
        
        /// <summary>
        /// Returns true if the database was ready (or broken) before the timeout. Otherwise, false.
        /// </summary>
        /// <param name="timeoutMilliseconds"> The milliseconds until timeout.</param>
        private static bool WaitUntilDatabaseReady(int timeoutMilliseconds)
        {
            try {
                SpinWait.SpinUntil(() => IsReadyToExecute || IsBroken, timeoutMilliseconds);
                return true;
            } catch (Exception _) {
                return false;
            }
        }

        private static void WaitUntilDatabaseReady()
        {
            SpinWait.SpinUntil(() => IsReadyToExecute);
        }

        private static void CreateConnection()
        {
            _connection = new SQLiteConnection();
            _connectionString = "Data Source = " + Path.GetFullPath(_expectedDatabasePath);
            _connection.ConnectionString = _connectionString;
        }

        private static void UpdateConnectionString()
        {
            if (_connection != null)
            {
                CreateConnection();
                return;
            }

            _connectionString = "Data Source = " + Path.GetFullPath(_expectedDatabasePath);
            _connection.ConnectionString = _connectionString;  
        }

        private static void OpenConnection()
        {
            // Synchronization error.
            if (_connection?.State == ConnectionState.Closed)
                _connection.Open();

            if (_connection == null)
            {
                CreateConnection();
                _connection.Open();
            }

        }

        private static void CreateDatabase()
        {
            var databasePath = Path.Combine(DPSettings.databasePath, "db.db");
            
            if (!Directory.Exists(databasePath))
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));

            SQLiteConnection.CreateFile(databasePath);
            UpdateConnectionString();
            // Create tables, indexes, and triggers.
            CreateTables();
            CreateIndexes();
            CreateTriggers();
            ExecutePragmas();
            CloseConnectionQ(true);
        }

        private static bool CreateTables()
        {
            // At this point, we are being called via Initialization, no need to wait.
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
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
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred while attempting to create database. REASON: {ex}");
                return false;
            }
            return true;
        }

        private static bool CreateIndexes()
        {
            // At this point, we are being called via Initialization, no need to wait.
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();

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
                using (var cmdObj = new SQLiteCommand(createTagToPIDCommand, _connection))
                {
                    cmdObj.ExecuteNonQuery();
                    cmdObj.CommandText = createPIDtoTagCommand;
                    cmdObj.ExecuteNonQuery();
                    cmdObj.CommandText = createProductNameToPIDCommand;
                    cmdObj.ExecuteNonQuery();
                    cmdObj.CommandText = createDateCreatedToPIDCommand;
                    cmdObj.ExecuteNonQuery();
                }
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred creating indexes. REASON: {ex}");
                return false;
            }
            return true;
        }
        
        private static bool CreateTriggers() {
            // At this point, we are being called via Initialization, no need to wait.
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();

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
                using (var createCommand = new SQLiteCommand(deleteOnProductRemoveTriggerCommand, _connection))
                {
                    createCommand.ExecuteNonQuery();
                    createCommand.CommandText = deleteOnExtractionRemoveTriggerCommand;
                    createCommand.ExecuteNonQuery();
                    createCommand.CommandText = updateOnExtractionInsertionTriggerCommand;
                    createCommand.ExecuteNonQuery();
                    createCommand.CommandText = updateProductCountTriggerCommand;
                    createCommand.ExecuteNonQuery();
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
                using (var createCommand = new SQLiteCommand(pramaCommmands, _connection))
                {
                    createCommand.ExecuteNonQuery();
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
            // At this point, we have priority, no need to wait.
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();

            const string removeTriggersCommand =
                        @"DROP TRIGGER IF EXISTS delete_on_extraction_removal;
                        DROP TRIGGER IF EXISTS delete_on_product_removal;
                        DROP TRIGGER IF EXISTS update_on_extraction_add;
                        DROP TRIGGER IF EXISTS update_product_count;";

            try
            {
                var deleteCommand = new SQLiteCommand(removeTriggersCommand, _connection);
                deleteCommand.ExecuteNonQuery();
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred removing triggers. REASON: {ex}");
                return false;
            }
            return true;
        }
        private static string[] GetColumns(string tableName, CancellationToken t)
        {
            if (t.IsCancellationRequested || tableName.Length == 0) return Array.Empty<string>();
            
            WaitUntilDatabaseReady();
            if (!Initalized) Initialize();
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return Array.Empty<string>();
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            if (_columnsCache.ContainsKey(tableName)) return _columnsCache[tableName];

            try {
                var randomCommand = $"SELECT * FROM {tableName} LIMIT 1;";
                var sqlCommand = new SQLiteCommand(randomCommand, _connection);
                var reader = sqlCommand.ExecuteReader();
                var table = reader.GetSchemaTable();

                List<string> columns = new List<string>();
                foreach (DataRow row in table.Rows) {
                    if (t.IsCancellationRequested) return Array.Empty<string>();
                    columns.Add((string) row.ItemArray[0]);
                }

                // Cache it.
                _columnsCache[tableName] = columns.ToArray();
                return _columnsCache[tableName];
            } catch (Exception e) {
                DPCommon.WriteToLog($"An unexpected error occurred attempting to get columns for table: {tableName}. REASON: {e}");
            } finally {
                
            }
            return Array.Empty<string>();
        }

        private static void UpdateProductRecordCount(SQLiteConnection connection, CancellationToken t)
        {
            if (t.IsCancellationRequested) return;

            WaitUntilDatabaseReady();
            if (!Initalized) Initialize();
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();

            const string getCmd = @"SELECT ""Product Record Count"" FROM DatabaseInfo;";

            try
            {
                using (var cmd = new SQLiteCommand(getCmd, connection))
                {
                    ProductRecordCount = Convert.ToUInt32(cmd.ExecuteScalar());
                }
                
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog("An unexpected error occurred while attempting to get product record count.");
            }
            DPCommon.WriteToLog("Product Record Count: ", ProductRecordCount);
        }

        private static void UpdateExtractionRecordCount(SQLiteConnection connection, CancellationToken t)
        {
            if (t.IsCancellationRequested) return;

            WaitUntilDatabaseReady();
            if (!Initalized) Initialize();
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();

            const string getCmd = @"SELECT ""Extraction Record Count"" FROM DatabaseInfo;";

            try
            {
                using (var cmd = new SQLiteCommand(getCmd, connection))
                {
                    ExtractionRecordCount = Convert.ToUInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog("An unexpected error occurred while attempting to get extraction record count.");
            }
            DPCommon.WriteToLog("Extraction Record Count: ", ExtractionRecordCount);

        }

        private static void InsertTags(string[] tags, CancellationToken t)
        {
            if (t.IsCancellationRequested) return;

            WaitUntilDatabaseReady();
            if (!Initalized) Initialize();
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            uint pid = GetLastProductID(t);
            if (pid == 0)
            {
                DPCommon.WriteToLog("Product ID returned 0; no tags added.");
                return;
            }

            List<string> tagsStripped = new List<string>(tags.Length);
            foreach (string tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                string tagTrimmed = tag.Trim();
                if (tagTrimmed.Length == 0) continue;
                tagsStripped.Add(tagTrimmed);
            }

            object[][] vals = new object[tagsStripped.Count][];
            for (var i = 0; i < tagsStripped.Count; i++)
            {
                vals[i] = new object[] { tagsStripped[i], pid };
            }

            InsertMultipleValuesToTable("Tags", new string[] { "Tag", "Product Record ID" }, vals, t);
        }

        private static void DeleteTags(uint pid, CancellationToken t) {
            RemoveValuesWithCondition("Tags", 
                    new Tuple<string, object>[] {new Tuple<string, object>("Product Record ID", pid) }
                    , false, t);
        }

        private static bool InsertMultipleValuesToTable(string tableName, string[] columns, object[][] values, CancellationToken t)
        {
            if (!Initalized) Initialize();
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();

            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return false;

            columns = columns?.Length == 0 ? GetColumns(tableName, t) : columns;
            if (values.Length == 0) return true;
            if (t.IsCancellationRequested || columns == null || columns.Length == 0) return false;

            SQLiteTransaction transaction = _connection.BeginTransaction();
            // Build columns.
            // Wrap in quotes
            for (var i = 0; i < columns.Length; i++)
            {
                columns[i] = '"' + columns[i] + '"';
            }
            var columnsToAdd = string.Join(',', columns);
            StringBuilder builder = new StringBuilder((values.Length) * 20);
            List<string> args = new List<string>(values.Length * 5);
            int startNum = 0;
            for (var i = 0; i < values.Length; i++)
            {
                var str = "(";
                var _args = CreateParams(ref str, values[i].Length, ref startNum);
                str += ')';
                foreach (var arg in _args)
                {
                    args.Add(arg);
                }
                builder.AppendLine(str + ',');
            }
            builder.Remove(builder.Length - 3, 2);
            object[] valsFlattened = new object[startNum];
            var nextOpen = 0;
            for (var i = 0; i < values.Length; i++)
            {
                var arrLength = values[i].Length;
                Array.Copy(values[i], 0, valsFlattened, nextOpen, arrLength);
                nextOpen += arrLength;
            }

            var insertCommand = $"INSERT INTO {tableName} ({columnsToAdd})\nVALUES {builder};";
            try
            {
                var sqlCommand = new SQLiteCommand(insertCommand, _connection, transaction);
                FillParamsToConnection(ref sqlCommand, args.ToArray(), valsFlattened.ToArray());
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to insert values to {columnsToAdd}. REASON: {ex}");
                transaction.Rollback();
                return false;
            }

            return true;
        }

        private static bool InsertDefaultValuesToTable(string tableName, CancellationToken t)
        {
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();

            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return false;

            SQLiteTransaction transaction = _connection.BeginTransaction();
            // TODO: Append params.
            var insertCommand = $"INSERT INTO {tableName} DEFAULT VALUES;";
            try
            {
                var sqlCommand = new SQLiteCommand(insertCommand, _connection, transaction);
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to insert default values to {tableName}. REASON: {ex}");
                transaction.Rollback();
                return false;
            }

            return true;
        }

        private static bool InsertValuesToTable(string tableName, string[] columns, object[] values, CancellationToken t)
        {
            if (!Initalized) Initialize();
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return false;

            columns = columns?.Length == 0 ? GetColumns(tableName, t) : columns;
            if (t.IsCancellationRequested || columns == null || columns.Length == 0) return false;

            SQLiteTransaction transaction = _connection.BeginTransaction();
            // Build columns.
            // Wrap in quotes
            for (var i = 0; i < columns.Length; i++)
            {
                columns[i] = '"' + columns[i] + '"';
            }
            var columnsToAdd = string.Join(',', columns);
            
            // TODO: Append params.
            var insertCommand = $"INSERT INTO {tableName} ({columnsToAdd})\nVALUES(";
            var args = CreateParams(ref insertCommand, values.Length);
            insertCommand += ");";
            try
            {
                var sqlCommand = new SQLiteCommand(insertCommand, _connection, transaction);
                FillParamsToConnection(ref sqlCommand, args, values);
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                DatabaseUpdated?.Invoke();
                sqlCommand.Dispose();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to insert values to {columnsToAdd}. REASON: {ex}");
                transaction.Rollback();
                return false;
            }
            
            return true;

        }

        private static bool UpdateValues(string tableName, string[] columns, object[] newValues, CancellationToken t)
        {
            if (!Initalized) Initialize();
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return false;

            columns = columns?.Length == 0 ? GetColumns(tableName, t) : columns;
            if (t.IsCancellationRequested || columns == null || 
                columns.Length == 0 || columns.Length != newValues.Length) return false;

            SQLiteTransaction transaction = _connection.BeginTransaction();
            // Build columns.
            var columnsToAdd = string.Join(',', columns);
            var valuesToAdd = string.Join(',', newValues);
            var insertCommand = $"INSERT INTO {tableName} ({columnsToAdd})\nVALUES({valuesToAdd});";
            try
            {
                var sqlCommand = new SQLiteCommand(insertCommand, _connection, transaction);
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to update {valuesToAdd} to {columnsToAdd}. REASON: {ex.Message}");
                transaction.Rollback();
                return false;
            }
            
            return true;
        }
        
        private static bool RemoveProductRecordsViaTag(string[] values, CancellationToken t) {
            if (!Initalized) Initialize();
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return false;

            if (values.Length == 0) return true;
            string args = ConvertParamsToString(values);
            SQLiteTransaction transaction = _connection.BeginTransaction();
            string idsCommand = $"SELECT \"Product Record ID\" FROM Tags WHERE Tag IN ({args})";
            string deleteCommand = $"DELETE FROM ProductRecords WHERE ID IN ({idsCommand});";
            try
            {
                var sqlCommand = new SQLiteCommand(deleteCommand, _connection, transaction);
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to delete from ProductRecords where ID IN {args}. REASON: {ex.Message}");
                transaction.Rollback();
                return false;
            }
            
            return true;
        }
        private static bool RemoveValuesWithCondition(string tableName, Tuple<string, object>[] conditions, bool or, CancellationToken t)
        {
            if (!Initalized) Initialize();
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return false;

            SQLiteTransaction transaction = _connection.BeginTransaction();
            // Build columns.
            string whereCommand = "";

            for (var i = 0; i < conditions.Length; i++) {
                var tuple = conditions[i];
                var column = tuple.Item1;
                var item = tuple.Item2;
                if (i == 0) {
                    whereCommand += $"WHERE {column} IN ({ConvertParamsToString(item)})";
                } else {
                    if (or) {
                        whereCommand += $"OR WHERE {column} IN ({ConvertParamsToString(item)})";
                    } else {
                        whereCommand += $"AND WHERE {column} IN ({ConvertParamsToString(item)})";
                    }
                }
            }

            string deleteCommand = $"DELETE FROM {tableName} {whereCommand};";
            try
            {
                var sqlCommand = new SQLiteCommand(deleteCommand, _connection, transaction);
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to delete from {tableName} {whereCommand}. REASON: {ex.Message}");
                transaction.Rollback();
                return false;
            }
            
            return true;
        }
        private static bool RemoveAllFromTable(string tableName, CancellationToken t)
        {
            if (!Initalized) Initialize();
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return false;

            SQLiteTransaction transaction = _connection.BeginTransaction();
            var deleteCommand = $"DELETE FROM {tableName};"; // Faster way is to drop the table & re-make it.
            try
            {
                var sqlCommand = new SQLiteCommand(deleteCommand, _connection, transaction);
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed delete all values for table: {tableName}. REASON: {ex.Message}");
                transaction.Rollback();
                return false;
            }
            
            return true;
        }
        
        private static bool RemoveAllRecords(CancellationToken t) {
            if (!Initalized) Initialize();
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return false;
            
            SQLiteTransaction transaction = _connection.BeginTransaction();
            // Also deletes from tags via trigger.
            var deleteCommand = $"DELETE FROM ProductRecords; DELETE FROM ExtractionRecords;"; // Faster way is to drop the table & re-make it.
            try
            {
                if (DeleteTriggers()) {
                    var sqlCommand = new SQLiteCommand(deleteCommand, _connection, transaction);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    transaction.Dispose();
                    CreateTriggers();
                    DatabaseUpdated?.Invoke();
                }
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to delete records. REASON: {ex}");
                transaction.Rollback();
                return false;
            }
            
            return true;
        }
        
        private static void SetupSQLRegexQuery(string regex, DPSortMethod method, ref SQLiteCommand command)
        {
            string sqlQuery = @"SELECT * FROM ProductRecords WHERE (SELECT ""Product Record ID"" FROM Tags WHERE ID REGEX @A";

            switch (method)
            {
                case DPSortMethod.Alphabetical:
                    sqlQuery += @") ORDER BY ""Product Name"" ASC;";
                    break;
                case DPSortMethod.Date:
                    sqlQuery += @") ORDER BY ""Date Created"" ASC;";
                    break;
                case DPSortMethod.Relevance:
                    sqlQuery += @"GROUP BY ""Product Record ID"" ORDER BY COUNT(*) DESC);";
                    break;
                default:
                    sqlQuery += ");";
                    break;
            }

            command.CommandText = sqlQuery;
            command.Parameters.Add(new SQLiteParameter("@A", regex));
        }

        private static void SetupSQLLibraryQuery(uint page, uint limit, DPSortMethod method, ref SQLiteCommand command)
        {
            uint beginningRowID = (page - 1) * limit;
            string sqlQuery = $"SELECT * FROM ProductRecords WHERE ROWID >= {beginningRowID} ";

            switch (method)
            {
                case DPSortMethod.Alphabetical:
                    sqlQuery += @"ORDER BY ""Product Name"" ASC";
                    break;
                case DPSortMethod.Date:
                    sqlQuery += @"ORDER BY ""Date Created"" ASC";
                    break;
            }

            sqlQuery += limit == 0 ? ";" : "LIMIT " + limit + ";";

            command.CommandText = sqlQuery;
        }
        private static void SetupSQLSearchQuery(string userQuery, DPSortMethod method, ref SQLiteCommand command)
        {
            string[] tokens = userQuery.Split(' ');
            string sqlQuery = @"SELECT * FROM ProductRecords WHERE ID IN (SELECT ""Product Record ID"" FROM Tags WHERE Tag IN (";
            StringBuilder sb = new StringBuilder(((int)Math.Floor(Math.Log10(tokens.Length)) + 1) * tokens.Length + (4 * tokens.Length));
            for (int i = 0; i < tokens.Length; i++)
            {
                sb.Append(i == tokens.Length - 1 ? "@A" + i :
                                                    "@A" + i + ", ");
            }
            sqlQuery += sb.ToString().Trim();

            switch (method)
            {
                case DPSortMethod.Alphabetical:
                    sqlQuery += @")) ORDER BY ""Product Name"" ASC;";
                    break;
                case DPSortMethod.Date:
                    sqlQuery += @")) ORDER BY ""Date Created"" ASC;";
                    break;
                case DPSortMethod.Relevance:
                    sqlQuery += @"GROUP BY ""Product Record ID"" ORDER BY COUNT(*) DESC));";
                    break;
                default:
                    sqlQuery += "));";
                    break;
            }

            command.CommandText = sqlQuery;

            for (int i = 0; i < tokens.Length; i++)
            {
                command.Parameters.Add(new SQLiteParameter("@A" + i, tokens[i]));
            }


        }
        
        private static DPProductRecord[] SearchProductRecordsViaTagsS(SQLiteCommand command, CancellationToken t)
        {
            if (t.IsCancellationRequested) {
                return Array.Empty<DPProductRecord>();
            }
            var reader = command.ExecuteReader();

            var searchResults = new List<DPProductRecord>(reader.StepCount);
            string productName, author, thumbnailPath, sku;
            string[] tags;
            DateTime dateCreated;
            uint extractionID,  pid;

            // TODO : Use new product search record.
            while (reader.Read())
            {
                if (t.IsCancellationRequested) {
                    return Array.Empty<DPProductRecord>();
                }
                // Construct product records
                // NULL values return type DB.NULL.
                productName = (string)reader["Product Name"];
                tags = ((string)reader["Tags"]).Trim().Split(", "); // May return null but never does.
                author = reader["Author"] as string; // May return NULL
                thumbnailPath = reader["Thumbnail Full Path"] as string; // May return NULL
                extractionID = Convert.ToUInt32(reader["Extraction Record ID"]);
                dateCreated = DateTime.FromFileTimeUtc((long)reader["Date Created"]);
                sku = reader["SKU"] as string; // May return NULL
                pid = Convert.ToUInt32(reader["ID"]);
                searchResults.Add(
                    new DPProductRecord(productName, tags, author, sku, dateCreated, thumbnailPath, extractionID, pid));
                
            }

            return searchResults.ToArray();
        }

        private static void GetAllValuesFromTable(string tableName, CancellationToken token)
        {
            if (!Initalized) Initialize();
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (token.IsCancellationRequested) return;

            try
            {
                var getCommand = $"SELECT * FROM {tableName}";
                var sqlCommand = new SQLiteCommand(getCommand, _connection);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlCommand);
                DataSet dataset = new DataSet(tableName);
                adapter.Fill(dataset);
                ViewUpdated?.Invoke(dataset);
            }
            catch (Exception ex) { }
            
            
        }

        // TO DO: Refresh database code.
        private static void RefreshDatabase(CancellationToken t) {
            if (t.IsCancellationRequested) return;
            
            WaitUntilDatabaseReady();
            try {
                _mainTaskManager.Stop();
                _priorityTaskManager.Stop();
                Initalized = false;
                Initialize();
                _columnsCache.Clear();
            } catch (Exception e) {
                DPCommon.WriteToLog($"An unexpected error occured while attempting to refresh the database. REASON: {e}");
            }
            
        }

        private static void CloseConnection(CancellationToken t) {
            if (t.IsCancellationRequested) return;
            _connection.Close();
        }
        
        /// <summary>
        /// Generates SQL command based on search query and returns a sorted list of products.
        /// </summary>
        /// <param name="searchQuery">The raw search query from the user.</param>
        /// <returns></returns>
        private static void DoSearchS(string searchQuery, DPSortMethod method, CancellationToken t) {
            // User initialized another search while an old search hasn't finished completing.
            if (isSearching)
            {
                _priorityTaskManager.Stop();
            }
            if (!Initalized) Initialize();
            if (IsBroken)
            {
                DPCommon.WriteToLog("Search cannot proceed due to database being broken.");
                SearchFailed?.Invoke();
            }
            isSearching = true;

            var constring = "Data Source = " + Path.GetFullPath(_expectedDatabasePath) + ";Read Only=True";
            using (var _connection = new SQLiteConnection(constring))
            {
                _connection.Open();
                SpinWait.SpinUntil(() => _connection.State != ConnectionState.Connecting
                                        || _connection.State != ConnectionState.Executing
                                        || _connection.State != ConnectionState.Fetching);
                if (_connection.State == ConnectionState.Broken) return;
                SQLiteCommand command = null;
                try
                {
                    command = new SQLiteCommand(_connection);
                    SetupSQLSearchQuery(searchQuery, method, ref command);
                    var results = SearchProductRecordsViaTagsS(command, t);
                    SearchUpdated?.Invoke(results);
                    command.Dispose();
                    UpdateProductRecordCount(_connection, t);
                    UpdateExtractionRecordCount(_connection, t);

                }
                catch (Exception e)
                {
                    command?.Dispose();
                    DPCommon.WriteToLog("An error occurred with the search function.");
                    SearchFailed?.Invoke();
                }
            }

            if (!t.IsCancellationRequested) isSearching = false;
        }
        /// <summary>
        /// Generates SQL command based on the regex and returns a sorted list of products.
        /// </summary>
        /// <param name="regex">The regex to perform from the user.</param>
        /// <returns></returns>
        private static void DoRegexSearchS(string regex, DPSortMethod method, CancellationToken t)
        {
            if (isSearching)
            {
                _priorityTaskManager.Stop();
            }
            if (!Initalized) Initialize();
            if (IsBroken)
            {
                DPCommon.WriteToLog("Search cannot proceed due to database being broken.");
                SearchFailed?.Invoke();
            }
            isSearching = true;

            var constring = "Data Source = " + Path.GetFullPath(_expectedDatabasePath) + ";Read Only=True";
            using (var _connection = new SQLiteConnection(constring))
            {
                _connection.Open();
                SpinWait.SpinUntil(() => _connection.State != ConnectionState.Connecting
                                        || _connection.State != ConnectionState.Executing
                                        || _connection.State != ConnectionState.Fetching);
                if (_connection.State == ConnectionState.Broken) return;
                SQLiteCommand command = null;
                try
                {
                    command = new SQLiteCommand(_connection);
                    SetupSQLRegexQuery(regex, method, ref command);
                    var results = SearchProductRecordsViaTagsS(command, t);
                    SearchUpdated?.Invoke(results);
                    command.Dispose();
                    UpdateProductRecordCount(_connection, t);
                    UpdateExtractionRecordCount(_connection, t);
                }
                catch (Exception e)
                {
                    command?.Dispose();
                    DPCommon.WriteToLog("An error occurred with the search function.");
                    SearchFailed?.Invoke();
                }
            }

            if (!t.IsCancellationRequested) isSearching = false;
        }
        /// <summary>
        /// Does an query for the library and emits the LibraryQueryCompleted event.
        /// </summary>
        /// <param name="limit">The limit amount of results to return.</param>
        /// <param name="method">The sorting method to apply to query results.</param>
        private static void DoLibraryQuery(uint page, uint limit, DPSortMethod method, CancellationToken t)
        {
            if (isSearching)
            {
                _priorityTaskManager.Stop();
            }
            if (!Initalized) Initialize();
            if (IsBroken)
            {
                DPCommon.WriteToLog("Search cannot proceed due to database being broken.");
                SearchFailed?.Invoke();
            }
            isSearching = true;

            var constring = "Data Source = " + Path.GetFullPath(_expectedDatabasePath) + ";Read Only=True";
            using (var _connection = new SQLiteConnection(constring)) {
                _connection.Open();
                SpinWait.SpinUntil(() => _connection.State != ConnectionState.Connecting 
                                        || _connection.State != ConnectionState.Executing
                                        || _connection.State != ConnectionState.Fetching);
                if (_connection.State == ConnectionState.Broken) return;
                SQLiteCommand command = null;
                try
                {
                    command = new SQLiteCommand(_connection);
                    SetupSQLLibraryQuery(page, limit, method, ref command);
                    var results = SearchProductRecordsViaTagsS(command, t);
                    LibraryQueryCompleted?.Invoke(results);
                    command.Dispose();
                    UpdateProductRecordCount(_connection, t);
                    UpdateExtractionRecordCount(_connection, t);

                }
                catch (Exception e)
                {
                    command?.Dispose();
                    DPCommon.WriteToLog("An error occurred with the search function.");
                    LibraryQueryFailed?.Invoke();
                }
                if (!t.IsCancellationRequested) isSearching = false;
            }
        }
        //private static void DoLibraryQuery(uint limit, DPSortMethod method, CancellationToken t)
        //{
        //    // User initialized another search while an old search hasn't finished completing.
        //    if (isSearching)
        //    {
        //        _searchTaskManager.Stop();
        //    }
        //    isSearching = true;
        //    try
        //    {
        //        SpinWait.SpinUntil(() => nonSearchTaskIsAboutToExecute == 0, 60 * 10000);
        //    }
        //    catch (Exception e)
        //    {
        //        DPCommon.WriteToLog("Search timed out due to previous tasks.");
        //        _searchTaskManager.Stop();
        //    }
        //    if (!Initalized) Initialize();
        //    if (IsBroken)
        //    {
        //        DPCommon.WriteToLog("Search cannot proceed due to database being broken.");
        //        LibraryQueryCompleted?.Invoke(Array.Empty<DPProductRecord>());
        //    }
        //    OpenConnection();
        //    DPProductRecord[] results;

        //    try
        //    {
        //        var command = new SQLiteCommand(_connection);
        //        SetupSQLLibraryQuery(limit, method, ref command);
        //        results = SearchProductRecordsViaTagsS(command, t);
        //        LibraryQueryCompleted?.Invoke(results);

        //    }
        //    catch (Exception e)
        //    {
        //        DPCommon.WriteToLog("An error occurred with the search function.");
        //        results = Array.Empty<DPProductRecord>();
        //        LibraryQueryCompleted?.Invoke(results);
        //    }

        //    if (!t.IsCancellationRequested) isSearching = false;
        //}

        private static string[] GetTables(CancellationToken cancellationToken)
        {
            if (!Initalized) Initialize();
            if (!IsReadyToExecute && IsBroken || !CanBeOpened) return Array.Empty<string>();
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (cancellationToken.IsCancellationRequested) return Array.Empty<string>();
            var tables = new List<string>();
            try
            {
                var randomCommand = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'";
                var sqlCommand = new SQLiteCommand(randomCommand, _connection);
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                        tables.Add(reader.GetString(0));
                }
                UpdateProductRecordCount(_connection, cancellationToken);
                UpdateExtractionRecordCount(_connection, cancellationToken);
                return tables.ToArray();
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"An unexpected error occurred attempting to get table names. REASON: {e}");
            }
            return Array.Empty<string>();

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

        private static void InsertRecords(DPProductRecord pRecord, DPExtractionRecord eRecord, CancellationToken t)
        {
            // Trigger will update the product record's extraction record ID to the newly created record.
            string[] pColumns = new string[] { "Product Name", "Tags", "Author", "SKU", "Date Created", "Thumbnail Full Path",};
            string[] eColumns = new string[] { "Archive Name", "Files", "Folders", "Destination Path", "Errored Files", "Error Messages"};
            if (!Initalized) Initialize();
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return;

            pRecord.Deconstruct(out var productName, out var tags, out var author, out var sku, 
                                 out var time, out var thumbnailPath, out var __, out var _);
            eRecord.Deconstruct(out var archiveFileName, out var destPath, out var files, 
                out var erroredFiles, out var erroredMessages, out var folders, out _);
            // We do not care about UID.

            // Order must match pColumns / eColumns

            try
            {
                object[] pObjs = new object[] { productName, JoinString(", ", tags), author, sku, time.ToFileTimeUtc(), thumbnailPath };
                object[] eObjs = new object[] { archiveFileName, JoinString(", ", files),
                JoinString(", ", folders), destPath, JoinString(", ", erroredFiles),
                JoinString(", ", erroredMessages) };

                // If both operations are successful, emit signal.
                if (InsertValuesToTable("ProductRecords", pColumns, pObjs, t))
                {
                    if (eRecord != DPExtractionRecord.NULL_RECORD)
                    {
                        InsertTags(tags, t);
                        InsertValuesToTable("ExtractionRecords", eColumns, eObjs, t);
                    }
                    DatabaseUpdated?.Invoke();
                }
            }
            catch (Exception ex) { }
            
            

        }

        private static string JoinString(string seperator, params string[] values)
        {
            if (values == null || values.Length == 0) return null;

            StringBuilder builder = new StringBuilder(512);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null) continue;
                if (values[i].Trim() != string.Empty)
                {
                    builder.Append(values[i] + seperator);
                }
            }
            builder.Remove(builder.Length - 1 - seperator.Length, seperator.Length);
            return builder.ToString().Trim();
        }

        private static string[] CreateParams(ref string str, int length)
        {
            int maxDigits = (int)Math.Floor(Math.Log10(length)) + 1;
            StringBuilder sb = new StringBuilder((maxDigits + 4) * length);
            string[] args = new string[length];
            for (var i = 0; i < length; i++)
            {
                var rawArg = "@A" + i;
                var arg = i != length - 1 ? rawArg + ", " : rawArg;
                sb.Append(arg);
                args[i] = rawArg;
            }
            str += sb.ToString().Trim();
            return args;
        }

        private static string[] CreateParams(ref string str, int length, ref int start)
        {
            int maxDigits = (int)Math.Floor(Math.Log10(length + start)) + 1;
            StringBuilder sb = new StringBuilder((maxDigits + 4) * length);
            string[] args = new string[length];
            for (var i = 0; i < length; i++, start++)
            {
                var rawArg = "@A" + start;
                var arg = i != length - 1 ? rawArg + ", " : rawArg;
                sb.Append(arg);
                args[i] = rawArg;
            }
            str += sb.ToString().Trim();
            return args;
        }

        private static void FillParamsToConnection(ref SQLiteCommand command, string[] cArgs, params object[] values)
        {
            for (var i = 0; i < cArgs.Length; i++)
            {
                command.Parameters.Add(new SQLiteParameter(cArgs[i], values[i]));
            }
        }

        private static string ConvertParamsToString(params object[] args) {
            if (args.Length == 0) return string.Empty;
            string[] sArgs = new string[args.Length];
            for (var i = 0; i < args.Length; i++) {
                var arg = args[i];
                var type = arg.GetType();
                if (type == typeof(string)) sArgs[i] = '"' + (string) arg + '"';
                else if (type == typeof(char)) sArgs[i] = '"' + (string) arg + '"';
                else sArgs[i] = Convert.ToString(arg);
            }
            return string.Join(", ", sArgs);
        }

        private static void GetExtractionRecord(uint id, CancellationToken t) {
            if (!Initalized) Initialize();
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            WaitUntilDatabaseReady();
            if (t.IsCancellationRequested) return;

            var getCmd = $"SELECT * FROM ExtractionRecords WHERE ID = {id};";
            try {
                var cmd = new SQLiteCommand(getCmd, _connection);
                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        string[] files, folders, erroredFiles, errorMessages;
                        string archiveFileName = (string) reader["Archive Name"];
                        string filesStr = reader["Files"] as string;
                        string foldersStr = reader["Folders"] as string;
                        string destinationPath = (string) reader["Destination Path"];
                        string erroredFilesStr = reader["Errored Files"] as string;
                        string errorMessagesStr = reader["Error Messages"] as string;
                        uint pid = Convert.ToUInt32(reader["Product Record ID"]);
                        
                        files = filesStr != null ? files = filesStr.Split(", ") : Array.Empty<string>();
                        folders = foldersStr != null ? folders = foldersStr.Split(", ") : Array.Empty<string>();
                        erroredFiles = erroredFilesStr != null ? erroredFiles = erroredFilesStr.Split(", ") : Array.Empty<string>();
                        errorMessages = errorMessagesStr != null ? errorMessages = erroredFilesStr.Split(", ") : Array.Empty<string>();

                        var record = new DPExtractionRecord(archiveFileName, destinationPath, files, erroredFiles, errorMessages, folders, pid);
                        RecordQueryCompleted?.Invoke(record);
                        return;
                    }
                }
                DPCommon.WriteToLog("Failed to get extraction record possibly due to extraction record was deleted.");
                RecordQueryFailed?.Invoke();
            } catch (Exception ex) {
                DPCommon.WriteToLog("Failed to get extraction record.");
            }
        }

        private static uint GetLastProductID(CancellationToken t)
        {
            if (t.IsCancellationRequested) return 0;
            var c = "SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1;";
            try
            {
                using (var cmd = new SQLiteCommand(c, _connection))
                {
                    return Convert.ToUInt32(cmd.ExecuteScalar());
                }
            } catch (Exception ex)
            {
                DPCommon.WriteToLog("Failed to get last product ID.");
            }
            return 0;
        }

        private static void TruncateJournal()
        {
            if (!Initalized) Initialize();
            OpenConnection();
            var pragmaCheckpoint = "PRAGMA wal_checkpoint(TRUNCATE);";
            try
            {
                using (var cmd = new SQLiteCommand(pragmaCheckpoint, _connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { }
            finally
            {
                _connection.Shutdown();
                _connection.Close();
            }
            _connection.Dispose();

            // Now check if -wal and -shm are available.
            var shmFile = Path.GetFullPath(_expectedDatabasePath + "-shm");
            var walFile = Path.GetFullPath(_expectedDatabasePath + "-wal");

            // TODO: Doesn't delete.
            try
            {
                if (File.Exists(shmFile)) File.Delete(shmFile);
                if (File.Exists(walFile)) File.Delete(walFile);
            } catch (Exception ex) { }
            
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
