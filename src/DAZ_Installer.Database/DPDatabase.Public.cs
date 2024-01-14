using System.Data;
using System.Data.SQLite;

namespace DAZ_Installer.Database
{
    public partial class DPDatabase : IDPDatabase
    {
        // This section is set up as an interface for other classes. You should use these methods
        // to get data. These methods can callback if a callback is specified and emit an event.
        // If you want to listen through an event, pass a constant caller id.
        // Example: a constant caller ID for DPLibrary = 3.
        #region Public methods
        // TO DO: Improve. This can be so much more efficient.
        // I lack the brain capacity to do this at the moment.

        public async Task<DPProductRecord[]> SearchQ(string searchQuery, DPSortMethod sortMethod = DPSortMethod.None,
            uint callerID = 0, Action<DPProductRecord[]>? callback = null)
        {
            // We only want to do searches on one thread. Calling priority task manager ensures
            // we only do searches on one thread.
            _priorityTaskManager.Stop();
            DPProductRecord[] results = Array.Empty<DPProductRecord>();
            await _priorityTaskManager.AddToQueue((t) =>
            {
                results = DoSearchS(searchQuery, sortMethod, null, t);
                callback?.Invoke(results);
                SearchUpdated?.Invoke(results, callerID);
            });
            return results;
        }

        public async Task<DPProductRecord[]> RegexSearchQ(string regex, DPSortMethod sortMethod = DPSortMethod.None,
            uint callerID = 0, Action<DPProductRecord[]> callback = null)
        {
            _priorityTaskManager.Stop();
            DPProductRecord[] results = Array.Empty<DPProductRecord>();

            await _priorityTaskManager.AddToQueue((t) =>
            {
                results = DoRegexSearchS(regex, sortMethod, null, t);
                callback?.Invoke(results);
                SearchUpdated?.Invoke(results, callerID);
            });

            return results;
        }

        public async Task<DPProductRecord[]> GetProductRecordsQ(DPSortMethod sortMethod, uint page = 1, uint limit = 0,
            uint callerID = 0, Action<DPProductRecord[]> callback = null)
        {
            _priorityTaskManager.Stop();
            DPProductRecord[] results = Array.Empty<DPProductRecord>();
            await _priorityTaskManager.AddToQueue((t) =>
            {
                results = DoLibraryQuery(page, limit, sortMethod, null, t);
                callback?.Invoke(results);
                MainQueryCompleted?.Invoke(callerID);
            });
            return results;
        }

        
        public void StopMainDatabaseOperations() => _mainTaskManager.Stop();

        
        public void StopAllDatabaseOperations()
        {
            _mainTaskManager.Stop();
            _priorityTaskManager.Stop();
        }

        #endregion
        #region Queryable methods
        public Task RefreshDatabaseQ(bool forceRefresh = false)
        {
            if (!forceRefresh) return _mainTaskManager.AddToQueue(RefreshDatabase);
            _mainTaskManager.Stop();
            _priorityTaskManager.Stop();
            Initalized = false;
            Initialize();
            return Task.CompletedTask;
        }

        public async Task<DataSet?> ViewTableQ(string tableName, uint callerID = 0, Action<DataSet?>? callback = null)
        {
            DataSet? result = null;
            await _mainTaskManager.AddToQueue((t) =>
            {
                result = GetAllValuesFromTable(tableName, null, t);
                callback?.Invoke(result);
                ViewUpdated?.Invoke(result, callerID);
            });
            return result;
        }
        
        public Task AddNewRecordEntry(DPProductRecord pRecord) => _mainTaskManager.AddToQueue(InsertRecords, pRecord, null as DPExtractionRecord, null as SQLiteConnection);
        
        public Task AddNewRecordEntry(DPProductRecord pRecord, DPExtractionRecord eRecord) => _mainTaskManager.AddToQueue(InsertRecords, pRecord, eRecord, null as SQLiteConnection);
        
