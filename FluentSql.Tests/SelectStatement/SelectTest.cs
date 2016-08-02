using Dapper;
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

                var testDatabase = new Database();

                testDatabase.Name = TestConstants.TestDatabaseName;
                testDatabase.TableNamesInPlural = false;

                var store = new EntityStore(_dbConnection);

                store.Execute(SqlScripts.CREATE_DATABASE, null, false, CommandType.Text);
                store.Execute(SqlScripts.CREATE_TABLES, null, false, CommandType.Text);

                new EntityMapper(_dbConnection, TestConstants.TestDatabaseName, assemblies);
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
            var loginRequest = new LoginRequest { Username = "GDiaz" };
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
        public void SelectStatementWherePredicateIsFieldAccess()
        {
            var entityStore = new EntityStore(_dbConnection);
            var lastname = "Diaz";

            var employees = entityStore.Get<Employee>(p => p.LastName == lastname);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public void SelectJoin()
        {
            var entityStore = new EntityStore(_dbConnection);

            var orders = entityStore.GetAllWithJoin<Employee, Order>((e, o) => e.Id == o.EmployeeId && e.Id == 1);

            Xunit.Assert.NotNull(orders);

            var tupleEmployee = orders.FirstOrDefault().Item1;
            var tupleOrder = orders.FirstOrDefault().Item2;

            Xunit.Assert.IsType<Employee>(tupleEmployee);

            Xunit.Assert.IsType<Order>(tupleOrder);
        }        

        //[Fact]
        //public async void SelectStatementWithJoinOnKeyAndTResult()
        //{
        //    var entityStore = new EntityStore(_dbConnection);
        //    var tResult = await entityStore.GetAsync<Person, User, User>((p, u) => p.Id == u.PersonId);

        //    Xunit.Assert.NotNull(tResult);

        //    var firstUser = tResult.FirstOrDefault();

        //    Xunit.Assert.IsType<User>(firstUser);
        //}

        [Fact]
        public void SelectGetAll()
        {
            var store = new EntityStore(_dbConnection);
            var employees = store.GetAll<Employee>();

            Xunit.Assert.NotNull(employees);
            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        //[Fact]
        //public void SelectStatementWithJoinOnKeyAndTupleReturnSet()
        //{
        //    var entityStore = new EntityStore(_dbConnection);
        //    var tuples = entityStore.GetAll<Person, User>();

        //    Xunit.Assert.NotNull(tuples);

        //    var firstTuple = tuples.FirstOrDefault();
        //    var person = firstTuple.Item1;
        //    var user = firstTuple.Item2;

        //    Xunit.Assert.IsType<Person>(person);

        //    Xunit.Assert.IsType<User>(user);
        //}

        //[Fact]
        //public void SelectStatementWithJoinOnKeyAndWhereClause()
        //{
        //    var entityStore = new EntityStore(_dbConnection);
        //    var tuples = entityStore.Get<Person, User>((p, u) => p.Id == u.PersonId);

        //    Xunit.Assert.NotNull(tuples);

        //    var firstTuple = tuples.FirstOrDefault();
        //    var person = firstTuple.Item1;
        //    var user = firstTuple.Item2;

        //    Xunit.Assert.IsType<Person>(person);

        //    Xunit.Assert.IsType<User>(user);
        //}

        public void Dispose()
        {
            _dbConnection.Close();
        }
    }
}
