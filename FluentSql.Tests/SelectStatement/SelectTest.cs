using Dapper;
using FluentSql.Mappers;
using FluentSql.Tests.Models;
using FluentSql.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
        private readonly string _serverName = "localhost";
        private readonly string _testDatabaseName = "Northwind";
        private readonly string _username = "appUser";
        private readonly string _password = "3044171035Fluent";

        public SelectTest()
        {
            string serverToken = string.Format("Server={0};", _serverName);
            string databaseToken = string.Format("Database={0};", _testDatabaseName);
            string usernameToken = string.Format("User ID={0};", _username);
            string passwordToken = string.Format("Password={0};", _password);

            string connString = serverToken + databaseToken + usernameToken + passwordToken;
            _dbConnection = new DbConnectionTest(connString);

            var assemblies = new[]
               {
                    Assembly.Load("FluentSql.Tests")
               };

            var testDatabase = new Database();

            testDatabase.Name = _testDatabaseName;
            testDatabase.TableNamesInPlural = false;

            if (!EntityMapper.EntityMap.Keys.Any())
            {
                new EntityMapper(_dbConnection, _testDatabaseName, assemblies);
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
            var loginRequest = new LoginRequest { Username = "NDavolio", Password = _password };
            var entityStore = new EntityStore(_dbConnection);

            var employees = await entityStore.GetAsync<Employee>(u => 1 == u.Id);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());

            var employee = employees.FirstOrDefault();

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Nancy");

        }

        [Fact]
        public async void SelectAsyncGetByKeyAnonymous()
        {
            var entityStore = new EntityStore(_dbConnection);

            var employee = await entityStore.GetByKeyAsync<Employee>(new { Id = 1 });

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Nancy");
        }

        [Fact]
        public async void SelectAsyncGetByKeyEntity()
        {
            var entityStore = new EntityStore(_dbConnection);
            var employeeTemplate = new Employee { Id = 1 };
            var employee = await entityStore.GetByKeyAsync<Employee>(employeeTemplate);

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Nancy");

        }

        [Fact]
        public async void SelectAsyncGetSingle()
        {
            var loginRequest = new LoginRequest { Username = "NDavolio", Password = _password };
            var entityStore = new EntityStore(_dbConnection);

            var employee = await entityStore.GetSingleAsync<Employee>(u => u.Username == loginRequest.Username);

            Xunit.Assert.NotNull(employee);

            Xunit.Assert.IsType<Employee>(employee);

            Xunit.Assert.True(employee.Id == 1 && employee.FirstName == "Nancy");
        }

        [Fact]
        public void SelectStatementWherePredicateIsFieldAccess()
        {
            var entityStore = new EntityStore(_dbConnection);
            var nancyLastname = "Davolio";

            var employees = entityStore.Get<Employee>(p => p.LastName == nancyLastname);

            Xunit.Assert.NotNull(employees);

            Xunit.Assert.IsType<Employee>(employees.FirstOrDefault());
        }

        [Fact]
        public void SelectJoin()
        {
            var entityStore = new EntityStore(_dbConnection);

            var nancysOrders = entityStore.GetAllWithJoin<Employee, Order>((e, o) => e.Id == o.EmployeeID && e.Id == 1);

            Xunit.Assert.NotNull(nancysOrders);

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
