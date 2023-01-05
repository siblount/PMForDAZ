using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SQLite;
using DAZ_Installer.Core;

namespace DAZ_Installer.Database
{
    public static partial class DPDatabase
    {
        // This section is set up as an interface for other classes. You should use these methods
        // to get data. These methods can callback if a callback is specified and emit an event.
        // If you want to listen through an event, pass a constant caller id.
        // Example: a constant caller ID for DPLibrary = 3.
        #region Public methods
        // TO DO: Improve. This can be so much more efficient.
        // I lack the brain capacity to do this at the moment.

        /// <summary>
        /// Search asynchroniously does a database search based on a user search query (ex: "hello world"). Do not 
        /// use this function for regex expressions; Instead use <c>RegexSearch</c>. This function does not return
        ///  the search results but instead returns it to the callback and/or an event. If you wish to retrieve 
        /// the values through an event, use a caller ID to identify the event is for that particular class at that
        /// time.
        /// </summary>
        /// <param name="searchQuery">A user search query.</param>
        /// <param name="sortMethod">The sorting method to apply for results.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        public static void SearchQ(string searchQuery, DPSortMethod sortMethod = DPSortMethod.None,
            uint callerID = 0, Action<DPProductRecord[]> callback = null)
        {
            // We only want to do searches on one thread. Calling priority task manager ensures
            // we only do searches on one thread.
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue((t) => {
                var results = DoSearchS(searchQuery, sortMethod, null, t);
                callback?.Invoke(results);
                SearchUpdated?.Invoke(results, callerID);
            });
        }

