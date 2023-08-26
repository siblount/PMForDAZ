// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using Serilog;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace DAZ_Installer.Database
{
    public partial class DPDatabase
    {
        #region Reads
        /// <summary>
        /// Updates the <c>ProductRecordCount</c> property.
        /// </summary>
        /// <param name="connection">A connection to reuse, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private void UpdateProductRecordCount(SQLiteConnection? connection, CancellationToken t)
        {
            const string getCmd = @"SELECT ""Product Record Count"" FROM DatabaseInfo;";
            if (t.IsCancellationRequested) return;
            try
            {
                using SQLiteCommand cmd = new(getCmd, connection);
                ProductRecordCount = Convert.ToUInt32(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                Logger.ForContext<DPDatabase>().Error(e, "An unexpected error occurred while attempting to get product record count.");
            }
            // DPCommon.WriteToLog("Product Record Count: ", ProductRecordCount);
        }

        /// <summary>
        /// Updates the <c>ExtractionRecordCount</c> property.
        /// </summary>
        /// <param name="connection">A connection to reuse, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private void UpdateExtractionRecordCount(SQLiteConnection? connection, CancellationToken t)
        {
            const string getCmd = @"SELECT ""Extraction Record Count"" FROM DatabaseInfo;";
            try
            {
                using SQLiteCommand cmd = new(getCmd, connection);
                ExtractionRecordCount = Convert.ToUInt32(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred while attempting to get extraction record count. REASON: {e}");
            }
            // DPCommon.WriteToLog("Extraction Record Count: ", ExtractionRecordCount);

        }
        /// <summary>
        /// Executes the reader to search for product records via tags. This only executes the reader and returns an array
        /// of product records.
        /// </summary>
        /// <param name="command">The command that is ready to execute. Cannot be null.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>An array of product records found from search.</returns>
        private DPProductRecord[] SearchProductRecordsViaTagsS(SQLiteCommand command, CancellationToken t)
        {
            if (t.IsCancellationRequested) return Array.Empty<DPProductRecord>();
            SQLiteDataReader reader = command.ExecuteReader();
            List<DPProductRecord> searchResults = new(25);
            string productName, author, thumbnailPath, sku;
            string[] tags;
            DateTime dateCreated;
            uint extractionID, pid;
            while (reader.Read())
            {
                if (t.IsCancellationRequested) return Array.Empty<DPProductRecord>();
                // Construct product records
                // NULL values return type DB.NULL.
                productName = (string)reader["Product Name"];
                // TODO: Tags have returned null; investigate why.
                var rawTags = reader["Tags"] as string ?? string.Empty;
                tags = rawTags.Trim().Split(", ");
                author = reader["Author"] as string; // May return NULL
                thumbnailPath = reader["Thumbnail Full Path"] as string; // May return NULL
                extractionID = Convert.ToUInt32(reader["Extraction Record ID"] is DBNull ?
                    0 : reader["Extraction Record ID"]);
                dateCreated = DateTime.FromFileTimeUtc((long)reader["Date Created"]);
                sku = reader["SKU"] as string; // May return NULL
                pid = Convert.ToUInt32(reader["ID"]);
                searchResults.Add(
                    new DPProductRecord(productName, tags, author, sku, dateCreated, thumbnailPath, extractionID, pid));
            }

            return searchResults.ToArray();
        }

        /// <summary>
        /// Returns an array of columns for the table name specified.
        /// </summary>
        /// <param name="tableName">The table to get columns from.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private string[] GetColumns(string tableName, SQLiteConnection? c,
            CancellationToken t)
        {
            if (t.IsCancellationRequested || tableName.Length == 0) return Array.Empty<string>();
            SQLiteConnection? connection = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c, true);
                var success = OpenConnection(connection);
                if (!success) return Array.Empty<string>();

                var randomCommand = $"SELECT * FROM {tableName} LIMIT 1;";
                sqlCommand = new SQLiteCommand(randomCommand, connection);
                SQLiteDataReader reader = sqlCommand.ExecuteReader();
                DataTable table = reader.GetSchemaTable();

                List<string> columns = new();
                foreach (DataRow row in table.Rows)
                {
                    if (t.IsCancellationRequested) return Array.Empty<string>();
                    columns.Add((string)row.ItemArray[0]);
                }
            }
            catch (Exception e)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred attempting to get columns for table: {tableName}. REASON: {e}");
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Returns all an array of all of the tables in the database.
        /// </summary>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="cancellationToken">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private string[] GetTables(SQLiteConnection? c, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Array.Empty<string>();
            List<string> tables = new();
            SQLiteConnection? connection = null;
            SQLiteCommand sqlCommand = null;

            try
            {
                connection = CreateAndOpenConnection(c, true);
                if (connection == null) return Array.Empty<string>();
                var randomCommand = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'";
                sqlCommand = new SQLiteCommand(randomCommand, connection);
                using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                        tables.Add(reader.GetString(0));
                }
                UpdateProductRecordCount(connection, cancellationToken);
                UpdateExtractionRecordCount(connection, cancellationToken);
                return tables.ToArray();
            }
            catch (Exception e)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred attempting to get table names. REASON: {e}");
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
            }
            return Array.Empty<string>();

        }
        /// <summary>
        /// Attempts to get the extraction record via the extraction record's ID in the database. May return null if it does not exist
        /// or there was an error parsing the data from the database.
        /// </summary>
        /// <param name="id">The extraction record ID to fetch from database.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private DPExtractionRecord? GetExtractionRecord(uint id, SQLiteConnection? c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return null;

            var getCmd = $"SELECT * FROM ExtractionRecords WHERE ID = {id};";
            SQLiteConnection? connection = null;
            SQLiteCommand cmd = null;
            try
            {
                connection = CreateAndOpenConnection(c, true);
                if (connection == null) return null;
                cmd = new SQLiteCommand(getCmd, connection);
                using SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string[] files, folders, erroredFiles, errorMessages;
                    var archiveFileName = reader["Archive Name"] as string;
                    var filesStr = reader["Files"] as string;
                    var foldersStr = reader["Folders"] as string;
                    var destinationPath = reader["Destination Path"] as string;
                    var erroredFilesStr = reader["Errored Files"] as string;
                    var errorMessagesStr = reader["Error Messages"] as string;
                    var pid = Convert.ToUInt32(reader["Product Record ID"]);

                    files = filesStr?.Split(", ") ?? Array.Empty<string>();
                    folders = foldersStr?.Split(", ") ?? Array.Empty<string>();
                    erroredFiles = erroredFilesStr?.Split(", ") ?? Array.Empty<string>();
                    errorMessages = errorMessagesStr?.Split(", ") ?? Array.Empty<string>();

                    DPExtractionRecord record = new(archiveFileName, destinationPath, files, erroredFiles, errorMessages, folders, pid);
                    // RecordQueryCompleted?.Invoke(record);
                    return record;
                }
                // DPCommon.WriteToLog("Failed to get extraction record possibly due to extraction record was deleted.");
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to get extraction record. REASON: {ex}");
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    cmd?.Dispose();
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a unique list of archive file names that have been successfully extracted. It returns a hashset
        /// which may be null if it fails to create & open a connection, and execute the reader. Otherwise, it may
        /// return an empty hashset indicating there was nothing there.
        /// </summary>
        /// <param name="c">The SQLiteCOnnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>A hashset containing successfully extracted archive file names.</returns>
        private HashSet<string>? GetArchiveFileNameList(SQLiteConnection? c, CancellationToken t)
        {
            SQLiteConnection _connection = null;
            SQLiteCommand cmd = null;
            SQLiteDataReader reader = null;
            HashSet<string> names = null;
            var getCmd = @"SELECT ""Archive Name"" FROM ExtractionRecords;";
            var constring = "Data Source = " + System.IO.Path.GetFullPath(Path) + ";Read Only=True";
            try
            {
                _connection = CreateAndOpenConnection(c, true);
                if (_connection == null) return names;
                cmd = new SQLiteCommand(getCmd, _connection);
                reader = cmd.ExecuteReader();
                names = new HashSet<string>(reader.StepCount);
                while (reader.Read())
                {
                    names.Add(reader.GetString(0));
                }
                // MainQueryCompleted?.Invoke();
                ArchiveFileNames = names;
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to get archive file name list. REASON: {ex}");
            }
            finally
            {
                if (c is null)
                {
                    _connection?.Dispose();
                    cmd?.Dispose();
                    reader?.DisposeAsync();
                }
            }
            return names;
        }

        /// <summary>
        /// Returns the last product ID which indicates the latest product record added to the database.
        /// It may return 0 if an error occurred (or if there are no product records in the database).
        /// </summary>
        /// <param name="conn">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>The last product record ID in the database.</returns>
        private uint GetLastProductID(SQLiteConnection? conn, CancellationToken t)
        {
            if (t.IsCancellationRequested) return 0;
            var c = "SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1;";
            SQLiteConnection? connection = null;
            SQLiteCommand cmd = null;
            try
            {
                connection = CreateAndOpenConnection(conn, true);
                cmd = new SQLiteCommand(c, connection);
                return Convert.ToUInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to get last product ID. REASON: {ex}");
            }
            return 0;
        }
        /// <summary>
        /// Returns all the rows from a table in the database. This may return an empty dataset
        /// if there was an error connecting to the database. Additionally, the dataset may be 
        /// empty indicating there was an issue internally or that there was nothing in the table.
        /// </summary>
        /// <param name="tableName">The table to get all rows from.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="token">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>A dataset containing all of the values from the table specified. May return null.</returns>
        private DataSet? GetAllValuesFromTable(string tableName, SQLiteConnection? c,
            CancellationToken token)
        {
            DataSet dataset = null;
            SQLiteConnection? connection = null;
            SQLiteCommand sqlCommand = null;
            if (token.IsCancellationRequested) return dataset;
            try
            {
                connection = CreateAndOpenConnection(c, true);
                var getCommand = $"SELECT * FROM {tableName}";
                sqlCommand = new SQLiteCommand(getCommand, connection);
                SQLiteDataAdapter adapter = new(sqlCommand);
                dataset = new DataSet(tableName);
                adapter.Fill(dataset);
                //ViewUpdated?.Invoke(dataset);
            }
            catch { }
            return dataset;
        }
        #endregion
        #region Writes
        #region Remove
        /// <summary>
        /// Removes all extraction and product records from the database. It temporary disables the triggers to
        /// remove all records safely. In the event of an internal failure, you should make sure the triggers are
        /// re-enabled by calling <c>CreateTriggers()</c>.
        /// </summary>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the removal was a success (true) or not (false).</returns>
        private bool RemoveAllRecords(SQLiteConnection? c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;

            // Also deletes from tags via trigger.
            // Faster way is to drop the table & re-make it.
            // TODO: Drop table and remake it.
            var deleteCommand = $"DELETE FROM ProductRecords; DELETE FROM ExtractionRecords;";
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                {
                    if (connection == null) return false;
                    transaction = connection.BeginTransaction();
                    try
                    {
                        if (DeleteTriggers())
                        {
                            sqlCommand = new SQLiteCommand(deleteCommand, connection, transaction);
                            sqlCommand.ExecuteNonQuery();
                            transaction.Commit();
                            CreateTriggers();
                            TableUpdated?.Invoke("ProductRecords");
                            TableUpdated?.Invoke("ExtractionRecords");
                            RecordsCleared?.Invoke();
                            TableUpdated?.Invoke("ProductRecords");
                            TableUpdated?.Invoke("ExtractionRecords");
                        }
                    }
                    catch (Exception ex)
                    {
                        // DPCommon.WriteToLog($"Failed to delete records. REASON: {ex}");
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to create and begin transaction. REASON: {ex}");
                return false;
            }
            finally
            {
                if (c is null)
                {
                    sqlCommand?.Dispose();
                    connection?.Dispose();
                }

            }

            return true;
        }

        /// <summary>
        /// Removes product records that have a tag specified in the <paramref name="tags"/> array. In other words, it removes
        /// any product records that contains a tag in <paramref name="tags"/>. For example, if you wanted to remove all product records
        /// that either has a "Environment" tag or "Clothes" tag, tags should contain these values. 
        /// <param name="tags">A list of tags that will be used to determine if a product record should be removed.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the removal was a success (true) or not (false).</returns>
        private bool RemoveProductRecordsViaTag(string[] tags, SQLiteConnection? c,
            CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            if (tags.Length == 0) return true;

            var args = ConvertParamsToString(tags);
            var idsCommand = $"SELECT \"Product Record ID\" FROM Tags WHERE Tag IN ({args})";
            var deleteCommand = $"DELETE FROM ProductRecords WHERE ID IN ({idsCommand});";
            SQLiteConnection? connection = null;
            SQLiteCommand sqlCommand = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();
                try
                {
                    sqlCommand = new SQLiteCommand(deleteCommand, connection, transaction);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated.Invoke("ProductRecords");
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to delete from ProductRecords where ID IN {args}. REASON: {ex.Message}");
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to create connection and transaction. REASON: {ex}");
                return false;
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();

                }
            }

            return true;
        }
        /// <summary>
        /// Removes values from the table specified with the conditions specified.
        /// </summary>
        /// <param name="tableName">The table you wish to remove values from.</param>
        /// <param name="conditions">An array of conditions to consider when removing rows.</param>
        /// <param name="or">Combine conditions with an OR statement (true) or an AND statement (false).</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the removal was a success (true) or not (false).</returns>
        private bool RemoveValuesWithCondition(string tableName, Tuple<string, object>[] conditions,
            bool or, SQLiteConnection? c, CancellationToken t)
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
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();
                try
                {
                    sqlCommand = new SQLiteCommand(builder.ToString(), connection, transaction);
                    FillParamsToConnection(sqlCommand, args, vals);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to delete from {tableName}. REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to create connection and transaction. REASON: {ex}");
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
            }

            return true;
        }
        /// <summary>
        /// Removes all of the rows from the table specified. 
        /// </summary>
        /// <param name="tableName">The table to remove everything from.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the removal was a success (true) or not (false).</returns>
        private bool RemoveAllFromTable(string tableName, SQLiteConnection? c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;


            var deleteCommand = $"DELETE FROM {tableName};"; // Faster way is to drop the table & re-make it.
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();
                try
                {
                    sqlCommand = new SQLiteCommand(deleteCommand, connection, transaction);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed delete all values for table: {tableName}. REASON: {ex.Message}");
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to create connection and transaction. REASON: {ex}");
                return false;
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
            }

            return true;
        }
        /// <summary>
        /// Removes all tags associated with the product record ID. 
        /// </summary>
        /// <param name="pid">The product record ID to remove tags associated with it.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private bool RemoveTags(uint pid, SQLiteConnection? c, CancellationToken t)
        {
            return RemoveValuesWithCondition("Tags",
                    new Tuple<string, object>[] { new Tuple<string, object>("Product Record ID", pid) }
                    , false, c, t);
        }

        #endregion
        #region Insert
        /// <summary>
        /// Insert tags to the tags table, the product record ID will be automatically set (via the trigger) which will be the 
        /// last product record ID in the database.
        /// </summary>
        /// <param name="tags">An array of tags to insert into the database.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private void InsertTags(string[] tags, SQLiteConnection? conn, CancellationToken t)
        {
            if (t.IsCancellationRequested) return;
            SQLiteConnection? connection = null;
            try
            {
                connection = CreateAndOpenConnection(conn);
                if (connection == null) return;

                var pid = GetLastProductID(connection, t);
                if (pid == 0)
                {
                    // DPCommon.WriteToLog("Product ID returned 0; no tags added.");
                    return;
                }

                List<string> tagsStripped = new(tags.Length);
                foreach (var tag in tags)
                {
                    if (string.IsNullOrEmpty(tag)) continue;
                    var tagTrimmed = tag.Trim();
                    if (tagTrimmed.Length == 0) continue;
                    tagsStripped.Add(tagTrimmed);
                }

                var vals = new object[tagsStripped.Count][];
                for (var i = 0; i < tagsStripped.Count; i++)
                {
                    vals[i] = new object[] { tagsStripped[i], pid };
                }

                InsertMultipleValuesToTable("Tags", new string[] { "Tag", "Product Record ID" },
                    vals, connection, t);

            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to insert tags. REASON: {ex}");
            }
            finally
            {
                if (conn is null) connection?.Dispose();
            }

        }
        /// <summary>
        /// Insert tags to the tags table using the specified <paramref name="pid"/>.
        /// </summary>
        /// <param name="tags">An array of tags to insert into the database.</param>
        /// <param name="pid">The product record ID to use.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private void InsertTags(string[] tags, uint pid, SQLiteConnection? conn, CancellationToken t)
        {
            if (t.IsCancellationRequested || pid == 0) return;
            SQLiteConnection? connection = null;
            try
            {
                connection = CreateAndOpenConnection(conn);
                if (connection == null) return;

                List<string> tagsStripped = new(tags.Length);
                foreach (var tag in tags)
                {
                    if (string.IsNullOrEmpty(tag)) continue;
                    var tagTrimmed = tag.Trim();
                    if (tagTrimmed.Length == 0) continue;
                    tagsStripped.Add(tagTrimmed);
                }

                var vals = new object[tagsStripped.Count][];
                for (var i = 0; i < tagsStripped.Count; i++)
                {
                    vals[i] = new object[] { tagsStripped[i], pid };
                }

                InsertMultipleValuesToTable("Tags", new string[] { "Tag", "Product Record ID" },
                    vals, connection, t);

            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to insert tags. REASON: {ex}");
            }
            finally
            {
                if (conn is null) connection?.Dispose();
            }

        }
        /// <summary>
        /// Inserts multiple values to the table using one transaction. The length of columns and the length of columns in values
        /// must be the same. You do not have to include all of the columns for the table you wish to add values to.
        /// However, you should include columns that are required. For example, if you wish to only add a new extraction record name,
        /// you can set the columns {"name"} and values to {{"hello"}}. Columns may be an empty string array which will use the columns found
        /// in the table.
        /// </summary>
        /// <param name="tableName">The table name to insert multiple values to.</param>
        /// <param name="columns">The columns to insert values into. Cannot be null.</param>
        /// <param name="values">The values to insert into the table. Cannot be null.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool InsertMultipleValuesToTable(string tableName, string[] columns, object[][] values,
            SQLiteConnection? c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();
                columns = columns?.Length == 0 ? GetColumns(tableName, connection, t) : columns;

                if (values.Length == 0) return true;
                if (t.IsCancellationRequested || columns == null || columns.Length == 0) return false;
                // Build columns.
                // Wrap in quotes
                for (var i = 0; i < columns.Length; i++)
                {
                    columns[i] = '"' + columns[i] + '"';
                }
                var columnsToAdd = string.Join(',', columns);
                StringBuilder builder = new((values.Length) * 20);
                List<string> args = new(values.Length * 5);
                var startNum = 0;
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
                var valsFlattened = new object[startNum];
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
                    sqlCommand = new SQLiteCommand(insertCommand, connection, transaction);
                    FillParamsToConnection(sqlCommand, args, valsFlattened);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to insert values to {columnsToAdd}. REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred while inserting multiple values to table. REASON: {ex}");
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
            }


            return true;
        }
        /// <summary>
        /// Inserts default values to the table specified.
        /// </summary>
        /// <param name="tableName">The table name to insert multiple values to.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool InsertDefaultValuesToTable(string tableName, SQLiteConnection? c,
            CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();
                var insertCommand = $"INSERT INTO {tableName} DEFAULT VALUES;";
                try
                {
                    sqlCommand = new SQLiteCommand(insertCommand, connection, transaction);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to insert default values to {tableName}. REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred while inserting default values. REASON: {ex}");
                return false;
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
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
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool InsertValuesToTable(string tableName, string[] columns, object[] values,
            SQLiteConnection? c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();

                columns = columns?.Length == 0 ? GetColumns(tableName, connection, t) : columns;
                if (t.IsCancellationRequested || columns == null || columns.Length == 0) return false;

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
                    sqlCommand = new SQLiteCommand(insertCommand, connection, transaction);
                    FillParamsToConnection(sqlCommand, args, values);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to insert values to {columnsToAdd}. REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"Failed to insert values to table. REASON: {ex}");
                return false;
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
            }

            return true;

        }
        /// <summary>
        /// Inserts a product record and/or an extraction record into the database. 
        /// <para>
        /// RECORDS SHOULD NEVER BE NULL! USE .NULL_RECORD TO INDICATE A NULL RECORD!
        /// </para>
        /// <para> If any of the product records are equal to NULL_RECORD, they will not be
        /// inserted into the database. </para>
        /// </summary>
        /// <param name="pRecord">The product record to insert. Cannot be null.</param>
        /// <param name="eRecord">The extraction record to insert. Cannot be null.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        private void InsertRecords(DPProductRecord? pRecord, DPExtractionRecord? eRecord,
            SQLiteConnection? c, CancellationToken t)
        {
            // Trigger will update the product record's extraction record ID to the newly created record.
            var pColumns = new string[] { "Product Name", "Tags", "Author", "SKU", "Date Created", "Thumbnail Full Path", };
            var eColumns = new string[] { "Archive Name", "Files", "Folders", "Destination Path", "Errored Files", "Error Messages" };
            if (t.IsCancellationRequested || pRecord is null || eRecord is null) return;
            SQLiteConnection? connection = null;
            pRecord.Deconstruct(out var productName, out var tags, out var author, out var sku,
                                 out DateTime time, out var thumbnailPath, out var __, out var _);
            eRecord.Deconstruct(out var archiveFileName, out var destPath, out var files,
                out var erroredFiles, out var erroredMessages, out var folders, out _);
            // We do not care about UID.
            // Order must match pColumns / eColumns
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return;

                // Shorten strings if applicable.
                productName = productName?.Length > 70 ? productName.Substring(0, 70) : productName;
                author = author?.Length > 30 ? author.Substring(0, 30) : author;
                sku = sku?.Length > 10 ? sku.Substring(0, 10) : sku;

                var pObjs = new object[] { productName, JoinString(", ", 70, tags), author, sku, time.ToFileTimeUtc(), thumbnailPath };
                var eObjs = new object[] { archiveFileName, JoinString(", ", 384, files),
                JoinString(", ", 384, folders), destPath, JoinString(", ", 384, erroredFiles),
                JoinString(", ", 65536, erroredMessages) };

                // If both operations are successful, emit signal.
                if (InsertValuesToTable("ProductRecords", pColumns, pObjs, connection, t))
                {
                    var lastID = GetLastProductID(connection, t);
                    if (eRecord != DPExtractionRecord.NULL_RECORD)
                    {
                        InsertTags(tags, connection, t);
                        var success = InsertValuesToTable("ExtractionRecords", eColumns, eObjs, connection, t);
                        if (success)
                        {
                            // Create a new extraction record to contain the PID and it's EID.
                            DPExtractionRecord newE = new(archiveFileName, destPath, files, erroredFiles, erroredMessages, folders, lastID);
                            ExtractionRecordAdded?.Invoke(newE);
                        }
                    }
                    // Create new product record to update ID.
                    DPProductRecord newP = new(productName, tags, author, sku, time, thumbnailPath, lastID, lastID);
                    ProductRecordAdded?.Invoke(newP, lastID);
                }
            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred while attempting to insert records. REASON: {ex}");
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                }
            }
        }
        #endregion
        #region Update
        /// <summary>
        /// Updates values from the table and columns specified. The length of columns and the length of values
        /// must be the same. You do not have to include all of the columns for the table you wish update.
        /// For example, if you wish to only update the extraction record name, you can set the columns {"name"} and values to {{"hello"}}. 
        /// Columns may be an empty string array which will use the columns found in the table.
        /// </summary>
        /// <param name="tableName">The table name to insert multiple values to.</param>
        /// <param name="columns">The columns to update values into. Cannot be null.</param>
        /// <param name="newValues">The values to update into the table. Cannot be null.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool UpdateValues(string tableName, string[] columns, object[] newValues,
            int id, SQLiteConnection? c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null!;
            SQLiteCommand sqlCommand = null!;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();

                columns = columns?.Length == 0 ? GetColumns(tableName, connection, t) : columns;
                if (t.IsCancellationRequested || columns == null ||
                    columns.Length == 0 || columns.Length != newValues.Length) return false;

                var updateCommand = $"UPDATE {tableName} SET ";
                var aParams = CreateAssignmentParams(ref updateCommand, columns.Length);
                updateCommand += $" WHERE ROWID = {id};";
                try
                {
                    sqlCommand = new SQLiteCommand(updateCommand, connection, transaction);
                    FillAssignmentParamsToConnection(sqlCommand, aParams, columns, newValues);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    TableUpdated?.Invoke(tableName);
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to update {tableName}.{string.Join(", ",columns)} REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred while attempting to update values. REASON: {ex}");
                return false;
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
            }
            return true;
        }
        /// <summary>
        /// Updates a product record using values from the <paramref name="newRecord"/> attributes at <paramref name="pid"/>.
        /// </summary>
        /// <param name="pid">The product record ID to update.</param>
        /// <param name="newRecord">The newly constructed DPProductRecord with new values to insert/update.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool UpdateProductRecord(uint pid, DPProductRecord newRecord, SQLiteConnection? c, CancellationToken t)
        {
            var pColumns = new string[] { "Product Name", "Tags", "Author", "SKU", "Date Created", "Thumbnail Full Path", "Extraction Record ID" };
            if (t.IsCancellationRequested || newRecord == null || newRecord == DPProductRecord.NULL_RECORD || pid < 0)
                return false;
            newRecord.Deconstruct(out var productName, out var tags, out var author, out var sku,
                                 out DateTime time, out var thumbnailPath, out var eid, out var _);

            // Shorten strings if applicable.
            productName = productName?.Length > 70 ? productName.Substring(0, 70) : productName;
            author = author?.Length > 30 ? author.Substring(0, 30) : author;
            sku = sku?.Length > 10 ? sku.Substring(0, 10) : sku;

            var pObjs = new object[] { productName, JoinString(", ", 70, tags), author, sku, time.ToFileTimeUtc(), thumbnailPath, eid };
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();

                StringBuilder updateCommand = new(250);
                updateCommand.Append("UPDATE ProductRecords SET ");
                for (var i = 0; i < 7; i++)
                {
                    updateCommand.Append($"\"{pColumns[i]}\" = @A{i}");
                    if (i + 1 != 7) updateCommand.Append(", ");
                }
                updateCommand.Append($" WHERE ID = {pid};");
                try
                {
                    sqlCommand = new SQLiteCommand(updateCommand.ToString(), connection, transaction);
                    for (var i = 0; i < 7; i++)
                    {
                        sqlCommand.Parameters.Add(new SQLiteParameter("@A" + i, pObjs[i]));
                    }
                    if (!RemoveTags(pid, connection, t)) return false;
                    // Temporarly disable triggers.
                    TempDeleteTriggers(connection, t);
                    InsertTags(tags, pid, connection, t);
                    RestoreTriggers(connection, t);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to update {newRecord.ProductName} entry (ProductRecord). REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred while attempting to update product record {newRecord.ProductName}. REASON: {ex}");
                return false;
            }
            finally
            {
                RestoreTriggers(connection, t);
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
                try { transaction?.Dispose(); } catch { } // not sure if this is needed if connection is disposed and transaction didn't commit.
            }
            return true;
        }
        /// <summary>
        /// Updates an extraction record using values from the <paramref name="newRecord"/> attributes at <paramref name="eid"/>.
        /// </summary>
        /// <param name="eid">The product record ID to update.</param>
        /// <param name="newRecord">The newly constructed DPExtractionRecord with new values to insert/update.</param>
        /// <param name="c">The SQLiteConnection to use, if any.</param>
        /// <param name="t">Cancel token. Required, cannot be null. Use CancellationToken.None instead (though not recommended).</param>
        /// <returns>Whether the insertion was successful (true) or not (false).</returns>
        private bool UpdateExtractionRecord(uint eid, DPExtractionRecord newRecord, SQLiteConnection? c, CancellationToken t)
        {
            var eColumns = new string[] { "Archive Name", "Files", "Folders", "Destination Path", "Errored Files", "Error Messages", "Product Record ID" };
            if (t.IsCancellationRequested || newRecord == null || newRecord == DPExtractionRecord.NULL_RECORD || eid < 0)
                return false;
            newRecord.Deconstruct(out var archiveFileName, out var destPath, out var files,
                out var erroredFiles, out var erroredMessages, out var folders, out var newPID);
            var eObjs = new object[] { archiveFileName, JoinString(", ", 384, files),
                JoinString(", ", 384, folders), destPath, JoinString(", ", 384, erroredFiles),
                JoinString(", ", 65536, erroredMessages), newPID };
            SQLiteConnection? connection = null;
            SQLiteTransaction transaction = null;
            SQLiteCommand sqlCommand = null;
            try
            {
                connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                transaction = connection.BeginTransaction();

                StringBuilder updateCommand = new(250);
                updateCommand.Append("UPDATE ExtractionRecords SET ");
                for (var i = 0; i < 7; i++)
                {
                    updateCommand.Append($"\"{eColumns[i]}\" = @A{i}");
                    if (i + 1 != 7) updateCommand.Append(", ");
                }
                updateCommand.Append($" WHERE ID = {eid};");
                try
                {
                    sqlCommand = new SQLiteCommand(updateCommand.ToString(), connection, transaction);
                    for (var i = 0; i < 7; i++)
                    {
                        sqlCommand.Parameters.Add(new SQLiteParameter("@A" + i, eObjs[i]));
                    }
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to update {newRecord.ArchiveFileName} entry (ExtractionRecord). REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                // DPCommon.WriteToLog($"An unexpected error occurred while attempting to update product record {newRecord.ArchiveFileName}. REASON: {ex}");
                return false;
            }
            finally
            {
                if (c is null)
                {
                    connection?.Dispose();
                    sqlCommand?.Dispose();
                }
            }
            return true;
        }
        #endregion
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
                using SQLiteConnection? connection = CreateAndOpenConnection(null, false);
                if (connection == null) return;
                using SQLiteCommand cmd = new(pragmaCheckpoint, connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { return; } // We don't want to delete if it failed.

            // Now check if -wal and -shm are available.
            var shmFile = System.IO.Path.GetFullPath(Path + "-shm");
            var walFile = System.IO.Path.GetFullPath(Path + "-wal");

            // This is required for the SQLiteConnection to truly release the handle on 
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
        private string? JoinString(string seperator, int maxSize = 256, params string[] values)
        {
            if (values == null || values.Length == 0) return null;

            StringBuilder builder = new(512);
            for (var i = 0; i < values.Length; i++)
            {
                var s = values[i];
                if (string.IsNullOrWhiteSpace(s)) continue;
                builder.Append(s.Length > maxSize ? s[..(maxSize - 1)] : s);
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
        /// Associates parameter placeholds with a value to the SQLiteCommand. You should call <c>CreateParams()</c> before
        /// to generate the parameter list to include into <paramref name="cArgs"/> and update the command string.
        /// For example, if the args is {"@A1", "@A2", "@A3"} and the values are {"hello", "solomon", "blount"}, then 
        /// the args will be replaced with its values when the query is executed.
        /// </summary>
        /// <param name="command">The command to add parameters into. Cannot be null.</param>
        /// <param name="cArgs">The argument placeholders to fill. Cannot be null.</param>
        /// <param name="values">The values to replace placeholders with. Cannot be null.</param>
        private void FillParamsToConnection(SQLiteCommand command, IReadOnlyList<string> cArgs, params object[] values)
        {
            for (var i = 0; i < cArgs.Count; i++)
            {
                command.Parameters.Add(new SQLiteParameter(cArgs[i], values[i]));
            }
        }
        /// <summary>
        /// Associates parameter placeholds with a value to the SQLiteCommand. You should call <c>CreateAssignmentParams()</c> before
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
        private void FillAssignmentParamsToConnection(SQLiteCommand command, IReadOnlyList<string> cArgs, object[] leftVals, object[] rightVals)
        {
            if (leftVals.Length != rightVals.Length)
            {
                // DPCommon.WriteToLog("FillAssignmentParamsToConnection() did not fill parameters due to unequal lengths in values.");
                // DPCommon.WriteToLog($"leftVals Length: {leftVals.Length} | rightVals Length: {rightVals.Length} ");
            }
            for (int i = 0, j = 0; i < cArgs.Count; i += 2, j++)
            {
                command.Parameters.Add(new SQLiteParameter(cArgs[i], leftVals[j]));
                command.Parameters.Add(new SQLiteParameter(cArgs[i + 1], rightVals[j]));
            }
        }
        /// <summary>
        /// Deprecated and should not be used.
        /// </summary>
        /// <see cref="CreateParams"/>
        /// <see cref="FillParamsToConnection"/>
        /// <param name="args">Arguments to wrap quotes over.</param>
        /// <returns>A string ready to use for a command.</returns>

        private string ConvertParamsToString(params object[] args)
        {
            if (args.Length == 0) return string.Empty;
            var sArgs = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                Type type = arg.GetType();
                if (type == typeof(string)) sArgs[i] = '"' + (string)arg + '"';
                else if (type == typeof(char)) sArgs[i] = '"' + (string)arg + '"';
                else sArgs[i] = Convert.ToString(arg);
            }
            return string.Join(", ", sArgs);
        }
        #endregion
    }
}
