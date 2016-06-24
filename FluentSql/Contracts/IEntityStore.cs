using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Contracts
{
    public interface IEntityStore
    {
        ISqlGenerator SqlGenerator { get; }

        #region Get
        /// <summary>
        /// Gets all the enities that match the criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setFilterExpression"></param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(Expression<Func<T, bool>> setFilterExpression);

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
        /// Gets a resulting set of entity T and R
        /// where T is join with R on T key = R referenced field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        IEnumerable<Tuple<T, R>> GetAll<T, R>() where R : new();

        /// <summary>
        /// Gets a set of Entities T, R that match the expression criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="filterExpression">Expression that determines how to filter the Entity set</param>
        /// <returns></returns>
        IEnumerable<Tuple<T, R>> Get<T, R>(Expression<Func<T, R, bool>> filterExpression) where R : new();

        #endregion

        #region GetAsynch
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
        /// Gets the TResult entity set from the Join of T and R on T.Key = R.TName.TKey
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> GetAsync<T, R, TResult>(Expression<Func<T, R, bool>> filterExpression) where R : new()
            where TResult : new();

        /// <summary>
        /// Gets the first occurrance of T that matches the
        /// expression criteria
        /// </summary>
        /// <typeparam name="T">Enitity type</typeparam>
        /// <param name="expression">Expression by which to filter the Entity set</param>
        /// <returns>Entity T</returns>
        Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> expression);
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
        int Update<T>(T entity);

        /// <summary>
        /// Updates the resulting Entity set produced by applying the filter
        /// with the non-null template field values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        int UpdateWithFilter<T>(T template, Expression<Func<T, bool>> expression);
        #endregion
    }
}
