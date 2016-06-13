using System;
using System.Linq;
using System.Text;

using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using FluentSql.Mappers;
using System.Linq.Expressions;
using FluentSql.Support.Helpers;

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

        protected ExpressionHelper Predicate;
        #endregion

        #region Constructor
        public Join(IQuery<L> leftQuery, IQuery<R> rightQuery, JoinType joinType = JoinType.Inner)
        {
            if (leftQuery == null || rightQuery == null) return;

            LeftQuery = leftQuery;
            RightQuery = rightQuery;
            JoinType = joinType;            
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

            return LeftQuery;
        }

        public IQuery<L> On(Expression<Func<L, R, bool>> joinExpression)
        {
            if (joinExpression == null) return LeftQuery;

            Predicate = new ExpressionHelper(joinExpression, LeftQuery.ParameterNameGenerator);
            
            return LeftQuery;
        }

        public virtual string ToSql()
        {
            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append(string.Format("JOIN {0} {1} ON ", RightQuery.TableName, RightQuery.TableAlias));
            sqlBuilder.Append(Predicate.ToSql());
            
            return sqlBuilder.ToString();
        }

        #endregion        
    }
}
