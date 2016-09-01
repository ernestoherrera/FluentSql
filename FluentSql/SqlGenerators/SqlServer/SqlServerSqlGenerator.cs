using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentSql.SqlGenerators.Contracts;
using System.Data.SqlClient;
using System.Linq.Expressions;
using FluentSql.Mappers;
using FluentSql.Support;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerSqlGenerator : ISqlGenerator
    {
        public string DriverParameterIndicator { get { return "@"; } }

        public bool IncludeDbNameInQuery { get; protected set; }

        public string And { get { return "AND"; } }
            

        public string Or { get { return "OR"; } }


        public string Null { get { return "NULL"; } }

        public string Top { get { return "TOP"; } }

        #region Constructor
        public SqlServerSqlGenerator(bool includeDbNameInQuery = true)
        {
            this.IncludeDbNameInQuery = includeDbNameInQuery;
        }
        #endregion
        /// <summary>
        /// Creates a filtered select statement
        /// </summary>
        /// <typeparam name="T">Enity type</typeparam>
        /// <param name="expression">Expression by which the enitity set is to be filtered</param>
        /// <returns></returns>
        public SelectQuery<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            var select = new SqlServerSelectQuery<T>();

            return select.Where(expression) as SqlServerSelectQuery<T>;
        }

        /// <summary>
        /// Creates a Sql Server Select query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SqlServerSelectQuery for T entity</returns>
        public SelectQuery<T> Select<T>()
        {
            return new SqlServerSelectQuery<T>();
        }

        public InsertQuery<T> Insert<T>(T entity)
        {
            return new SqlServerInsertQuery<T>(entity);
        }

        public UpdateQuery<T> Update<T>(T entity)
        {
            return new SqlServerUpdateQuery<T>(entity);
        }

        public UpdateQuery<T> Update<T>()
        {
            return new SqlServerUpdateQuery<T>();
        }

        public DeleteQuery<T> Delete<T>(T entity)
        {
            return new SqlServerDeleteQuery<T>();
        }

        public DeleteQuery<T> Delete<T>()
        {
            return new SqlServerDeleteQuery<T>();
        }

        public Join<T, TRightEntity> JoinOn<T, TRightEntity>(IQuery<T> leftQuery, IQuery<TRightEntity> rightQuery, JoinType joinType)
        {
            return new SqlServerJoin<T, TRightEntity>(leftQuery, rightQuery, joinType);
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
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Default:
                    return string.Empty;
                case ExpressionType.Not:
                    return "NOT";
                default:
                    throw new NotImplementedException();
            }
        }

        public string GetJoinOperator(JoinType joinType)
        {
            switch (joinType)
            {
                case JoinType.Inner:
                    return "INNER JOIN";
                case JoinType.Right:
                    return "RIGHT JOIN";
                case JoinType.Left:
                    return "LEFT JOIN";
                case JoinType.Cross:
                    return "CROSS JOIN";
                case JoinType.FullOuter:
                    return "FULL JOIN";
                default:
                    throw new NotImplementedException();
            }
        }

        public string GetSortOrderToken(SortOrder sortDirection)
        {
            return sortDirection == SortOrder.Ascending ? "ASC" : "DESC";
        }

        public string FormatFieldforSql(Type type, string fieldName)
        {
            var tableAlias = EntityMapper.EntityMap[type].TableAlias;
            var token = string.Format("[{0}].[{1}]", tableAlias, fieldName);

            return token;
        }

        public string FormatFieldforSql(string fieldName, string tableAlias = "")
        {
            string token = string.Empty;

            if (String.IsNullOrEmpty(tableAlias))
                token = string.Format("[{0}]", fieldName);
            else
                token = string.Format("{0}.[{1}]", tableAlias, fieldName);

            return token;
        }
    }
}
