using FluentSql.DatabaseMappers.Common;
using FluentSql.Mappers;
using FluentSql.Tests.Models;
using FluentSql.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FluentSql.Tests.UpdateStatement
{
    [TestClass]
    public class UpdateTest
    {
        private DbConnectionTest _dbConnection;
        private EntityStore _store;

        public UpdateTest()
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
        public void BasicUpdate()
        {
            var employee7 = _store.GetSingle<Employee>(e => e.Id == 7);

            Xunit.Assert.NotNull(employee7);

            employee7.FirstName = "Timothy";
            employee7.LastName = "Dugan";
            employee7.Address = "100 Fletcher Drive";
            employee7.City = "Gainesville";

            var i = _store.UpdateByKey(employee7);

            var employee7Copy = _store.GetSingle<Employee>(e => e.Id == 7);

            Xunit.Assert.True(employee7.Id == employee7Copy.Id);
            Xunit.Assert.True(employee7.FirstName == employee7Copy.FirstName);
            Xunit.Assert.True(employee7.LastName == employee7Copy.LastName);
            Xunit.Assert.True(employee7.Address == employee7Copy.Address);
            Xunit.Assert.True(employee7.City == employee7Copy.City);
        }

        [Fact]
        public void UpdateGetQuery()
        {
            var updateQuery = _store.GetUpdateQuery<Employee>()
                                    .Set( new { Birthdate = DateTime.Now.AddYears(-27)})
                                    .Where(e => e.State == "TX");

            var iModifiedRecords = _store.ExecuteScalar(updateQuery);

            //The value of DateTime.Now includes the time
            var texasEmployees = _store.Get<Employee>(e => e.Birthdate <= DateTime.Now.AddYears(-27) && 
                                                        e.State == "TX");

            Xunit.Assert.True(texasEmployees != null);

            var iCount = texasEmployees.Count();

            Xunit.Assert.True((int)iModifiedRecords == iCount);
        }

        [Fact]
        public void UpdateGetQueryJoin()
        {
            var updateQuery = _store.GetUpdateQuery<OrderDetail>()
                                    .Set(new { Discount = 0.05 })
                                    .JoinOn<Order>((od, o) => od.OrderId == o.Id)
                                    .JoinOn<Order, Employee>((o, e) => o.EmployeeId == e.Id)
                                    .Where<OrderDetail, Employee>((od, e) => od.Quantity >= 50 && e.LastName == "Rogers");

            var iModifiedRecords = _store.ExecuteScalar(updateQuery);

            Xunit.Assert.True((int)iModifiedRecords >= 2);
        }

        [Fact]
        public void UpdateEntity()
        {
            var orderDetail = _store.GetSingle<OrderDetail>(od => od.OrderId == 2 && od.ProductId == 3);

            Xunit.Assert.NotNull(orderDetail);

            orderDetail.Discount = 0.03f;

            var iModifiedRecords = _store.UpdateByKey(orderDetail);

            Xunit.Assert.True((int)iModifiedRecords == 1);
        }

    }
}
