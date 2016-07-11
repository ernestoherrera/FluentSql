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
            var sqlBuilder = new StringBuilder(this.Verb);

            if (EntityMapper.SqlGenerator.IncludeDbNameInQuery)
                sqlBuilder.Append(string.Format("FROM [{0}].[{1}].[{2}] {3} ", DatabaseName, SchemaName, TableName, TableAlias));
            else
                sqlBuilder.Append(string.Format("FROM [{0}].[{1}] {2} ", SchemaName, TableName, TableAlias));

            foreach (var join in Joins)
            {
                sqlBuilder.Append(join.ToSql());
            }

            if (PredicateParts != null && PredicateParts.Any())
                sqlBuilder.Append(string.Format("WHERE {0} ", PredicateParts.ToSql()));
            else if (Predicate != null)
                sqlBuilder.Append(string.Format("WHERE {0} ", Predicate.ToSql()));


            return sqlBuilder.ToString(); 
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
