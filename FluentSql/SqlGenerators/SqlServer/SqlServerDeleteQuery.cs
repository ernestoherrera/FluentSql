using FluentSql.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using FluentSql.SqlGenerators.SqlServer;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerDeleteQuery<T> : DeleteQuery<T>
    {
        public SqlServerDeleteQuery() : base()
        { }

        public override IQuery<T> JoinOn<TRightEntity>(System.Linq.Expressions.Expression<Func<T, TRightEntity, bool>> joinExpression, JoinType joinType = JoinType.Inner)
        {
            if (joinExpression == null)
                throw new ArgumentNullException("Join expression can't be null.");

            var sqlServerJoin = new SqlServerJoin<T, TRightEntity>(this, new SqlServerSelectQuery<TRightEntity>());

            Joins.Enqueue(sqlServerJoin);

            return sqlServerJoin.On(joinExpression);
        }

        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var dbNameFormatted = string.Format("[{0}].", DatabaseName);
            var includeDbName = EntityMapper.SqlGenerator.IncludeDbNameInQuery;

            sqlBuilder.AppendFormat("{0} {4} FROM {1}[{2}].[{3}] {4} ", 
                                    Verb,
                                    includeDbName ? dbNameFormatted : "",
                                    SchemaName, 
                                    TableName, 
                                    TableAlias);

            foreach (var join in Joins)
            {
                sqlBuilder.Append(join.ToSql());
            }

            if (PredicateParts != null && PredicateParts.Any())
                sqlBuilder.AppendFormat("WHERE {0}; ", PredicateParts.ToSql());
            else if (Predicate != null)
                sqlBuilder.AppendFormat("WHERE {0}; ", Predicate.ToSql());

            sqlBuilder.Append("SELECT @@ROWCOUNT;");

            return sqlBuilder.ToString(); 
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
