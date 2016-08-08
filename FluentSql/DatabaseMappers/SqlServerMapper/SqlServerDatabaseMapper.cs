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
        private IDbConnection _dbConnection;

        private IEnumerable<Database> _allDatabases;
        #endregion

        #region IDatabaseMapper Implementation
        public IEnumerable<Table> MapDatabase(IDbConnection dbConnection, IEnumerable<Database> databases)
        {
            if (dbConnection == null || databases == null)
                throw new ArgumentNullException("Database connection or database name(s) can not be null.");

            _dbConnection = dbConnection;
            _allDatabases = databases;

            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();

            var dbTableList = new List<Table>();

            try
            {
                foreach (var db in _allDatabases)
                {
                    _dbConnection.ChangeDatabase(db.Name);

                    var tableList = _dbConnection.Query<Table>(SqlServerConstants.USER_TABLES_QUERY);
                    var dbColumns = _dbConnection.Query<Column>(SqlServerConstants.USER_COLUMNS_QUERY);
                    var dbForeignKeys = _dbConnection.Query<ForeignKey>(SqlServerConstants.USER_FOREIGN_KEYS_QUERY);
                    var primaryKeys = _dbConnection.Query<Column>(SqlServerConstants.PRIMARY_KEYS);
                    var columnDefault = default(Column);

                    foreach (var column in dbColumns)
                    {
                        column.DatabaseType = GetDatabaseType(column.DataType);
                    }

                    foreach (var tbl in tableList)
                    {
                        tbl.Columns = dbColumns.Where(c => string.Equals(c.TableName, tbl.Name, StringComparison.CurrentCultureIgnoreCase))
                                                .ToList();

                        tbl.ForeignKeys = dbForeignKeys.Where(fk => string.Equals(fk.BaseTableName, tbl.Name, StringComparison.CurrentCultureIgnoreCase))
                                                        .ToList();

                        var tablePrimaryKeys = primaryKeys.Where(pk => string.Equals(pk.TableName, tbl.Name, StringComparison.CurrentCultureIgnoreCase))
                                                            .ToList();

                        foreach (var primaryKey in tablePrimaryKeys)
                        {
                            var pkColumn = tbl.Columns.FirstOrDefault(pk => string.Equals(pk.ColumnName, primaryKey.ColumnName, StringComparison.CurrentCultureIgnoreCase));

                            if (pkColumn == columnDefault) continue;

                            pkColumn.IsPrimaryKey = true;
                        }
                    }

                    dbTableList.AddRange(tableList);
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
