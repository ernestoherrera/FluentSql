using FluentSql.DatabaseMappers.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.DatabaseMappers.Contracts
{
    public interface IDatabaseMapper
    {        
        IEnumerable<Table> MapDatabase(IDbConnection connection, IEnumerable<string> databaseNames);
    }
}
