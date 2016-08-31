using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentSql.Support;
using FluentSql.Mappers;
using System.Linq.Expressions;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerUpdateQuery<T> : UpdateQuery<T>
    {
        private List<string> _unsupportedTypes = new List<string> { "text", "image" };
        public SqlServerUpdateQuery() : base()
        {
            Fields = Fields.Where(fld => !_unsupportedTypes.Contains(fld.ColumnDataType))
                            .ToList();
        }

        public SqlServerUpdateQuery(T entity) : this()
        {
            Entity = entity;

            SetClause = new SqlServerSetClause<T>(this);
        }

        public override UpdateQuery<T> Set(object setFields)
        {
            SetClause = new SqlServerSetClause<T>(this, setFields);
            return this;
        }

        public override string ToSql()
        {
            if (Fields == null || !Fields.Any()) return string.Empty;

            var sqlJoinBuilder = new StringBuilder();
            var sqlBuilder = new StringBuilder();
            var predicateSql = string.Empty;
            var includeDbName = EntityMapper.SqlGenerator.IncludeDbNameInQuery;
            var dbNameFormatted = string.Format("[{0}].", DatabaseName);

            if (PredicateParts != null)
                predicateSql = PredicateParts.ToSql();
            else
                predicateSql = Predicate == null ? "" : Predicate.ToSql();

            foreach (var join in Joins)
            {
                sqlJoinBuilder.Append(join.ToSql());
            }

            sqlBuilder.AppendFormat("{0} [{1}] SET {2} FROM {3}[{4}].[{5}] [{1}] {6} WHERE {7}; ",
                                Verb,
                                TableAlias,
                                SetClause.ToSql(),
                                includeDbName ? dbNameFormatted : "",
                                SchemaName,
                                TableName,
                                sqlJoinBuilder.ToString(),
                                predicateSql);
            
            sqlBuilder.Append("SELECT @@ROWCOUNT;");

            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
