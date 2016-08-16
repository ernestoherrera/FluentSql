using Dapper;
using FluentSql.DatabaseMappers.Common;
using FluentSql.Mappers;
using FluentSql.SqlGenerators;
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

namespace FluentSql.Tests.SelectStatement
{
    [TestClass]
    public class SelectTest
    {
        private DbConnectionTest _dbConnection;

        public SelectTest()
        {
            string connString = TestConstants.ServerPair + TestConstants.DatabasePair + 
                                TestConstants.UsernamePair + TestConstants.PasswordPair;

            _dbConnection = new DbConnectionTest(connString);

            if (!EntityMapper.EntityMap.Keys.Any())
            {
                var assemblies = new[]
               {
                    Assembly.Load("FluentSql.Tests")
               };

                var store = new EntityStore(_dbConnection);
                var database = new Database
                {
                    Name = TestConstants.TestDatabaseName,
                    TableNamesInPlural = true,
                    NameSpace = "FluentSql.Tests.Models"
                };

                store.ExecuteScript(SqlScripts.CREATE_DATABASE, null, false, CommandType.Text);
                store.ExecuteScript(SqlScripts.CREATE_TABLES, null, false, CommandType.Text);

                //new EntityMapper(_dbConnection, assemblies);
                new EntityMapper(_dbConnection, new List<Database> { database }, null);
            }
        }

        #region Asynchronous Gets
        [Fact]
        public async void SelectAsyncWherePredicateIsConstant()
        {
            var entityStore = new EntityStore(_dbConnection);

            var employees = await entityStore.GetAsync<Employee>(emp => emp.Id == 1);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public async void SelectAsyncConstant2()
        {
            var entityStore = new EntityStore(_dbConnection);

            var employees = await entityStore.GetAsync<Employee>(u => 1 == u.Id);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());

            var employee = employees.FirstOrDefault();

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Steve");

        }

        [Fact]
        public async void SelectAsyncGetByKeyAnonymous()
        {
            var entityStore = new EntityStore(_dbConnection);

            var employee = await entityStore.GetByKeyAsync<Employee>(new { Id = 1 });

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Steve");
        }

        [Fact]
        public async void SelectAsyncGetByKeyEntity()
        {
            var entityStore = new EntityStore(_dbConnection);
            var employeeTemplate = new Employee { Id = 1 };
            var employee = await entityStore.GetByKeyAsync<Employee>(employeeTemplate);

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Steve");

        }

        [Fact]
        public async void SelectAsyncGetSingle()
        {
            var loginRequest = new LoginRequest { Username = "SRogers" };
            var entityStore = new EntityStore(_dbConnection);

            var employee = await entityStore.GetSingleAsync<Employee>(u => u.Username == loginRequest.Username);

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Steve");
        }

        [Fact]
        public async void SelectGetAllAsync()
        {
            var store = new EntityStore(_dbConnection);
            var employees = await store.GetAllAsync<Employee>();

            Xunit.Assert.NotNull(employees);
            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public async void SelectWithJoinOnAndTResultAsync()
        {
            var entityStore = new EntityStore(_dbConnection);
            var steve = new Employee { Id = 1, FirstName = "Steve" };
            var tResult = await entityStore.GetWithJoinAsync<Employee, Order, Order>((e, o) => e.Id == o.EmployeeId,
                                                            (e, o) => e.FirstName == steve.FirstName);

            Xunit.Assert.NotNull(tResult);

            var firstOrder = tResult.FirstOrDefault();

            Xunit.Assert.IsType<Order>(firstOrder);
        }

        #endregion

        #region synchronous tests
        [Fact]
        public void SelectByKey()
        {
            var store = new EntityStore(_dbConnection);
            var SteveEmployeeId = 1;

            var employee = store.GetByKey<Employee>(1);
            var employee1 = store.GetByKey<Employee>(new { Id = 1 });
            var employee2 = store.GetByKey<Employee>(new Employee { Id = 1, FirstName = "Steve", LastName = "Rogers" });
            var employee3 = store.GetByKey<Employee>(SteveEmployeeId);

            var order = store.GetByKey<Order>(1);
            var order1 = store.GetByKey<Order>(new { Id = 1 });

            Xunit.Assert.NotNull(employee);
            Xunit.Assert.NotNull(employee1);
            Xunit.Assert.NotNull(employee2);
            Xunit.Assert.NotNull(employee3);
            Xunit.Assert.True(employee.Id == 1);
            Xunit.Assert.True(employee1.Id == 1);
            Xunit.Assert.True(employee2.Id == 1);
            Xunit.Assert.True(employee3.Id == 1);

            Xunit.Assert.NotNull(order);
            Xunit.Assert.NotNull(order1);
            Xunit.Assert.True(order.Id == 1);
            Xunit.Assert.True(order1.Id == 1);
        }

        [Fact]
        public void SelectSingle()
        {
            var store = new EntityStore(_dbConnection);
            var customerId = 1;

            var customer = store.GetSingle<Customer>(c => c.Id == customerId);

            Xunit.Assert.NotNull(customer);
            Xunit.Assert.IsType<Customer>(customer);
            Xunit.Assert.True(customer.Id == customerId);
        }

        [Fact]
        public void SelectStatementWherePredicateIsFieldAccess()
        {
            var entityStore = new EntityStore(_dbConnection);
            var lastname = "Rogers";

            var employees = entityStore.Get<Employee>(p => p.LastName == lastname);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public void SelectWithJoinOnAndTResult()
        {
            var entityStore = new EntityStore(_dbConnection);
            var steve = new Employee { Id = 1, FirstName = "Steve" };
            var tResult = entityStore.GetWithJoin<Employee, Order, Order>((e, o) => e.Id == o.EmployeeId,
                                                            (e, o) => e.FirstName == steve.FirstName);

            Xunit.Assert.NotNull(tResult);

            var firstOrder = tResult.FirstOrDefault();

            Xunit.Assert.IsType<Order>(firstOrder);
        }

        [Fact]
        public void SelectJoin()
        {
            var entityStore = new EntityStore(_dbConnection);

            var orderTuples = entityStore.GetAllWithJoin<Employee, Order>((e, o) => e.Id == o.EmployeeId && e.Id == 1);

            Xunit.Assert.NotNull(orderTuples);

            var tupleEmployee = orderTuples.FirstOrDefault().Item1;
            var tupleOrder = orderTuples.FirstOrDefault().Item2;

            Xunit.Assert.IsType<Employee>(tupleEmployee);

            Xunit.Assert.IsType<Order>(tupleOrder);
        }

        [Fact]
        public void SelectGetAll()
        {
            var store = new EntityStore(_dbConnection);
            var employees = store.GetAll<Employee>();

            Xunit.Assert.NotNull(employees);
            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public void SelectWithJoinAndWhereClause()
        {
            var entityStore = new EntityStore(_dbConnection);
            var tuples = entityStore.GetWithJoin<Employee, Order>((e, o) => e.Id == o.EmployeeId,
                                                                    (e, o) => e.Id == 1);

            Xunit.Assert.NotNull(tuples);

            var firstTuple = tuples.FirstOrDefault();
            var employee = firstTuple.Item1;
            var order = firstTuple.Item2;

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1);

            Xunit.Assert.IsType<Order>(order);

            Xunit.Assert.True(order.EmployeeId == 1);
        }
        #endregion

        [Fact]
        public void GetSelectQuery()
        {
            var store = new EntityStore(_dbConnection);

            var selectQuery = store.GetSelectQuery<Employee>();

            selectQuery.Where(e => ( e.Id >= 1 && e.LastName.StartsWith("s") ) ||
                                    (e.LastName.StartsWith("rog")) )
                        .OrderBy(e => e.LastName)
                        .OrderByDescending(e => e.FirstName);
                       

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());

            var employeeLastNameWithSp = employeeSet.FirstOrDefault(e => e.LastName.ToLower().StartsWith("s"));

            Xunit.Assert.NotNull(employeeLastNameWithSp);
        }

        [Fact]
        public void GetSelectQueryWithJoin1()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Employee>()
                                    .JoinOn<Order>((e, o) => e.Id == o.EmployeeId)
                                    .Where<Employee, Order>((e, o) => o.Id >= 1 && o.OrderDate > startingOrderDate)
                                    .OrderBy(e => e.Username);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());
        }

