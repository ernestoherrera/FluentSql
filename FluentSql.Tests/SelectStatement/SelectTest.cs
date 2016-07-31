using Dapper;
using FluentSql.Mappers;
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
    class SelectTest
    {
        private DbConnectionTest _dbConnection;
        private readonly string _testDatabaseName = "FluentSqlTestDb";
        private readonly string _username = "appUser";
        private readonly string _password = "supersecretpasswordnumber1";

        public SelectTest()
        {
            string connString = $"Server=localhost;Database={_testDatabaseName};User ID={_username};Password={_password};";
            _dbConnection = new DbConnectionTest(connString);

            _dbConnection.Open();

            var assemblies = new[]
               {
                    Assembly.Load("FluentSql.Tests")
               };

            var sqlDbSetup = TestSqlScripts.CREATE_DATABASE;
            SqlMapper.Execute(_dbConnection, sqlDbSetup);

            _dbConnection.ChangeDatabase(_testDatabaseName);
            SqlMapper.Execute(_dbConnection, TestSqlScripts.CREATE_TABLES);

            var testDatabase = new Database();

            testDatabase.Name = _testDatabaseName;
            testDatabase.TableNamesInPlural = false;

            if (!EntityMapper.EntityMap.Keys.Any())
            {
                new EntityMapper(_dbConnection, _testDatabaseName, assemblies);
            }
        }

        [Fact]
        public async void SelectStatementWithJoinOnKeyAndTResult()
        {
            var entityStore = new EntityStore(_dbConnection);
            var tResult = await entityStore.GetAsync<Person, User, User>((p, u) => p.Id == u.PersonId);

            Xunit.Assert.NotNull(tResult);

            var firstUser = tResult.FirstOrDefault();

            Xunit.Assert.IsType<User>(firstUser);
        }

        [Fact]
        public void SelectStatementWithJoinOnKeyAndTupleReturnSet()
        {
            var entityStore = new EntityStore(_dbConnection);
            var tuples = entityStore.GetAll<Person, User>();

            Xunit.Assert.NotNull(tuples);

            var firstTuple = tuples.FirstOrDefault();
            var person = firstTuple.Item1;
            var user = firstTuple.Item2;

            Xunit.Assert.IsType<Person>(person);

            Xunit.Assert.IsType<User>(user);
        }

        [Fact]
        public void SelectStatementWithJoinOnKeyAndWhereClause()
        {
            var entityStore = new EntityStore(_dbConnection);
            var tuples = entityStore.Get<Person, User>((p, u) => p.Id == u.PersonId);

            Xunit.Assert.NotNull(tuples);

            var firstTuple = tuples.FirstOrDefault();
            var person = firstTuple.Item1;
            var user = firstTuple.Item2;

            Xunit.Assert.IsType<Person>(person);

            Xunit.Assert.IsType<User>(user);
        }

        [Fact]
        public void SelectStatementWherePredicateIsFieldAccess()
        {
            var entityStore = new EntityStore(_dbConnection);
            var tonyLastname = "Stark";

            var persons = entityStore.Get<Person>(p => p.LastName == tonyLastname);

            Xunit.Assert.NotNull(persons);

            Xunit.Assert.IsType<Person>(persons.FirstOrDefault());
        }

        [Fact]
        public async void SelectStatementWherePredicateIsClass()
        {
            var loginRequest = new LoginRequest { username = "tstark", password = _password };
            var entityStore = new EntityStore(_dbConnection);

            var users = await entityStore.GetAsync<User>(u => u.UserName == loginRequest.username);

            Xunit.Assert.NotNull(users);

            Xunit.Assert.IsType<User>(users.FirstOrDefault());
        }

        [Fact]
        public async void SelectStatementWherePredicateIsConstant()
        {
            var loginRequest = new LoginRequest { username = "tstark", password = _password };
            var entityStore = new EntityStore(_dbConnection);

            var users = await entityStore.GetAsync<User>(u => u.Id == 1);

            Xunit.Assert.NotNull(users);

            Xunit.Assert.IsType<User>(users.FirstOrDefault());

        }

        [Fact]
        public async void SelectStatementWherePredicateIsConstant2()
        {
            var loginRequest = new LoginRequest { username = "tstark", password = _password };
            var entityStore = new EntityStore(_dbConnection);

            var users = await entityStore.GetAsync<User>(u => 1 == u.Id);

            Xunit.Assert.NotNull(users);

            Xunit.Assert.IsType<User>(users.FirstOrDefault());

        }

        [Fact]
        public async void SelectStatementGetSingleWithWhere()
        {
            var loginRequest = new LoginRequest { username = "tstark", password = _password };
            var entityStore = new EntityStore(_dbConnection);

            var user = await entityStore.GetSingleAsync<User>(u => u.UserName == loginRequest.username);

            Xunit.Assert.NotNull(user);

            Xunit.Assert.IsType<User>(user);
        }

        [Fact]
        public async void SelectStatementGetByKeyAnonymous()
        {
            var entityStore = new EntityStore(_dbConnection);

            var user = await entityStore.GetByKeyAsync<User>(new { Id = 1 });

            Xunit.Assert.NotNull(user);

            Xunit.Assert.IsType<User>(user);

            Xunit.Assert.True(user.Id == 1);
        }

        [Fact]
        public async void SelectStatementGetByKeyEntity()
        {
            var entityStore = new EntityStore(_dbConnection);
            var userTemplate = new User { Id = 1 };
            var user = await entityStore.GetByKeyAsync<User>(userTemplate);

            Xunit.Assert.NotNull(user);

            Xunit.Assert.IsType<User>(user);

            Xunit.Assert.True(user.Id == 1);

        }

        public void Dispose()
        {
            _dbConnection.Close();
        }
    }
}
