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
        private IEntityStore _store;

        public InsertStatementTest()
        {
            string connString = TestConstants.ServerPair + TestConstants.DatabasePair +
                                TestConstants.UsernamePair + TestConstants.PasswordPair;

            _dbConnection = new DbConnectionTest(connString);
            new Bootstrap(_dbConnection);

            _store = new EntityStore(_dbConnection);
        }

        [Fact]
        public void InsertBasic()
        {
            var employee = new Employee { FirstName = "Grant", LastName = "Rogers" };

            employee = _store.Insert(employee);
            Xunit.Assert.NotNull(employee);
            Xunit.Assert.True(employee.Id > 0);

            var customer = new Customer
            {
                CompanyName = "US Army",
                ContactName = "Nick Fury",
                Country = "USA"
            };

            customer = _store.Insert(customer);
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

            newOrder = _store.Insert(newOrder);
            Xunit.Assert.NotNull(newOrder);
            Xunit.Assert.True(newOrder.Id > 0);
        }

        [Fact]
        public async Task InsertBasicAsync()
        {
            var employee = new Employee { FirstName = "Sam", LastName = "Wilson" };

            employee = await _store.InsertAsync(employee);
            Xunit.Assert.NotNull(employee);
            Xunit.Assert.True(employee.Id > 0);

            var customer = new Customer
            {
                CompanyName = "US Army",
                ContactName = "Lieutenant James Rhodes",
                Country = "USA"
            };

            customer = _store.Insert(customer);
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

            newOrder = _store.Insert(newOrder);
            Xunit.Assert.NotNull(newOrder);
            Xunit.Assert.True(newOrder.Id > 0);
        }

        [Fact]
        public void InsertTest()
        {
            var store = new EntityStore(_dbConnection);
            var emp = new Employee { FirstName = "Red", LastName = "Skull", Enabled = false };
            var insertQuery = store.GetInsertQuery<Employee>(emp);

            var resultSet = store.ExecuteQuery(insertQuery);

            Xunit.Assert.NotNull(resultSet);

            var insertedEmployee = resultSet.FirstOrDefault();

            Xunit.Assert.IsType<Employee>(insertedEmployee);

            Xunit.Assert.True(insertedEmployee.Id > 0);
        }

        [Fact]
        public void InsertManyTest()
        {
            var store = new EntityStore(_dbConnection);
            var customers = new List<Customer>
            {
                new Customer
                {
                    CompanyName = "SHIELD",
                    ContactName = "James Buchanan",
                    Address = "3037 La Follette Circle",
                    City = "Santa Clara",
                    Region = "CA"
                },
                new Customer
                {
                    CompanyName = "Stark Enterprises",
                    ContactName = "Colonel Chester Phillips",
                    Address = "905 Sunnyside Terrace",
                    City = "Spokane",
                    Region = "WA"
                },
                new Customer
                {
                    CompanyName = "Marvel",
                    ContactName = "Private Lorraine",
                    Address = "69232 Ramsey Park",
                    City = "Sacramento",
                    Region = "CA"
                }
            };

            var newCustomers = store.InsertMany(customers);

            Xunit.Assert.NotNull(newCustomers);
            Xunit.Assert.NotEmpty(newCustomers);

            foreach (var cust in newCustomers)
            {
                Xunit.Assert.True(cust.Id > 0);
            }
        }

        [Fact]
        void InsertNonAutoIncrement()
        {
            var orderDetail = new OrderDetail
            {
                OrderId = 3,
                ProductId = 4,
                UnitPrice = 10.99M,
                Quantity = 100,
                Discount = 0.05F
            };

            orderDetail = _store.Insert(orderDetail);

            Xunit.Assert.NotNull(orderDetail);
            Xunit.Assert.True(orderDetail.OrderId == 3);
        }
    }
}
