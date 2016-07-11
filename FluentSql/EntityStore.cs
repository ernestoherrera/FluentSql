using FluentSql.Contracts;
using System;
using System.Collections.Generic;
using FluentSql.SqlGenerators.Contracts;
using System.Linq.Expressions;
using System.Data;
using FluentSql.Mappers;
using Dapper;
using System.Linq;
using System.Reflection;
using FluentSql.Support.Helpers;
using FluentSql.SqlGenerators;
using System.Threading.Tasks;

namespace FluentSql
{
    public class EntityStore : IEntityStore
    {
        public ISqlGenerator SqlGenerator { get; private set; }

        public IDbConnection DbConnection { get; private set; }
                
        public IDbTransaction DbTransaction { get; private set; }

        public IDbCommand DbCommand { get; private set; }

        public int? CommandTimeout { get; private set; }

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

            var query = GetSelectQueryByKey<T>(key);

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

        public IEnumerable<Tuple<T, R>> GetWithJoin<T, R>(Expression<Func<T, R, bool>> joinExpression, Expression<Func<T, R, bool>> filterExpression) where R : new()
        {
            var selectQuery = SqlGenerator.Select<T>().JoinOn<R>(joinExpression).Where(filterExpression);
            var resultSet = DapperHelper.QueryMultiSet<T, R>(DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }
        #endregion

        #region Asynchronous Get Calls
        public async Task<T> GetByKeyAsync<T>(dynamic key)
        {
            if (key == null) return default(T);

            var query = GetSelectQueryByKey<T>(key);

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
        #endregion

        #region Entity inserts
        public T Insert<T>(T entity) where T : new()
        {
            var insertQuery = SqlGenerator.Insert<T>(entity);
            var id = DapperHelper.ExecuteScalar(DbConnection, insertQuery.ToSql(), insertQuery.Parameters);

            T result = new T();

            result = entity;

            if (id != null)
            {
                var field = EntityMapper.EntityMap[typeof(T)].Properties.FirstOrDefault(p => p.IsAutoIncrement);

                if (field != null)
                    field.PropertyInfo.SetValue(result, id);
            }

            return result;
        }
        
        public IEnumerable<T> InsertMany<T>(IEnumerable<T> entityList) where T : new()
        {
            var resultSet = new List<T>();

            if (entityList == null) return resultSet;
            
            var iter = entityList.GetEnumerator();
            var field = EntityMapper.EntityMap[typeof(T)].Properties.FirstOrDefault(p => p.IsAutoIncrement);

            while (iter.MoveNext())
            {
                T entity = iter.Current;
                var insertQuery = SqlGenerator.Insert<T>(entity);
                var id = DapperHelper.ExecuteScalar(DbConnection, insertQuery.ToSql(), insertQuery.Parameters);

                if (field != null)
                {
                    var resultEntity = new T();

                    resultEntity = entity;
                    field.PropertyInfo.SetValue(resultEntity, id);
                    resultSet.Add(resultEntity);
                }
            }

            return resultSet;
        }
        #endregion

        #region Entity Updates        
        public int Update<T>(T entity)
        {
            var updateQuery = SqlGenerator.Update<T>(entity);

            var recordsAffected = DapperHelper.Execute(DbConnection, updateQuery.ToSql(), updateQuery.Parameters);

            return recordsAffected;
        }

        public int UpdateWithFilter<T>(T entity, Expression<Func<T, bool>> filterExpression)
        {
            var updateQuery = SqlGenerator.Update<T>(entity).Where(filterExpression);
            var recordsAffected = DapperHelper.Execute(DbConnection, updateQuery.ToSql(), updateQuery.Parameters);

            return recordsAffected;
        }
        #endregion

        private SelectQuery<T> GetSelectQueryByKey<T>(dynamic key)
        {
            var query = SqlGenerator.Select<T>();
            var keyColumns = query.Fields.Where(fld => fld.IsPrimaryKey).ToList();
            ExpressionType? linkingField = null;

            if (!keyColumns.Any()) return null;

            if (key.GetType().Namespace == null)
            {
                Type keyType = key.GetType();

                foreach (var column in keyColumns)
                {
                    var keyPropInfo = keyType.GetProperty(column.Name);
                    var keyPropValue = keyPropInfo.GetValue(key);
                    var lefOperand = EntityMapper.SqlGenerator.FormatFieldforSql(column.Name, query.TableAlias);
                    var value = string.Format("{0}", keyPropValue);

                    query.Where(lefOperand, ExpressionType.Equal, value, true, linkingField);
                    linkingField = ExpressionType.AndAlso;
                }
            }
            else
            {
                var column = keyColumns.FirstOrDefault();
                var lefOperand = EntityMapper.SqlGenerator.FormatFieldforSql(column.Name, query.TableAlias);
                var value = string.Format("{0}", key);

                query.Where(lefOperand, ExpressionType.Equal, value, true, linkingField);
            }

            return query;
        }
    }
}
