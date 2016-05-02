using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerPredicate<T> : Predicate<T>
    {
        public SqlServerPredicate(IQuery<T> parentQuery) : base(parentQuery)
        { }

        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var iter = Predicates.GetEnumerator();

            while (iter.MoveNext())
            {
                var item = iter.Current as PredicateUnit;

                if (item.LeftOperandType.IsValueType)
                {
                    sqlBuilder.AppendFormat("{0} @{1} ", string.IsNullOrEmpty(item.Link) ? "" : item.Link,
                                                            item.LeftOperand);
                }
                else
                {
                    sqlBuilder.AppendFormat("{0} {1}.[{2}] ", string.IsNullOrEmpty(item.Link) ? "" : item.Link,
                                                            ParentQuery.ResolveTableAlias(item.LeftOperandType),
                                                            item.LeftOperand);
                }

                sqlBuilder.AppendFormat("{0}", item.Operator);

                if (item.RightOperandType.IsValueType)
                {
                    sqlBuilder.AppendFormat(" @{1} ", item.RightOperand);
                }
                else
                {
                    sqlBuilder.AppendFormat("{1}.[{2}] ", ParentQuery.ResolveTableAlias(item.RightOperandType),
                                                        item.RightOperand);
                }
            }

            return sqlBuilder.ToString();
        }
    }
}
