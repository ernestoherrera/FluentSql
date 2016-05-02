using FluentSql.Mappers;
using FluentSql.Support.Helpers;
using System;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace FluentSql.SqlGenerators
{
    public class SortOrderField<T>
    {
        public SortOrder SortOrderDirection { get; set; }
        public string FieldName { get; set; }
        public string TableAlias { get; set; }

        public Query<T> ParentQuery { get; private set; }

        public SortOrderField()
        { }

        public SortOrderField(Query<T> parentQuery)
        {
            ParentQuery = parentQuery;
        }

        public SortOrderField(Expression<Func<T, object>> expression)
        {
            if (expression == null) return;

            FieldName = ExpressionHelper.GetPropertyName((UnaryExpression)expression.Body);
        }

        public SortOrderField(Expression<Func<T, object>> expression, SortOrder sortOrderDirection)
        {
            if (expression == null) return;

            FieldName = ExpressionHelper.GetPropertyName((UnaryExpression)expression.Body);
            SortOrderDirection = sortOrderDirection;
        }

        public string SortOrderSql()
        {
            return EntityMapper.SqlGenerator.GetSortOrderToken(SortOrderDirection);
        }

        public virtual string ToSql()
        {
            if (ParentQuery != null)
                return string.Format("{0}.{1} {2}",
                                        ParentQuery.ResolveTableAlias(typeof(T)),
                                        FieldName,
                                        SortOrderSql());
            else
                return string.Format("{0} {1}", FieldName, SortOrderSql());
        }

        public override string ToString()
        {
            return this.ToSql();
        }
    }
}