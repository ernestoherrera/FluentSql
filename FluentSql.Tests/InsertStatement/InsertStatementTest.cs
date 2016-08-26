using FluentSql.Tests.Models;
using FluentSql.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FluentSql.Tests.InsertStatement
{
    [TestClass]
    public class InsertStatementTest
    {
        private DbConnectionTest _dbConnection;

        public InsertStatementTest()
        {
            string connString = TestConstants.ServerPair + TestConstants.DatabasePair +
                                TestConstants.UsernamePair + TestConstants.PasswordPair;

            _dbConnection = new DbConnectionTest(connString);
            new Bootstrap(_dbConnection);
        }

        [Fact]
        public void InsertBasic()
        {
            var store = new EntityStore(_dbConnection);
            var employee = new Employee { FirstName = "Grant", LastName = "Rogers" };

            employee = store.Insert(employee);
            Xunit.Assert.NotNull(employee);
            Xunit.Assert.True(employee.Id > 0);

            var customer = new Customer { CompanyName = "US Army", ContactName = "Nick Fury", Country = "USA" };

            customer = store.Insert(customer);
            Xunit.Assert.NotNull(customer);
            Xunit.Assert.True(customer.Id > 0);

            var newOrder = new Order
            {
                CustomerId = customer.Id,
                EmployeeId = employee.Id,
                OrderDate = DateTime.Now,
                RequiredDate = DateTime.Now.AddMonths(1),
                ShipName = "Steve Rogers",
                ShipAddress = "79 Victoria Hill",
                ShipCity = "DC",
                ShipCountry = "USA"
            };

            newOrder = store.Insert(newOrder);
            Xunit.Assert.NotNull(newOrder);
            Xunit.Assert.True(newOrder.Id > 0);
        }

        [Fact]
        public void InsertTest()
        {
            var store = new EntityStore(_dbConnection);
            var emp = new Employee { FirstName = "Red", LastName = "Skull", Enabled = false };
            var insertQuery = store.GetInsertQuery<Employee>(emp);

            var id = store.ExecuteScalar(insertQuery);

            emp.Id = (int)id;

            Xunit.Assert.True(emp.Id > 0);
        }
    }
}
