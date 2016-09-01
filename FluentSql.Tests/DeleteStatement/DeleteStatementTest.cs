using FluentSql.Tests.Models;
using FluentSql.Tests.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FluentSql.Tests.DeleteStatement
{
    public class DeleteStatementTest
    {
        private DbConnectionTest _dbConnection;
        private EntityStore _store;

        public DeleteStatementTest()
        {
            string connString = TestConstants.ServerPair + TestConstants.DatabasePair +
                                TestConstants.UsernamePair + TestConstants.PasswordPair;

            _dbConnection = new DbConnectionTest(connString);

            if (_dbConnection == null)
                throw new Exception("Could not acquire a database connection");

            new Bootstrap(_dbConnection);
            _store = new EntityStore(_dbConnection);
        }

        [Fact]
        public void BasicDelete()
        {
            var employee7 = _store.GetSingle<Employee>(e => e.Id == 21);

            Xunit.Assert.NotNull(employee7);

            var iDeleted = _store.DeleteByKey(employee7);

            Xunit.Assert.True(iDeleted == 1);

            employee7 = _store.GetSingle<Employee>(e => e.Id == 21);

            Xunit.Assert.Null(employee7);
        }

        [Fact]
        public void DeleteGetQuery()
        {
            var deleteQuery = _store.GetDeleteQuery<Employee>();

            var iModifiedRecords = _store.ExecuteScalar(deleteQuery);

            Xunit.Assert.True((int)iModifiedRecords == 1);
        }
    }
}
