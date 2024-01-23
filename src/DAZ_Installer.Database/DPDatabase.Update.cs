using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;

namespace DAZ_Installer.Database
{
    public partial class DPDatabase : IDPDatabase
    {
        // This section is set up as an interface for other classes. You should use these methods
        // to get data. These methods can callback if a callback is specified and emit an event.
        // If you want to listen through an event, pass a constant caller id.
        // Example: a constant caller ID for DPLibrary = 3.
        #region Update mthods
        public Task UpdateDatabase(CancellationToken t) {
            _mainTaskManager.StopAndWait();
            _priorityTaskManager.StopAndWait();
            return _priorityTaskManager.AddToQueue(() => {
                var opts = new SqliteConnectionOpts(null, null, t);
                using var connection = CreateInitialConnection(ref opts);
                if (!OpenConnection(connection))
                {
                    throw new Exception("Failed to open connection.");
                }
                UpdateToVersion3(opts);
            });
        }
        #endregion
    }
}
