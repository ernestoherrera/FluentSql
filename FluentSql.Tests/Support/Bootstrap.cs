using FluentSql.DatabaseMappers.Common;
using FluentSql.Mappers;
using FluentSql.SqlGenerators.SqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Tests.Support
{
    public class Bootstrap
    {
        static readonly object _lockObject = new object();
        public Bootstrap(IDbConnection dbConnection)
        {
            lock (_lockObject)
            {
                if (!EntityMapper.EntityMap.Keys.Any())
                {
                    var assemblies = new[]
                    {
                    Assembly.Load("FluentSql.Tests")
                    };

                    var store = new EntityStore(dbConnection);
                    var database = new Database
                    {
                        Name = TestConstants.TestDatabaseName,
                        TableNamesInPlural = true,
                        NameSpace = "FluentSql.Tests.Models"
                    };

                    var sqlGenerator = new SqlServerSqlGenerator(includeDbNameInQuery: true);

                    store.ExecuteScript(SqlScripts.CREATE_DATABASE, null, false, CommandType.Text);
                    store.ExecuteScript(SqlScripts.CREATE_TABLES, null, false, CommandType.Text);

                    new EntityMapper(dbConnection, sqlGenerator, assemblies);
                }
            }
        }
    }
}
