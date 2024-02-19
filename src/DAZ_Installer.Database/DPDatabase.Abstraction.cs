// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using Serilog;
using System.Data;
using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.Text;

namespace DAZ_Installer.Database
{
    public partial class DPDatabase
    {
        #region Reads
        /// <summary>
        /// Updates the <c>ProductRecordCount</c> property.
        /// </summary>
        /// <param name="opts">The SqliteConnectionOpts to use.</param>
        private void UpdateProductRecordCount(SqliteConnectionOpts opts)
        {
            const string getCmd = $@"SELECT ""Product Record Count"" FROM {DatabaseInfoTable};";
            if (opts.IsCancellationRequested) return;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts, true);
                using var cmd = opts.CreateCommand(getCmd);
                ProductRecordCount = Convert.ToUInt64(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                Logger.ForContext<DPDatabase>().Error(e, "An unexpected error occurred while attempting to get product record count");
            }
        }

        /// <summary>
        /// Executes the reader to search for product records via tags. This only executes the reader and returns an array
        /// of product records.
        /// </summary>
        /// <param name="command">The command that is ready to execute. Cannot be null.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>A list of <see cref="DPProductRecordLite"/>s or an empty list if cancelled.</returns>
        /// <exception cref="Exception"/>
        private List<DPProductRecordLite> SearchProductRecords(DbDataReader reader, SqliteConnectionOpts opts)
        {
            List<DPProductRecordLite> searchResults = new(25);
            if (opts.IsCancellationRequested) return new List<DPProductRecordLite>(0);
            while (reader.Read())
            {
                if (opts.IsCancellationRequested) return new List<DPProductRecordLite>(0);
                var record = new DPProductRecordLite(
                    reader.GetString("Name"),
                    reader.IsDBNull("Thumbnail") ? null : reader.GetString("Thumbnail"),
                    reader.GetString("Tags").Split(", "),
                    reader.GetInt64("PID")
                );
                searchResults.Add(record);
            }
            return searchResults;
        }

        /// <summary>
        /// Returns an array of columns for the table name specified.
        /// </summary>
        /// <param name="tableName">The table to get columns from.</param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>The columns of the table, or an empty array if cancelled or an error occurred.</returns>
        private string[] GetColumns(string tableName, SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested || string.IsNullOrEmpty(tableName)) return Array.Empty<string>();
            try
            {
                using var connection = CreateAndOpenConnection(ref opts, true);

                var randomCommand = $"SELECT * FROM {tableName} LIMIT 1;";
                using var sqlCommand = opts.CreateCommand(randomCommand);
                using var reader = sqlCommand.ExecuteReader();
                DataTable table = reader.GetSchemaTable();

                List<string> columns = new();
                foreach (DataRow row in table.Rows)
                {
                    if (opts.IsCancellationRequested) return Array.Empty<string>();
                    columns.Add((string)row.ItemArray[0]);
                }
                return columns.ToArray();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get columns for table: {tableName}.");
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Returns all an array of all of the tables in the database.
        /// </summary>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="cancellationToken">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private string[] GetTables(SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return Array.Empty<string>();
            List<string> tables = new();

            try
            {
                using var connection = CreateAndOpenConnection(ref opts, true);
                if (connection == null) return Array.Empty<string>();
                var randomCommand = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'";
                using var sqlCommand = opts.CreateCommand(randomCommand);

                using var reader = sqlCommand.ExecuteReader();
                {
                    while (reader.Read())
                        tables.Add(reader.GetString(0));
                }
                UpdateProductRecordCount(opts);
                return tables.ToArray();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get table names");
            }
            return Array.Empty<string>();

        }

