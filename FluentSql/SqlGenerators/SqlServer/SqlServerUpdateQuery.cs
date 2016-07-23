using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentSql.Support;
using FluentSql.Mappers;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerUpdateQuery<T> : UpdateQuery<T>
    {

        public SqlServerUpdateQuery() : base()
        {
            
        }

        public SqlServerUpdateQuery(T entity) : base()
        {
            Entity = entity;
            
            SetClause = new SqlServerSetClause<T>(this);
        }        
        
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
            if (Fields == null || !Fields.Any()) return string.Empty;            

            var sqlBuilder = new StringBuilder();
            var predicateSql = string.Empty;

            if (PredicateParts != null)
                predicateSql = PredicateParts.ToSql();
            else
                predicateSql = Predicate == null ? "" : Predicate.ToSql();

            if (EntityMapper.SqlGenerator.IncludeDbNameInQuery)
            {                
                sqlBuilder.AppendFormat("{0} [{1}] SET {2} FROM [{3}].[{4}].[{5}] [{1}]  WHERE {6};",
                                    Verb,
                                    TableAlias,
                                    SetClause.ToSql(),
                                    DatabaseName,
                                    SchemaName,
                                    TableName,
                                    predicateSql);
            }
            else
            {
                sqlBuilder.AppendFormat("{0} [{1}] SET {2} FROM [{3}].[{4}] [{1}]  WHERE {5};",
                                    Verb,
                                    TableAlias,
                                    SetClause.ToSql(),                                    
                                    SchemaName,
                                    TableName,
                                    predicateSql);
            }

            sqlBuilder.Append("SELECT @@ROWCOUNT;");

            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
