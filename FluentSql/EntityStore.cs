using FluentSql.Contracts;
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
        public ISqlGenerator SqlGenerator { get; private set; }

        public IDbConnection DbConnection { get; private set; }

        public EntityStore(IDbConnection dbConnection )
        {
            DbConnection = dbConnection;
            SqlGenerator = EntityMapper.SqlGenerator;
        }

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
        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression);
            var resultSet = DapperHelper.Query<T>( DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        public T GetSingle<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression).GetTopRows(1);
            var resultSet = DapperHelper.Query<T>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet.FirstOrDefault();
        }

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

        public IEnumerable<T> GetAll<T>()
        {
            var selectQuery = SqlGenerator.Select<T>();
            var resultSet = DapperHelper.Query<T>(DbConnection, selectQuery.ToSql());

            return resultSet;

        }

        public IEnumerable<Tuple<T, R>> GetAllWithJoin<T, R>(Expression<Func<T, R, bool>> joinExpression) where R : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression);
            var resultSet = DapperHelper.QueryMultiSet<T, R>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        public IEnumerable<Tuple<T, R>> GetWithJoin<T, R>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) where R : new() where T : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression).Where(filterExpression);
            var resultSet = DapperHelper.QueryMultiSet<T, R>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        public IEnumerable<TResult> GetWithJoin<T, R, TResult>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) where R : new() where T : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression).Where(filterExpression);
            var resultSet = DapperHelper.Query<TResult>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }
        #endregion

        #region Asynchronous Get Calls
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

        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression);
            var resultSet = await DapperHelper.QueryAsync<T>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }        

        public async Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression).GetTopRows(1);
            var resultSet = await DapperHelper.QueryAsync<T>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>()
        {
            var selectQuery = SqlGenerator.Select<T>();
            var resultSet = await DapperHelper.QueryAsync<T>(DbConnection, selectQuery.ToSql());

            return resultSet;
        }

        public async Task<IEnumerable<TResult>> GetWithJoinAsync<T, R, TResult>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) where R : new() where T : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression).Where(filterExpression);
            var resultSet = await DapperHelper.QueryAsync<TResult>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }
        #endregion

        #region Entity inserts
        public T Insert<T>(T entity) where T : new()
        {
            var insertQuery = SqlGenerator.Insert<T>(entity);
            IEnumerable<T> entityIn = DapperHelper.Query<T>(DbConnection, insertQuery.ToSql(), insertQuery.Parameters);

            T result = entityIn.FirstOrDefault();

            return result;
        }
        
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
        public int UpdateByKey<T>(T entity)
        {
            var updateQuery = SqlGenerator.Update<T>(entity);
            var query = GetQueryByKey<T>(entity, updateQuery);

            var recordsAffected = DapperHelper.Execute(DbConnection, updateQuery.ToSql(), updateQuery.Parameters);

            return recordsAffected;
        }

        public int UpdateWithFilter<T>(Expression<Func<T, bool>> filterExpression, params Expression<Func<T, bool>>[] setExpression)
        {
            var updateQuery = SqlGenerator.Update<T>().Set(setExpression).Where(filterExpression);
            var recordsAffected = DapperHelper.Execute(DbConnection, updateQuery.ToSql(), updateQuery.Parameters);

            return recordsAffected;
        }
        #endregion

        #region Delete
        public int Delete<T>(T entity)
        {
            var deleteQuery = SqlGenerator.Delete<T>(entity);
            var query = GetQueryByKey<T>(entity, deleteQuery);

            var recordsAffected = DapperHelper.Execute(DbConnection, deleteQuery.ToSql(), deleteQuery.Parameters);

            return recordsAffected;
        }
        #endregion

        #region Get query objects

        public SelectQuery<T> GetSelectQuery<T>()
        {
            return SqlGenerator.Select<T>();
        }

        public SelectQuery<T> GetSelectQuery<T, R>(Expression<Func<T, R, bool>> joinExpression) where R : new()
        {
            var leftQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression);

            return leftQuery as SelectQuery<T>;
        }

        public UpdateQuery<T> GetUpdateQuery<T>(T entity)
        {
            var updateQuery = SqlGenerator.Update<T>(entity);
            var query = GetQueryByKey<T>(entity, updateQuery);

            return query as UpdateQuery<T>;
        }

        public UpdateQuery<T> GetUpdateQuery<T>()
        {
            var updateQuery = SqlGenerator.Update<T>();

            return updateQuery;
        }

        public InsertQuery<T> GetInsertQuery<T>(T entity)
        {
            var insertQuery = SqlGenerator.Insert<T>(entity);

            return insertQuery;
        }

        public DeleteQuery<T> GetDeleteQuery<T>(T entity)
        {
            var deleteQuery = SqlGenerator.Delete<T>(entity);

            return deleteQuery;
        }

        #endregion

        #region Execute Query Methods

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(IQuery<T> query)
        {
            var entities = await DapperHelper.QueryAsync<T>(DbConnection, query.ToSql(), query.Parameters);
            return entities;
        }

        public IEnumerable<T> ExecuteQuery<T>(IQuery<T> query)
        {
            var entities = DapperHelper.Query<T>(DbConnection, query.ToSql(), query.Parameters);

            return entities;
        }

        public IEnumerable<TResult> ExecuteQuery<T, R, TResult>(IQuery<T> query)
        {
            var entities = DapperHelper.Query<TResult>(DbConnection, query.ToSql(), query.Parameters);

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

        public IEnumerable<SqlDbParameter> ExecuteProcedure(string sql, IEnumerable<SqlDbParameter> parameters, bool executeInTransaction = false, int? commandTimeout = null)
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
            var keyColumns = EntityMapper.EntityMap[typeof(T)].Properties.Where(p => p.IsPrimaryKey).ToList();
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