        /// <summary>
        /// RegexSearch asynchroniously does a database search based on a user regex pattern (ex: "[^abc]").
        /// This function does not return the search results but instead returns it to the callback and/or an event. 
        /// If you wish to retrieve the values through an event, use a caller ID to identify the event is for that particular
        /// class at that time.
        /// </summary>
        /// <param name="regex">A regex expression.</param>
        /// <param name="sortMethod">The sorting method to apply for results.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        public static void RegexSearchQ(string regex, DPSortMethod sortMethod = DPSortMethod.None,
            uint callerID = 0, Action<DPProductRecord[]> callback = null)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue((t) => {
                var results = DoRegexSearchS(regex, sortMethod, null, t);
                callback?.Invoke(results);
                SearchUpdated?.Invoke(results, callerID);
            });
        }
        /// <summary>
        /// Gets product records on the page specified by <paramref name="page"/>. The page is determined by the 
        /// limit of <paramref name="limit"/>. This means that if there are 50 records, and the limit is 10, the max
        /// amount of pages is 5. This function does not return any results but will return the results via the callback
        /// and via MainQueryCompleted event.
        /// </summary>
        /// <param name="sortMethod">The sorting method to apply to the results.</param>
        /// <param name="page">The page to get product records from.</param>
        /// <param name="limit">The max amount of product records per page; the max amount of records to receive.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        public static void GetProductRecordsQ(DPSortMethod sortMethod, uint page = 1, uint limit = 0,
            uint callerID = 0, Action<DPProductRecord[]> callback = null)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue((t) => {
                var results = DoLibraryQuery(page, limit, sortMethod, null, t);
                callback?.Invoke(results);
                MainQueryCompleted?.Invoke(callerID);
            });
        }

        /// <summary>
        /// Stops the pending chain of main queries such as insert, update, and delete queries.
        /// </summary>
        public static void StopMainDatabaseOperations() => _mainTaskManager.Stop();

        /// <summary>
        /// Stops the pending chain of main queries such as insert, update, delete, and get queries. 
        /// This also stops pending search queries and "view" queries.
        /// </summary>
        public static void StopAllDatabaseOperations()
        {
            _mainTaskManager.Stop();
            _priorityTaskManager.Stop();
        }

        #endregion
        #region Queryable methods

        /// <summary>
        /// If `forceRefresh` is false, the refresh action will be queued. Otherwise, the action queue will be cleared and database will be refreshed immediately.
        /// </summary>
        /// <param name="forceRefresh">Refreshes immediately if True, otherwise it is queued.</param>
        public static void RefreshDatabaseQ(bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                _mainTaskManager.Stop();
                _priorityTaskManager.Stop();
                Initalized = false;
                Initialize();
                _columnsCache.Clear();
            }
            else
            {
                _mainTaskManager.AddToQueue(RefreshDatabase);
            }
        }

        /// <summary>
        /// Returns all the values from the table specified by the <paramref name="tableName"/> table via the 
        /// callback or via ViewUpdated event. This may return null.
        /// </summary>
        /// <param name="tableName">The table name to </param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return the dataset to.</param>
        public static void ViewTableQ(string tableName, uint callerID = 0, Action<DataSet?> callback = null)
        {
            _mainTaskManager.AddToQueue((t) => {
                var result = GetAllValuesFromTable(tableName, null, t);
                callback?.Invoke(result);
                ViewUpdated?.Invoke(result, callerID);
            });
        }
        /// <summary>
        /// Queries/Adds a new product record to the database.
        /// </summary>
        /// <param name="pRecord">The new product record to add.</param>
        public static void AddNewRecordEntry(DPProductRecord pRecord)
        {
            _mainTaskManager.AddToQueue(InsertRecords, pRecord, null as DPExtractionRecord, null as SQLiteConnection);
        }
        /// <summary>
        /// Queries/Adds a new product and extraction record to the database.
        /// </summary>
        /// <param name="pRecord">The new product record to add.</param>
        /// <param name="eRecord">The new extraction record to add.</param>
        public static void AddNewRecordEntry(DPProductRecord pRecord, DPExtractionRecord eRecord)
        {
            _mainTaskManager.AddToQueue(InsertRecords, pRecord, eRecord, null as SQLiteConnection);
        }
        /// <summary>
        /// Inserts a new row to the table specified by <paramref name="tableName">. It requires the columns that
        /// new values will be inserted into. Columns and values length must match.
        /// For example, if you want to insert a new row to ProductRecors but only want to insert the name for now,
        /// columns would be <c>{"Name"}</c> and values would be <c>{"poppy stick"}</c>. Columns do not have to match
        /// the columns in the database, but should include the required, non-null columns.
        /// </summary>
        /// <param name="tableName">The table to insert values into.</param>
        /// <param name="values">The values to insert.</param>
        /// <param name="columns">The corresponding columns to insert values to.</param>
        public static void InsertNewRowQ(string tableName, object[] values, string[] columns)
        {
            _mainTaskManager.AddToQueue(InsertValuesToTable, tableName, columns, values, null as SQLiteConnection);
        }
        /// <summary>
        /// Removes a row by it's ID at the table specifed by <paramref name="tableName"/>.
        /// </summary>
        /// <param name="tableName">The table to insert values into.</param>
        /// <param name="id">The ID of the row to remove.</param>
        public static void RemoveRowQ(string tableName, int id)
        {
            var arg = new Tuple<string, object>[1] { new Tuple<string, object>("ID", id) };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, arg, false, null as SQLiteConnection);
        }

        public static void RemoveProductRecord(DPProductRecord record, Action<uint> callback = null)
        {
            _mainTaskManager.AddToQueue((t) =>
            {
                var arg = new Tuple<string, object>[1] { new Tuple<string, object>("ID", Convert.ToInt32(record.ID)) };
                var success = RemoveValuesWithCondition("ProductRecords", arg, false, null as SQLiteConnection, t);
                if (success)
                {
                    callback?.Invoke(record.ID);
                    ProductRecordRemoved?.Invoke(record.ID);
                    ExtractionRecordRemoved?.Invoke(record.EID);
                }
            });
        }

        /// <summary>
        /// Removes all the values from the table. Triggers in the database are temporarly disabled for deleting.
        /// </summary>
        /// <param name="tableName"></param>
        public static void ClearTableQ(string tableName)
        {
            _mainTaskManager.AddToQueue(RemoveAllFromTable, tableName, null as SQLiteConnection);
        }

        /// <summary>
        /// Not fully implemented. Do not use.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        /// <param name="columns"></param>
        public static void UpdateValuesQ(string tableName, object[] values, string[] columns, int id)
        {
            _mainTaskManager.AddToQueue(UpdateValues, tableName, columns, values, id, null as SQLiteConnection);
        }

        /// <summary>
        /// Updates a product record and extraction record. This is currently used for applying changes from the product
        /// record form.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newProductRecord"></param>
        /// <param name="newExtractionRecord"></param>
        public static void UpdateRecordQ(uint id, DPProductRecord newProductRecord, DPExtractionRecord newExtractionRecord, Action<uint> callback = null)
        {
            _mainTaskManager.AddToQueue(t =>
            {
                var success = UpdateProductRecord(id, newProductRecord, null, t);
                if (!success) return;
                success = UpdateExtractionRecord(id, newExtractionRecord, null, t);
                if (!success) return;
                callback?.Invoke(newProductRecord.ID);
                ProductRecordModified?.Invoke(newProductRecord, id);
                ExtractionRecordModified?.Invoke(newExtractionRecord, id);
            });
        }

        /// <summary>
        /// Removes all product records (and corresonding extraction records) from the database that contain a tag
        /// specified in tags. Basically, for every record in product records, if the product record contains ANY 
        /// of the tags specified in <paramref name="tags"/>, it is removed from the database.
        /// </summary>
        /// <param name="tags">Product records' tags to specify for deletion.</param>
        public static void RemoveProductRecordsViaTagsQ(string[] tags)
        {
            // i suck at english.
            _mainTaskManager.AddToQueue(RemoveProductRecordsViaTag, tags, null as SQLiteConnection);
        }
        /// <summary>
        /// Removes all product records that satisfy the condition specified by <paramref name="condition"/>.
        /// </summary>
        /// <param name="condition">The prerequisite for removing a row that must be met.</param>
        public static void RemoveProductRecordsQ(Tuple<string, object> condition)
        {
            var t = new Tuple<string, object>[] { condition };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, "ProductRecords", t, false, null as SQLiteConnection);
        }
        /// <summary>
        /// Removes all product records that satisfy the conditions specified by <paramref name="conditions"/>.
        /// </summary>
        /// <param name="conditions">The prerequisites for removing a row that must be met.</param>
        public static void RemoveProductRecordsQ(Tuple<string, object>[] conditions)
        {
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, "ProductRecords", conditions, false, null as SQLiteConnection);
        }
        /// <summary>
        /// Removes all rows that satisfy the condition specified by <paramref name="condition"/>.
        /// </summary>
        /// <param name="tableName">The table to remove rows from.</param>
        /// <param name="condition">The prerequisite for removing a row that must be met.</param>
        public static void RemoveRowWithConditionQ(string tableName, Tuple<string, object> condition)
        {
            var t = new Tuple<string, object>[] { condition };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, t, false, null as SQLiteConnection);
        }
        /// <summary>
        /// Removes all rows that satisfy the conditions specified by <paramref name="conditions"/>.
        /// </summary>
        /// <param name="tableName">The table to remove rows from.</param>
        /// <param name="conditions">The prerequisite for removing a row that must be met.</param>
        public static void RemoveRowWithConditionsQ(string tableName, Tuple<string, object>[] conditions)
        {
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, conditions, false, null as SQLiteConnection);
        }
        /// <summary>
        /// Removes all product and extraction records from the database.
        /// </summary>
        public static void RemoveAllRecordsQ()
        {
            _mainTaskManager.AddToQueue(RemoveAllRecords, null as SQLiteConnection);
        }

        /// <summary>
        /// Removes all tags associated with the product ID specified by <paramref name="pid"/> from the database.
        /// </summary>
        /// <param name="pid"></param>
        public static void RemoveTagsQ(uint pid)
        {
            _mainTaskManager.AddToQueue(RemoveTags, pid, null as SQLiteConnection);
        }
        /// <summary>
        /// Gets the extraction records associated with the extraction record ID specified by <paramref name="eid"/> 
        /// from the database. This function returns the records via the callback function or via the RecordQueryCompleted
        /// event. This may return null if the record does not exist in the database or an internal error occurred.
        /// </summary>
        /// <param name="eid">The extraction record ID to get.</param>
        /// <param name="callerID">A caller id for classifying event invocations.</param>
        /// <param name="callback">The function to return values to.</param>
        public static void GetExtractionRecordQ(uint eid, uint callerID = 0, Action<DPExtractionRecord> callback = null)
        {
            _priorityTaskManager.AddToQueue((t) => {
                var result = GetExtractionRecord(eid, null, t);
                callback?.Invoke(result);
                RecordQueryCompleted?.Invoke(result, callerID);
            });
        }
        /// <summary>
        /// Updates the static <c>ArchiveFileNames</c> variable and returns it via callback function.
        /// It returns a unique set of installed archive names.
        /// </summary>
        /// <param name="callback">The function to return values to.</param>
        public static void GetInstalledArchiveNamesQ(Action<HashSet<string>> callback = null)
        {
            _priorityTaskManager.AddToQueue((t) => {
                var result = GetArchiveFileNameList(null, t);
                callback?.Invoke(result);
            });
        }
        #endregion
        #region Private
        private static void OnTimeout()
        {

        }
        #endregion
    }
}
