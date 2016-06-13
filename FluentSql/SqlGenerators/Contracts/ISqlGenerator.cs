using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.Contracts
{
    public interface ISqlGenerator
    {
        /// <summary>
        ///  Generates Select query object without where clause
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>A set of entities of type T</returns>
        ISelectQuery<T> Select<T>();
        /// <summary>
        /// Generates a filtered select statement
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>A set of entities of type T filtered by the expression</returns>
        ISelectQuery<T> Select<T>(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Generates an Insert query object
        /// </summary>
        /// <typeparam name="T">Entity Type</typeparam>
        /// <param name="entity">Entity to be inserted into the database</param>
        /// <returns>The value of the autoincremented column</returns>
        IInsertQuery<T> Insert<T>(T entity);

        /// <summary>
        /// Generates an Update query object that updates the 
        /// passed entity by key
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity to be updated</param>
        /// <returns></returns>
        IUpdateQuery<T> Update<T>(T entity);

        /// <summary>
        /// Generates an Update query object that updates a
        /// filtered entity set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        IUpdateQuery<T> UpdateMany<T>(T entity, Expression<Func<T, bool>> expression);

        /// <summary>
        /// Generates a Delete query object that deletes the passed entity
        /// by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns>The number of affected records</returns>
        IDeleteQuery<T> Delete<T>(T entity);

        /// <summary>
        /// Sql specific operators
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetOperator(ExpressionType type);

        /// <summary>
        /// Return the sort order direction for the specific SQL dialect.
        /// </summary>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        string GetSortOrderToken(SortOrder sortDirection);

        /// <summary>
        /// Return the And equivalent
        /// </summary>
        string And { get; }

        /// <summary>
        /// Returns the Or equivalent.
        /// </summary>
        string Or { get; }

        /// <summary>
        /// Returns the NULL equivalent.
        /// </summary>
        string Null { get; }

        /// <summary>
        /// Return the keyword to refer to selecting a specific number of rows.
        /// </summary>
        string Top { get; }

        /// <summary>
        /// Returns the required field formatting for the specific SQL dialect.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        string FormatFieldforSql(Type type, string fieldName);
    }
}
