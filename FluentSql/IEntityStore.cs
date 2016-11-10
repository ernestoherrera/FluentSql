using FluentSql.SqlGenerators;
using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FluentSql
{
    public interface IEntityStore
    {
        /// <summary>
        /// Creates the Sql statements based on the provider.
        /// </summary>
        ISqlGenerator SqlGenerator { get; }

        /// <summary>
        /// Database connection
        /// </summary>
        IDbConnection DbConnection { get; }

        /// <summary>
        /// Changes the database for the current connection
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        EntityStore WithDatabase(string databaseName);

        #region Synchronous Get
        /// <summary>
        /// Gets all the enities that match the criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setFilterExpression"></param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(Expression<Func<T, bool>> setFilterExpression);

        /// <summary>
        /// Gets a single entity by selecting the first entity in the set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        T GetSingle<T>(Expression<Func<T, bool>> filterExpression);

        /// <summary>
        /// Gets the entity that matches its unique key
        /// </summary>
        /// <typeparam name="T">Enitity type</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetByKey<T>(dynamic key);

        /// <summary>
        /// Gets the entire set of Entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetAll<T>();

        /// <summary>
        /// Gets a set of Entities T, R that match the join expression criteria.
        /// It splits the entities base 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        IEnumerable<Tuple<T, R>> GetAllWithJoin<T, R>(Expression<Func<T, R, bool>> joinExpression) where R : new();

        /// <summary>
        /// Gets a set of Entities T, R that match the join and filter expression criteria.
        /// The Field we should split and start reading the second object (default is Id)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="filterExpression">Expression that determines how to filter the Entity set</param>
        /// <returns></returns>
        IEnumerable<Tuple<T, R>> GetWithJoin<T, R>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) 
            where R : new() 
            where T : new();

        /// <summary>
        /// Get TResult entity set that match the join and filter expression criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="joinExpression"></param>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        IEnumerable<TResult> GetWithJoin<T, R, TResult>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) 
            where R : new() 
            where T : new();

        #endregion

        #region Asynchronous Gets
        /// <summary>
        /// Gets an entity by key 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> GetByKeyAsync<T>(dynamic key);

        /// <summary>
        /// Gets a set of Entities that match the expression criteria
        /// using asynchronous threads
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression);
        

        /// <summary>
        /// Gets the first occurrance of T that matches the
        /// expression criteria
        /// </summary>
        /// <typeparam name="T">Enitity type</typeparam>
        /// <param name="expression">Expression by which to filter the Entity set</param>
        /// <returns>Entity T</returns>
        Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Gets all the entities from the 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAllAsync<T>();

        /// <summary>
        /// Get TResult entity set that match the join and filter expression criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="joinExpression"></param>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> GetWithJoinAsync<T, R, TResult>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) 
            where R : new() 
            where T : new();

        #endregion

        #region Insert

        /// <summary>
        /// Inserts an entity into the database
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">The entity to by inserted</param>
        /// <returns>The new entity which autoincremented field represents the new number</returns>
        T Insert<T>(T entity) where T : new();

        Task<T> InsertAsync<T>(T entity) where T : new();

        /// <summary>
        /// Inserts multiple entities into the database. One entity at a time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList"></param>
        /// <returns>A list of new autoIncremented entities</returns>
        IEnumerable<T> InsertMany<T>(IEnumerable<T> entityList) where T : new();

        #endregion

        #region Update
        /// <summary>
        /// Updates the entity based on the entity's key fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        int UpdateByKey<T>(T entity);

        Task<int> UpdateByKeyAsync<T>(T entity);

        /// <summary>
        /// Updates fields in Set of type T given a filter criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fielsToUpdate">Anonymous type describing the fields to update</param>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        int Update<T>(object fieldsToUpdate, Expression<Func<T, bool>> filterExpression);

        /// <summary>
        /// Updates fields in Set of type T given a filter criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fielsToUpdate">Anonymous type describing the fields to update</param>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        Task<int> UpdateAsync<T>(object fieldsToUpdate, Expression<Func<T, bool>> filterExpression);

        #endregion

        #region Delete
        /// <summary>
        /// Deletes the typed entity by using the passed entity, Id, or anonymous type
        /// to identify the key value field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        int DeleteByKey<T>(dynamic entity);

        Task<int> DeleteByKeyAsync<T>(dynamic entity);

        int Delete<T>(Expression<Func<T, bool>> filterExpression);

        Task<int> DeleteAsync<T>(Expression<Func<T, bool>> filterExpression);

        #endregion

        #region Get query objects
        /// <summary>
        /// Returns a select query object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        SelectQuery<T> GetSelectQuery<T>();

        /// <summary>
        /// Returns a select query object with the specifed join
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="joinExpression"></param>
        /// <returns></returns>
        SelectQuery<T> GetSelectQuery<T, R>(Expression<Func<T, R, bool>> joinExpression) where R : new();

        /// <summary>
        /// Returns an Insert query object for the specified type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        InsertQuery<T> GetInsertQuery<T>(T entity);

        /// <summary>
        /// Returns an Update query for the specified type parameter
        /// </summary>
        /// <typeparam name="T">Entity to be updated</typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        UpdateQuery<T> GetUpdateQuery<T>(T entity);

        /// <summary>
        /// Returns an Update query object for the specified type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UpdateQuery<T> GetUpdateQuery<T>();

        /// <summary>
        /// Returns a Delete query object for the specified type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        DeleteQuery<T> GetDeleteQuery<T>(T entity);

        /// <summary>
        /// Returns a Delete query object for the specified type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        DeleteQuery<T> GetDeleteQuery<T>();
        #endregion

        #region Stored Procedure T-Sql script support
        int ExecuteScript(string sql, object parameters = null, bool executeInTransaction = false, CommandType? commandType = null, int? commandTimeout = 0);

        IEnumerable<SqlDbParameter> ExecuteProcedure(string sql, IEnumerable<SqlDbParameter> parameters, bool executeInTransaction = false, int? commandTimeout = null);

        object ExecuteScript(string sql, object parameters = null, IDbTransaction dbTransaction = null, CommandType? commandType = null, int? commandTimeout = 0);

        object ExecuteScalar(string sql, IEnumerable<SqlDbParameter> parameters = null, bool executeInTransaction = false, int? commandTimeout = null, CommandType? commandType = null);

        #endregion

        #region Execute Query
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(IQuery<T> query);

        Task<IEnumerable<T>> ExecuteQueryAsync<T>(IQuery<T> query, IDbTransaction dbTransaction, int? commandTimeout = null);

        IEnumerable<T> ExecuteQuery<T>(IQuery<T> query);

        IEnumerable<T> ExecuteQuery<T>(IQuery<T> query, IDbTransaction dbTransaction, int? commandTimeout);

        IEnumerable<TResult> ExecuteQuery<T, R, TResult>(IQuery<T> query);

        Task<IEnumerable<TResult>> ExecuteQueryAsyc<T, R, TResult>(IQuery<T> query, IDbTransaction dbTransaction, int? commandTimeout = null);

        IEnumerable<Tuple<T, R>> ExecuteQuery<T, R>(IQuery<T> query);

        object ExecuteScalar<T>(IQuery<T> query);

        object ExecuteScalar<T>(IQuery<T> query, IDbTransaction dbTransaction, int? commandTimeout = null);

        Task<T> ExecuteScalarAsync<T>(IQuery<T> query);

        Task<T> ExecuteScalarAsync<T>(IQuery<T> query, IDbTransaction dbTransaction, int? commandTimeout = null);
        #endregion
    }
}
