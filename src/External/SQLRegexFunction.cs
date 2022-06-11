using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace DAZ_Installer.External
{
    // Found from https://stackoverflow.com/questions/24229785/sqlite-net-sqlitefunction-not-working-in-linq-to-sql/26155359#26155359
    // taken from http://sqlite.phxsoftware.com/forums/p/348/1457.aspx#1457
    [SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
    internal class SQLRegexFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return Regex.IsMatch(Convert.ToString(args[1]), Convert.ToString(args[0]));
        }
    }
}