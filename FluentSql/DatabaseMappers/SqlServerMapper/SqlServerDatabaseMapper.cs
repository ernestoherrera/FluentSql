using FluentSql.DatabaseMappers.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentSql.DatabaseMappers.Common;
using System.Data;

namespace FluentSql.DatabaseMappers.SqlServerMapper
{
    class SqlServerDatabaseMapper : IDatabaseMapper
    {
        public IEnumerable<Table> MapDatabase(IDbConnection connection, IEnumerable<string> databaseNames)
        {
            throw new NotImplementedException();
        }
    }
}
