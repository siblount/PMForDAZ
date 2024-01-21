using System.Data.Common;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.Sqlite;

namespace DAZ_Installer.Database
{
    internal class DPDatabaseDataAdapter : DbDataAdapter
    {
        private static object _updatingEventPH = new object();
        private static object _updatedEventPH = new object();
        private bool disposeSelect = true;
        private bool disposed = false;
        public DPDatabaseDataAdapter() { }
        public DPDatabaseDataAdapter(DbCommand cmd)
        {
            SelectCommand = cmd;
            disposeSelect = false;
        }

        public DPDatabaseDataAdapter(string commandText, SqliteConnection connection)
        {
            SelectCommand = new SqliteCommand(commandText, connection);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposed || !disposing) return;
                if (disposeSelect)
                {
                    SelectCommand?.Dispose();
                    SelectCommand = null;
                }

                InsertCommand?.Dispose();
                InsertCommand = null;

                UpdateCommand?.Dispose();
                UpdateCommand = null;

                DeleteCommand?.Dispose();
                DeleteCommand = null;
            }
            finally
            {
                base.Dispose(disposing);
                disposed = true;
            }
        }

        public event EventHandler<RowUpdatingEventArgs> RowUpdating
        {
            add
            {
                ThrowIfDisposed();
                var eventHandler = (EventHandler<RowUpdatingEventArgs>) Events[_updatingEventPH];
                if (eventHandler != null && value.Target is DbCommandBuilder)
                {
                    var eventHandler2 = (EventHandler<RowUpdatingEventArgs>) FindBuilder(eventHandler);
                    if (eventHandler2 != null)
                    {
                        Events.RemoveHandler(_updatingEventPH, eventHandler2);
                    }
                }

                Events.AddHandler(_updatingEventPH, value);
            }
            remove
            {
                ThrowIfDisposed();
                Events.RemoveHandler(_updatingEventPH, value);
            }
        }

        //
        // Summary:
        //     Row updated event handler
        public event EventHandler<RowUpdatedEventArgs> RowUpdated
        {
            add
            {
                ThrowIfDisposed();
                Events.AddHandler(_updatedEventPH, value);
            }
            remove
            {
                ThrowIfDisposed();
                Events.RemoveHandler(_updatedEventPH, value);
            }
        }

        internal static Delegate? FindBuilder(MulticastDelegate mcd)
        {
            if (mcd != null)
            {
                Delegate[] invocationList = mcd.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    if (invocationList[i].Target is DbCommandBuilder)
                    {
                        return invocationList[i];
                    }
                }
            }

            return null;
        }

        //
        // Summary:
        //     Raised by the underlying DbDataAdapter when a row is being updated
        //
        // Parameters:
        //   value:
        //     The event's specifics
        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            if (Events[_updatingEventPH] is EventHandler<RowUpdatingEventArgs> eventHandler)
            {
                eventHandler(this, value);
            }
        }

        //
        // Summary:
        //     Raised by DbDataAdapter after a row is updated
        //
        // Parameters:
        //   value:
        //     The event's specifics
        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            if (Events[_updatedEventPH] is EventHandler<RowUpdatedEventArgs> eventHandler)
            {
                eventHandler(this, value);
            }
        }
    }
}
