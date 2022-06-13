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
        #region Reads
        private static void UpdateProductRecordCount(SQLiteConnection connection, CancellationToken t)
        {
            const string getCmd = @"SELECT ""Product Record Count"" FROM DatabaseInfo;";
            if (t.IsCancellationRequested) return;

            try
            {
                using (var cmd = new SQLiteCommand(getCmd, connection))
                {
                    ProductRecordCount = Convert.ToUInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"An unexpected error occurred while attempting to get product record count. REASON: {e}");
            }
            DPCommon.WriteToLog("Product Record Count: ", ProductRecordCount);
        }

        private static void UpdateExtractionRecordCount(SQLiteConnection connection, CancellationToken t)
        {
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

        private static DPProductRecord[] SearchProductRecordsViaTagsS(SQLiteCommand command, CancellationToken t)
        {
            if (t.IsCancellationRequested)
            {
                return Array.Empty<DPProductRecord>();
            }
            var reader = command.ExecuteReader();

            var searchResults = new List<DPProductRecord>(reader.StepCount);
            string productName, author, thumbnailPath, sku;
            string[] tags;
            DateTime dateCreated;
            uint extractionID, pid;

            // TODO : Use new product search record.
            while (reader.Read())
            {
                if (t.IsCancellationRequested)
                    return Array.Empty<DPProductRecord>();
                // Construct product records
                // NULL values return type DB.NULL.
                productName = (string)reader["Product Name"];
                // TODO: Tags have returned null; investigate why.
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

        private static string[] GetColumns(string tableName, SQLiteConnection c, 
            CancellationToken t)
        {
            if (t.IsCancellationRequested || tableName.Length == 0) return Array.Empty<string>();
            if (_columnsCache.ContainsKey(tableName)) return _columnsCache[tableName];

            try
            {
                using (var connection = CreateAndOpenConnection(c, true))
                {
                    var success = OpenConnection(connection);
                    if (!success) return Array.Empty<string>();

                    var randomCommand = $"SELECT * FROM {tableName} LIMIT 1;";
                    var sqlCommand = new SQLiteCommand(randomCommand, connection);
                    var reader = sqlCommand.ExecuteReader();
                    var table = reader.GetSchemaTable();

                    List<string> columns = new List<string>();
                    foreach (DataRow row in table.Rows)
                    {
                        if (t.IsCancellationRequested) return Array.Empty<string>();
                        columns.Add((string)row.ItemArray[0]);
                    }

                    // Cache it.
                    _columnsCache[tableName] = columns.ToArray();
                    return _columnsCache[tableName];

                }
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"An unexpected error occurred attempting to get columns for table: {tableName}. REASON: {e}");
            }
            return Array.Empty<string>();
        }


        private static string[] GetTables(SQLiteConnection c, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Array.Empty<string>();
            var tables = new List<string>();
            try
            {
                using (var connection = CreateAndOpenConnection(c, true))
                {
                    if (connection == null) return Array.Empty<string>();
                    var randomCommand = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'";
                    var sqlCommand = new SQLiteCommand(randomCommand, connection);
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                            tables.Add(reader.GetString(0));
                    }
                    UpdateProductRecordCount(connection, cancellationToken);
                    UpdateExtractionRecordCount(connection, cancellationToken);
                    return tables.ToArray();
                }
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"An unexpected error occurred attempting to get table names. REASON: {e}");
            }
            return Array.Empty<string>();

        }

        private static DPExtractionRecord? GetExtractionRecord(uint id, SQLiteConnection c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return null;

            var getCmd = $"SELECT * FROM ExtractionRecords WHERE ID = {id};";
            try
            {
                using var connection = CreateAndOpenConnection(c, true);
                if (connection == null) return null;
                var cmd = new SQLiteCommand(getCmd, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string[] files, folders, erroredFiles, errorMessages;
                        string archiveFileName = (string)reader["Archive Name"];
                        string filesStr = reader["Files"] as string;
                        string foldersStr = reader["Folders"] as string;
                        string destinationPath = (string)reader["Destination Path"];
                        string erroredFilesStr = reader["Errored Files"] as string;
                        string errorMessagesStr = reader["Error Messages"] as string;
                        uint pid = Convert.ToUInt32(reader["Product Record ID"]);

                        files = filesStr != null ? files = filesStr.Split(", ") : Array.Empty<string>();
                        folders = foldersStr != null ? folders = foldersStr.Split(", ") : Array.Empty<string>();
                        erroredFiles = erroredFilesStr != null ? erroredFiles = erroredFilesStr.Split(", ") : Array.Empty<string>();
                        errorMessages = errorMessagesStr != null ? errorMessages = erroredFilesStr.Split(", ") : Array.Empty<string>();

                        var record = new DPExtractionRecord(archiveFileName, destinationPath, files, 
                            erroredFiles, errorMessages, folders, pid);
                        return record;
                    }
                }
                DPCommon.WriteToLog("Failed to get extraction record possibly due to extraction record was deleted.");
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to get extraction record. REASON: {ex}");
            }
            return null;
        }

        private static HashSet<string> GetArchiveFileNameList(SQLiteConnection c, CancellationToken t)
        {
            HashSet<string> names = null;
            var getCmd = @"SELECT ""Archive Name"" FROM ExtractionRecords;";
            try
            {
                using (var _connection = CreateAndOpenConnection(c, true))
                {
                    if (_connection == null) return names;
                    using (var cmd = new SQLiteCommand(getCmd, _connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            names = new HashSet<string>();
                            while (reader.Read())
                            {
                                names.Add(reader.GetString(0));
                            }
                        }
                    }
                }
                ArchiveFileNames = names;

            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to get archive file name list. REASON: {ex}");
            }
            return names;
        }

        private static uint GetLastProductID(SQLiteConnection conn, CancellationToken t)
        {
            if (t.IsCancellationRequested) return 0;
            var c = "SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1;";
            try
            {
                using (var connection = CreateAndOpenConnection(conn, true))
                {
                    using (var cmd = new SQLiteCommand(c, connection))
                    {
                        return Convert.ToUInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to get last product ID. REASON: {ex}");
            }
            return 0;
        }

        private static DataSet? GetAllValuesFromTable(string tableName, SQLiteConnection c, 
            CancellationToken token)
        {
            if (token.IsCancellationRequested) return null;
            try
            {
                using (var connection = CreateAndOpenConnection(c, true))
                {
                    var getCommand = $"SELECT * FROM {tableName}";
                    var sqlCommand = new SQLiteCommand(getCommand, connection);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlCommand);
                    DataSet dataset = new DataSet(tableName);
                    adapter.Fill(dataset);
                    return dataset;
                }
            }
            catch (Exception ex){
                DPCommon.WriteToLog($"Failed to get all values from table. REASON: {ex}");
            }
            return null;
        }
        #endregion
        #region Writes
        #region Remove
        private static bool RemoveAllRecords(SQLiteConnection c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;

            // Also deletes from tags via trigger.
            var deleteCommand = $"DELETE FROM ProductRecords; DELETE FROM ExtractionRecords;"; // Faster way is to drop the table & re-make it.
            try
            {
                using (var connection = CreateAndOpenConnection(c))
                {
                    if (connection == null) return false;
                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        if (DeleteTriggers())
                        {
                            var sqlCommand = new SQLiteCommand(deleteCommand, connection, transaction);
                            sqlCommand.ExecuteNonQuery();
                            transaction.Commit();
                            transaction.Dispose();
                            CreateTriggers();
                            DatabaseUpdated?.Invoke();
                        }
                    } catch (Exception ex)
                    {
                        DPCommon.WriteToLog($"Failed to delete records. REASON: {ex}");
                        transaction.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to create and begin transcation. REASON: {ex}");
                return false;
            }

            return true;
        }


        private static bool RemoveProductRecordsViaTag(string[] values, SQLiteConnection c,
            CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            if (values.Length == 0) return true;

            string args = ConvertParamsToString(values);
            string idsCommand = $"SELECT \"Product Record ID\" FROM Tags WHERE Tag IN ({args})";
            string deleteCommand = $"DELETE FROM ProductRecords WHERE ID IN ({idsCommand});";
            try
            {
                using var connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction();
                try
                {
                    var sqlCommand = new SQLiteCommand(deleteCommand, connection, transaction);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    transaction.Dispose();
                    DatabaseUpdated?.Invoke();
                } catch (Exception ex)
                {
                    DPCommon.WriteToLog($"Failed to delete from ProductRecords where ID IN {args}. REASON: {ex.Message}");
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to create connection and transaction. REASON: {ex}");
                return false;
            }

            return true;
        }
        private static bool RemoveValuesWithCondition(string tableName, Tuple<string, object>[] conditions, 
            bool or, SQLiteConnection c, CancellationToken t)
        {
            // Build columns.
            string whereCommand = "";

            for (var i = 0; i < conditions.Length; i++)
            {
                var tuple = conditions[i];
                var column = tuple.Item1;
                var item = tuple.Item2;
                if (i == 0)
                {
                    whereCommand += $"WHERE {column} IN ({ConvertParamsToString(item)})";
                }
                else
                {
                    if (or)
                    {
                        whereCommand += $"OR WHERE {column} IN ({ConvertParamsToString(item)})";
                    }
                    else
                    {
                        whereCommand += $"AND WHERE {column} IN ({ConvertParamsToString(item)})";
                    }
                }
            }

            string deleteCommand = $"DELETE FROM {tableName} {whereCommand};";
            try
            {
                using var connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction();
                try
                {
                    var sqlCommand = new SQLiteCommand(deleteCommand, connection, transaction);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    transaction.Dispose();
                    DatabaseUpdated?.Invoke();
                } catch (Exception ex)
                {
                    DPCommon.WriteToLog($"Failed to delete from {tableName} {whereCommand}. REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to create connection and transcation. REASON: {ex}");
            }

            return true;
        }
        private static bool RemoveAllFromTable(string tableName, SQLiteConnection c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;

            
            var deleteCommand = $"DELETE FROM {tableName};"; // Faster way is to drop the table & re-make it.
            try
            {
                using var connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction();
                try
                {
                    var sqlCommand = new SQLiteCommand(deleteCommand, connection, transaction);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    transaction.Dispose();
                    DatabaseUpdated?.Invoke();
                } catch (Exception ex)
                {
                    DPCommon.WriteToLog($"Failed delete all values for table: {tableName}. REASON: {ex.Message}");
                    transaction.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to create connection and transaction. REASON: {ex}");
                return false;
            }

            return true;
        }

        private static void RemoveTags(uint pid, CancellationToken t)
        {
            RemoveValuesWithCondition("Tags",
                    new Tuple<string, object>[] { new Tuple<string, object>("Product Record ID", pid) }
                    , false, null, t);
        }

        #endregion
        #region Insert
        private static void InsertTags(string[] tags, SQLiteConnection conn, CancellationToken t)
        {
            if (t.IsCancellationRequested) return;

            try
            {
                using var connection = CreateAndOpenConnection(conn);
                if (connection == null) return;

                uint pid = GetLastProductID(connection, t);
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

                InsertMultipleValuesToTable("Tags", new string[] { "Tag", "Product Record ID" }, 
                    vals, connection, t);

            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to insert tags. REASON: {ex}");
            }

        }
        private static bool InsertMultipleValuesToTable(string tableName, string[] columns, object[][] values,
            SQLiteConnection c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;

            try
            {
                using var connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction();
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
                    var sqlCommand = new SQLiteCommand(insertCommand, connection, transaction);
                    FillParamsToConnection(sqlCommand, args, valsFlattened);
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
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An unexpected error occurred while inserting multiple values to table. REASON: {ex}");
            }


            return true;
        }

        private static bool InsertDefaultValuesToTable(string tableName, SQLiteConnection c, 
            CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            try
            {
                using var connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction();
                var insertCommand = $"INSERT INTO {tableName} DEFAULT VALUES;";
                try
                {
                    var sqlCommand = new SQLiteCommand(insertCommand, connection, transaction);
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
            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An unexpected error occurred while inserting default values. REASON: {ex}");
                return false;
            }
            // TODO: Append params.

            return true;
        }

        private static bool InsertValuesToTable(string tableName, string[] columns, object[] values,
            SQLiteConnection c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;

            try
            {
                using var connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction();

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
                    var sqlCommand = new SQLiteCommand(insertCommand, connection, transaction);
                    FillParamsToConnection(sqlCommand, args, values);
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

            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"Failed to insert values to table. REASON: {ex}");
                return false;
            }

            return true;

        }
        private static void InsertRecords(DPProductRecord pRecord, DPExtractionRecord eRecord, 
            SQLiteConnection c, CancellationToken t)
        {
            // Trigger will update the product record's extraction record ID to the newly created record.
            string[] pColumns = new string[] { "Product Name", "Tags", "Author", "SKU", "Date Created", "Thumbnail Full Path", };
            string[] eColumns = new string[] { "Archive Name", "Files", "Folders", "Destination Path", "Errored Files", "Error Messages" };
            if (t.IsCancellationRequested) return;

            pRecord.Deconstruct(out var productName, out var tags, out var author, out var sku,
                                 out var time, out var thumbnailPath, out var __, out var _);
            eRecord.Deconstruct(out var archiveFileName, out var destPath, out var files,
                out var erroredFiles, out var erroredMessages, out var folders, out _);
            // We do not care about UID.
            // Order must match pColumns / eColumns
            try
            {
                using var connection = CreateAndOpenConnection(c);
                if (connection == null) return;

                object[] pObjs = new object[] { productName, JoinString(", ", tags), author, sku, time.ToFileTimeUtc(), thumbnailPath };
                object[] eObjs = new object[] { archiveFileName, JoinString(", ", files),
                JoinString(", ", folders), destPath, JoinString(", ", erroredFiles),
                JoinString(", ", erroredMessages) };

                // If both operations are successful, emit signal.
                if (InsertValuesToTable("ProductRecords", pColumns, pObjs, connection, t))
                {
                    if (eRecord != DPExtractionRecord.NULL_RECORD)
                    {
                        InsertTags(tags, connection, t);
                        InsertValuesToTable("ExtractionRecords", eColumns, eObjs, connection, t);
                    }
                    DatabaseUpdated?.Invoke();
                }
            }
            catch (Exception ex) {
                DPCommon.WriteToLog($"An unexpected error occurred while attempting to insert records. REASON: {ex}");
            }
        }
        #endregion
        #region Update
        private static bool UpdateValues(string tableName, string[] columns, object[] newValues, 
            SQLiteConnection c, CancellationToken t)
        {
            if (t.IsCancellationRequested) return false;
            try
            {
                using var connection = CreateAndOpenConnection(c);
                if (connection == null) return false;
                using var transaction = connection.BeginTransaction();

                columns = columns?.Length == 0 ? GetColumns(tableName, connection, t) : columns;
                if (t.IsCancellationRequested || columns == null ||
                    columns.Length == 0 || columns.Length != newValues.Length) return false;

                // Build columns.
                var columnsToAdd = string.Join(',', columns);
                var valuesToAdd = string.Join(',', newValues);
                var insertCommand = $"INSERT INTO {tableName} ({columnsToAdd})\nVALUES({valuesToAdd});";
                try
                {
                    var sqlCommand = new SQLiteCommand(insertCommand, connection, transaction);
                    sqlCommand.ExecuteNonQuery();
                    transaction.Commit();
                    transaction.Dispose();
                    DatabaseUpdated?.Invoke();
                }
                catch (Exception ex)
                {
                    DPCommon.WriteToLog($"Failed to update {valuesToAdd} to {columnsToAdd}. REASON: {ex}");
                    transaction.Rollback();
                    return false;
                }

            } catch (Exception ex)
            {
                DPCommon.WriteToLog($"An unexpected error occurred while attempting to update values. REASON: {ex}");
                return false;
            }
            return true;
        }
        #endregion
        #endregion
        #region etc
        private static void TruncateJournal()
        {
            var pragmaCheckpoint = "PRAGMA wal_checkpoint(TRUNCATE);";
            try
            {
                using var connection = CreateAndOpenConnection(null, false);
                if (connection == null) return;
                using var cmd = new SQLiteCommand(pragmaCheckpoint, connection);
                cmd.ExecuteNonQuery();
            }
            catch { }

            // Now check if -wal and -shm are available.
            var shmFile = Path.GetFullPath(_expectedDatabasePath + "-shm");
            var walFile = Path.GetFullPath(_expectedDatabasePath + "-wal");

            // TODO: Doesn't delete.
            try
            {
                if (File.Exists(shmFile)) File.Delete(shmFile);
                if (File.Exists(walFile)) File.Delete(walFile);
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
            return builder.ToString();
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
            str += sb.ToString();
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
            str += sb.ToString();
            return args;
        }

        private static void FillParamsToConnection(SQLiteCommand command, IReadOnlyList<string> cArgs, params object[] values)
        {
            for (var i = 0; i < cArgs.Count; i++)
            {
                command.Parameters.Add(new SQLiteParameter(cArgs[i], values[i]));
            }
        }

        private static string ConvertParamsToString(params object[] args)
        {
            if (args.Length == 0) return string.Empty;
            string[] sArgs = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var type = arg.GetType();
                if (type == typeof(string)) sArgs[i] = '"' + (string)arg + '"';
                else if (type == typeof(char)) sArgs[i] = '"' + (string)arg + '"';
                else sArgs[i] = Convert.ToString(arg);
            }
            return string.Join(", ", sArgs);
        }
        #endregion
    }
}
