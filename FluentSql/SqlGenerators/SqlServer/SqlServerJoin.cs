using FluentSql.Mappers;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerJoin<L, R> : Join<L, R>
    {
        #region Constructor
        public SqlServerJoin(IQuery<L> leftQuery, IQuery<R> rightQuery, JoinType joinType = JoinType.Inner)
            : base(leftQuery, rightQuery, joinType)
        { }
        #endregion

        #region Overrides
        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var selectedJoin = EntityMapper.SqlGenerator.GetJoinOperator(JoinType);

            if (JoinType == JoinType.Cross && EntityMapper.SqlGenerator.IncludeDbNameInQuery)
            {
                sqlBuilder.AppendFormat("{0} [{1}].[{2}].[{3}] {4} ", selectedJoin,
                                                                RightQuery.DatabaseName,
                                                                RightQuery.SchemaName,
                                                                RightQuery.TableName,
                                                                RightQuery.TableAlias);
            }
            else if (JoinType == JoinType.Cross && !EntityMapper.SqlGenerator.IncludeDbNameInQuery)
            {
                sqlBuilder.AppendFormat("{0} [{1}].[{2}] {3} ", selectedJoin,
                                                                RightQuery.SchemaName,
                                                                RightQuery.TableName,
                                                                RightQuery.TableAlias);
            }
            else
            {
                if (EntityMapper.SqlGenerator.IncludeDbNameInQuery)
                    sqlBuilder.AppendFormat("{0} [{1}].[{2}].[{3}] {4} ON ", selectedJoin,
                                                                    RightQuery.DatabaseName,
                                                                    RightQuery.SchemaName,
                                                                    RightQuery.TableName,
                                                                    RightQuery.TableAlias);
                else
                    sqlBuilder.AppendFormat("{0} [{1}].[{2}] {3} ON ", selectedJoin,
                                                                    RightQuery.SchemaName,
                                                                    RightQuery.TableName,
                                                                    RightQuery.TableAlias);
                sqlBuilder.Append(Predicate.ToSql());
            }
            return sqlBuilder.ToString();
        }
        #endregion
    }
}
