using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Tests.Support
{
    public class TestConstants
    {
        private static readonly string _serverName = "localhost";
        private static readonly string _testDatabaseName = "FluentSqlTestDb";
        private static readonly string _username = "appUser";
        private static readonly string _password = "3044171035Fluent";

        public static string TestDatabaseName { get { return _testDatabaseName; } }

        #region Connection String elements
        public static string ServerPair { get { return string.Format("Server={0};", _serverName); } }
        public static string DatabasePair { get { return string.Format("Database={0};", _testDatabaseName); } }
        public static string UsernamePair { get { return string.Format("User ID={0};", _username); } }
        public static string PasswordPair { get { return string.Format("Password={0};", _password); } }
        #endregion

        #region Where clause test constants
        public static string USERNAME = "MCarter";
        #endregion

        public TestConstants() { }
    }
}
