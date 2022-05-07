using System.Text;
using System.Data.SQLite;

internal static class Database
{
    internal static bool Initialized = false;
    internal static string DatabasePath = "";
    internal static SQLiteConnection Connection = new SQLiteConnection();
    internal static string ConnectionString = "";
    private static Task lastTask;
    private static string[] pColumns = new string[] { "Product Name", "Tags", "Author", "SKU", "Date Created", "Thumbnail Full Path", };
    private static string[] eColumns = new string[] { "Archive Name", "Files", "Folders", "Destination Path", "Errored Files", "Error Messages" };
    private static string[] pCols = new string[] { "Product Name", "Tags", "Author", "Date Created", "Thumbnail Full Path" };
    private static string[] eCols = new string[] { "Files", "Folders", "Destination Path", "Archive Name" };
    public static void Initialize(string databasePath)
    {
        DatabasePath = databasePath;
        ConnectionString = "Data Source = " + "\"" + Path.GetFullPath(databasePath) + "\"";
        Connection.ConnectionString = ConnectionString;
        Test();
    }

    public static void QueueInsertion(object[] pVals, object[] eVals, string[] tags)
    {
        InsertValuesToTable("ProductRecords", pColumns, pVals);
        InsertValuesToTable("ExtractionRecords", eColumns, eVals);
        CreateTags(tags);
        
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

    private static void FillParamsToConnection(ref SQLiteCommand command, string[] cArgs, params object[] values)
    {
        for (var i = 0; i < cArgs.Length; i++)
        {
            command.Parameters.AddWithValue(cArgs[i], values[i]);
        }
    }

    private static void Test()
    {
        if (Connection.State != System.Data.ConnectionState.Open)
            Connection.Open();
        var cmd = "SELECT * FROM ProductRecords;";
        using (var sqlcmd = new SQLiteCommand(cmd, Connection))
        {
            using (var reader = sqlcmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetValue(0));
                }
            }
        }
    }

    private static uint GetLastProductID()
    {
        var c = "SELECT ID FROM ProductRecords ORDER BY ID DESC LIMIT 1;";
        try
        {
            using (var cmd = new SQLiteCommand(c, Connection))
            {
                return Convert.ToUInt32(cmd.ExecuteScalar());
            }
        }
        catch (Exception ex)
        {
            Program.Print("Failed to get last product ID.");
        }
        return 0;
    }

    private static void CreateTags(string[] tags)
    {
        uint pid = GetLastProductID();
        if (pid == 0)
        {
            Program.Print("Product ID returned 0; no tags added.");
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

        InsertMultipleValuesToTable("Tags", new string[] { "Tag", "Product Record ID" }, vals);
    }

    private static bool InsertMultipleValuesToTable(string tableName, string[] columns, object[][] values)
    {
        if (values.Length == 0) return true;
        if (columns == null || columns.Length == 0) return false;

        SQLiteTransaction transaction = Connection.BeginTransaction();
        // Build columns.
        // Wrap in quotes
        columns = (string[]) columns.Clone();
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
        try { 
            builder.Remove(builder.Length - 3, 2);
        } catch (Exception ex) { }
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
            var sqlCommand = new SQLiteCommand(insertCommand, Connection, transaction);
            FillParamsToConnection(ref sqlCommand, args.ToArray(), valsFlattened.ToArray());
            sqlCommand.ExecuteNonQuery();
            transaction.Commit();
            transaction.Dispose();
        }
        catch (Exception ex)
        {
            Program.Print($"Failed to insert values to {columnsToAdd}. REASON: {ex}");
            transaction.Rollback();
            return false;
        }

        return true;
    }

    private static bool InsertValuesToTable(string tableName, string[] columns, object[] values)
    {
        columns = (string[]) columns.Clone();
        if (Connection.State != System.Data.ConnectionState.Open)
            Connection.Open();
        SQLiteTransaction transaction = Connection.BeginTransaction();
        // Build columns.
        // Wrap in quotes
        for (var i = 0; i < columns.Length; i++)
        {
            columns[i] = new string($"\"{columns[i]}\"");
        }
        var columnsToAdd = string.Join(',', columns);

        // TODO: Append params.
        var insertCommand = $"INSERT INTO {tableName} ({columnsToAdd})\n VALUES(";
        var args = CreateParams(ref insertCommand, values.Length);
        insertCommand += ")";
        try
        {
            var sqlCommand = new SQLiteCommand(insertCommand, Connection, transaction);
            FillParamsToConnection(ref sqlCommand, args, values);
            sqlCommand.ExecuteNonQuery();
            transaction.Commit();
            transaction.Dispose();
            sqlCommand.Dispose();
        }
        catch (Exception ex)
        {
            Program.Print($"Failed to insert values to {columnsToAdd}. REASON: {ex}");
            transaction.Rollback();
            return false;
        }

        return true;

    }

    public static void Close()
    {
        var pragmaCheckpoint = "PRAGMA wal_checkpoint(TRUNCATE);";
        try
        {
            using (var cmd = new SQLiteCommand(pragmaCheckpoint, Connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex) { }
        finally
        {
            Connection.Close();
        }
    }
}

