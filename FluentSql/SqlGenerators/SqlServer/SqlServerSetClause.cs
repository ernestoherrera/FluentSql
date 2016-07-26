using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerSetClause<T> : SetClause<T>
    {
        public SqlServerSetClause(SqlServerUpdateQuery<T> parentQuery) : base(parentQuery)
        {  }

        public SqlServerSetClause(SqlServerUpdateQuery<T> parentQuery, object setFields)
            :base (parentQuery, setFields)
        { }

        public override string ToSql()
        {
            if (FieldParameterPairs == null)
                return string.Empty;

            if (SetClauseParts == null)
                SetClauseParts = new List<string>();

            foreach (var pair in FieldParameterPairs)
            {
                SetClauseParts.Add(string.Format("[{0}].[{1}] = {2} ", ParentQuery.TableAlias, pair.Value.ColumnName, pair.Key));
            }

            return string.Join(",", SetClauseParts);
        }
    }
}
