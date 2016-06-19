using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerUpdateQuery<T> : UpdateQuery<T>
    {
        public SqlServerUpdateQuery(T entity) : base(entity)
        {

        }

        public override string ToSql()
        {
            if (Fields == null || !Fields.Any()) return string.Empty;

            SetClause = new SqlServerSetClause<T>(this);

            var sqlBuilder = new StringBuilder();

            sqlBuilder.AppendFormat("{0} {1}.[{2}] SET {3} WHERE {4} ",
                                    Verb,
                                    SchemaName,
                                    TableName,
                                    SetClause.ToSql(),
                                    Predicate.ToSql());


            sqlBuilder.Append("SELECT @@ROWCOUNT;");

            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
