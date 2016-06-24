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

        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression);
            var resultSet = DapperHelper.Query<T>( DbConnection, selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }

        public T GetByKey<T>(dynamic key)
        {
            if (key == null) return default(T);
                                    
            var query = SqlGenerator.Select<T>();
            var keyColumns = query.Fields.Where(fld => fld.IsPrimaryKey).ToList();
            ExpressionType? linkingField = null;

            if (!keyColumns.Any()) return default(T);
            
            if (key.GetType().Namespace == null)
            {
                foreach (var column in keyColumns)
                {
                    var value = column.PropertyInfo.GetValue(key);
                    var keyPredicate = new PredicateUnit
                    {
                        LeftOperand = EntityMapper.SqlGenerator.FormatFieldforSql(column.Name, query.TableAlias),
                        Operator = ExpressionType.Equal,
                        RightOperand = value,
                        IsRightOperandParameter = true,
                        LinkingOperator = linkingField
                    };

                    query.Where(keyPredicate);
                    linkingField = ExpressionType.AndAlso;
                }                
            }
            else
            {
                var column = keyColumns.FirstOrDefault();
                
                var keyPredicate = new PredicateUnit
                {
                    LeftOperand = EntityMapper.SqlGenerator.FormatFieldforSql(column.Name, query.TableAlias),
                    Operator = ExpressionType.Equal,
                    RightOperand = key,
                    IsRightOperandParameter = true,
                    LinkingOperator = linkingField
                };

                query.Where(keyPredicate);
            }

            var entities = DapperHelper.Query<T>(DbConnection, query.ToSql(), query.Parameters);

            T entity = default(T);

            foreach (var ent in entities)
            {
                entity = ent;
                break;
            }

            return entity;
        }

        public IEnumerable<T> GetAll<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Tuple<T, R>> GetAll<T, R>() where R : new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Tuple<T, R>> Get<T, R>(Expression<Func<T, R, bool>> filterExpression) where R : new()
        {
            throw new NotImplementedException();
        }

        public Task<T> GetByKeyAsync<T>(dynamic key)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> GetAsync<T, R, TResult>(Expression<Func<T, R, bool>> filterExpression)
            where R : new()
            where TResult : new()
        {
            throw new NotImplementedException();
        }

        public Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public T Insert<T>(T entity) where T : new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> InsertMany<T>(IEnumerable<T> entityList) where T : new()
        {
            throw new NotImplementedException();
        }

        public int Update<T>(T entity)
        {
            throw new NotImplementedException();
        }

        public int UpdateWithFilter<T>(T template, Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
