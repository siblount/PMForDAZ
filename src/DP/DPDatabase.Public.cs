using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            uint callerID = 0, Action<DPProductRecord[]> callback = null, uint timeout = 0)
        {
            // We only want to do searches on one thread. Calling priority task manager ensures
            // we only do searches on one thread.
           // _priorityTaskManager.AddToQueue(() =>
           //{

           //});
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue(DoSearchS, searchQuery, sortMethod);
        }

        //public static DPProductRecord[] SearchWait(string searchQuery, uint timeout, DPSortMethod sortMethod = DPSortMethod.None)
        //{

        //}

        public static void RegexSearch(string regex, DPSortMethod sortMethod = DPSortMethod.None)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue(DoRegexSearchS, regex, sortMethod);
        }

        public static void GetProductRecords(DPSortMethod sortMethod, uint page = 1, uint limit = 0)
        {
            _priorityTaskManager.Stop();
            _priorityTaskManager.AddToQueue(DoLibraryQuery, page, limit, sortMethod);
        }

        public static void StopMainDatabaseOperations()
        {
            _mainTaskManager.Stop();
        }


        public static void StopAllDatabaseOperations()
        {
            _mainTaskManager.Stop();
            _priorityTaskManager.Stop();
        }

        #endregion
        #region Queryable methods

        public static void InitializeQ()
        {
            if (!Initalized)
                _mainTaskManager.AddToQueue(Initialize);
        }

        /// <summary>
        /// If `forceClose` is false, the close action will be queued. Otherwise, the action queue will be cleared and database will be closed immediately.
        /// </summary>
        /// <param name="forceClose">Closes the database connection immediately if True, otherwise database closure is queued.</param>
        public static void CloseConnectionQ(bool forceClose = false)
        {
            if (!forceClose)
                _mainTaskManager.AddToQueue(CloseConnection);
            else
                CloseConnection(CancellationToken.None);
        }
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

        public static void ViewTableQ(string tableName)
        {
            _mainTaskManager.AddToQueue(GetAllValuesFromTable, tableName);
        }

        public static void AddNewRecordEntry(DPProductRecord pRecord)
        {
            _mainTaskManager.AddToQueue(InsertRecords, pRecord, DPExtractionRecord.NULL_RECORD);
        }

        public static void AddNewRecordEntry(DPProductRecord pRecord, DPExtractionRecord eRecord)
        {
            _mainTaskManager.AddToQueue(InsertRecords, pRecord, eRecord);
        }

        public static void InsertNewRowQ(string tableName, object[] values, string[] columns)
        {
            _mainTaskManager.AddToQueue(InsertValuesToTable, tableName, columns, values);
        }

        public static void RemoveRowQ(string tableName, int id)
        {
            var arg = new Tuple<string, object>[1] { new Tuple<string, object>("ID", id) };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, arg, false);
        }

        public static void ClearTableQ(string tableName)
        {
            _mainTaskManager.AddToQueue(RemoveAllFromTable, tableName);
        }

        public static void UpdateValuesQ(string tableName, object[] values, string[] columns)
        {
            _mainTaskManager.AddToQueue(UpdateValues, tableName, columns, values);
        }

        public static void RemoveProductRecordsViaTagsQ(string[] tags)
        {
            _mainTaskManager.AddToQueue(RemoveProductRecordsViaTag, tags);
        }

        public static void RemoveProductRecordsQ(Tuple<string, object> condition)
        {
            var t = new Tuple<string, object>[] { condition };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, "ProductRecords", t, false);
        }
        public static void RemoveProductRecordsQ(Tuple<string, object>[] conditions)
        {
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, "ProductRecords", conditions, false);
        }

        public static void RemoveRowWithConditionQ(string tableName, Tuple<string, object> condition)
        {
            var t = new Tuple<string, object>[] { condition };
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, t, false);
        }

        public static void RemoveRowWithConditionsQ(string tableName, Tuple<string, object>[] conditions)
        {
            _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, conditions, false);
        }

        public static void RemoveAllRecordsQ()
        {
            _mainTaskManager.AddToQueue(RemoveAllRecords);
        }

        public static void RemoveTagsQ(uint pid)
        {
            _mainTaskManager.AddToQueue(RemoveTags, pid);
        }

        public static void GetExtractionRecordQ(uint eid)
        {
            _priorityTaskManager.AddToQueue(GetExtractionRecord, eid);
        }

        public static void GetInstalledArchiveNamesQ()
        {
            _priorityTaskManager.AddToQueue(GetArchiveFileNameList);
        }
        #endregion
        #region Private
        private static void OnTimeout()
        {

        }
        #endregion
    }
}
