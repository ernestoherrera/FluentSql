using FluentSql.SqlGenerators;
using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Contracts
{
    public interface IEntityStore
    {
        ISqlGenerator SqlGenerator { get; }

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

        #endregion

        #region Delete
        int DeleteByKey<T>(T entity);
        #endregion

        #region Get query objects

        SelectQuery<T> GetSelectQuery<T>();
        SelectQuery<T> GetSelectQuery<T, R>(Expression<Func<T, R, bool>> joinExpression) where R : new();

        InsertQuery<T> GetInsertQuery<T>(T entity);

        UpdateQuery<T> GetUpdateQuery<T>(T entity);

        UpdateQuery<T> GetUpdateQuery<T>();

        DeleteQuery<T> GetDeleteQuery<T>(T entity);

        DeleteQuery<T> GetDeleteQuery<T>();
        #endregion

        #region Stored Procedure T-Sql script support
        int ExecuteScript(string sql, object parameters = null, bool executeInTransaction = false, CommandType? commandType = null, int? commandTimeout = 0);

        IEnumerable<SqlDbParameter> ExecuteProcedure(string sql, IEnumerable<SqlDbParameter> parameters, bool executeInTransaction = false, int? commandTimeout = null);

        #endregion

        #region Execute Query
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(IQuery<T> query);

        IEnumerable<T> ExecuteQuery<T>(IQuery<T> query);

        IEnumerable<TResult> ExecuteQuery<T, R, TResult>(IQuery<T> query);

        IEnumerable<Tuple<T, R>> ExecuteQuery<T, R>(IQuery<T> query);

        object ExecuteScalar<T>(IQuery<T> query);
        #endregion
    }
}
