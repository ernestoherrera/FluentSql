using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using FluentSql.Mappers;
using System.Linq.Expressions;

namespace FluentSql.SqlGenerators
{
    public class Join<L, R>
    {
        #region Properties
        public IQuery<L> LeftQuery { get; protected set; }

        public IQuery<R> RightQuery { get; protected set; }

        public JoinType JoinType { get; protected set; }

        public Type LeftJoinType { get { return typeof(L); } }

        public Type RightJoinType { get { return typeof(R); } }

        protected Predicate<L> JoinPredicate;
        #endregion

        #region Constructor
        public Join(IQuery<L> leftQuery, IQuery<R> rightQuery, JoinType joinType = JoinType.Inner)
        {
            if (leftQuery == null || rightQuery == null) return;

            LeftQuery = leftQuery;
            RightQuery = rightQuery;
            JoinType = joinType;

            JoinPredicate = new Predicate<L>(LeftQuery);
        }
        #endregion

        #region Public Methods
        public virtual IQuery<L> OnKey()
        {
            var primaryField = LeftQuery.Fields.FirstOrDefault(f => f.IsPrimaryKey);
            var linkedField = RightQuery.Fields.FirstOrDefault(f => string.Compare(f.Name,
                                LeftQuery.TableName + primaryField.Name,
                                StringComparison.CurrentCultureIgnoreCase) == 0);

            if (primaryField == null || linkedField == null) return LeftQuery;

            var equalOperator = EntityMapper.SqlGenerator.GetOperator(ExpressionType.Equal);
            var joinUnit = new PredicateUnit
            {
                LeftOperand = primaryField.ColumnName,
                LeftOperandType = typeof(L),
                Operator = equalOperator,
                RightOperand = linkedField.ColumnName,
                RightOperandType = typeof(R)                
            };

            JoinPredicate.Add(joinUnit);

            return LeftQuery;
        }

        public IQuery<L> On(Expression<Func<L, R, bool>> joinExpression)
        {
            ExpressionHelper.WalkTree((BinaryExpression)joinExpression.Body, ExpressionType.Default, ref JoinPredicates);
            return LeftQuery;
        }

        public virtual string ToSql()
        {
            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append(string.Format("JOIN {0} {1} ON ", RightQuery.TableName, RightQuery.TableAlias));

            for (int i = 0; i < JoinPredicate.Count(); i++)
            {
                var item = JoinPredicate[i];

                if (item.LeftOperandType.IsValueType)
                {
                    sqlBuilder.AppendFormat("{0}  @{1} ", string.IsNullOrEmpty(item.Link) ? "" : item.Link,
                                                    item.LeftOperand);
                }
                else
                {
                    sqlBuilder.AppendFormat("{0}  {1}.{2} ", string.IsNullOrEmpty(item.Link) ? "" : item.Link,
                                                    TableAliasResolver(item.LeftOperandType),
                                                    item.LeftOperand);
                }

                sqlBuilder.AppendFormat("{0}", item.Operator);

                if (item.RightOperandType.IsValueType)
                {
                    sqlBuilder.AppendFormat(" @{1} ", item.RightOperand);
                }
                else
                {
                    sqlBuilder.AppendFormat("{1}.{2} ", TableAliasResolver(item.RightOperandType),
                                                        item.RightOperand);
                }
            }

            return sqlBuilder.ToString();
        }

        #endregion

        #region Protected Methods
        protected string TableAliasResolver(Type type)
        {
            return LeftQuery.EntityType == type ? LeftQuery.TableAlias : RightQuery.TableAlias;
        }
        #endregion
    }
}
