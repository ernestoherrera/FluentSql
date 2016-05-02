using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region ToString
        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append(string.Format("JOIN [{0}] {1} ON ", RightQuery.TableName, RightQuery.TableAlias));

            for (int i = 0; i < JoinPredicate.Count(); i++)
            {
                var item = JoinPredicate[i];

                if (!string.IsNullOrEmpty(item.Link) && i > 0)
                    sqlBuilder.Append(string.Format("{0}  {1}.[{2}] {3} {4}.[{5}] ",
                                                        item.Link,
                                                        TableAliasResolver(item.LeftOperandType),
                                                        item.LeftOperand,
                                                        item.Operator,
                                                        TableAliasResolver(item.RightOperandType),
                                                        item.RightOperand));
                else
                    sqlBuilder.Append(string.Format("{0}.[{1}] {2} {3}.[{4}] ",
                                                        TableAliasResolver(item.LeftOperandType),
                                                        item.LeftOperand,
                                                        item.Operator,
                                                        TableAliasResolver(item.RightOperandType),
                                                        item.LeftOperand));
            }

            return sqlBuilder.ToString();
        }
        #endregion
    }
}
