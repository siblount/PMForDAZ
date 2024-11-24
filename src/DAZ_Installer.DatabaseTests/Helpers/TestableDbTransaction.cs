using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.DatabaseTests.Helpers
{
    public abstract class TestableDbTransaction : DbTransaction
    {
        protected sealed override void Dispose(bool disposing)
        {
            DisposePublic();
            base.Dispose(disposing);
        }

        public abstract void DisposePublic();
    }
}
