using FluentSql.DatabaseMappers.Common;
using FluentSql.Mappers;
using FluentSql.SqlGenerators.SqlServer;
using FluentSql.Tests.Models;
using FluentSql.Tests.SqlScripts.SqlServer;
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
                if (!EntityMapper.Entities.Keys.Any())
                {
                    var assembliesOfModelTypes = new[]
                    {
                    Assembly.Load("FluentSql.Tests")
                    };

                    var store = new EntityStore(dbConnection);
                    var fluentTestDb = new Database
                    {
                        Name = TestConstants.TestDatabaseName,
                        TableNamesInPlural = true,
                        NameSpace = "FluentSql.Tests.Models"
                    };

                    var sqlGenerator = new SqlServerSqlGenerator(includeDbNameInQuery: true);
                    var databases = new List<Database> { fluentTestDb };

                    store.ExecuteScript(SqlServereSqlScript.CREATE_DATABASE, null, false, CommandType.Text);
                    store.ExecuteScript(SqlServereSqlScript.CREATE_TABLES, null, false, CommandType.Text);

                    new EntityMapper(dbConnection, databases, assembliesOfModelTypes, null, onPostEntityMapping, null);
                }
            }
        }

        /// <summary>
        /// Method used to map tables/views that were not mapped
        /// through the EntityMapper constructor.
        /// </summary>
        private void onPostEntityMapping()
        {
            var viewName = "vwCustomerOrders";
            var customerOrdersView = EntityMapper.Tables.Where(t => t.Name.ToLower() == viewName.ToLower() && !t.IsMapped).FirstOrDefault();

            if (customerOrdersView == null)
                throw new Exception(string.Format("Could not find table {0}", viewName));

            EntityMapper.MapEntity(typeof(CustomerOrder), customerOrdersView.Name, "cust_order");
        }
    }
}