        [Fact]
        public void GetSelectQueryWithJoin2()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Employee>()
                        .JoinOn<Order>((e, o) => e.Id == o.EmployeeId && o.Id >= 1)
                        .JoinOn<Order, Customer>((o, c) => o.CustomerId == c.Id)
                        .Where<Order, Customer, Employee>((o, c, e) => o.OrderDate > startingOrderDate 
                                                && c.City == "Gainesville" && e.Id >= 1)
                        .OrderBy(e => e.Username);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());
        }

        [Fact]
        public void GetSelectQueryWithJoin3()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Employee, Order>((e, o) => e.Id == o.EmployeeId && e.Id >= 1);

            selectQuery.Where<Order>((e, o) => o.Id >= 1 && o.OrderDate > startingOrderDate);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());
        }

        [Fact]
        public async void GetSelectQueryWithJoin2Async()
        {
            var store = new EntityStore(_dbConnection);
            var startingOrderDate = new DateTime(2015, 12, 1);
            var selectQuery = store.GetSelectQuery<Employee>()
                        .JoinOn<Order>((e, o) => e.Id == o.EmployeeId && o.Id >= 1)
                        .JoinOn<Order, Customer>((o, c) => o.CustomerId == c.Id)
                        .Where<Order, Customer, Employee>((o, c, e) => o.OrderDate > startingOrderDate
                                                && c.City == "Gainesville" && e.Id >= 1)
                        .OrderBy(e => e.Username);

            var employeeSet = await store.ExecuteQueryAsync(selectQuery);

            Xunit.Assert.NotNull(employeeSet);
            Xunit.Assert.True(employeeSet.Count() >= 2);
            Xunit.Assert.IsType<Employee>(employeeSet.FirstOrDefault());
        }

        [Fact]
        public void SeletTop()
        {
            var store = new EntityStore(_dbConnection);
            int skip = 10, take = 10;

            var selectQuery = store.GetSelectQuery<Employee>()
                                    .GetTopRows(skip + take)
                                    .Where(e => e.Id > 1)
                                    .OrderBy(e => e.LastName);


            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);

            var returnSet = employeeSet.Skip(skip).Take(take);

            Xunit.Assert.NotNull(returnSet);
            Xunit.Assert.True(returnSet.Count() == take);
        }

        [Fact]
        public void SeletTop1()
        {
            var store = new EntityStore(_dbConnection);
            int skip = 10, take = 10;
            var sortOrder = new List<SortOrderField<Employee>>();

            var selectQuery = store.GetSelectQuery<Employee>()
                                    .GetTopRows(skip + take)
                                    .Where(e => e.Id > 1)
                                    .OrderBy(e => e.LastName)
                                    .OrderBy(e => e.FirstName)
                                    .OrderByDescending(e => e.City);

            var employeeSet = store.ExecuteQuery(selectQuery);

            Xunit.Assert.NotNull(employeeSet);

            var returnSet = employeeSet.Skip(skip).Take(take);

            Xunit.Assert.NotNull(returnSet);
            Xunit.Assert.True(returnSet.Count() == take);
        }

        public void Dispose()
        {
            _dbConnection.Close();
        }
    }
}
