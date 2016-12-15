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
using FluentSql.Support.Helpers;

namespace FluentSql.SqlGenerators.SqlServer
{
    /// <summary>
    /// Sql Server implementation of Sqlgenerator
    /// </summary>
    public class SqlServerSqlGenerator : ISqlGenerator
    {
        private static string[] DateParts = { "year", "quarter", "month", "dayofyear", "day", "week", "weekday", "hour", "minute", "second", "milliseconds", "microseconds", "nanosecods" };

        public string DriverParameterIndicator { get { return "@"; } }

        public string StringPatternMatchAny { get { return "%"; } }

        public bool IncludeDbNameInQuery { get; protected set; }

        public string And { get { return "AND"; } }
            

        public string Or { get { return "OR"; } }


        public string Null { get { return "NULL"; } }

        public string Top { get { return "TOP"; } }

        public string In { get { return "IN"; } }

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

        public string GetDatePartFunc(string datePart, Type entityType, string fieldName)
        {
            if (string.IsNullOrEmpty(datePart) || entityType == null || string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("Arguements can not be null.");

            var DateFunction = "DATEPART({0}, {1})";
            var tableAlias = EntityMapper.Entities[entityType].TableAlias;
            var verifiedField = EntityMapper.Entities[entityType].Properties.FirstOrDefault(p => p.Name == fieldName);

            if (verifiedField == null)
                throw new Exception(string.Format("Could not find field {0} in type {1}", fieldName, entityType));

            var formattedField = string.Format("[{0}].[{1}]", tableAlias, fieldName);

            if (datePart == Methods.ADDYEARS)
                return string.Format(DateFunction, "year", formattedField);

            else if (datePart == Methods.ADDMONTHS)
                return string.Format(DateFunction, "month", formattedField);

            else if (datePart == Methods.ADDDAYS)
                return string.Format(DateFunction, "day", formattedField);

            else if (datePart == Methods.ADDHOURS)
                return string.Format(DateFunction, "hour", formattedField);

            else if (datePart == Methods.ADDMINUTES)
                return string.Format(DateFunction, "minute", formattedField);

            else if (datePart == Methods.ADDSECONDS)
                return string.Format(DateFunction, "second", formattedField);

            else if (datePart == Methods.ADDMILLISECONDS)
                return string.Format(DateFunction, "ms", formattedField);

            else if (datePart == Methods.ADDHOURS)
                return string.Format(DateFunction, "hour", formattedField);


            else
                throw new NotSupportedException(string.Format("DatePart not supported: {0}", datePart));
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

        public string GetNullEquality()
        {
            return "IS NULL";
        }

        public string GetStringComparisonOperator()
        {
            return "LIKE";
        }

        public string GetDatePartFunction(string datePart, Type type, string fieldName)
        {
            var tableAlias = EntityMapper.Entities[type].TableAlias;
            var token = string.Format("DATEPART({0}, [{0}].[{1}])",datePart, tableAlias, fieldName);

            return token;
        }

        public string GetDateAddFunction(string datePart, Type entityType, string fieldName, int number)
        {
            if (string.IsNullOrEmpty(datePart) || entityType == null || string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("Arguements can not be null.");

            var DateFunction = "DATEADD({0}, {1}, {2})";
            var tableAlias = EntityMapper.Entities[entityType].TableAlias;
            var verifiedField = EntityMapper.Entities[entityType].Properties.FirstOrDefault(p => p.Name == fieldName);

            if (verifiedField == null)
                throw new Exception(string.Format("Could not find field {0} in type {1}", fieldName, entityType));

            var formattedField = FormatFieldforSql(entityType, fieldName);

            return string.Format(DateFunction, datePart, number, formattedField);
        }

        public string FormatFieldforSql(Type type, string fieldName)
        {
            var tableAlias = EntityMapper.Entities[type].TableAlias;
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
