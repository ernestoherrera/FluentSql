using FluentSql.Tests.Models;
using FluentSql.Tests.Support;
using System;
using System.Collections.Generic;
using System.Data;
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
            var employee21 = _store.GetSingle<Employee>(e => e.Id == 21);

            Xunit.Assert.NotNull(employee21);

            var iDeleted = _store.DeleteByKey<Employee>(employee21);

            Xunit.Assert.True(iDeleted == 1);

            employee21 = _store.GetSingle<Employee>(e => e.Id == 21);

            Xunit.Assert.Null(employee21);
        }

        public async Task BasicDeleteAsync()
        {
            var employee26 = await _store.GetSingleAsync<Employee>(e => e.Id == 26);

            Xunit.Assert.NotNull(employee26);
            Xunit.Assert.IsType<Employee>(employee26);

            var iDeleted = await _store.DeleteByKeyAsync<Employee>(employee26);

            Xunit.Assert.True(iDeleted == 1);

            employee26 = await _store.GetSingleAsync<Employee>(e => e.Id == 26);

            Xunit.Assert.Null(employee26);
        }

        [Fact]
        public void DeleteWithTransaction()
        {
            using (var dbTran = _store.DbConnection.BeginTransaction())
            {
                var selectQuery = _store.GetSelectQuery<Employee>()
                                        .Where(e => e.Id == 25);

                var employees = _store.ExecuteQuery<Employee>(selectQuery, dbTran);

                Xunit.Assert.NotNull(employees);

                var employee25 = employees.FirstOrDefault();

                Xunit.Assert.NotNull(employee25);

                var deleteQuery = _store.GetDeleteQuery<Employee>()
                                        .Where(e => e.Id == 25);

                var iDeleted = _store.ExecuteScript(deleteQuery.ToSql(), deleteQuery.Parameters, dbTran, CommandType.Text);

                Xunit.Assert.True((int)iDeleted == 1);

                var insertQuery = _store.GetInsertQuery<Employee>(employee25);

                var insertedEntities = _store.ExecuteQuery<Employee>(insertQuery, dbTran);

                Xunit.Assert.NotNull(insertedEntities);

                var insertedEntity = insertedEntities.FirstOrDefault();

                Xunit.Assert.NotNull(insertedEntity);
                Xunit.Assert.IsType<Employee>(insertedEntity);
                Xunit.Assert.True(insertedEntity.Id != 25);

                dbTran.Commit();
            }
        }

        [Fact]
        public void DeleteByKeyConstant()
        {
            var employee = _store.GetSingle<Employee>(e => e.Id == 24);

            Xunit.Assert.NotNull(employee);

            var iDeleted = _store.DeleteByKey<Employee>(24);

            Xunit.Assert.True(iDeleted == 1);

            employee = _store.Insert<Employee>(employee);
        }

        [Fact]
        public void DeleteGetQuery()
        {
            var deleteQuery = _store.GetDeleteQuery<OrderDetail>()
                                    .JoinOn<Order>((od, o) => od.OrderId == o.Id)
                                    .JoinOn<Order, Employee>((o, e) => o.EmployeeId == e.Id)
                                    .Where<OrderDetail, Order, Employee>((od, o, e) => e.Username == "SRogers" && od.UnitPrice == 45.99M);

            var iModifiedRecords = _store.ExecuteScalar(deleteQuery);

            Xunit.Assert.True((int)iModifiedRecords == 1);
        }
    }
}
