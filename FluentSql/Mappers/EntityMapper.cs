using System;
using System.Collections.Generic;
using System.Data;
using System.Collections.Concurrent;
using System.Linq;

using FluentSql.DatabaseMappers.Contracts;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.DatabaseMappers.SqlServerMapper;
using FluentSql.SqlGenerators;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using FluentSql.DatabaseMappers.Common;
using FluentSql.Support.Extensions;
using FluentSql.SqlGenerators.SqlServer;
using System.Reflection;
using FluentSql.EntityReaders;

namespace FluentSql.Mappers
{
    public class EntityMapper
    {
        #region Public Properties
        /// <summary>
        /// Default Database mapper
        /// </summary>
        public IDatabaseMapper DefaultDatabaseMapper { get; private set; }

        public IEnumerable<string> DatabaseNames { get; private set; }

        public static ConcurrentDictionary<Type, EntityMap> EntityMap = new ConcurrentDictionary<Type, EntityMap>();

        public static IEnumerable<Table> DatabaseTables { get; set; }

        public static ISqlGenerator SqlGenerator { get; private set; }

        #endregion

        #region Private Properties
        private bool TableNamesInPlural { get; set; }
        private static IEnumerable<Type> EntityTypes { get; set; }
        #endregion

        #region Constructors
        public EntityMapper(IDbConnection dbConnection, Type entityInterface, IEnumerable<string> databaseNames, IDatabaseMapper defaultDatabaseMapper = null, bool tableNamesInPlural = true, Action onPostEntityMapping = null)
        {
            if (dbConnection == null || entityInterface == null || databaseNames == null)
                throw new ArgumentNullException("Database connection, Entity Interface or Database names can not be null.");

            DefaultDatabaseMapper = defaultDatabaseMapper ?? new SqlServerDatabaseMapper();
            DatabaseNames = databaseNames;

            if (dbConnection.State == ConnectionState.Closed)
                dbConnection.Open();

            TableNamesInPlural = tableNamesInPlural;

            SetDefaultSqlGenerator();
            MapEntities(dbConnection, entityInterface, databaseNames);
            
            onPostEntityMapping?.Invoke();
        }

        public EntityMapper(IDbConnection dbConnection, Type entityInterface, IEnumerable<string> databaseNames) :
            this(dbConnection, entityInterface, databaseNames, null)
        { }

        public EntityMapper(IDbConnection dbConnection, Type entityInterface, IEnumerable<string> databaseNames, Action onPostEntityMapping) :
            this(dbConnection, entityInterface, databaseNames, null, true, onPostEntityMapping)
        { }

        public EntityMapper(IDbConnection dbConnection, Type entityInterface, string databaseName) :
           this(dbConnection, entityInterface, new List<string> { databaseName }, null)
        { }

        public EntityMapper(IDbConnection dbConnection, Type entityInterface, string databaseName, Action onPostEntityMapping) :
           this(dbConnection, entityInterface, new List<string> { databaseName }, null, true, onPostEntityMapping)
        { }

        #endregion

        #region Public Methods
        public static void MapEntity(Type entityType, string tableName, string tableAlias)
        {
            if (entityType == null || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(tableAlias)) return;

            var table = DatabaseTables.FirstOrDefault(
               t => string.Compare(t.Name, tableName, StringComparison.CurrentCultureIgnoreCase) == 0);

            if (table == null) return;

            var map = new EntityMap(entityType);

            map.TableAlias = tableAlias;
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

            table.IsMapped = true;
            map.Properties.Sort();

            if (!EntityMap.TryAdd(entityType, map))
            {
                throw new ArgumentNullException("Can not add a key with null value.");
            }

        }
        #endregion

        #region Private Methods
        private void MapEntities(IDbConnection dbconnection, Type entityInterface, IEnumerable<string> databaseNames)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var sqlHelper = new SqlGeneratorHelper();
            var entityReader = new DefaultEntityReader();
            var service = PluralizationService.CreateService(CultureInfo.CurrentCulture);

            DatabaseTables = DefaultDatabaseMapper.MapDatabase(dbconnection, databaseNames);
            EntityTypes = entityReader.ReadEntities(entityInterface, assemblies);

            foreach (var type in EntityTypes)
            {
                var tableName = type.Name;
                var tableAlias = sqlHelper.GetTableAlias(type);

                if (TableNamesInPlural)
                    tableName = service.Pluralize(type.Name);

                MapEntity(type, tableName, tableAlias);
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
