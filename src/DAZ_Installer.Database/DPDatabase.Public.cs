using System.Data;
using Microsoft.Data.Sqlite;

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
        public async Task<DPProductRecord?> GetFullProductRecord(long id, Action<DPProductRecord?>? callback = null)
        {
            DPProductRecord? record = null;
            await _mainTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                record = GetProductRecord(id, opts);
                callback?.Invoke(record);
            });
            return record;
        }
        public async Task<List<DPProductRecordLite>> SearchQ(string searchQuery, DPSortMethod sortMethod = DPSortMethod.None,
            uint callerID = 0, Action<List<DPProductRecordLite>>? callback = null)
        {
            // We only want to do searches on one thread. Calling priority task manager ensures
            // we only do searches on one thread.
            _priorityTaskManager.Stop();
            List<DPProductRecordLite> results = new(0);
            await _priorityTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                results = DoSearchS(searchQuery, sortMethod, opts);
                callback?.Invoke(results);
                SearchUpdated?.Invoke(results, callerID);
            });
            return results;
        }

        public async Task<List<DPProductRecordLite>> GetProductRecordsQ(DPSortMethod sortMethod, uint page = 1, uint limit = 0,
            uint callerID = 0, Action<List<DPProductRecordLite>>? callback = null)
        {
            _priorityTaskManager.Stop();
            List<DPProductRecordLite> results = new(0);
            await _priorityTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                results = DoLibraryQuery(page, limit, sortMethod, opts);
                callback?.Invoke(results);
                MainQueryCompleted?.Invoke(callerID);
            });
            return results;
        }

        public void StopMainDatabaseOperations() => _mainTaskManager.Stop();
        
        public void StopAllDatabaseOperations(bool wait = true)
        {
            if (wait)
            {
                _mainTaskManager.StopAndWait();
                _priorityTaskManager.StopAndWait();
            } else
            {
                _mainTaskManager.Stop();
                _priorityTaskManager.Stop();
            }
        }

        #endregion
        #region Queryable methods
        public Task RefreshDatabaseQ(bool forceRefresh = false)
        {
            if (!forceRefresh) return _mainTaskManager.AddToQueue((t) =>
                RefreshDatabase(new SqliteConnectionOpts(null, null, t))
            );
            try
            {
                Flags |= DPArchiveFlags.Locked;
                _mainTaskManager.StopAndWait();
                _priorityTaskManager.StopAndWait();
                _mainTaskManager.AddToQueue((t) => RefreshDatabase(new SqliteConnectionOpts(null, null, t)));
            } finally
            {
                Flags &= ~DPArchiveFlags.Locked;
            }

            return Task.CompletedTask;
        }

        public async Task<DataSet?> ViewTableQ(string tableName, uint callerID = 0, Action<DataSet?>? callback = null)
        {
            DataSet? result = null;
            await _mainTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                result = GetAllValuesFromTable(tableName, opts);
                callback?.Invoke(result);
                if (result is not null)
                    ViewUpdated?.Invoke(result, callerID);
            });
            return result;
        }
        
        public Task AddNewRecordEntry(DPProductRecord record)
        {
            return _mainTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                var success = InsertRecords(record, opts);
                if (!success) return;
            });
        }

        public Task InsertNewRowQ(string tableName, object[] values, string[] columns) {
            return _mainTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                InsertValuesToTable(tableName, columns, values, opts);
            });
        }
        
        public Task RemoveRowQ(string tableName, int id)
        {
            var arg = new Tuple<string, object>[1] { new Tuple<string, object>("ROWID", id) };
            return _mainTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                RemoveValuesWithCondition(tableName, arg, false, opts);
            });
        }

        public Task RemoveProductRecordQ(DPProductRecord record, Action<long>? callback = null)
        {
            return _mainTaskManager.AddToQueue((t) =>
            {
                var arg = new Tuple<string, object>[1] { new("ROWID", Convert.ToInt32(record.ID)) };
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                var success = RemoveValuesWithCondition(ProductTable, arg, false, opts);
                if (success)
                {
                    callback?.Invoke(record.ID);
                    ProductRecordRemoved?.Invoke(record.ID);
                }
            });
        }

        public Task RemoveProductRecordQ(DPProductRecordLite record, Action<long>? callback = null)
        {
            return _mainTaskManager.AddToQueue((t) =>
            {
                var arg = new Tuple<string, object>[1] { new("ROWID", Convert.ToInt32(record.ID)) };
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                var success = RemoveValuesWithCondition(ProductTable, arg, false, opts);
                if (success)
                {
                    callback?.Invoke(record.ID);
                    ProductRecordRemoved?.Invoke(record.ID);
                }
            });
        }

        public Task ClearTableQ(string tableName)
        {
            return _mainTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                RemoveAllFromTable(tableName, opts);
            });
        }

        public Task UpdateRecordQ(long id, DPProductRecord newProductRecord, Action<long>? callback = null)
        {
            return _mainTaskManager.AddToQueue(t =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                var success = UpdateProductRecord(id, newProductRecord, opts);
                if (!success) return;
                callback?.Invoke(newProductRecord.ID);
                ProductRecordModified?.Invoke(newProductRecord, id);
            });
        }

        public Task RemoveAllRecordsQ() => _mainTaskManager.AddToQueue((ct) =>
        {
            var opts = new SqliteConnectionOpts() { CancellationToken = ct };
            RemoveAllRecords(opts);
        });
        
        public async Task<HashSet<string>?> GetInstalledArchiveNamesQ(Action<HashSet<string>>? callback = null)
        {
            HashSet<string>? result = null;
            await _priorityTaskManager.AddToQueue((t) =>
            {
                var opts = new SqliteConnectionOpts() { CancellationToken = t };
                result = GetArchiveFileNameList(opts);
                callback?.Invoke(result);
            });
            return result;
        }
        #endregion
    }
}
