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

        public string GetDateDiffFunction(Type minuendType, string minuend, Type subtrahendType, string subtrahend)
        {
            if (minuendType == null || string.IsNullOrEmpty(minuend))
                throw new ArgumentNullException("entityTypeMinuend cannot be null");

            if (subtrahendType == null || string.IsNullOrEmpty(subtrahend))
                throw new ArgumentNullException("entityTypeMinuend cannot be null");

            var datediffFunction = "DATEDIFF({0}, {1}, {2})";
            var formattedMinuend = GetDateDiffField(minuendType, minuend);
            var formattedSubtrahend = GetDateDiffField(subtrahendType, subtrahend);
            var datePart = GetDatePartArgument(Methods.GETDAYDIFF);

            return string.Format(datediffFunction, datePart, formattedMinuend, formattedSubtrahend);

        }

        public string GetDatePartFunction(string methodName, Type entityType, string fieldName)
        {
            if (string.IsNullOrEmpty(methodName) || entityType == null || string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("Arguements can not be null.");

            var datePartFunction = "DATEPART({0}, {1})";
            var verifiedField = EntityMapper.Entities[entityType].Properties.FirstOrDefault(p => p.Name == fieldName);

            if (verifiedField == null)
                throw new Exception(string.Format("Could not find field {0} in type {1}", fieldName, entityType));

            var formattedField = FormatFieldforSql(entityType, fieldName);
            var datePart = GetDatePartArgument(methodName);

            return string.Format(datePartFunction, datePart, formattedField);
        }

        public string GetDateAddFunction(string methodName, Type entityType, string fieldName, int number)
        {
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentNullException("Method Name arguement can not be null.");

            if (entityType == null)
                throw new ArgumentNullException("Entity type (entityType) can not be null.");

            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("Field Name (fieldName) can not be null.");

            var DateFunction = "DATEADD({0}, {1}, {2})";
            var verifiedField = EntityMapper.Entities[entityType].Properties.FirstOrDefault(p => p.Name == fieldName);

            if (verifiedField == null)
                throw new Exception(string.Format("Could not find field {0} in type {1}", fieldName, entityType));

            var formattedField = FormatFieldforSql(entityType, fieldName);
            var datePart = GetDatePartArgument(methodName);

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

        #region Private Methods
        private string GetDatePartArgument(string methodName)
        {
            var datePart = string.Empty;

            if (methodName == Methods.ADDYEARS || methodName == Methods.GETYEAR)
                return "year";

            if (methodName == Methods.ADDQUARTERS || methodName == Methods.GETQUARTER)
                return "quarter";

            if (methodName == Methods.ADDDAYOFYEAR || methodName == Methods.GETDAYOFYEAR)
                return "dayofyear";

            else if (methodName == Methods.ADDMONTHS || methodName == Methods.GETMONTH)
                return "month";

            else if (methodName == Methods.ADDDAYS || methodName == Methods.GETDAY || methodName == Methods.GETDAYDIFF)
                return "day";

            if (methodName == Methods.ADDWEEKS || methodName == Methods.GETWEEK)
                return "week";

            else if (methodName == Methods.ADDHOURS || methodName == Methods.GETHOUR)
                return "hour";

            else if (methodName == Methods.ADDMINUTES || methodName == Methods.GETMINUTE)
                return "minute";

            else if (methodName == Methods.ADDSECONDS || methodName == Methods.GETSECOND)
                return "second";

            else if (methodName == Methods.ADDMILLISECONDS || methodName == Methods.GETMILLISECOND)
                return "ms";

            else if (methodName == Methods.ADDDAYOFWEEK || methodName == Methods.GETWEEKDAY)
                return "weekday";

            else
                throw new NotSupportedException(string.Format("Method Name not supported: {0}", methodName));
        }

        private string GetDateDiffField(Type operandType, string operandName)
        {
            if (operandType == null) return null;

            if (operandType == typeof(DateTime?)) return operandName;
            
            var verifiedField = EntityMapper.Entities[operandType].Properties
                                        .FirstOrDefault(p => p.Name == operandName);

            if (verifiedField == null)
                throw new Exception(string.Format("Could not find field {0} in type {1}", operandType, operandType));

            return FormatFieldforSql(operandType, operandName);
        }
        #endregion
    }
}
