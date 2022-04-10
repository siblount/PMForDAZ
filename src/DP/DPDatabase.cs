// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
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
    internal static class DPDatabase
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
        // Internal
        internal static bool DatabaseExists { get; private set; } = false;
        internal static bool Initalized { get; private set; } = false;

        // Database State
        internal static bool IsReadyToExecute => _connection.State == ConnectionState.Open && !isSearching;
        internal static bool IsBroken => _connection.State == ConnectionState.Broken;
        internal static bool CanBeOpened
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

        // Search
        internal static DPSearchRecord[] results;

        // Events
        internal static event Action SearchUpdated;
        internal static event Action DatabaseUpdated;

        // Private
        private static SQLiteConnection _connection = new SQLiteConnection();
        private static string _expectedDatabasePath { get; set; } = Path.Join(DPSettings.databasePath, "db.db");
        private static string _connectionString { get; set; } = string.Empty;

        // Main task manager...

        private static DPTaskManager _mainTaskManager = new DPTaskManager();

        private static DPTaskManager _searchTaskManager = new DPTaskManager();

        // Task state.
        private static bool isSearching = false;
        private static byte nonSearchTaskIsAboutToExecute = 0;
        private const byte DATABASE_VERSION = 2;

        // Cache :D
        // TODO: Limit cache to 5.
        // Might remove to keep low-memory profile.
        private readonly static DPCache<string, string[]> _columnsCache = new();


        #region Queryable methods

        internal static void InitializeQ()
        {
            if (!Initalized)
                _mainTaskManager.AddToQueue(Initialize);
        }

        /// <summary>
        /// If `forceClose` is false, the close action will be queued. Otherwise, the action queue will be cleared and database will be closed immediately.
        /// </summary>
        /// <param name="forceClose">Closes the database connection immediately if True, otherwise database closure is queued.</param>
        internal static void CloseConnectionQ(bool forceClose = false)
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
        internal static void RefreshDatabaseQ(bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                _mainTaskManager.Stop();
                Initalized = false;
                InitializeQ();
                _columnsCache.Clear();
            } else
            {
                _mainTaskManager.AddToQueue(RefreshDatabase);
            }
            
        }

        // TO DO: Improve. This can be so much more efficient.
        // I lack the brain capacity to do this at the moment.
        internal static void Search(string searchQuery)
        {
            _searchTaskManager.Stop();
            _searchTaskManager.AddToQueue(DoSearchS, searchQuery);
        }

        internal static void CancelDatabaseOperations() => _mainTaskManager.Stop();

        #endregion

        #region Private methods
        private static void Initialize()
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
                InsertValuesToTable("DatabaseInfo", new string[] { "Version" }, new object[] { DATABASE_VERSION }, CancellationToken.None);
            }
            else
            {
                UpdateConnectionString();
            }
            Initalized = true;
            DatabaseUpdated?.Invoke();
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

            // Create tables.
            CreateTables();
            CreateIndexes();

            CloseConnectionQ(true);
            //_connection.ReleaseMemory();
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
	            ""SKU""   INTEGER,
                ""Extraction Record ID""  INTEGER NOT NULL UNIQUE,
                ""Thumbnail Full Path""	TEXT,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            ); ";
            const string createExtractionRecordsCommand = @"
            CREATE TABLE ""ExtractionRecords"" (

                ""ID""    INTEGER NOT NULL UNIQUE,

                ""Folders""   TEXT,
	            ""Files"" TEXT,
	            ""Extraction Date""   INTEGER,
	            ""Destination Path""  TEXT,
	            ""Error Count""   INTEGER,
	            ""Warning Count"" INTEGER,
	            ""Error Messages""    TEXT,
	            ""Warning Messages""  TEXT,
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
            const string createDatabaseInfoCommand = @"
            CREATE TABLE ""DatabaseInfo"" (

                ""Version""   INTEGER NOT NULL
            )";
            const string createTagsCommand = @"
                CREATE TABLE ""Tags""(
                ""Tag""   TEXT NOT NULL,
                ""Product Record ID""	TEXT NOT NULL,
                PRIMARY KEY(""Tag"")
            )";
            //var createCommand = new SQLiteCommand(createSequenceCommand, _connection);
            //var task = createCommand.ExecuteNonQueryAsync();
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
            CREATE UNIQUE INDEX ""idx_TagToPID"" ON ""Tags"" (
                ""Tag""   ASC
            )";

            const string createArchiveNameToEIDCommand = @"
            CREATE UNIQUE INDEX ""idx_ArchiveNameToEID"" ON ""ExtractionRecords"" (

                ""Archive Name"",
	            ""ID""
            )";

            try
            {
                var createCommand = new SQLiteCommand(createTagToPIDCommand, _connection);
                createCommand.ExecuteNonQueryAsync();
                createCommand = new SQLiteCommand(createArchiveNameToEIDCommand, _connection);
                createCommand.ExecuteNonQueryAsync();
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred creating indexes. REASON: {ex}");
                return false;
            }
            return true;
        }
        
        private static string[] GetColumns(string tableName, CancellationToken t)
        {
            if (t.IsCancellationRequested || tableName.Length == 0) return Array.Empty<string>();
            nonSearchTaskIsAboutToExecute++;
            WaitUntilDatabaseReady();
            if (!Initalized) InitializeQ();
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
                nonSearchTaskIsAboutToExecute--;
            }
            return Array.Empty<string>();
        }

        private static string GetUserSearchQueryCommandS(string searchQuery, CancellationToken t)
        {
            if (t.IsCancellationRequested || searchQuery.Length == 0) return string.Empty;
            // $[Name]:[query]
            // [$]\[\(?\w\]:\w
            var anyCaseStr = "SELECT * FROM Tags WHERE Tag";
            var ur = "OR";
            bool addWildcard = true;
            var potTags = searchQuery.Split(' ');
            StringBuilder whereBuilder =
                new StringBuilder(searchQuery.Length + (searchQuery.Length - potTags.Length) * 8);
            for (var i = 0; i < potTags.Length; i++)
            {
                if (t.IsCancellationRequested) return string.Empty;
                var tag = EscapeTag(potTags[i]);
                switch (tag)
                {
                    case "|AND|":
                        ur = "AND";
                        break;
                    case "|OR|":
                        ur = "OR";
                        break;
                    case "|EQUAL|":
                        addWildcard = false;
                        break;
                    default:
                        if (addWildcard) whereBuilder.Append(" LIKE \"" + tag + "%\" ESCAPE \"\\\" " + ur);
                        else whereBuilder.Append(" LIKE \"" + tag + "%\" " + ur);
                        break;
                }
            }
            anyCaseStr += whereBuilder.ToString().TrimEnd();
            anyCaseStr = anyCaseStr.Substring(0, anyCaseStr.Length - ur.Length - 1); // Remove last OR/AND.

            return anyCaseStr;
        }

        private static string GetProductsFromUserSearchCommandS(uint[] ids, CancellationToken t)
        {
            if (t.IsCancellationRequested || ids.Length == 0) return string.Empty;
            // WARNING: ids should not have length 0!
            var baseStr = "SELECT * FROM ProductRecords WHERE";
            var op = " ID = ";
            int lastNumDigitCount = Convert.ToInt16(Math.Ceiling(Math.Log10(ids[ids.Length - 1] + 1)));
            var builder = new StringBuilder((op.Length * ids.Length) + ( ids.Length * lastNumDigitCount));

            foreach (var id in ids)
            {
                if (t.IsCancellationRequested || ids.Length == 0) return string.Empty;
                builder.Append(op + id);
            }

            baseStr += builder.ToString().TrimEnd();
            baseStr = baseStr.Substring(0, baseStr.Length - op.Length); // Remove last op.
            return baseStr;

        }

        private static bool InsertValuesToTable(string tableName, IEnumerable<string> columns, object[] values, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            // TODO: Create a transaction in case of database failure.
            if (IsBroken || (!IsReadyToExecute && !CanBeOpened)) return false;
            if (!IsReadyToExecute && CanBeOpened) OpenConnection();
            
            SQLiteTransaction transaction = _connection.BeginTransaction();
            // Build columns.
            var columnsToAdd = string.Join(',', columns);
            var valuesToAdd = string.Join(',', values);
            var insertCommand = $"INSERT INTO {tableName} ({columnsToAdd})\nVALUES({valuesToAdd});";
            try
            {
                var sqlCommand = new SQLiteCommand(insertCommand, _connection, transaction);
                WaitUntilDatabaseReady();
                nonSearchTaskIsAboutToExecute++;
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                DatabaseUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to insert {valuesToAdd} to {columnsToAdd}. REASON: {ex.Message}");
                transaction.Rollback();
                return false;
            }
            nonSearchTaskIsAboutToExecute--;
            return true;

        }
        private static uint[] SearchViaTagsS(string getUserCommand, CancellationToken t)
        {
            if (t.IsCancellationRequested) {
                return Array.Empty<uint>();
            }
            // TODO: Make all of this a private function.
            var sqlCommand = new SQLiteCommand(getUserCommand, _connection);
            // Here create a command, and try REGEXP, for example
            // SELECT * FROM "table" WHERE "column" REGEXP '(?i)\btest\b'
            // looks for the word 'test', case-insensitive in a string column
            // example SQL: SELECT* FROM Foo WHERE Foo.Name REGEXP '$bar'
            var reader = sqlCommand.ExecuteReader();
            var IDtoCountMap = new SortedDictionary<uint, uint>();
            // Sort by value.
            // Get the product IDs
            // For each product ID in the ID column, add it to the dictionary for sorting.
            while (reader.Read())
            {
                // Get the product IDs in the column.
                var PDIDs = ((string)reader.GetValue(1)).Split(',');
                // For every product ID...
                foreach (var id in PDIDs)
                {
                    // Convert it into an unsigned integer.
                    uint n_id = uint.Parse(id);
                    // If it exists, increment the count for sorting.
                    if (IDtoCountMap.ContainsKey(n_id)) IDtoCountMap[n_id]++;
                    // Else add it to dictionary and setting the count to 1.
                    else IDtoCountMap[n_id] = 1;
                }
                if (t.IsCancellationRequested) {
                    return Array.Empty<uint>();
                }
            }
            // Now sort by relevance.
            var ids = IDtoCountMap.ToList();
            ids.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            // Now let's return only the ids (not the count and the class).
            var raw_ids = new uint[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                raw_ids[i] = ids[i].Value;
            }
            return raw_ids;
        }
        private static DPSearchRecord[] SearchProductRecordsViaTagsS(string getProductsCommand, CancellationToken t)
        {
            if (t.IsCancellationRequested) {
                return Array.Empty<DPSearchRecord>();
            }
            var sqlCommand = new SQLiteCommand(getProductsCommand, _connection);
            var reader = sqlCommand.ExecuteReader();

            var searchResults = new List<DPSearchRecord>(reader.StepCount);
            string productName, author, thumbnailPath;
            string[] tags;
            uint extractionID, sku, pid;

            // TODO : Use new product search record.
            while (reader.Read())
            {
                if (t.IsCancellationRequested) {
                    return Array.Empty<DPSearchRecord>();
                }
                // Construct product records
                // NULL values return type DB.NULL.
                productName = (string)reader["Product Name"];
                tags = ((string)reader["Tags"]).Trim().Split(','); // May return null but never does.
                author = reader["Author"] as string; // May return NULL
                thumbnailPath = reader["Thumbnail Full Path"] as string; // May return NULL
                extractionID = Convert.ToUInt32(reader["Extraction Record ID"]);
                sku = reader["SKU"] is DBNull ? 0 : Convert.ToUInt32(reader["SKU"]); // May return NULL - cannot have uint as null.
                pid = Convert.ToUInt32(reader["ID"]);
                searchResults.Add(
                    new DPSearchRecord(pid, productName, tags, author, 
                                        sku, extractionID, thumbnailPath));
                
            }

            return searchResults.ToArray();
        }
        // TO DO: Add to queue.
        private static string[][] GetAllValuesFromTable(string tableName, out string[] headers, CancellationToken t)
        {
            headers = Array.Empty<string>();
            if (!Initalized) Initialize();
            if (_connection.State != ConnectionState.Open) _connection.Open();
            if (t.IsCancellationRequested) {
                return Array.Empty<string[]>();
            }
            WaitUntilDatabaseReady();
            nonSearchTaskIsAboutToExecute++;
            try {
                headers = GetColumns(tableName, t);
                var getCommand = $"SELECT * FROM {tableName}";
                var sqlCommand = new SQLiteCommand(getCommand, _connection);
                var reader = sqlCommand.ExecuteReader();

                if (headers.Length == 0) return Array.Empty<string[]>();

                List<string[]> values = new List<string[]>(5);
                while (reader.Read())
                {
                    string[] arr = new string[headers.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = Convert.ToString(reader.GetValue(i));
                    }
                    values.Add(arr);
                }
                return values.ToArray();
            } catch (Exception e) {
                DPCommon.WriteToLog($"Unexpected error occured while trying to get all the values from table: {tableName}. REASON: {e}");
            } finally {
                nonSearchTaskIsAboutToExecute--;
            }
            return Array.Empty<string[]>();
        }

        private static string EscapeTag(string tag) => tag.Replace("%", "\\%").Replace("_", "\\_");

        // TO DO: Refresh database code.
        private static void RefreshDatabase(CancellationToken t) {
            if (t.IsCancellationRequested) return;
            nonSearchTaskIsAboutToExecute++;
            WaitUntilDatabaseReady();
            try {
                _mainTaskManager.Stop();
                _searchTaskManager.Stop();
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
        private static void DoSearchS(string searchQuery, CancellationToken t) {
            // User initialized another search while an old search hasn't finished completing.
            if (isSearching) {
                _searchTaskManager.Stop();
            }
            isSearching = true;
            try {
                SpinWait.SpinUntil(() => nonSearchTaskIsAboutToExecute == 0, 60 * 10000);
            } catch (Exception e) {
                DPCommon.WriteToLog("Search timedout due to previous tasks.");
            }
            if (!Initalized) Initialize();
            if (IsBroken) {
                DPCommon.WriteToLog("Search cannot proceed due to database being broken.");
                results = Array.Empty<DPSearchRecord>();
                SearchUpdated?.Invoke();
            }
            OpenConnection();
            try {
                var getUserCommand = GetUserSearchQueryCommandS(searchQuery, t);
                var tagSearchResults = SearchViaTagsS(getUserCommand, t);
                var getProductsCommand = GetProductsFromUserSearchCommandS(tagSearchResults, t);
                results = SearchProductRecordsViaTagsS(getProductsCommand, t);
                SearchUpdated?.Invoke();
            } catch (Exception e) {
                DPCommon.WriteToLog("An error occurred with the search function.");
            }

            if (!t.IsCancellationRequested) isSearching = false;
        }
        
        private static void BackupDatabase(CancellationToken t) {
            return;
        }

        private static void RestoreDatabase(CancellationToken t) {
            return;
        }


        #endregion
    }
}
