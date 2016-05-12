using FluentSql.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentSql.SqlGenerators.Contracts;
using System.Linq.Expressions;
using System.Data;
using FluentSql.Mappers;
using Dapper;

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

        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> filterExpression)
        {
            var selectQuery = SqlGenerator.Select<T>(filterExpression);
            var resultSet = DbConnection.Query<T>(selectQuery.ToSql(), selectQuery.Parameters);

            return resultSet;
        }
    }
}
