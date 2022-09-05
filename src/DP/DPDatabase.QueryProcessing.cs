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
        /// <param name="method">The sort method to perform.</param>
        /// <returns></returns>
        private static DPProductRecord[] DoSearchS(string searchQuery, DPSortMethod method, 
            SQLiteConnection c, CancellationToken t)
        {
            DPProductRecord[] results = Array.Empty<DPProductRecord>();
            try
            {
                using var connection = CreateAndOpenConnection(c, true);
                if (connection == null) return results;
                using var command = new SQLiteCommand(connection);
                SetupSQLSearchLikeQuery(searchQuery, true, method, command);
                // SetupSQLSearchQuery(searchQuery, method, command);
                results = SearchProductRecordsViaTagsS(command, t);
                UpdateProductRecordCount(connection, t);
                UpdateExtractionRecordCount(connection, t);
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred doing a regular search. REASON: {ex}");
            }
            return results;
        }

        /// <summary>
        /// Generates SQL command based on the regex and returns a sorted list of products.
        /// </summary>
        /// <param name="regex">The regex to perform from the user.</param>
        /// <returns></returns>
        private static DPProductRecord[] DoRegexSearchS(string regex, DPSortMethod method, 
            SQLiteConnection c, CancellationToken t)
        {
            var results = Array.Empty<DPProductRecord>();
            try
            {
                using (var connection = CreateAndOpenConnection(c, true))
                {
                    if (connection == null) return results;

                    var attribute = (SQLiteFunctionAttribute)typeof(SQLRegexFunction).GetCustomAttributes(typeof(SQLiteFunctionAttribute), true)[0];
                    connection.BindFunction(attribute, new SQLRegexFunction());
                    SpinWait.SpinUntil(() => connection.State != ConnectionState.Connecting
                                            || connection.State != ConnectionState.Executing
                                            || connection.State != ConnectionState.Fetching);
                    if (connection.State == ConnectionState.Broken) return results;
                    using var command = new SQLiteCommand(connection);
                    SetupSQLRegexQuery(regex, method, command);
                    results = SearchProductRecordsViaTagsS(command, t);
                    command.Dispose();
                    UpdateProductRecordCount(connection, t);
                    UpdateExtractionRecordCount(connection, t);
                }
            } catch (Exception e)
            {
                DPCommon.WriteToLog($"An error occurred with the regex search function. REASON: {e}");
            }
            return results;
        }
        /// <summary>
        /// Does an query for the library and emits the LibraryQueryCompleted event.
        /// </summary>
        /// <param name="limit">The limit amount of results to return.</param>
        /// <param name="method">The sorting method to apply to query results.</param>
        private static DPProductRecord[] DoLibraryQuery(uint page, uint limit, DPSortMethod method, 
            SQLiteConnection c, CancellationToken t)
        {
            var results = Array.Empty<DPProductRecord>();
            try
            {
                using (var _connection = CreateAndOpenConnection(c, true))
                {
                    if (_connection == null) return results;
                    using var command = new SQLiteCommand(_connection);
                    SetupSQLLibraryQuery(page, limit, method, command);
                    results = SearchProductRecordsViaTagsS(command, t);
                    UpdateProductRecordCount(_connection, t);
                    UpdateExtractionRecordCount(_connection, t);
                }
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An error occurred with the search function. {ex}");
            }
            return results;
        }

        /// <summary>
        /// Creates and sets up the SQLiteCommand for a regex search query and the sorting method.
        /// </summary>
        /// <param name="regex">The regex to perform. Cannot be null.</param>
        /// <param name="method">The sorting method to use for search results. Cannot be null.</param>
        /// <param name="command">The command to set up the query for. Cannot be null.</param>
        private static void SetupSQLRegexQuery(string regex, DPSortMethod method, SQLiteCommand command)
        {
            string sqlQuery = @"SELECT DISTINCT * FROM ProductRecords WHERE ID IN (SELECT ""Product Record ID"" FROM Tags WHERE Tag REGEXP @A";

            switch (method)
            {
                case DPSortMethod.Alphabetical:
                    sqlQuery += @") ORDER BY ""Product Name"" COLLATE NOCASE ASC;";
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
        /// <summary>
        /// Creates and sets up the SQLiteCommand for a library query (for switching pages).
        /// </summary>
        /// <param name="page">The page to get product records from.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="method">The sorting method to use for search results. Cannot be null.</param>
        /// <param name="command">The command to set up the query for. Cannot be null.</param>
        private static void SetupSQLLibraryQuery(uint page, uint limit, DPSortMethod method, SQLiteCommand command)
        {
            uint beginningRowID = (page - 1) * limit;
            string sqlQuery = $"SELECT * FROM ProductRecords ";

            switch (method)
            {
                case DPSortMethod.Alphabetical:
                    sqlQuery += @"ORDER BY ""Product Name"" COLLATE NOCASE DESC ";
                    break;
                case DPSortMethod.Date:
                    sqlQuery += @"ORDER BY ""Date Created"" ASC ";
                    break;
            }

            sqlQuery += limit == 0 ? $"LIMIT -1 OFFSET {beginningRowID};" : $"LIMIT {limit} OFFSET {beginningRowID};";

            command.CommandText = sqlQuery;
        }
        /// <summary>
        /// Creates and sets up the SQLiteCommand for a user search query and the sorting method.
        /// </summary>
        /// <param name="userQuery">The user search query to process.</param>
        /// <param name="method">The sorting method to use for search results. Cannot be null.</param>
        /// <param name="command">The command to set up the query for. Cannot be null.</param>
        private static void SetupSQLSearchQuery(string userQuery, DPSortMethod method, SQLiteCommand command)
        {
            string[] tokens = userQuery.Split(' ');
            string sqlQuery = @"SELECT DISTINCT * FROM ProductRecords WHERE ID IN (SELECT ""Product Record ID"" FROM Tags WHERE Tag IN (";
            StringBuilder sb = new StringBuilder(((int)Math.Floor(Math.Log10(tokens.Length)) + 1) * tokens.Length + (4 * tokens.Length));
            for (int i = 0; i < tokens.Length; i++)
            {
                sb.Append(i == tokens.Length - 1 ? "@A" + i :
                                                    "@A" + i + ", ");
            }
            sqlQuery += sb.ToString();

            switch (method)
            {
                case DPSortMethod.Alphabetical:
                    sqlQuery += @")) ORDER BY ""Product Name"" COLLATE NOCASE ASC;";
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
        /// <summary>
        /// Creates and sets up the SQLiteCommand for a user search query and the sorting method. Compared to <see cref="SetupSQLSearchQuery"/>,
        /// this function uses the LIKE search to find product records. LIKE Sql Calls are like this: "something%" by default.
        /// The both sides boolean paratemer will make it so the wildcard is on both sides, like this: "%something%" but will be 
        /// significantly slower due to inability to use index./>
        /// </summary>
        /// <param name="userQuery">The user search query to process.</param>
        /// <param name="bothSides">Whether or not to use wildcards on both sides of search query.</param>
        /// <param name="method">The sorting method to use for search results. Cannot be null.</param>
        /// <param name="command">The command to set up the query for. Cannot be null.</param>
        private static void SetupSQLSearchLikeQuery(string userQuery, bool bothSides, DPSortMethod method, SQLiteCommand command)
        {
            string[] tokens = userQuery.Split(' ');
            string sqlQuery = @"SELECT DISTINCT * FROM ProductRecords WHERE ID IN (SELECT ""Product Record ID"" FROM Tags WHERE Tag LIKE ";
            StringBuilder sb = new StringBuilder(((int)Math.Floor(Math.Log10(tokens.Length)) + 1) * tokens.Length + (14 * tokens.Length));
            for (int i = 0; i < tokens.Length; i++)
            {   
                if (bothSides)
                    sb.Append(i == tokens.Length - 1 ? "@A" + i :
                                                        "@A" + i + " OR TAG LIKE ");
                else
                    sb.Append(i == tokens.Length - 1 ? "@A" + i :
                                                        "@A" + i + " OR TAG LIKE ");
            }
            sqlQuery += sb.ToString();

        switch (method)
            {
                case DPSortMethod.Alphabetical:
                    sqlQuery += @") ORDER BY ""Product Name"" COLLATE NOCASE ASC;";
                    break;
                case DPSortMethod.Date:
                    sqlQuery += @") ORDER BY ""Date Created"" ASC;";
                    break;
                case DPSortMethod.Relevance:
                    sqlQuery += @" GROUP BY ""Product Record ID"" ORDER BY COUNT(*) DESC);";
                    break;
                default:
                    sqlQuery += ");";
                    break;
            }

            command.CommandText = sqlQuery;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (bothSides)
                    command.Parameters.Add(new SQLiteParameter("@A" + i, '%' + tokens[i] + '%'));
                else
                    command.Parameters.Add(new SQLiteParameter("@A" + i, '%' + tokens[i]));
            }

        }
    }
}
