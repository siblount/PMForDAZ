// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
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
        // TODO: Backup Database.
        // TODO: Tool to use backup database.
    {
        // Internal
        internal static bool DatabaseExists { get; private set; } = false;
        internal static bool Initalized { get; private set; } = false;

        // Private
        private static SQLiteConnection _connection = new SQLiteConnection();
        private static string _expectedDatabasePath { get; set; } = Path.Join(DPSettings.databasePath, "db.db");
        private static string _connectionString { get; set; } = string.Empty;

        // Main task manager...

        private static DPTaskManager _taskManager = new DPTaskManager();
        
        // Search task manager...

        private const byte DATABASE_VERSION = 2;

        // Cache :D
        // TODO: Limit cache to 5.
        // Might remove to keep low-memory profile.
        private static DPCache<string, string[]> _columnsCache = new();

        /// <summary>
        /// This function is called at Library initalization async'ly. It is also always called when exposed functions
        /// are called when the database has not been initalize.
        /// </summary>
        internal static void Initalize()
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
                InsertValueToTable("DatabaseInfo", new string[] {"Version"}, new object[] {DATABASE_VERSION});
            } else
            {
                _connectionString = "Data Source = " + Path.GetFullPath(_expectedDatabasePath);
                _connection.ConnectionString = _connectionString;
            }
            
            Initalized = true;
            

        }
        #region Queryable methods
        internal static void LoadDatabase()
        {
            
            if (_connection.State != ConnectionState.Open)
            {
                if (_connection.State == ConnectionState.Broken) throw new Exception("Database is corrupted.");
                _taskManager.AddToQueue(OpenConnection);
            }

        }

        /// <summary>
        /// If `forceClose` is false, the close action will be queued. Otherwise, the action queue will be cleared and database will be closed immediately.
        /// </summary>
        /// <param name="forceClose">Closes the database connection immediately if True, otherwise database closure is queued.</param>
        internal static void CloseConnection(bool forceClose = false)
        {
            if (!forceClose)
                _taskManager.AddToQueue(() => _connection.Close()); 
            else
                _connection.Close();
        }
        /// <summary>
        /// If `forceRefresh` is false, the refresh action will be queued. Otherwise, the action queue will be cleared and database will be refreshed immediately.
        /// </summary>
        /// <param name="forceRefresh">Refreshes immediately if True, otherwise it is queued.</param>
        internal static void RefreshDatabase(bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                _taskManager.Stop();
            } else
            {
                // TO DO: Refresh database code.
                _taskManager.AddToQueue(RefreshDatabase, false);
            }
            
        }

        private static void InsertValueToTable(string tableName, IEnumerable<string> columns, object[] values)
        {
            // TODO: Create a transcation in case of database failure.
            
            if (!Initalized) Initalize();
            if (_connection.State != ConnectionState.Open) _connection.Open();

            SQLiteTransaction transaction = _connection.BeginTransaction();
            // Build columns.
            var columnsToAdd = string.Join(',', columns);
            var valuesToAdd = string.Join(',', values);
            var insertCommand = $"INSERT INTO {tableName} ({columnsToAdd})\nVALUES({valuesToAdd});";
            try
            {
                var sqlCommand = new SQLiteCommand(insertCommand, _connection);
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to insert {valuesToAdd} to {columnsToAdd}. REASON: {ex.Message}");
                transaction.Rollback();
            }

        }

        // TO DO: Add to queue.
        internal static void GetAllValuesFromTable(string tableName)
        {
            if (!Initalized) Initalize();
            if (_connection.State != ConnectionState.Open) _connection.Open();

            var columns = GetColumns(tableName);
            var getCommand = $"SELECT * FROM {tableName}";
            var sqlCommand = new SQLiteCommand(getCommand, _connection);
            var reader = sqlCommand.ExecuteReader();
            var table = reader.GetSchemaTable();

            foreach (DataRow row in table.Rows) DPCommon.WriteToLog(row.ItemArray[0]); // row.ItemArray[0] = column name

            
        }
        
        // TO DO: Improve. This can be so much more efficient.
        // I lack the brain capacity to do this at the moment.
        /// <summary>
        /// Generates SQL command based on search query and returns a sorted list of products.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        internal static DPProductRecord[] Search(string searchQuery)
        {

            var getUserCommand = GetUserSearchQueryCommand(searchQuery);
            var tagSearchResults = SearchViaTags(getUserCommand);

            var getProductsCommand = GetProductsFromUserSearchCommand(tagSearchResults);
            return SearchProductRecordsViaTags(getProductsCommand);

        }

        #endregion

        #region Private methods

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
            if (_connection?.State != ConnectionState.Open || _connection?.State != ConnectionState.Connecting)
            {
                _connection.Open();
                return;
            }
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
            CreateTables(databasePath);
            CreateIndexes(databasePath);

            CloseConnection(true);
            //_connection.ReleaseMemory();
        }

        private static void CreateTables(string databasePath)
        {
            _connectionString = "Data Source = " + Path.GetFullPath(databasePath);
            if (_connectionString != _connection.ConnectionString)
                _connection.ConnectionString = _connectionString;
            _connection.Open();

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

        private static void CreateIndexes(string databasePath)
        {
            _connectionString = "Data Source = " + Path.GetFullPath(databasePath);
            if (_connectionString != _connection.ConnectionString)
                _connection.ConnectionString = _connectionString;
            if (_connection.State != ConnectionState.Open) _connection.Open();

            const string createTagToPIDCommand = @"
            CREATE UNIQUE INDEX ""idx_TagToPID"" ON ""Tags"" (
                ""Tag""   ASC
            )";

            const string createArchiveNameToEIDCommand = @"
            CREATE UNIQUE INDEX ""idx_ArchiveNameToEID"" ON ""ExtractionRecords"" (

                ""Archive Name"",
	            ""ID""
            )"; 

            var createCommand = new SQLiteCommand(createTagToPIDCommand, _connection);
            createCommand.ExecuteNonQueryAsync();
            createCommand = new SQLiteCommand(createArchiveNameToEIDCommand, _connection);
            createCommand.ExecuteNonQueryAsync();


        }
        
        private static string[] GetColumns(string tableName)
        {
            if (!Initalized) Initalize();
            if (_columnsCache.ContainsKey(tableName)) return _columnsCache[tableName];

            if (_connection.State != ConnectionState.Open) _connection.Open();

            var randomCommand = $"SELECT * FROM {tableName} LIMIT 1;";
            var sqlCommand = new SQLiteCommand(randomCommand, _connection);
            var reader = sqlCommand.ExecuteReader();
            var table = reader.GetSchemaTable();

            List<string> columns = new List<string>();
            foreach (DataRow row in table.Rows) columns.Add((string) row.ItemArray[0]);

            // Cache it.
            _columnsCache[tableName] = columns.ToArray();
            return _columnsCache[tableName];
        }

        private static string GetUserSearchQueryCommand(string searchQuery)
        {
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
                var tag = potTags[i];
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

        private static string GetProductsFromUserSearchCommand(uint[] ids)
        {
            // WARNING: ids should not have length 0!
            var baseStr = "SELECT * FROM ProductRecords WHERE";
            var op = " ID = ";
            int lastNumDigitCount = Convert.ToInt16(Math.Ceiling(Math.Log10(ids[ids.Length - 1] + 1)));
            var builder = new StringBuilder((op.Length * ids.Length) + ( ids.Length * lastNumDigitCount));

            foreach (var id in ids)
            {
                builder.Append(op + id);
            }

            baseStr += builder.ToString().TrimEnd();
            baseStr = baseStr.Substring(0, baseStr.Length - op.Length); // Remove last op.
            return baseStr;

        }

        private static uint[] SearchViaTags(string getUserCommand)
        {
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
        private static DPProductRecord[] SearchProductRecordsViaTags(string getProductsCommand)
        {
            var sqlCommand = new SQLiteCommand(getProductsCommand, _connection);
            var reader = sqlCommand.ExecuteReader();

            List<DPProductRecord> searchResults = new List<DPProductRecord>(reader.StepCount);
            string productName, author, thumbnailPath;
            string[] tags;
            uint extractionID, sku, pid;

            // TODO : Use new product search record.
            while (reader.Read())
            {
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
                    new DPProductRecord(productName, tags, null,
                    DateTime.Today, null, "", thumbnailPath, pid));
            }

            return searchResults.ToArray();
        }

        #endregion

        #region Handle Events
        /// <summary>
        /// Closes the connection at the appropriate time. Closes only when not executing a command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CloseOnStatusChange(object sender, StateChangeEventArgs e)
        {
            if (_connection.State != System.Data.ConnectionState.Executing)
            {
                _connection.StateChange -= CloseOnStatusChange;
                CloseConnection();
            }
        }



        #endregion
    }
}