        public Task InsertNewRowQ(string tableName, object[] values, string[] columns) => _mainTaskManager.AddToQueue(InsertValuesToTable, tableName, columns, values, null as SQLiteConnection);
        
        public Task RemoveRowQ(string tableName, int id)
        {
            var arg = new Tuple<string, object>[1] { new Tuple<string, object>("ID", id) };
            return _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, arg, false, null as SQLiteConnection);
        }

        public Task RemoveProductRecord(DPProductRecord record, Action<uint>? callback = null)
        {
            return _mainTaskManager.AddToQueue((t) =>
            {
                var arg = new Tuple<string, object>[1] { new Tuple<string, object>("ID", Convert.ToInt32(record.ID)) };
                var success = RemoveValuesWithCondition("ProductRecords", arg, false, null, t);
                if (success)
                {
                    callback?.Invoke(record.ID);
                    ProductRecordRemoved?.Invoke(record.ID);
                    ExtractionRecordRemoved?.Invoke(record.EID);
                }
            });
        }
        
        public Task ClearTableQ(string tableName) => _mainTaskManager.AddToQueue(RemoveAllFromTable, tableName, null as SQLiteConnection);

        public Task UpdateValuesQ(string tableName, object[] values, string[] columns, int id) => _mainTaskManager.AddToQueue(UpdateValues, tableName, columns, values, id, null as SQLiteConnection);

        public Task UpdateRecordQ(uint id, DPProductRecord newProductRecord, DPExtractionRecord newExtractionRecord, Action<uint>? callback = null)
        {
            return _mainTaskManager.AddToQueue(t =>
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
        
        public Task RemoveProductRecordsViaTagsQ(string[] tags) =>
            _mainTaskManager.AddToQueue(RemoveProductRecordsViaTag, tags, null as SQLiteConnection);
        
        public Task RemoveProductRecordsQ(Tuple<string, object> condition)
        {
            var t = new Tuple<string, object>[] { condition };
            return _mainTaskManager.AddToQueue(RemoveValuesWithCondition, "ProductRecords", t, false, null as SQLiteConnection);
        }
        
        public Task RemoveProductRecordsQ(Tuple<string, object>[] conditions) => _mainTaskManager.AddToQueue(RemoveValuesWithCondition, "ProductRecords", conditions, false, null as SQLiteConnection);
        
        public Task RemoveRowWithConditionQ(string tableName, Tuple<string, object> condition)
        {
            var t = new Tuple<string, object>[] { condition };
            return _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, t, false, null as SQLiteConnection);
        }
        
        public Task RemoveRowWithConditionsQ(string tableName, Tuple<string, object>[] conditions) => _mainTaskManager.AddToQueue(RemoveValuesWithCondition, tableName, conditions, false, null as SQLiteConnection);
        
        public Task RemoveAllRecordsQ() => _mainTaskManager.AddToQueue(RemoveAllRecords, null as SQLiteConnection);
        
        public Task RemoveTagsQ(uint pid) => _mainTaskManager.AddToQueue(RemoveTags, pid, null as SQLiteConnection);
        
        public async Task<DPExtractionRecord?> GetExtractionRecordQ(uint eid, uint callerID = 0, Action<DPExtractionRecord>? callback = null)
        {
            DPExtractionRecord? result = null;
            await _priorityTaskManager.AddToQueue((t) =>
            {
                result = GetExtractionRecord(eid, null, t);
                callback?.Invoke(result);
                RecordQueryCompleted?.Invoke(result, callerID);
            });
            return result;
        }
        
        public async Task<HashSet<string>?> GetInstalledArchiveNamesQ(Action<HashSet<string>>? callback = null)
        {
            HashSet<string>? result = null;
            await _priorityTaskManager.AddToQueue((t) =>
            {
                result = GetArchiveFileNameList(null, t);
                callback?.Invoke(result);
            });
            return result;
        }
        #endregion
    }
}
