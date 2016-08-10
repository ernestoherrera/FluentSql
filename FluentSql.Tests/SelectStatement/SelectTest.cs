using Dapper;
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

                store.Execute(SqlScripts.CREATE_DATABASE, null, false, CommandType.Text);
                store.Execute(SqlScripts.CREATE_TABLES, null, false, CommandType.Text);

                //new EntityMapper(_dbConnection, assemblies);
                new EntityMapper(_dbConnection, new List<Database> { database }, null);
            }
        }

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

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Gary");

        }

        [Fact]
        public async void SelectAsyncGetByKeyAnonymous()
        {
            var entityStore = new EntityStore(_dbConnection);

            var employee = await entityStore.GetByKeyAsync<Employee>(new { Id = 1 });

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Gary");
        }

        [Fact]
        public async void SelectAsyncGetByKeyEntity()
        {
            var entityStore = new EntityStore(_dbConnection);
            var employeeTemplate = new Employee { Id = 1 };
            var employee = await entityStore.GetByKeyAsync<Employee>(employeeTemplate);

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Gary");

        }

        [Fact]
        public async void SelectAsyncGetSingle()
        {
            var loginRequest = new LoginRequest { Username = "GDiaz" };
            var entityStore = new EntityStore(_dbConnection);

            var employee = await entityStore.GetSingleAsync<Employee>(u => u.Username == loginRequest.Username);

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Gary");
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
        public void SelectByKey()
        {
            var store = new EntityStore(_dbConnection);
            var garyEmployeeId = 1;

            var employee = store.GetByKey<Employee>(1);
            var employee1 = store.GetByKey<Employee>(new { Id = 1 });
            var employee2 = store.GetByKey<Employee>(new Employee { Id = 1, FirstName = "Gary", LastName = "Diaz" });
            var employee3 = store.GetByKey<Employee>(garyEmployeeId);

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
        public void SelectStatementWherePredicateIsFieldAccess()
        {
            var entityStore = new EntityStore(_dbConnection);
            var lastname = "Diaz";

            var employees = entityStore.Get<Employee>(p => p.LastName == lastname);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public void SelectWithJoinOnAndTResult()
        {
            var entityStore = new EntityStore(_dbConnection);
            var gary = new Employee { Id = 1, FirstName = "Gary" };
            var tResult = entityStore.GetWithJoin<Employee, Order, Order>((e, o) => e.Id == o.EmployeeId,
                                                            (e, o) => e.FirstName == gary.FirstName);

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

        public void Dispose()
        {
            _dbConnection.Close();
        }
    }
}
