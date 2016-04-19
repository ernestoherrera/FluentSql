using FluentSql.DatabaseMappers.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentSql.DatabaseMappers.Common;
using System.Data;
using FluentSql.DatabaseMappers.SqlServerMapper;

namespace FluentSql.DatabaseMappers
{
    public class DefaultDatabaseMapper : IDatabaseMapper
    {
        #region Properties
        protected IDatabaseMapper DatabaseMapper { get; set; }
        #endregion

        #region IDatabaseMapper Implementation
        /// <summary>
        /// IDatabaseMapper implementation
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="databaseNames"></param>
        /// <returns></returns>
        public IEnumerable<Table> MapDatabase(IDbConnection connection, IEnumerable<string> databaseNames)
        {
            if (connection == null || databaseNames == null)
                throw new ArgumentNullException("Connection or DatabaseNames objects can not be null.");

            SetDatabaseMapper(DatabaseMapper);

            if (DatabaseMapper == null)
                throw new ArgumentNullException("Database mapper cannot be null.");

            var dbTables = DatabaseMapper.MapDatabase(connection, databaseNames);

            AfterDatabaseMapping(dbTables);

            return dbTables;
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Override this method to ignore tables/views that the database mapper has
        /// picked up from reading the database schema
        /// </summary>
        /// <param name="userTables"></param>
        public virtual void AfterDatabaseMapping (IEnumerable<Table> userTables)
        {

        }

        /// <summary>
        /// Override this method to set a custom Database mapper. The default database
        /// mapper is SqlServerDatabaseMapper class
        /// </summary>
        /// <param name="dbMapper"></param>
        public virtual void SetDatabaseMapper(IDatabaseMapper dbMapper)
        {
            dbMapper = new SqlServerDatabaseMapper();
        }
        #endregion
    }
}
