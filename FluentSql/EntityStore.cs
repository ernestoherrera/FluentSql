/*
    Copyright 2016 Ernesto Herrera

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using FluentSql;
using System;
using System.Collections.Generic;
using FluentSql.SqlGenerators.Contracts;
using System.Linq.Expressions;
using System.Data;
using FluentSql.Mappers;
using Dapper;
using System.Linq;
using FluentSql.SqlGenerators;
using System.Threading.Tasks;
using FluentSql.Support.Helpers;

namespace FluentSql
{
    public class EntityStore : IEntityStore
    {
        /// <summary>
        /// Creates the Sql statements based on the provider.
        /// </summary>
        public ISqlGenerator SqlGenerator { get; private set; }

        /// <summary>
        /// Database connection
        /// </summary>
        public IDbConnection DbConnection { get; private set; }

        public EntityStore(IDbConnection dbConnection )
        {
            DbConnection = dbConnection;
            SqlGenerator = EntityMapper.SqlGenerator;
        }

        /// <summary>
        /// Changes the database for the current connection
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public EntityStore WithDatabase(string databaseName)
        {
            if (DbConnection == null || string.IsNullOrEmpty(databaseName))
                throw new NullReferenceException("Connection object can not be null");

            if (DbConnection.State == ConnectionState.Closed)
                throw new Exception("Database Connection is closed.");

            DbConnection.ChangeDatabase(databaseName);
            return this;
        }

        #region synchronous Get calls
        /// <summary>
        /// Gets all the enities that match the criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setFilterExpression"></param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression);
            var resultSet = DapperHelper.Query<T>( DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        /// <summary>
        /// Gets a single entity by selecting the first entity in the set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public T GetSingle<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression).GetTopRows(1);
            var resultSet = DapperHelper.Query<T>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet.FirstOrDefault();
        }

        /// <summary>
        /// Gets the entity that matches its unique key
        /// </summary>
        /// <typeparam name="T">Enitity type</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetByKey<T>(dynamic key)
        {
            if (key == null) return default(T);

            var selectQuery = SqlGenerator.Select<T>();

            var query = GetQueryByKey<T>(key, selectQuery);

            if (query == null) return default(T);

            var resultSet = DapperHelper.Query<T>(DbConnection, query.ToSql(), query.Parameters);

            T entity = default(T);

            foreach (var ent in resultSet)
            {
                entity = ent;
                break;
            }

            return entity;
        }

        /// <summary>
        /// Gets the entire set of Entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetAll<T>()
        {
            var selectQuery = SqlGenerator.Select<T>();
            var resultSet = DapperHelper.Query<T>(DbConnection, selectQuery.ToSql(), null);

            return resultSet;
        }

        /// <summary>
        /// Gets a set of Entities T, R that match the join expression criteria.
        /// It splits the entities base 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        public IEnumerable<Tuple<T, R>> GetAllWithJoin<T, R>(Expression<Func<T, R, bool>> joinExpression) where R : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression);
            var resultSet = DapperHelper.QueryMultiSet<T, R>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        /// <summary>
        /// Gets a set of Entities T, R that match the join and filter expression criteria.
        /// The Field we should split and start reading the second object (default is Id)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="filterExpression">Expression that determines how to filter the Entity set</param>
        /// <returns></returns>
        public IEnumerable<Tuple<T, R>> GetWithJoin<T, R>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) where R : new() where T : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression).Where(filterExpression);
            var resultSet = DapperHelper.QueryMultiSet<T, R>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        /// <summary>
        /// Get TResult entity set that match the join and filter expression criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="joinExpression"></param>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public IEnumerable<TResult> GetWithJoin<T, R, TResult>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) where R : new() where T : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression).Where(filterExpression);
            var resultSet = DapperHelper.Query<TResult>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }
        #endregion

        #region Asynchronous Get Calls
        /// <summary>
        /// Gets an entity by key 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetByKeyAsync<T>(dynamic key)
        {
            if (key == null) return default(T);

            var selectQuery = SqlGenerator.Select<T>();

            var query = GetQueryByKey<T>(key, selectQuery);

            if (query == null) return default(T);

            var resultSet = await DapperHelper.QueryAsync<T>(DbConnection, query.ToSql(), query.Parameters);

            T entity = default(T);

            foreach (var ent in resultSet)
            {
                entity = ent;
                break;
            }

            return entity;
        }

        /// <summary>
        /// Gets a set of Entities that match the expression criteria
        /// using asynchronous threads
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression);
            var resultSet = await DapperHelper.QueryAsync<T>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        /// <summary>
        /// Gets the first occurrance of T that matches the
        /// expression criteria
        /// </summary>
        /// <typeparam name="T">Enitity type</typeparam>
        /// <param name="expression">Expression by which to filter the Entity set</param>
        /// <returns>Entity T</returns>
        public async Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression).GetTopRows(1);
            var resultSet = await DapperHelper.QueryAsync<T>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet.FirstOrDefault();
        }

        /// <summary>
        /// Gets all the entities from the 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetAllAsync<T>()
        {
            var selectQuery = SqlGenerator.Select<T>();
            var resultSet = await DapperHelper.QueryAsync<T>(DbConnection, selectQuery.ToSql());

            return resultSet;
        }

        /// <summary>
        /// Get TResult entity set that match the join and filter expression criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="joinExpression"></param>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TResult>> GetWithJoinAsync<T, R, TResult>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) where R : new() where T : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression).Where(filterExpression);
            var resultSet = await DapperHelper.QueryAsync<TResult>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        #endregion

        #region Entity inserts

        /// <summary>
        /// Inserts an entity into the database
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">The entity to by inserted</param>
        /// <returns>The new entity which autoincremented field represents the new number</returns>
        public T Insert<T>(T entity) where T : new()
        {
            var insertQuery = SqlGenerator.Insert<T>(entity);
            IEnumerable<T> entityIn = DapperHelper.Query<T>(DbConnection, insertQuery.ToSql(), insertQuery.Parameters);

            T result = entityIn.FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Inserts multiple entities into the database. One entity at a time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList"></param>
        /// <returns>A list of new autoIncremented entities</returns>
        public IEnumerable<T> InsertMany<T>(IEnumerable<T> entityList) where T : new()
        {
            var resultSet = new List<T>();

            if (entityList == null) return resultSet;

            var iter = entityList.GetEnumerator();

            while (iter.MoveNext())
            {
                T entity = iter.Current;
                var insertedEntity = Insert<T>(entity);

                if (insertedEntity != null)
                    resultSet.Add(insertedEntity);
            }

            return resultSet;
        }
        #endregion

        #region Entity Updates

        /// <summary>
        /// Updates the entity based on the entity's key fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int UpdateByKey<T>(T entity)
        {
            if (entity == null) return 0;

            var updateQuery = SqlGenerator.Update<T>(entity);
            var query = GetQueryByKey<T>(entity, updateQuery);

            var recordsAffected = DapperHelper.Execute(DbConnection, updateQuery.ToSql(), updateQuery.Parameters);

            return recordsAffected;
        }

        #endregion

        #region Delete
        /// <summary>
        /// Deletes the typed entity by using the passed entity, Id, or anonymous type
        /// to identify the key value field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int DeleteByKey<T>(dynamic entity)
        {
            if (entity == null) return 0;

            var deleteQuery = SqlGenerator.Delete<T>();
            var query = GetQueryByKey<T>(entity, deleteQuery);

            var recordsAffected = DapperHelper.Execute(DbConnection, deleteQuery.ToSql(), deleteQuery.Parameters);

            return recordsAffected;
        }
        #endregion

        #region Get query objects
        /// <summary>
        /// Returns a select query object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SelectQuery<T> GetSelectQuery<T>()
        {
            return SqlGenerator.Select<T>();
        }

        /// <summary>
        /// Returns a select query object with the specifed join
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="joinExpression"></param>
        /// <returns></returns>
        public SelectQuery<T> GetSelectQuery<T, R>(Expression<Func<T, R, bool>> joinExpression) where R : new()
        {
            var leftQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression);

            return leftQuery as SelectQuery<T>;
        }

        /// <summary>
        /// Returns an Insert query object for the specified type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public InsertQuery<T> GetInsertQuery<T>(T entity)
        {
            var insertQuery = SqlGenerator.Insert<T>(entity);

            return insertQuery;
        }

        /// <summary>
        /// Returns an Update query for the specified type parameter
        /// </summary>
        /// <typeparam name="T">Entity to be updated</typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public UpdateQuery<T> GetUpdateQuery<T>(T entity)
        {
            var updateQuery = SqlGenerator.Update<T>(entity);
            var query = GetQueryByKey<T>(entity, updateQuery);

            return query as UpdateQuery<T>;
        }

        /// <summary>
        /// Returns an Update query object for the specified type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UpdateQuery<T> GetUpdateQuery<T>()
        {
            var updateQuery = SqlGenerator.Update<T>();

            return updateQuery;
        }

        /// <summary>
        /// Returns a Delete query object for the specified type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DeleteQuery<T> GetDeleteQuery<T>(T entity)
        {
            var deleteQuery = SqlGenerator.Delete<T>(entity);

            return deleteQuery;
        }

        /// <summary>
        /// Returns a Delete query object for the specified type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DeleteQuery<T> GetDeleteQuery<T>()
        {
            var deleteQuery = SqlGenerator.Delete<T>();

            return deleteQuery;
        }

        #endregion

        #region Execute Query Methods

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(IQuery<T> query)
        {
            var entities = await DapperHelper.QueryAsync<T>(DbConnection, query.ToSql(), query.Parameters);
            return entities;
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(IQuery<T> query, IDbTransaction dbTransaction, int? commandTimeout = null)
        {
            var entities = await DapperHelper.QueryAsync<T>(DbConnection, query.ToSql(), query.Parameters, dbTransaction, commandTimeout, CommandType.Text);
            return entities;
        }

        public IEnumerable<T> ExecuteQuery<T>(IQuery<T> query)
        {
            var entities = DapperHelper.Query<T>(DbConnection, query.ToSql(), query.Parameters);

            return entities;
        }

        public IEnumerable<T> ExecuteQuery<T>(IQuery<T> query, IDbTransaction dbTransaction, int? commandTimeout = null)
        {
            var entities = DapperHelper.Query<T>(DbConnection, query.ToSql(), query.Parameters, dbTransaction, CommandType.Text, commandTimeout);

            return entities;
        }

        public IEnumerable<TResult> ExecuteQuery<T, R, TResult>(IQuery<T> query)
        {
            var entities = DapperHelper.Query<TResult>(DbConnection, query.ToSql(), query.Parameters);

            return entities;
        }

        public async Task<IEnumerable<TResult>> ExecuteQueryAsyc<T, R, TResult>(IQuery<T> query, IDbTransaction dbTransaction, int? commandTimeout = null)
        {
            var entities = await DapperHelper.QueryAsync<TResult>(DbConnection, query.ToSql(), query.Parameters, dbTransaction, commandTimeout, CommandType.Text);

            return entities;
        }

        public IEnumerable<Tuple<T, R>> ExecuteQuery<T, R>(IQuery<T> query)
        {
            var entities = DapperHelper.QueryMultiSet<T, R>(DbConnection, query.ToSql(), query.Parameters);

            return entities;
        }

        public object ExecuteScalar<T>(IQuery<T> query)
        {
            return DapperHelper.ExecuteScalar(DbConnection, query.ToSql(), query.Parameters, null, null, CommandType.Text);
        }

        #endregion

        #region Stored Procedures - SQL script Execution
        public int ExecuteScript(string sql, object parameters = null, bool executeInTransaction = false, CommandType? commandType = null, int? commandTimeout = 0)
        {
            try
            {
                int result = 0;

                if (executeInTransaction)
                {
                    using (var dbTransaction = DbConnection.BeginTransaction())
                    {
                        result = DapperHelper.Execute(DbConnection, sql, parameters, dbTransaction, commandTimeout, commandType);
                        dbTransaction.Commit();
                    }
                }
                else
                {
                    result = DapperHelper.Execute(DbConnection, sql, parameters, null, commandTimeout, commandType);
                }

                return result;
            }
            finally
            {
            }
        }

        public object ExecuteScript(string sql, object parameters = null, IDbTransaction dbTransaction = null, CommandType? commandType = null, int? commandTimeout = 0)
        {
            var result = DapperHelper.Execute(DbConnection, sql, parameters, dbTransaction, commandTimeout, commandType);

            return result;
        }

        public IEnumerable<SqlDbParameter> ExecuteProcedure(string sql, IEnumerable<SqlDbParameter> parameters = null, bool executeInTransaction = false, int? commandTimeout = null)
        {
            var dynamicParams = ConvertToDynamc(parameters);

            ExecuteScript(sql, dynamicParams, executeInTransaction, CommandType.StoredProcedure, commandTimeout);

            var returnParams = ConvertToSqlDbParameter(parameters, dynamicParams);

            return returnParams;
        }

        public object ExecuteScalar(string sql, IEnumerable<SqlDbParameter> parameters = null, bool executeInTransaction = false, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (executeInTransaction)
            {
                using (var dbTransaction = DbConnection.BeginTransaction())
                {
                    var dynamicParams = ConvertToDynamc(parameters);
                    var result = DapperHelper.ExecuteScalar(DbConnection, sql, dynamicParams, dbTransaction, commandTimeout, commandType);

                    dbTransaction.Commit();
                    parameters = ConvertToSqlDbParameter(parameters, dynamicParams);

                    return result;
                }
            }
            else
            {
                var dynamicParams = ConvertToDynamc(parameters);
                var result = DapperHelper.ExecuteScalar(DbConnection, sql, dynamicParams, null, commandTimeout, commandType);

                parameters = ConvertToSqlDbParameter(parameters, dynamicParams);

                return result;
            }

        }

        #endregion

        #region Private Methods
        private DynamicParameters ConvertToDynamc(IEnumerable<SqlDbParameter> parameters)
        {
            if (parameters == null) return null;

            var dynamicParams = new DynamicParameters();

            foreach (var param in parameters)
            {
                dynamicParams.Add(param.ParameterName, param.Value, param.DbType, param.Direction, param.Size);
            }

            return dynamicParams;
        }

        private IEnumerable<SqlDbParameter> ConvertToSqlDbParameter(IEnumerable<SqlDbParameter> dbParameters, DynamicParameters dynamicParameters)
        {
            if (dynamicParameters == null || dbParameters == null) return dbParameters;

            foreach (var paramName in dynamicParameters.ParameterNames)
            {

                var paramValue = dynamicParameters.Get<dynamic>(paramName);

                var dbParam = dbParameters.FirstOrDefault(p => p.ParameterName == $"@{paramName}");

                if (dbParam == null) continue;

                dbParam.Value = paramValue;
            }

            return dbParameters;
        }

        internal static Query<T> GetQueryByKey<T>(dynamic key, Query<T> query)
        {
            var keyColumns = EntityMapper.Entities[typeof(T)].Properties.Where(p => p.IsPrimaryKey).ToList();
            ExpressionType? linkingField = null;

            if (!keyColumns.Any())
                throw new Exception("There is no primary key for type " + typeof(T).ToString());

            var keyType = key.GetType();

            foreach (var column in keyColumns)
            {
                var leftOperand = EntityMapper.SqlGenerator.FormatFieldforSql(column.Name, query.TableAlias);
                var value = string.Empty;

                if (keyType.Namespace == null || !SystemTypes.All.Contains(keyType))
                {
                    var keyPropInfo = keyType.GetProperty(column.Name);
                    var keyPropValue = keyPropInfo.GetValue(key);

                    value = string.Format("{0}", keyPropValue);
                }
                else if (SystemTypes.All.Contains(keyType))
                {
                    value = string.Format("{0}", key);
                }

                query.Where(leftOperand, ExpressionType.Equal, value, true, linkingField);
                linkingField = ExpressionType.AndAlso;

            }

            return query;
        }

        #endregion
    }
}
