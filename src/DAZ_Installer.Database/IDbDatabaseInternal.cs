using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Database
{
    /// <summary>
    /// The interface for the database. This is used for other classes to get data from the database.
    /// All methods are executed asynchronously.
    /// </summary>
    internal interface IDPDatabaseInternal : IDPDatabase
    {
        /// <summary>
        /// Describes if the current database has been initialized.
        /// </summary>
        public bool Initialized { get; }
        /// <summary>
        /// Describes if the current database is in a locked state.
        /// </summary>
        /// <remarks>
        /// This is not referring to connection locks. The database may be 'locked' to prevent
        /// additional requests.
        /// </remarks>
        public bool Locked { get; }
        /// <summary>
        /// Describes if the current database requires an update in order for it to work properly.
        /// </summary>
        public bool UpdateRequired { get; }
        /// <summary>
        /// Describes if the current database is in a corrupted state.
        /// </summary>
        /// <remarks>
        /// This could simply mean that there are missing tables, or in the worse case,
        /// the database is really corrupted (data decay). It may be possible for this to be true
        /// while <see cref="UpdateRequired"/> is also true, meaning this may be true due to an outdated
        /// schema.
        /// </remarks>
        /// <seealso cref="UpdateRequired"/>
        public bool Corrupted { get; }
        /// <summary>
        /// Describes if the current database file is missing (or not accessible due to permissions).
        /// </summary>
        public bool Missing { get; }
        /// <summary>
        /// Describes if the database is not in a working state to accept connections and/or queries.
        /// </summary>
        /// <remarks>
        /// Typically, this is true if only <see cref="Initialized"/> is true. Otherwise, false.
        /// </remarks>
        public bool DatabaseNotReady { get; }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <returns>True if the database initialized successfully, otherwise false.</returns>
        public bool Initialize();
    }
}