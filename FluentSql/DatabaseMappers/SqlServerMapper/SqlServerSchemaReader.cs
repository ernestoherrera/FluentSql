using FluentSql.DatabaseMappers.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace FluentSql.DatabaseMappers.SqlServerMapper
{
    internal class SqlServerSchemaReader
    {
        private readonly IDbConnection DbConnection;

        private readonly IEnumerable<string> DatabaseNames;

        public SqlServerSchemaReader(IDbConnection dbConnection, IEnumerable<string> databaseNames)
        {
            if (dbConnection == null || databaseNames == null)
                throw new ArgumentNullException("Database connection or database name(s) can not be null.");

            DbConnection = dbConnection;

            if (DbConnection.State == ConnectionState.Closed)
                DbConnection.Open();

        }

        public IEnumerable<Table> ReadSchema(IEnumerable<string> databaseNames)
        {
            if (databaseNames == null)
                throw new ArgumentNullException("Database names can not be empty or null.");

            IEnumerable<Table> dbTableList;

            try
            {
                dbTableList = DbConnection.Query<Table>(SqlServerConstants.USER_TABLES_QUERY);
                var dbColumns = DbConnection.Query<Column>(SqlServerConstants.USER_COLUMNS_QUERY);

                foreach (var column in dbColumns)
                {
                    column.DatabaseType = GetDatabaseType(column.DataType);
                }

                foreach (var tbl in dbTableList)
                {
                    tbl.Columns = dbColumns.Where(c => string.Equals(c.TableName, tbl.Name, StringComparison.CurrentCultureIgnoreCase))
                                            .ToList();
                }

                return dbTableList;

            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }           
        }

        private DbType GetDatabaseType(string sqlType)
        {
            DbType sysType = DbType.String;
            switch (sqlType)
            {
                case "bigint":
                    sysType = DbType.Int64;
                    break;
                case "smallint":
                    sysType = DbType.Int16;
                    break;
                case "int":
                    sysType = DbType.Int32;
                    break;
                case "uniqueidentifier":
                    sysType = DbType.Guid;
                    break;
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "date":
                case "time":
                    sysType = DbType.DateTime;
                    break;
                case "float":
                    sysType = DbType.Double;
                    break;
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType = DbType.Decimal;
                    break;
                case "tinyint":
                    sysType = DbType.Byte;
                    break;
                case "bit":
                    sysType = DbType.Boolean;
                    break;
                case "image":
                case "binary":
                case "varbinary":
                case "timestamp":
                    sysType = DbType.Binary;
                    break;
                case "geography":
                    sysType = DbType.Binary;
                    break;
                case "geometry":
                    sysType = DbType.Binary;
                    break;
            }
            return sysType;
        }
    }
}
