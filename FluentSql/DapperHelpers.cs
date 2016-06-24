using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace FluentSql
{
    public static class DapperHelper
    {
        public static async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, dynamic parameters = null, 
                                                                IDbTransaction transaction = null, int? commandTimeout = null, 
                                                                CommandType? commandType = null)
        {
            try
            {                
                var results = await SqlMapper.QueryAsync<T>(connection, sql, parameters, transaction, commandTimeout, commandType);

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static IEnumerable<T> Query<T>(IDbConnection connection, string sql, object parameters = null)
        {
            try
            {
                var results = connection.Query<T>(sql, parameters, null, buffered: true,
                                                        commandTimeout: null,
                                                        commandType: null);

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(IDbConnection connection, string sql, Func<TFirst, TSecond, TReturn> map, 
                                                            dynamic param = null, IDbTransaction transaction = null, 
                                                            bool buffered = true, string splitOn = "Id", 
                                                            int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
        {
            try
            {
                var result = await SqlMapper.QueryAsync<TFirst, TSecond, TReturn>(connection, sql, map, param, transaction, true, splitOn, commandTimeout, commandType);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static IEnumerable<Tuple<T, R>> QueryMultiSet<T, R>(IDbConnection connection, string sql, dynamic parameters, string splitOn = "Id", 
                                                                    IDbTransaction transaction = null, int? commandTimeout = null,
                                                                    CommandType? commandType = null)
        {
            return connection.Query<T, R, Tuple<T, R>>(sql, Tuple.Create, parameters as object, transaction, true, splitOn, commandTimeout, commandType);
        }

        public static int Execute(IDbConnection connection, string sql, object parameters = null, IDbTransaction transaction = null,
                                    int? commandTimeout = null, CommandType? commandType = null)
        {
            try
            {

                var result = connection.Execute(sql, parameters, transaction, commandTimeout, commandType);

                return result;
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
    }
}
