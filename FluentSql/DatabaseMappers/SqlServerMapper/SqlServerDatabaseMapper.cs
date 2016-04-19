using FluentSql.DatabaseMappers.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentSql.DatabaseMappers.Common;
using System.Data;

using Dapper;

namespace FluentSql.DatabaseMappers.SqlServerMapper
{
    class SqlServerDatabaseMapper : IDatabaseMapper
    {
        #region Private Properties
        private IDbConnection DbConnection;

        private IEnumerable<string> DatabaseNames;
        #endregion

        #region IDatabaseMapper Implementation
        public IEnumerable<Table> MapDatabase(IDbConnection dbConnection, IEnumerable<string> databaseNames)
        {
            if (dbConnection == null || databaseNames == null)
                throw new ArgumentNullException("Database connection or database name(s) can not be null.");

            DbConnection = dbConnection;
            DatabaseNames = databaseNames;

            if (DbConnection.State == ConnectionState.Closed)
                DbConnection.Open();

            IEnumerable<Table> dbTableList = new List<Table>();

            try
            {
                foreach (var dbName in DatabaseNames)
                {
                    DbConnection.ChangeDatabase(dbName);

                    dbTableList = DbConnection.Query<Table>(SqlServerConstants.USER_TABLES_QUERY);
                    var dbColumns = DbConnection.Query<Column>(SqlServerConstants.USER_COLUMNS_QUERY);
                    var dbForeignKeys = DbConnection.Query<ForeignKey>(SqlServerConstants.USER_FOREIGN_KEYS_QUERY);

                    foreach (var column in dbColumns)
                    {
                        column.DatabaseType = GetDatabaseType(column.DataType);
                    }

                    foreach (var tbl in dbTableList)
                    {
                        tbl.Columns = dbColumns.Where(c => string.Equals(c.TableName, tbl.Name, StringComparison.CurrentCultureIgnoreCase))
                                                .ToList();

                        tbl.ForeignKeys = dbForeignKeys.Where(fk => string.Equals(fk.BaseTableName, tbl.Name, StringComparison.CurrentCultureIgnoreCase))
                                                        .ToList();
                    }
                }
                return dbTableList;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        #endregion

        #region Private Methods
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
        #endregion
    }
}
