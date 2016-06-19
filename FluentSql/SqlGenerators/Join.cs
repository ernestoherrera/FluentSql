using System;
using System.Linq;
using System.Text;

using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using FluentSql.Mappers;
using System.Linq.Expressions;
using FluentSql.Support.Helpers;
using Dapper;

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

        public DynamicParameters Parameters { get; protected set; }

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
        
        public IQuery<L> On(Expression<Func<L, R, bool>> joinExpression)
        {
            if (joinExpression == null) return LeftQuery;

            Predicate = new ExpressionHelper(joinExpression, LeftQuery.ParameterNameGenerator);
            Parameters = Predicate.QueryParameters;

            return LeftQuery;
        }

        public virtual string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var selectedJoin = Enum.GetName(typeof(JoinType), JoinType);

            sqlBuilder.Append(string.Format("{0} JOIN {1} {2} ON ", selectedJoin, RightQuery.TableName, RightQuery.TableAlias));
            sqlBuilder.Append(Predicate.ToSql());
            
            return sqlBuilder.ToString();
        }

        #endregion        
    }
}
