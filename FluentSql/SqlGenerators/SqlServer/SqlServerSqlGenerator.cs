using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentSql.SqlGenerators.Contracts;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerSqlGenerator : ISqlGenerator
    {
        /// <summary>
        /// Creates a filtered select statement
        /// </summary>
        /// <typeparam name="T">Enity type</typeparam>
        /// <param name="expression">Expression by which the enitity set is to be filtered</param>
        /// <returns></returns>
        public ISelectQuery<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            var select = new SqlServerSelect<T>();

            return select.Where(expression) as SqlServerSelect<T>;
        }

        /// <summary>
        /// Creates a Select statement without a where clause predicates
        /// Use to get an entire set without filtering nor sorting
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>A set of entities of type T</returns>
        public ISelectQuery<T> Select<T>()
        {
            return new SqlServerSelect<T>();
        }

        public IInsertQuery<T> Insert<T>(T entity)
        {
            return new SqlServerInsertQuery<T>(entity);
        }

        public IUpdateQuery<T> Update<T>(T entity)
        {
            return new SqlServerUpdateQuery<T>(entity);
        }

        public IUpdateQuery<T> UpdateMany<T>(T entity, Expression<Func<T, bool>> expression)
        {
            var updateQuery = new SqlServerUpdateQuery<T>(entity);

            return updateQuery.Where(expression) as SqlServerUpdateQuery<T>;
        }

        public IDeleteQuery<T> Delete<T>(T entity)
        {
            var deleteQuery = new SqlServerDeleteQuery<T>(entity);

            return deleteQuery;
        }

        /// <summary>
        /// Gets the operator specific for SQL server queries
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The expression types SQL server equivalent operator.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    return "AND";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Default:
                    return string.Empty;
                default:
                    throw new NotImplementedException();
            }
        }

        public string GetSortOrderToken(SortOrder sortDirection)
        {
            return sortDirection == SortOrder.Ascending ? "ASC" : "DESC";
        }
    }
}
