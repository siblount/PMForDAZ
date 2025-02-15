using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Serilog;

namespace DAZ_Installer.Database {
    /// <summary>
    /// A class that manages the connection to the database.
    /// </summary>
    public interface IDPConnectionManager
    {
        /// <summary>
        /// Creates and returns a connection with the connection string setup
        /// </summary>
        /// <remarks>
        /// This will always be a read-write connection. This should only be used during Initialization and for database updates.
        /// Compared to <see cref="CreateConnection(ref DPConnectionOpts, bool)"/>, this does not check if the database is ready or if
        /// the database is Initialized. This will also create the database file if it does not exist.
        /// </remarks>
        /// <param name="opts">The SqliteConnectionOpts to modify (sets the Connection property).</param>
        /// <returns>An unopened read-write DPConnection.</returns>
        /// <seealso cref="CreateConnection(ref DPConnectionOpts, bool)"/>
        /// <seealso cref="CreateAndOpenConnection(ref DPConnectionOpts, bool)"/>
        DPConnection? CreateInitialConnection(ref DPConnectionOpts opts);

        /// <summary>
        /// Creates, opens, and returns a read-only connection with the connection string setup. 
        /// </summary>
        /// <remarks>
        /// For most operations, you should use this method compared to <see cref="CreateConnection(ref DPConnectionOpts, bool)"/>.
        /// If you need a 
        /// If connection is null, a connection will be created for you. If the connection fails to open or be
        /// created, it will return null. This will create the database file if it does not exist.
        /// </remarks>
        /// <param name="opts">The SqliteConnectionOpts to modify (sets the Connection property).</param>
        /// <param name="readOnly">Determine if the new connection should be read only.</param>
        /// <returns>
        /// If the <see cref="DPConnectionOpts.Connection"/> property was not null AND was successfully opened, it will return the connection.
        /// Otherwise, a new connection will be created and opened. If the connection fails to open or be created, it will return null.
        /// </returns>
        /// <seealso cref="CreateInitialConnection(ref DPConnectionOpts)"/>
        DPConnection? CreateAndOpenConnection(ref DPConnectionOpts opts, bool readOnly = false);

        /// <summary>
        /// Attempts to open the connection and returns whether it was successful or not.
        /// </summary>
        /// <remarks>
        /// Any errors including if connection is null will return false.
        /// </remarks>
        /// <param name="connection">The connection to open.</param>
        /// <returns>True if the connection opened successfully, otherwise false.</returns>
        bool OpenConnection([NotNullWhen(true)] IDbConnection? connection);
    }

}