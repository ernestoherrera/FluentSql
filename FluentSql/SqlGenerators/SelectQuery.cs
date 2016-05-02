using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class SelectQuery<T> : Query<T> , ISelectQuery<T>, IDisposable
    {
        #region Properties
        protected readonly string SELECT = "SELECT";

        protected List<SortOrderField<T>> OrderByFields;
        #endregion

        #region Constructors
        public SelectQuery() : base()
        {
            Verb = SELECT;

        }
        #endregion        

        #region Sort Order
        public virtual ISelectQuery<T> OrderBy(Expression<Func<T, object>> expression)
        {
            return SetOrderByClause(expression, SortOrder.Ascending);
        }

        public virtual ISelectQuery<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            return SetOrderByClause(expression, SortOrder.Descending);
        }

        protected ISelectQuery<T> SetOrderByClause(Expression<Func<T, object>> expression, SortOrder sortOrderDirection)
        {
            if (OrderByFields == null) OrderByFields = new List<SortOrderField<T>>();

            if (expression == null) return this;

            var orderByFieldName = ExpressionHelper.GetPropertyName((UnaryExpression)expression.Body);

            OrderByFields.Add(new SortOrderField<T>
            {
                FieldName = orderByFieldName,
                SortOrderDirection = sortOrderDirection,
                TableAlias = ResolveTableAlias(typeof(T))
            });

            return this;
        }

        public virtual ISelectQuery<T> OrderBy(params SortOrderField<T>[] sortOrderArray)
        {
            if (sortOrderArray == null) return this;

            if (OrderByFields == null) OrderByFields = new List<SortOrderField<T>>();

            OrderByFields.AddRange(sortOrderArray);

            return this;
        }

        public virtual ISelectQuery<T> OrderBy(List<SortOrderField<T>> sortOrderFields)
        {
            if (sortOrderFields == null) return this;

            if (OrderByFields == null) OrderByFields = new List<SortOrderField<T>>();

            OrderByFields.AddRange(sortOrderFields);

            return this;
        }
        #endregion

        #region ToString methods
        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var selectFields = new List<string>();
            var sqlJoinBuilder = new StringBuilder();

            selectFields.AddRange(Fields.Select(f => string.Format("{0}.{1}", TableAlias, f.ColumnName)));

            foreach (var join in Joins)
            {
                var rightTableAlias = join.RightQuery.TableAlias;

                foreach (var field in join.RightQuery.Fields)
                {
                    selectFields.Add(string.Format("{0}.{1}", rightTableAlias, field.ColumnName));
                }

                sqlJoinBuilder.Append(join.ToSql());
            }

            sqlBuilder.Append(string.Format("{0} {1} ", Verb, string.Join(",", selectFields)));
            sqlBuilder.Append(string.Format("FROM {0} {1} ", TableName, TableAlias));
            sqlBuilder.Append(sqlJoinBuilder.ToString());


            if (Predicate.Any())
                sqlBuilder.Append(string.Format("WHERE {0}", Predicate.ToSql()));

            if (OrderByFields != null)
                sqlBuilder.Append(string.Format("ORDER BY {0}", string.Join(",", OrderByFields.Select(f => f.ToSql()))));

            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }

        #endregion
    }
}
