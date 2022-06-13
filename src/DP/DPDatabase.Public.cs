using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using DAZ_Installer.External;

namespace DAZ_Installer.DP
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

        public static void Search(string searchQuery, DPSortMethod sortMethod = DPSortMethod.None,
            uint callerID = 0, Action<DPProductRecord[]> callback = null)
        {
            // We only want to do searches on one thread. Calling priority task manager ensures
            // we only do searches on one thread.
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue((t) =>
            {
                var results = DoSearchS(searchQuery, sortMethod, null, t);
                if (t.IsCancellationRequested) return;
                callback?.Invoke(results);
                SearchUpdated?.Invoke(results, callerID);
                
            });
        }

        public static void RegexSearch(string regex, DPSortMethod sortMethod = DPSortMethod.None,
            uint callerID = 0, Action<DPProductRecord[]> callback = null)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue((t) =>
            {
                var results = DoRegexSearchS(regex, sortMethod, null, t);
                if (t.IsCancellationRequested) return;
                callback?.Invoke(results);
                SearchUpdated?.Invoke(results, callerID);
            });
        }

        public static void GetProductRecords(DPSortMethod sortMethod, uint page = 1, uint limit = 0,
            uint callerID = 0, Action<DPProductRecord[]> callback = null)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue((t) =>
            {
                var results = DoLibraryQuery(page, limit, sortMethod, null, t);
                if (t.IsCancellationRequested) return;
                callback?.Invoke(results);
                LibraryQueryCompleted?.Invoke(results, callerID);
            });
        }

        public static void StopMainDatabaseOperations() => _mainTaskManager.Stop();


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

        public static void ViewTableQ(string tableName, uint callerID = 0, 
            Action<DataSet> callback = null)
        {
            _mainTaskManager.AddToQueue((t) => {
                var result = GetAllValuesFromTable(tableName, null, t);
                if (result == null) return;
                callback?.Invoke(result);
                ViewUpdated?.Invoke(result, callerID);
            });
        }

        public static void AddNewRecordEntry(DPProductRecord pRecord)
        {
            _mainTaskManager.AddToQueue(InsertRecords, pRecord, DPExtractionRecord.NULL_RECORD, 
                null as SQLiteConnection); // doing (SQLiteConnection) on null throws.
        }

        public static void AddNewRecordEntry(DPProductRecord pRecord, DPExtractionRecord eRecord)
        {
            _mainTaskManager.AddToQueue(InsertRecords, pRecord, eRecord, null as SQLiteConnection);
        }

        public static void InsertNewRowQ(string tableName, object[] values, string[] columns)
        {
            _mainTaskManager.AddToQueue((t) =>
                InsertValuesToTable(tableName, columns, values, null, t));
        }

        public static void RemoveRowQ(string tableName, int id)
        {
            var arg = new Tuple<string, object>[1] { new Tuple<string, object>("ID", id) };
            _mainTaskManager.AddToQueue((t) => 
                RemoveValuesWithCondition(tableName, arg, false, null, t));
        }

        public static void ClearTableQ(string tableName)
        {
            _mainTaskManager.AddToQueue((t) => RemoveAllFromTable(tableName, null, t));
        }

        public static void UpdateValuesQ(string tableName, object[] values, string[] columns)
        {
            _mainTaskManager.AddToQueue((t) => UpdateValues(tableName, columns, values, null, t));
        }

        public static void RemoveProductRecordsViaTagsQ(string[] tags)
        {
            _mainTaskManager.AddToQueue((t) => RemoveProductRecordsViaTag(tags, null, t));
        }

        public static void RemoveProductRecordsQ(Tuple<string, object> condition)
        {
            var tu = new Tuple<string, object>[] { condition };
            _mainTaskManager.AddToQueue((t) => 
                RemoveValuesWithCondition("ProductRecords", tu, false, null, t));
        }
        public static void RemoveProductRecordsQ(Tuple<string, object>[] conditions)
        {
            _mainTaskManager.AddToQueue((t) =>
                RemoveValuesWithCondition("ProductRecords", conditions, false, null, t));
        }

        public static void RemoveRowWithConditionQ(string tableName, Tuple<string, object> condition)
        {
            var tu = new Tuple<string, object>[] { condition };
            _mainTaskManager.AddToQueue((t) => RemoveValuesWithCondition(tableName, tu, false, null, t));
        }

        public static void RemoveRowWithConditionsQ(string tableName, Tuple<string, object>[] conditions)
        {
            _mainTaskManager.AddToQueue((t) => RemoveValuesWithCondition(tableName, conditions, false, null, t));
        }

        public static void RemoveAllRecordsQ()
        {
            _mainTaskManager.AddToQueue(RemoveAllRecords, null as SQLiteConnection);
        }

        public static void RemoveTagsQ(uint pid)
        {
            _mainTaskManager.AddToQueue(RemoveTags, pid);
        }

        public static void GetExtractionRecordQ(uint eid, uint callerID = 0, 
            Action<DPExtractionRecord> callback = null)
        {
            _priorityTaskManager.AddToQueue((t) =>
            {
                var result = GetExtractionRecord(eid, null, t);
                if (result == null) return;
                callback?.Invoke(result);
                RecordQueryCompleted?.Invoke(result, callerID);
            });
        }

        public static void GetInstalledArchiveNamesQ(uint callerID = 0, Action<HashSet<string>> callback = null)
        {
            _priorityTaskManager.AddToQueue((t) =>
            {
                var list = GetArchiveFileNameList(null, t);
                if (list == null) return;
                callback?.Invoke(list);
                MainQueryCompleted?.Invoke(callerID);
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
