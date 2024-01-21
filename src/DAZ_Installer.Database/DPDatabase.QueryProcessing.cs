// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using DAZ_Installer.Database.External;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Text;

namespace DAZ_Installer.Database
{
    public partial class DPDatabase
    {
        /// <summary>
        /// Generates SQL command based on search query and returns a sorted list of products.
        /// </summary>
        /// <param name="searchQuery">The raw search query from the user.</param>
        /// <param name="method">The sort method to perform.</param>
        /// <param name="c">The SqliteConnection to use. If null, a new connection will be created.</param>
        /// <param name="t">The cancellation token to use. Use <see cref="CancellationToken.None"/> if you never wish to cancel.</param>
        /// <returns></returns>
        private List<DPProductRecordLite> DoSearchS(string searchQuery, DPSortMethod method,
            SqliteConnection? c, CancellationToken t)
        {
            SqliteConnection? connection = null;
            SqliteCommand? command = null;
            try
            {
                connection = CreateAndOpenConnection(c, true);
                if (connection == null) return new List<DPProductRecordLite>(0);
                command = new(string.Empty, connection);
                SetupSearch(searchQuery, method, command);
                var results = SearchProductRecords(command, t);
                UpdateProductRecordCount(connection, t);
                return results;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred doing a regular search.");
            } finally
            {
                if (c is null)
                {
                    command?.Dispose();
                    connection?.Dispose();
                }
            }
            return new List<DPProductRecordLite>(0);
        }

        /// <summary>
        /// Does an query for the library and emits the LibraryQueryCompleted event.
        /// </summary>
        /// <param name="limit">The limit amount of results to return.</param>
        /// <param name="method">The sorting method to apply to query results.</param>
        private List<DPProductRecordLite> DoLibraryQuery(uint page, ulong limit, DPSortMethod method,
            SqliteConnection? c, CancellationToken t)
        {
            SqliteConnection? connection = null;
            SqliteCommand? command = null;
            try
            {
                connection = CreateAndOpenConnection(c, true);
                if (connection == null) return new List<DPProductRecordLite>(0);
                command = new(string.Empty, connection);
                SetupSQLLibraryQuery(page, limit, method, command);
                var results = SearchProductRecords(command, t);
                UpdateProductRecordCount(connection, t);
                return results;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred doing a library query.");
            } finally
            {
                if (c is null)
                {
                    command?.Dispose();
                    connection?.Dispose();
                }
            }
            return new List<DPProductRecordLite>(0);
        }

        /// <summary>
        /// Creates and sets up the SqliteCommand for a library query (for switching pages).
        /// </summary>
        /// <param name="page">The page to get product records from.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="method">The sorting method to use for search results. Cannot be null.</param>
        /// <param name="command">The command to set up the query for. Cannot be null.</param>
        private void SetupSQLLibraryQuery(uint page, ulong limit, DPSortMethod method, SqliteCommand command)
        {
            var beginningRowID = (page - 1) * limit;
            command.CommandText = method switch
            {
                DPSortMethod.Alphabetical => "SELECT * FROM ProductsLite_Alphabetical ",
                DPSortMethod.Date => "SELECT * FROM ProductsLite_Date ",
                _ => "SELECT * FROM ProductsLite ",
            };
            command.CommandText += limit == 0 ? $"LIMIT -1 OFFSET {beginningRowID};" : $"LIMIT {limit} OFFSET {beginningRowID};";
        }

        /// <summary>
        /// Creates and sets up the SqliteCommand for a user search query and the sorting method. Compared to <see cref="SetupSQLSearchQuery"/>,
        /// this function uses the LIKE search to find product records. LIKE Sql Calls are like this: "something%" by default.
        /// The both sides boolean paratemer will make it so the wildcard is on both sides, like this: "%something%" but will be 
        /// significantly slower due to inability to use index./>
        /// </summary>
        /// <param name="userQuery">The user search query to process.</param>
        /// <param name="method">The sorting method to use for search results. Cannot be null.</param>
        /// <param name="command">The command to set up the query for. Cannot be null.</param>
        private void SetupSearch(string userQuery, DPSortMethod method, SqliteCommand command)
        {
            // SELECT * FROM ProductsLite p WHERE p.ROWID IN (SELECT ROWID FROM ProductsFTS5 WHERE Tags MATCH "Genesis" ORDER BY rank LIMIT 25);
            StringBuilder sb = new(userQuery + 50);
            if (method == DPSortMethod.Relevance)
                sb.AppendFormat("SELECT * FROM ProductsLite p WHERE p.ROWID IN (SELECT ROWID FROM ProductsFTS5 WHERE Tags MATCH \"{0}\" ORDER BY rank);", userQuery);

            switch (method)
            {
                case DPSortMethod.Alphabetical:
                    sb.AppendFormat("SELECT * FROM ProductsLite_Alphabetical p WHERE p.ROWID IN (SELECT ROWID FROM ProductsFTS5 WHERE Tags MATCH \"{0}\");", userQuery);
                    break;
                case DPSortMethod.Date:
                    sb.AppendFormat("SELECT * FROM ProductsLite_Date p WHERE p.ROWID IN (SELECT ROWID FROM ProductsFTS5 WHERE Tags MATCH \"{0}\");", userQuery);
                    break;
                case DPSortMethod.Relevance:
                    sb.AppendFormat("SELECT * FROM ProductsLite p WHERE p.ROWID IN (SELECT ROWID FROM ProductsFTS5 WHERE Tags MATCH \"{0}\" ORDER BY rank);", userQuery);
                    break;
                default:
                    sb.AppendFormat("SELECT * FROM ProductsLite p WHERE p.ROWID IN (SELECT ROWID FROM ProductsFTS5 WHERE Tags MATCH \"{0}\");", userQuery);
                    break;
            }

            command.CommandText = sb.ToString();

        }
    }
}
