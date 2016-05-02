using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerSelectQuery<T> : SelectQuery<T>
    {
        #region Properties
        protected int TopRows { get; set; }

        public SqlServerSelectQuery<T> Top(int topRows)
        {
            TopRows = topRows;
            return this;
        }
        #endregion

        #region Constructor
        public SqlServerSelectQuery() : base()
        {
            Predicate = new SqlServerPredicate<T>(this);
        }
        #endregion

        #region Overrides
        public override IQuery<T> JoinOnKey<TRightEntity>()
        {
            var rightQuery = new Query<TRightEntity>();
            var join = new SqlServerJoin<T, TRightEntity>(this, rightQuery);

            Joins.Enqueue(join);

            return join.OnKey();
        }
        public override ISelectQuery<T> OrderBy(Expression<Func<T, object>> expression)
        {
            if (OrderByFields == null) OrderByFields = new List<SortOrderField<T>>();

            if (expression == null) return this;

            var orderByFieldName = ExpressionHelper.GetPropertyName((UnaryExpression)expression.Body);

            OrderByFields.Add(new SqlServerSortOrderField<T>
            {
                FieldName = orderByFieldName,
                SortOrderDirection = SortOrder.Ascending,
                TableAlias = TableAlias
            });

            return this;
        }

        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var selectFields = new List<string>();
            var sqlJoinBuilder = new StringBuilder();

            selectFields = SqlServerHelper.BraketFieldNames(Fields.Select(f => f.ColumnName), TableAlias).ToList();

            foreach (var join in Joins)
            {
                var rightTableAlias = join.RightQuery.TableAlias;

                foreach (var field in join.RightQuery.Fields)
                {
                    selectFields.Add(string.Format("{0}.[{1}]", rightTableAlias, field.ColumnName));
                }

                sqlJoinBuilder.Append(join.ToSql());
            }

            sqlBuilder.Append(string.Format("{0} {1} ", Verb, string.Join(",", selectFields)));
            sqlBuilder.Append(string.Format("FROM [{0}] {1} ", TableName, TableAlias));
            sqlBuilder.Append(sqlJoinBuilder.ToString());


            if (Predicate.Any())
                sqlBuilder.Append(string.Format("WHERE {0}", Predicate.ToSql()));

            if (OrderByFields != null)
                sqlBuilder.Append(string.Format("ORDER BY {0}", string.Join(",", OrderByFields.Select(f => f.ToSql()))));

            return sqlBuilder.ToString();
        }
        #endregion
    }
}
