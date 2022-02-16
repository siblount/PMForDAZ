// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace DAZ_Installer.DP
{
    /// <summary>
    /// This class will handle all database operations such as initializing the database, creating tables, rows, deleting, etc.
    /// </summary>
    internal static class DPDatabase
    {
        // Internal
        internal static bool DatabaseExists { get; private set; } = false;
        internal static bool Initalized { get; private set; } = false;

        // Private
        private static SQLiteConnection _connection = new SQLiteConnection();
        private static string _connectionString { get; set; } = string.Empty;

        private static Queue<Action> _actionQueue = new Queue<Action>(1); // Queue of Action or Dictionary<Action<params>, params>
        private const int _databaseVersion = 1;

        // Cache :D
        private static Dictionary<string, string[]> _columnsCache = new();


        internal static void Initalize()
        {
            // Check if database exists.
            string expectedDatabasePath = Path.Join(DPSettings.databasePath, "db.db");
            DatabaseExists = File.Exists(expectedDatabasePath);
            if (!DatabaseExists)
            {
                // Create the database.
                CreateDatabase();
                // Update database info.
                InsertValueToTable("DatabaseInfo", new string[] {"Version"}, new ArrayList { 1 });
            } else
            {
                _connectionString = "Data Source = " + Path.GetFullPath(expectedDatabasePath);
                _connection.ConnectionString = _connectionString;
            }
            

            Initalized = true;

        }
        internal static void LoadDatabase()
        {
            
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                if (_connection.State == System.Data.ConnectionState.Broken) throw new Exception("Database is corrupted.");
                _actionQueue.Enqueue(LoadDatabase);
                OpenConnection(true);
            }

        }
        internal static void CloseConnection(bool forceClose = false)
        {
            if (_connection.State == ConnectionState.Executing && !forceClose)
            {
                _connection.StateChange += CloseOnStatusChange;
            } else
            {
                var task = _connection.CloseAsync();
                DPCommon.WriteToLog(task.Status);
                //task.Start(); // Not sure if it was already started.
            }
            
        }

        internal static void RefreshDatabase(bool forceRefresh = false)
        {
            // Call this function when the location of the database has been updated!
            CloseConnection(forceRefresh);

            // Clear all queues, this is top priority!
            _actionQueue.Clear();
            _actionQueue.Enqueue(() => Initalized = false);
            _actionQueue.Enqueue(new Action(Initalize));
            // Reset all event calls.

            //Initalized = false;
            //Initalize();
        }

        private static void InsertValueToTable(string tableName, IEnumerable<string> columns, IEnumerable values)
        {
            if (!Initalized) Initalize();
            if (_connection.State != ConnectionState.Open) _connection.Open();

            // Build columns.
            var columnsToAdd = string.Join(',', columns);
            var valuesToAdd = string.Join(',', values);
            var insertCommand = $"INSERT INTO {tableName} ({columnsToAdd})\nVALUES({valuesToAdd});";

            var sqlCommand = new SQLiteCommand(insertCommand, _connection);
            sqlCommand.ExecuteNonQueryAsync();

        }

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
        
        internal static void Search(string searchQuery)
        {
            var processedStr = "";
            var anyCaseStr = "SELECT * FROM ProductRecords WHERE * MATCH *";

            // TODO: Make sure strings are lowercased.
            // TODO: Make sure word itself is not *
        }

        #region Private methods

        private static void OpenConnection(bool addListener = false)
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                var task = _connection.OpenAsync();
                if (addListener) _connection.StateChange += OpenStatusEmitter;
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

            //var createCommand = new SQLiteCommand(createSequenceCommand, _connection);
            //var task = createCommand.ExecuteNonQueryAsync();
            var createCommand = new SQLiteCommand(createProductRecordsCommand, _connection);
            createCommand.ExecuteNonQueryAsync();
            createCommand = new SQLiteCommand(createExtractionRecordsCommand, _connection);
            createCommand.ExecuteNonQueryAsync();
            createCommand = new SQLiteCommand(createCachedSearchCommand, _connection);
            createCommand.ExecuteNonQueryAsync();
            createCommand = new SQLiteCommand(createDatabaseInfoCommand, _connection);
            createCommand.ExecuteNonQueryAsync();
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
        
        private static string GenerateSearchParamaters(string searchQuery)
        {
            // $[Name]:[query]
            // [$]\[\(?\w\]:\w

            string workingFinalString = string.Empty;
            List<string> workingStrings = new List<string>();
            Stack<string> conditionalStrings = new Stack<string>();
            HashSet<string> keywords = new HashSet<string> { "NOT", "AND", "OR" };
            bool use_raw = searchQuery.StartsWith(':') && searchQuery.EndsWith(':');

            string[] wordsSplit = searchQuery.Split(' ');

            if (use_raw) return searchQuery;

            
            
            for (int i = 0; i < wordsSplit.Length; i++)
            {
                var word = wordsSplit[i];
                
                // Find any special conditions first.
                if (word.EndsWith(')'))
                {
                    // ( must be the first character.
                    // Check to see if it is in this word.
                }

            }
            return "";
        }
        #endregion

        #region Handle Events
        /// <summary>
        /// Closes the connection at the appropriate time. Closes only when not executing a command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CloseOnStatusChange(object sender,System.Data.StateChangeEventArgs e)
        {
            if (_connection.State != System.Data.ConnectionState.Executing)
            {
                _connection.StateChange -= CloseOnStatusChange;
                CloseConnection();
            }
        }

        private static void OpenStatusEmitter(object sender, System.Data.StateChangeEventArgs e)
        {
            
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                while (_actionQueue.Count > 0)
                {
                    _actionQueue.Dequeue()(); // Call the function.
                    _connection.StateChange -= OpenStatusEmitter;
                }
            }
        }



        #endregion
    }
}
