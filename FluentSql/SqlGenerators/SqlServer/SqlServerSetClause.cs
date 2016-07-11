using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerSetClause<T> : SetClause<T>
    {
        public SqlServerSetClause(SqlServerUpdateQuery<T> parentQuery) : base(parentQuery)
        {  }

        public override string ToSql()
        {
            var setClause = new List<string>();

            foreach (var pair in this.FieldParameterPairs)
            {
                setClause.Add(string.Format(" [{0}] = {1} ", pair.Key.ColumnName, pair.Value));
            }

            return string.Join(",", setClause);
        }
    }
}
