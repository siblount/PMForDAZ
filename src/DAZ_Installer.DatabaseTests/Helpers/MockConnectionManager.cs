using DAZ_Installer.Database;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.DatabaseTests.Helpers
{
    public class MockConnectionManager : IDPConnectionManager
    {
        /// <summary>
        /// Returns this connection for all methods if the connection is null.
        /// </summary>
        public DPConnection? ConnectionToReturn { get; set; } = new DPConnection(Mock.Of<TestableDbConnection>(), false);
        /// <summary>
        /// Returns <see cref="ConnectionToReturn"/> if connection is null.
        /// Otherwise, it will return the connection in <paramref name="opts"/>.
        /// </summary>
        /// <param name="opts">The connection options.</param>
        /// <param name="readOnly">Not used.</param>
        /// <returns>A 'new' connection if null, otherwise, the same connection in <paramref name="opts"/></returns>
        public virtual DPConnection? CreateAndOpenConnection(ref DPConnectionOpts opts, bool readOnly = false)
        {
            if (opts.Connection is not null) opts.Connection = null;
            else opts.Connection = ConnectionToReturn;
            return opts.Connection;
        }

        /// <summary>
        /// Returns <see cref="ConnectionToReturn"/> if connection is null.
        /// Otherwise, it will return the connection in <paramref name="opts"/>.
        /// </summary>
        /// <param name="opts">The connection options.</param>
        /// <returns>A 'new' connection if null, otherwise, the same connection in <paramref name="opts"/></returns>
        public virtual DPConnection? CreateInitialConnection(ref DPConnectionOpts opts)
        {
            if (opts.Connection is null)
                opts.Connection = ConnectionToReturn;
            else
                opts.Connection = null;
            return opts.Connection;
        }

        /// <summary>
        /// Returns true if <paramref name="connection"/> is a <see cref="DPConnection"/> (and not null).
        /// </summary>
        /// <param name="connection">The connection to 'open'.</param>
        /// <returns>True if connection is not null, otherwise false.</returns>
        public virtual bool OpenConnection([NotNullWhen(true)] IDbConnection? connection)
        {
            return connection is DPConnection;
        }
    }
}
