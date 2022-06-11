// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Data.SQLite;
using System.IO;
using DAZ_Installer.External;

namespace DAZ_Installer.DP
{
    public static partial class DPDatabase
    {
        /// <summary>
        /// Generates SQL command based on search query and returns a sorted list of products.
        /// </summary>
        /// <param name="searchQuery">The raw search query from the user.</param>
        /// <returns></returns>
        private static void DoSearchS(string searchQuery, DPSortMethod method, CancellationToken t)
        {
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
                var attribute = (SQLiteFunctionAttribute)typeof(SQLRegexFunction).GetCustomAttributes(typeof(SQLiteFunctionAttribute), true)[0];
                _connection.Open();
                _connection.BindFunction(attribute, new SQLRegexFunction());
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


        private static void SetupSQLRegexQuery(string regex, DPSortMethod method, ref SQLiteCommand command)
        {
            string sqlQuery = @"SELECT * FROM ProductRecords WHERE ID IN (SELECT ""Product Record ID"" FROM Tags WHERE Tag REGEXP @A";

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
    }
}
