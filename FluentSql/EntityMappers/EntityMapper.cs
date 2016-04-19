using System;
using System.Collections.Generic;
using System.Data;
using System.Collections.Concurrent;
using System.Linq;

using FluentSql.DatabaseMappers.Contracts;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.DatabaseMappers.SqlServerMapper;
using FluentSql.Support;
using FluentSql.SqlGenerators;

namespace FluentSql.EntityMappers
{
    public class EntityMapper
    {
        #region Public Properties
        /// <summary>
        /// Default Database mapper
        /// </summary>
        public IDatabaseMapper DefaultDatabaseMapper { get; private set; }

        public static ConcurrentDictionary<Type, EntityMap> EntityMap = new ConcurrentDictionary<Type, EntityMappers.EntityMap>();

        public static ISqlGenerator SqlGenerator { get; private set; }
        #endregion

        #region Constructor
        public EntityMapper(IDbConnection dbConnection, Type entityInterface, IEnumerable<string> databaseNames, IDatabaseMapper defaultDatabaseMapper = null)
        {
            if (dbConnection == null || entityInterface == null || databaseNames == null)
                throw new ArgumentNullException("Database connection, Entity Interface or Database names can not be null.");

            DefaultDatabaseMapper = defaultDatabaseMapper ?? new SqlServerDatabaseMapper();

            if (dbConnection.State == ConnectionState.Closed)
                dbConnection.Open();



        }

        #endregion

        #region Private Methods
        private void MapEntities(IDbConnection dbconnection, Type entityInterface, IEnumerable<string> databaseNames)
        {
            List<Type> entities = new List<Type>();

            var dbTables = DefaultDatabaseMapper.MapDatabase(dbconnection, databaseNames);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var sqlHelper = new SqlGeneratorHelper();

            foreach (var lib in assemblies)
            {
                entities.AddRange(lib.GetTypes(entityInterface));
            }

            foreach (var type in entities)
            {
                var map = new EntityMap(type);
                var table = dbTables.FirstOrDefault(
                    t => string.Compare(t.Name, map.Name, StringComparison.CurrentCultureIgnoreCase) == 0);

                if (table == null) continue;

                map.TableAlias = sqlHelper.GetTableAlias(type);
                map.TableName = table.Name;
                map.SchemaName = table.Schema;
                map.Database = table.Database;

                foreach (var col in table.Columns)
                {
                    var prop = map.Properties.FirstOrDefault(p => p.Name.Equals(col.ColumnName));

                    if (prop == null) continue;

                    prop.IsTableField = true;
                    prop.ColumnName = col.ColumnName;
                    prop.HasDefault = col.HasDefault;
                    prop.IsPrimaryKey = col.IsPrimaryKey;
                    prop.IsAutoIncrement = col.IsAutoIncrement;
                    prop.IsReadOnly = col.IsReadOnly;
                    prop.Ignored = col.Ignore;
                    prop.Size = col.Size;
                    prop.OrdinalPosition = col.OrdinalPosition;

                }
                map.Properties.Sort();

                if (!EntityMap.TryAdd(type, map))
                {
                    //TODO: Log Error;
                }
            }
        }

        private static void SetDefaultSqlGenerator()
        {
            var currentLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedTypes = new List<Type>();
            Type defaultSqlGeneratorType = null;

            foreach (var assembly in currentLoadedAssemblies)
            {
                var loadableTypes = assembly.GetLoadableTypes();

                if (loadableTypes != null && loadableTypes.FirstOrDefault(t => t.IsSubclassOf(typeof(DefaultSqlGenerator))) != null)
                {
                    defaultSqlGeneratorType = loadableTypes.FirstOrDefault(t => t.IsSubclassOf(typeof(DefaultSqlGenerator)));
                    break;
                }
            }

            DefaultSqlGenerator defaultSqlGenerator = null;

            if (defaultSqlGeneratorType != null)
            {
                defaultSqlGenerator = (DefaultSqlGenerator)Activator.CreateInstance(defaultSqlGeneratorType);

                if (defaultSqlGenerator != null)
                {
                    defaultSqlGenerator.SetSqlGenerator();
                    SqlGenerator = defaultSqlGenerator.Generator;
                    return;
                }
            }

            SqlGenerator = new SqlServerSqlGenerator();

        }
        #endregion
    }
}