        /// <summary>
        /// Returns a unique list of archive file names that have been successfully extracted. It returns a hashset
        /// which may be null if it fails to create & open a connection, and execute the reader. Otherwise, it may
        /// return an empty hashset indicating there was nothing there.
        /// </summary>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>A hashset containing successfully extracted archive file names.</returns>
        // TODO: Deprecate this. As more products are installed, the hashset will grow and grow. This is not good.
        // Just query the database to check if the archive exists.
        private bool ArchiveNameExists(string arcName, SqliteConnectionOpts opts)
        {
            var getCmd = $@"SELECT EXISTS(SELECT 1 FROM {ProductTable} WHERE ""ArcName"" = @A LIMIT 1);";
            try
            {
                using var _connection = CreateAndOpenConnection(ref opts, true);
                using var cmd = opts.CreateCommand(getCmd);
                if (_connection == null) return false;
                cmd.Parameters.Add(new SqliteParameter("@A", arcName));
                return Convert.ToByte(cmd.ExecuteScalar()) == 1;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get archive file name list");
            }
            return false;
        }

        /// <summary>
        /// Returns the last product ID which indicates the latest product record added to the database.
        /// It may return 0 if an error occurred (or if there are no product records in the database).
        /// </summary>
        /// <param name="conn">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>The last product record ID in the database.</returns>
        private long GetLastProductID(SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return 0;
            var c = $@"SELECT ROWID FROM {ProductTable} ORDER BY ROWID DESC LIMIT 1;";
            try
            {
                using var connection = CreateAndOpenConnection(ref opts, true);
                using var cmd = opts.CreateCommand(c);
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get last product ID");
            }
            return 0;
        }
        /// <summary>
        /// Returns all the rows from a table in the database. This may return an empty dataset
        /// if there was an error connecting to the database. Additionally, the dataset may be 
        /// empty indicating there was an issue internally or that there was nothing in the table.
        /// </summary>
        /// <param name="tableName">The table to get all rows from.</param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="token">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>A dataset containing all of the values from the table specified. May return null.</returns>
        private DataSet? GetAllValuesFromTable(string tableName, SqliteConnectionOpts opts)
        {
            DataSet? dataset = null;
            DPDatabaseDataAdapter? adapter = null;
            if (opts.IsCancellationRequested) return dataset;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts, true);
                using var sqlCommand = opts.CreateCommand($"SELECT * FROM {tableName}");
                adapter = new(sqlCommand);
                dataset = new DataSet(tableName);
                adapter.Fill(dataset);
                //ViewUpdated?.Invoke(dataset);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get all values from table {table}", tableName);
                return null;
            }
            return dataset;
        }

        /// <summary>
        /// Gets the destination ID from the database. 
        /// If the destination does not exist or an error occurred, -1 is returned.
        /// </summary>
        /// <param name="destination">The associated destination string.</param>
        /// <param name="c">The connection to use, if any. Otherwise, one will be created.</param>
        /// <param name="t">The cancellation token to use for cancellation. If none, use <see cref="CancellationToken.None"/>.</param>
        /// <returns>The associated destination ID of <paramref name="destination"/>, otherwise -1 on errors.</returns>
        private int GetDestinationID(string destination, SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return -1;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts, true);
                if (connection == null) return -1;
                var getCommand = $"SELECT ID from {DestinationTable} WHERE Destination = @A0";
                using var sqlCommand = opts.CreateCommand(getCommand);
                sqlCommand.Parameters.Add(new SqliteParameter("@A0", destination));
                return Convert.ToInt32(sqlCommand.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get destination ID for destination {destination}", destination);
                return -1;
            }
        }

        /// <summary>
        /// Gets the full product record from the database.
        /// </summary>
        /// <param name="pid">The product record ID to fetch.</param>
        /// <param name="c">The connection to use, if any. Otherwise, one will be created.</param>
        /// <param name="t">The cancellation token to use for cancellation. If none, use <see cref="CancellationToken.None"/>.</param>
        /// <returns></returns>
        private DPProductRecord? GetProductRecord(long pid, SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return null;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts, true);
                if (connection == null) return null;
                var getCmd = $"SELECT * FROM {ProductFullView} WHERE PID = {pid};";
                using var command = opts.CreateCommand(getCmd);
                using var reader = command.ExecuteReader();
                if (!reader.HasRows) return null;
                reader.Read();
                var record = new DPProductRecord(
                    Name: reader.GetString("Name"),
                    Authors: reader.IsDBNull("Authors") ? Array.Empty<string>()
                                                     : reader.GetString("Authors").Split(", "),
                    Date: DateTime.FromFileTimeUtc(reader.GetInt64("Date")),
                    ThumbnailPath: reader.IsDBNull("Thumbnail") ? null : reader.GetString("Thumbnail"),
                    ArcName: reader.GetString("ArcName"),
                    Destination: reader.GetString("Destination"),
                    Tags: reader.IsDBNull("Tags") ? Array.Empty<string>()
                                                  : reader.GetString("Tags").Split(", "),
                    Files: reader.IsDBNull("Files") ? Array.Empty<string>()
                                                    : reader.GetString("Files").Split(", "),
                    ID: reader.GetInt64("PID")
                );
                return record;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get product record");
            }
            return null;
        }
        #endregion
        #region Writes
        #region Remove
        /// <summary>
        /// Removes product records from the database. It temporary disables the triggers to
        /// remove all records safely. In the event of an internal failure, you should make sure the triggers are
        /// re-enabled by calling <c>CreateTriggers()</c>.
        /// </summary>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the removal was a success (true) or not (false).</returns>
        private bool RemoveAllRecords(SqliteConnectionOpts opts) => ResetDatabase(opts);

        /// <summary>
        /// Removes values from the table specified with the conditions specified.
        /// </summary>
        /// <param name="tableName">The table you wish to remove values from.</param>
        /// <param name="conditions">An array of conditions to consider when removing rows.</param>
        /// <param name="or">Combine conditions with an OR statement (true) or an AND statement (false).</param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the removal was a success (true) or not (false).</returns>
        private bool RemoveValuesWithCondition(string tableName, Tuple<string, object>[] conditions,
            bool or, SqliteConnectionOpts opts)
        {
            // Build columns.
            List<string> args = new(conditions.Length);
            var vals = new object[conditions.Length];
            StringBuilder builder = new(250);
            builder.Append($"DELETE FROM {tableName}");

            for (var i = 0; i < conditions.Length; i++)
            {
                Tuple<string, object> tuple = conditions[i];
                var column = tuple.Item1;
                var item = tuple.Item2;
                var arg = $"@A{i}";
                if (i == 0)
                    builder.Append($" WHERE \"{column}\" = {arg}");
                else
                {
                    builder.Append(or ? " OR WHERE" : " AND WHERE");
                    builder.Append($"\"{column}\" = {arg}");
                }
                args.Add(arg);
                vals[i] = item;
            }
            builder.Append(';');

            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                try
                {
                    using var sqlCommand = connection.CreateCommand(builder.ToString());
                    FillParamsToConnection(sqlCommand, args, vals);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to remove values with condition for table {table}", tableName);
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create connection and transaction");
            }
            return true;
        }
        /// <summary>
        /// Removes all of the rows from the table specified. 
        /// </summary>
        /// <param name="tableName">The table to remove everything from.</param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the removal was a success (true) or not (false).</returns>
        private bool RemoveAllFromTable(string tableName, SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return false;

            var deleteCommand = $"DELETE FROM {tableName}; pragma vaccum;";
            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                try
                {
                    using var sqlCommand = connection.CreateCommand(deleteCommand);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to remove all from table {table}", tableName);
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to create connection and transaction");
                return false;
            }
            return true;
        }

        #endregion
        #region Insert
        /// <summary>
        /// Insert or replace files into the database for the associated PID. If the PID is 0, it will get the last PID.
        /// </summary>
        /// <param name="files">A list of files to insert into the database. Make sure there are no duplicates, otherwise this will fail. </param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <param name="pid">The product ID to insert the files to. If 0, it will get the last product ID in the database.</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool UpdateFiles(IReadOnlyList<string> files, SqliteConnectionOpts opts, long pid = 0)
        {
            if (opts.IsCancellationRequested) return false;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection == null) return false;

                if (pid == 0 && (pid = GetLastProductID(opts)) == 0)
                {
                    Logger.Error("GetLastProductID returned 0");
                    return false;
                }
                using var transaction = connection.BeginTransaction(ref opts);
                var insertCommand = $"INSERT OR REPLACE INTO {FilesTable} VALUES ({pid}, @A0);";
                using var sqlCommand = connection.CreateCommand(insertCommand);

                var filesString = JoinString(", ", files, 70);
                var param = new SqliteParameter("@A0", filesString is null ? DBNull.Value : filesString);
                sqlCommand.Parameters.Add(param);
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                TableUpdated?.Invoke(FilesTable);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to insert files");
                return false;
            }
            return true;

        }
        /// <summary>
        /// Inserts default values to the table specified.
        /// </summary>
        /// <param name="tableName">The table name to insert multiple values to.</param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool InsertDefaultValuesToTable(string tableName, SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return false;
            try
            {
                using var connection = CreateInitialConnection(ref opts);
                if (!OpenConnection(connection)) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                var insertCommand = $"INSERT INTO {tableName} DEFAULT VALUES;";
                try
                {
                    using var sqlCommand = connection.CreateCommand(insertCommand);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to insert default values to table {table}", tableName);
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occurred while inserting default values to table {table}", tableName);
                return false;
            }
            // TODO: Append params.

            return true;
        }
        /// <summary>
        /// Inserts values to the table specified. The length of columns and the length of columns in values
        /// must be the same. You do not have to include all of the columns for the table you wish to add values to.
        /// However, you should include columns that are required. For example, if you wish to only add a new extraction record name,
        /// you can set the columns {"name"} and values to {"hello"}. Columns may be an empty string array which will use the columns found
        /// in the table.
        /// </summary>
        /// <param name="tableName">The table name to insert multiple values to.</param>
        /// <param name="columns">The columns to insert values into. Cannot be null.</param>
        /// <param name="values">The values to insert into the table. Cannot be null.</param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool InsertValuesToTable(string tableName, string[] columns, object?[] values,
            SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested) return false;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction(ref opts);

                columns = columns?.Length == 0 ? GetColumns(tableName, opts) : columns;
                if (opts.IsCancellationRequested || columns == null || columns.Length == 0) return false;

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
                    using var sqlCommand = connection.CreateCommand(insertCommand);
                    FillParamsToConnection(sqlCommand, args, values);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to insert values to table {table} with columns {columns}", tableName, columnsToAdd);
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occurred while inserting values to table {table}", tableName);
                return false;
            }
            return true;

        }
        /// <summary>
        /// Inserts a product record into the database. 
        /// </summary>
        /// <param name="pRecord">The product record to insert. Cannot be null.</param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private bool InsertRecords(DPProductRecord pRecord, SqliteConnectionOpts opts)
        {
            // Trigger will update the product record's extraction record ID to the newly created record.
            if (opts.IsCancellationRequested) return false;
            // We do not care about ID.
            pRecord.Deconstruct(out var productName, out var authors, out var time, out var thumbnailPath, out var arcName, out var destination, out var tags, out var files, out var _);
            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection == null) return false;

                // Shorten strings if applicable.
                productName = productName.Length > 70 ? productName.Substring(0, 70) : productName;
                using var transaction = connection.BeginTransaction(ref opts);
                if (!SetDestination(destination, out var destID, opts)) return false;
                if (destID == -1) return false;

                var pColumns = new string[] { "Name", "Authors", "Date", "Thumbnail", "ArcName", "DestID", "Tags" };
                var pObjs = new object?[] { productName, JoinString(", ", authors, 70), time.ToFileTimeUtc(), thumbnailPath, arcName, destID, JoinString(", ", tags, 70) };

                if (opts.IsCancellationRequested || !InsertValuesToTable(ProductTable, pColumns, pObjs, opts)) return false;
                var lastID = GetLastProductID(opts);
                if (opts.IsCancellationRequested || lastID == 0 || !UpdateFiles(files, opts, lastID)) return false;
                transaction.Commit();

                // Create new product record to update ID.
                DPProductRecord newP = pRecord with { ID = lastID };
                ProductRecordAdded?.Invoke(newP);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to insert product record: {record}", pRecord);
                return false;
            }
            return true;
        }
        #endregion
        #region Update

        /// <summary>
        /// Updates a product record using values from the <paramref name="newRecord"/> attributes at <paramref name="pid"/>. The ID will not be changed.
        /// </summary>
        /// <param name="pid">The product record ID to update.</param>
        /// <param name="newRecord">The newly constructed DPProductRecord with new values to insert/update.</param>
        /// <param name="c">The SqliteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool UpdateProductRecord(long pid, DPProductRecord newRecord, SqliteConnectionOpts opts)
        {
            if (opts.IsCancellationRequested || newRecord == null || pid < 0)
                return false;
            // Deconstruct newRecord
            newRecord.Deconstruct(out var productName, out var authors, out var time, out var thumbnailPath, out var arcName, out var destination, out var tags, out var files, out var _);

            // Shorten strings if applicable.
            productName = productName?.Length > 70 ? productName.Substring(0, 70) : productName;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                if (!SetDestination(destination, out var destID, opts)) return false;
                var pColumns = new string[] { "Name", "Authors", "Date", "Thumbnail", "ArcName", "DestID", "Tags" };
                var pObjs = new object?[] { productName, JoinString(", ", authors, 70), time.ToFileTimeUtc(), thumbnailPath, arcName, destID, JoinString(", ", tags, 70) };
                
                StringBuilder updateCommand = new(250);
                updateCommand.AppendLine("UPDATE ").Append(ProductTable).Append(" SET "); // UPDATE {ProductTable} SET
                for (var i = 1; i < pColumns.Length + 1; i++)
                {
                    // "{Column} = @A{index}"
                    updateCommand.Append('"').Append(pColumns[i - 1]).Append("\" = @A").Append(i).AppendLine(", ");
                }
                updateCommand.Remove(updateCommand.Length - ", ".Length - Environment.NewLine.Length, ", ".Length + Environment.NewLine.Length); // Remove last comma and space.
                updateCommand.Append(" WHERE ROWID = ").Append(pid).Append(';');
                try
                {
                    if (opts.IsCancellationRequested || !UpdateFiles(files, opts, pid)) return false;
                    using var sqlCommand = connection.CreateCommand(updateCommand.ToString());
                    sqlCommand.Parameters.Add(new SqliteParameter("@A0", destination));
                    for (var i = 1; i < pColumns.Length + 1; i++)
                    {
                        sqlCommand.Parameters.Add(new SqliteParameter("@A" + i, pObjs[i - 1] ?? DBNull.Value));
                    }
                    if (opts.IsCancellationRequested) return false;
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(ProductTable);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to update product record {record}", newRecord);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occurred while attempting to update product record {record}", newRecord);
                return false;
            }
            return true;
        }

        private bool SetDestination(string dest, out int destID, SqliteConnectionOpts opts)
        {
            destID = -1;
            if (opts.IsCancellationRequested) return false;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                var insertCommand = $"INSERT OR IGNORE INTO {DestinationTable} (Destination) VALUES (@A0);";
                using var sqlCommand = connection.CreateCommand(insertCommand);

                sqlCommand.Parameters.Add(new SqliteParameter("@A0", dest));
                sqlCommand.ExecuteNonQuery();
                transaction.Commit();
                destID = GetDestinationID(dest, opts);
                if (destID == -1) return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to set destination {dest}", dest);
                return false;
            }
            return true;
        }
        #endregion
        #endregion
        #region Update
        private bool UpdateToVersion3(SqliteConnectionOpts opts)
        {
            /// <summary>
            /// We will create a new database file and copy all of the data from the old database file to the new one.
            /// </summary>
            if (opts.IsCancellationRequested) return false;
            try
            {
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection is null) return false;
                using var transaction = connection.BeginTransaction(ref opts);
                var dropTablesCommand = $@"
                    DROP TRIGGER IF EXISTS delete_on_extraction_removal;
                    DROP TRIGGER IF EXISTS delete_on_product_removal;
                    DROP TRIGGER IF EXISTS update_on_extraction_add;
                    DROP TRIGGER IF EXISTS update_product_count;

                    DROP INDEX IF EXISTS idx_DateCreatedToPID;
                    DROP INDEX IF EXISTS idx_ProductNameToPID;
                    DROP INDEX IF EXISTS idx_PIDtoTag;
                    DROP INDEX IF EXISTS idx_TagToPID;

                    DROP TABLE IF EXISTS CachedSearches;
                    DROP TABLE IF EXISTS DatabaseInfo;
                    DELETE FROM sqlite_sequence;
                ";
                try
                {
                    using var sqlCommand = connection.CreateCommand(dropTablesCommand);
                    sqlCommand.ExecuteNonQuery();
                    if (!CreateTables(opts)) return false;
                    if (!CreateIndexes(opts)) return false;
                    if (!CreateTriggers(opts)) return false;
                    if (!CreateViews(opts)) return false;
                    if (!ExecutePragmas(opts)) return false;
                    InsertDefaultValuesToTable("DatabaseInfo", opts);
                    // Now we will INSERT INTO the new tables, starting with Products
                    sqlCommand.CommandText =
                        $@"INSERT INTO Destinations (Destination)
                            SELECT DISTINCT ""Destination Path"" FROM ExtractionRecords;
                        INSERT INTO Products
                        SELECT ""Product Name"", Author, ""Date Created"", ""Thumbnail Full Path"", ""Archive Name"", (SELECT ""ID"" FROM Destinations WHERE Destination = ""Destination Path""), Tags 
	                    FROM ProductRecords p 
	                    JOIN ExtractionRecords e 
	                    ON p.ID = e.""Product Record ID"";

                        INSERT INTO Files (PID, File)
                        SELECT DISTINCT ""Product Record ID"", ""Files"" FROM ExtractionRecords;

                        DROP TABLE ProductRecords;
                        DROP TABLE ExtractionRecords;
                        DROP TABLE Tags;
                        pragma VACCUM;
";
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to update database to version 3. Rolling back transaction.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to rollback update transaction");
                return false;
            }
            return true;
        }
        #endregion
        #region etc
        /// <summary>
        /// If you notice, the database file (db.db) also has a .db-shm file and a .db-wal file include it.
        /// Those are used to allow multiple read connections and a single write connection to the database.
        /// Those files are considered to be the journal. This function asks the database to truncate/shrink the 
        /// journal and merge it into the database file. Additionally, it attempts to delete the journal files
        /// as well.
        /// </summary>
        private void TruncateJournal()
        {
            var pragmaCheckpoint = "PRAGMA wal_checkpoint(TRUNCATE);";
            try
            {
                var opts = new SqliteConnectionOpts();
                using var connection = CreateAndOpenConnection(ref opts);
                if (connection == null) return;
                using var cmd = connection.CreateCommand(pragmaCheckpoint);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to truncate journal");
                return;  // We don't want to delete if it failed.
            }

            // Now check if -wal and -shm are available.
            var shmFile = System.IO.Path.GetFullPath(Path + "-shm");
            var walFile = System.IO.Path.GetFullPath(Path + "-wal");

            // This is required for the SqliteConnection to truly release the handle on 
            // the database file.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                if (File.Exists(shmFile)) File.Delete(shmFile);
                if (File.Exists(walFile)) File.Delete(walFile);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// Similar to string.Join() but will skip values that are null or empty (after trim).
        /// <para/>
        /// <paramref name="values"/> can be null and will return null. Otherwise, seperator must not 
        /// be null, otherwise an exception will be thrown.
        /// <para/>
        /// Additionally, you can set the max size of each string value. The default is 256. Meaning, for each
        /// tag, the value will be trimmed to 256 characters if the value exceeds that size.
        /// </summary>
        /// <param name="seperator">The seperator to add in between values in string. Cannot be null.</param>
        /// <param name="maxSize">The maximum string size of each value. Default is 256.</param>
        /// <param name="values">The values to join.</param>
        /// <returns>The values combined into a string seperated by the sepertor or null if values is null.</returns>
        private string? JoinString(string seperator, IReadOnlyList<string> values, int maxSize = 256)
        {
            if (values is { Count: 0 }) return null;

            StringBuilder builder = new((maxSize + seperator.Length) * (values.Count + 1));
            for (var i = 0; i < values.Count; i++)
            {
                var s = values[i];
                if (string.IsNullOrWhiteSpace(s)) continue;
                if (s.Length > maxSize)
                    builder.Append(s, 0, maxSize - 1);
                else builder.Append(s);
                builder.Append(seperator);
            }
            builder.Remove(builder.Length - seperator.Length, seperator.Length);
            return builder.ToString();
        }
        /// <summary>
        /// Creates a string array of parameter placeholders (ex: "@A") determined by the length specified.
        /// It also updates the <paramref name="str"> to append all of the parameters. For example, if the length is
        /// 3, and you have a str equal "INSERT INTO foo WHERE VALUES IN (".
        /// This function will return {"@A1", "@A2", "@A3"} and will append "@A1, @A2, @A3" to str.
        /// </summary>
        /// <param name="str">A referenced string of a query to add parameter placeholders. May not be null.</param>
        /// <param name="length">The amount of parameters to create.</param>
        /// <returns>An array of parameters generated.</returns>
        private string[] CreateParams(ref string str, int length)
        {
            var maxDigits = (int)Math.Floor(Math.Log10(length)) + 1;
            StringBuilder sb = new((maxDigits + 4) * length);
            var args = new string[length];
            for (var i = 0; i < length; i++)
            {
                var rawArg = "@A" + i;
                var arg = i != length - 1 ? rawArg + ", " : rawArg;
                sb.Append(arg);
                args[i] = rawArg;
            }
            str += sb.ToString();
            return args;
        }
        /// <summary>
        /// Creates a string array of parameter placeholders (ex: "@A") determined by the length specified.
        /// It also updates the <paramref name="str"> to append all of the parameters. <paramref name="start"/>
        /// is used to indicate the number to start with for creating the parameter placeholders.
        /// For example, if the length is 3, and start is 5, and you have a str equal "INSERT INTO foo WHERE VALUES IN (".
        /// This function will return {"@A5", "@A6", "@A7"} and will append "@A5, @A6, @A7" to str.
        /// </summary>
        /// <param name="str">A referenced string of a query to add parameter placeholders. May not be null.</param>
        /// <param name="length">The amount of parameters to create.</param>
        /// <returns>An array of parameters generated.</returns>

        private string[] CreateParams(ref string str, int length, ref int start)
        {
            var maxDigits = (int)Math.Floor(Math.Log10(length + start)) + 1;
            StringBuilder sb = new((maxDigits + 4) * length);
            var args = new string[length];
            for (var i = 0; i < length; i++, start++)
            {
                var rawArg = "@A" + start;
                var arg = i != length - 1 ? rawArg + ", " : rawArg;
                sb.Append(arg);
                args[i] = rawArg;
            }
            str += sb.ToString();
            return args;
        }
        /// <summary>
        /// Creates a string array of parameter placeholders (ex: "@A = @B") determined by the length specified.
        /// It also updates the <paramref name="str"> to append all of the parameters. For example, if the length is
        /// 3, and you have a str equal "UPDATE table SET (".
        /// This function will return {"@A1", "@A2", "@A3"} and will append "@A1 = A2, @A2 = @A3, @A4 = @A5" to str.
        /// </summary>
        /// <param name="str">A referenced string of a query to add parameter placeholders. May not be null.</param>
        /// <param name="length">The amount of parameters to create. For example, for updating two columns, the length should be 2, not 4.</param>
        /// <returns>An array of parameters generated.</returns>
        private string[] CreateAssignmentParams(ref string str, int length)
        {
            var maxDigits = (int)Math.Floor(Math.Log10(length)) + 1;
            StringBuilder sb = new((maxDigits + 8) * length);
            var args = new string[length * 2];
            for (var i = 0; i < length * 2; i += 2)
            {
                var rawArg = "@A" + i + " = @A" + (i + 1);
                var arg = i != (length * 2) - 2 ? rawArg + ", " : rawArg;
                sb.Append(arg);
                args[i] = "@A" + i;
                args[i + 1] = "@A" + (i + 1);
            }
            str += sb.ToString();
            return args;
        }
        /// <summary>
        /// Associates parameter placeholds with a value to the SqliteCommand. You should call <c>CreateParams()</c> before
        /// to generate the parameter list to include into <paramref name="cArgs"/> and update the command string.
        /// For example, if the args is {"@A1", "@A2", "@A3"} and the values are {"hello", "solomon", "blount"}, then 
        /// the args will be replaced with its values when the query is executed.
        /// </summary>
        /// <param name="command">The command to add parameters into. Cannot be null.</param>
        /// <param name="cArgs">The argument placeholders to fill. Cannot be null.</param>
        /// <param name="values">The values to replace placeholders with. Cannot be null.</param>
        private void FillParamsToConnection(DbCommand command, IReadOnlyList<string> cArgs, params object[] values)
        {
            for (var i = 0; i < cArgs.Count; i++)
            {
                command.Parameters.Add(new SqliteParameter(cArgs[i], values[i] ?? DBNull.Value));
            }
        }
        /// <summary>
        /// Associates parameter placeholds with a value to the SqliteCommand. You should call <c>CreateAssignmentParams()</c> before
        /// to generate the parameter list to include into <paramref name="cArgs"/> and update the command string. <para/>
        /// Then, use <paramref name="leftVals"/> for values that should be on the left side of the equal sign.
        /// Use <paramref name="rightVals"/> for values that should be on the right. <para/>
        /// For example, if the args is {"@A1", "@A2", "@A3"} and the leftVals are {"Product Name", "Tags"} and rightVals are {"My Product Name", "My Tags"}, then 
        /// the args will be replaced with its values when the query is executed. The executed query would be:
        /// <c>"Product Name" = "My Product Name", "Tags" = "My Tags"</c>
        /// </summary>
        /// <param name="command">The command to add parameters into. Cannot be null.</param>
        /// <param name="cArgs">The argument placeholders to fill. Cannot be null.</param>
        /// <param name="leftVals">The values to replace placeholders with on the left side of the equal sign. Cannot be null.</param>
        /// <param name="rightVals">The values to replace placeholders with on the right side of the equal sign. Cannot be null.</param>
        /// 
        private void FillAssignmentParamsToConnection(DbCommand command, IReadOnlyList<string> cArgs, object[] leftVals, object[] rightVals)
        {
            if (leftVals.Length != rightVals.Length)
                Logger.Warning("FillAssignmentParamsToConnection() did not fill parameters due to unequal lengths in values.");
            for (int i = 0, j = 0; i < cArgs.Count; i += 2, j++)
            {
                command.Parameters.Add(new SqliteParameter(cArgs[i], leftVals[j] ?? DBNull.Value));
                command.Parameters.Add(new SqliteParameter(cArgs[i + 1], rightVals[j] ?? DBNull.Value));
            }
        }
        #endregion
    }
}
