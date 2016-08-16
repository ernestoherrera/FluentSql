using FluentSql.DatabaseMappers.Common;
using FluentSql.Mappers;
using FluentSql.Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Tests.UpdateStatement
{
    [TestClass]
    public class UpdateTest
    {
        private DbConnectionTest _dbConnection;

        public UpdateTest()
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
    }
}
