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
using System.Diagnostics;

namespace FluentSql.Mappers
{
    public class EntityMapper
    {
        #region Public Properties

        public static IEnumerable<Table> Tables { get; internal set; }

        /// <summary>
        /// Default Database mapper
        /// </summary>
        public IDatabaseMapper DefaultDatabaseMapper { get; private set; }

        internal IEnumerable<Database> TargetDatabases { get; private set; }

        public static ConcurrentDictionary<Type, EntityMap> Entities = new ConcurrentDictionary<Type, EntityMap>();
        
        public static ISqlGenerator SqlGenerator { get; private set; }
        #endregion

        #region Constructors
        public EntityMapper(IDbConnection dbConnection, IEnumerable<Database> targetDatabases, Assembly[] assembliesOfModelTypes = null, 
                            IDatabaseMapper defaultDatabaseMapper = null, Action onPostEntityMapping = null, ISqlGenerator sqlGenerator = null)
        {
            if (dbConnection == null || targetDatabases == null)
                throw new ArgumentNullException("Database connection, or Database names can not be null.");

            if (sqlGenerator != null)
                SqlGenerator = sqlGenerator;
            else
                SetDefaultSqlGenerator();

            DefaultDatabaseMapper = defaultDatabaseMapper ?? new SqlServerDatabaseMapper();
            TargetDatabases = targetDatabases;

            if (dbConnection.State == ConnectionState.Closed)
                dbConnection.Open();

            MapTablesToEntities(dbConnection, assembliesOfModelTypes);
            
            onPostEntityMapping?.Invoke();
        }

        public EntityMapper(IDbConnection dbConnection, IEnumerable<Database> allDatabases, Action onPostEntityMapping) :
            this(dbConnection, allDatabases, null, null, onPostEntityMapping)
        { }

        public EntityMapper(IDbConnection dbConnection, bool tableNamesInPlural = true) :
           this(dbConnection, new List<Database> { new Database { Name = dbConnection.Database, TableNamesInPlural = tableNamesInPlural } }, null, null, null)
        { }

        public EntityMapper(IDbConnection dbConnection, bool tableNamesInPlural = true, Assembly[] assembliesOfModelTypes = null, IDatabaseMapper databaseMapper = null) :
           this(dbConnection, new List<Database> { new Database { Name = dbConnection.Database, TableNamesInPlural = tableNamesInPlural } }, assembliesOfModelTypes, databaseMapper)
        { }

        public EntityMapper(IDbConnection dbConnection, bool tableNamesInPlural = true, Assembly[] assembliesOfModelTypes = null, IDatabaseMapper databaseMapper = null, Action onPostEntityMapping = null) :
          this(dbConnection, new List<Database> { new Database { Name = dbConnection.Database, TableNamesInPlural = tableNamesInPlural } }, assembliesOfModelTypes, databaseMapper, onPostEntityMapping)
        { }

        public EntityMapper(IDbConnection dbConnection, Assembly[] assembliesOfModelTypes = null, bool tableNamesInPlural = true) :
           this(dbConnection, new List<Database> { new Database { Name = dbConnection.Database, TableNamesInPlural = tableNamesInPlural } },
               assembliesOfModelTypes)
        { }

        public EntityMapper(IDbConnection dbConnection, ISqlGenerator sqlGenerator , Assembly[] assembliesOfModelTypes = null, IDatabaseMapper databaseMapper = null, bool tableNamesInPlural = true) :
           this(dbConnection, new List<Database> { new Database { Name = dbConnection.Database, TableNamesInPlural = tableNamesInPlural } }, assembliesOfModelTypes, databaseMapper)
        { }

        #endregion

        #region Public Methods
        public static void MapEntity(Type entityType, string tableName, string tableAlias)
        {
            if (entityType == null || string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(tableAlias)) return;

            var table = Tables.FirstOrDefault(
               t => string.Compare(t.Name, tableName, StringComparison.CurrentCultureIgnoreCase) == 0);

            if (table == null)
                throw new Exception(string.Format("Table {0} not found.", tableName));

            if (!table.Columns.Any())
                throw new Exception(string.Format("Columns not found for table {0}.", tableName));

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
                prop.ColumnDataType = col.DataType;
                prop.IsComputed = col.IsComputed;
            }

            table.IsMapped = true;
            map.Properties.Sort();

            if (!Entities.TryAdd(entityType, map))
            {
                throw new ArgumentNullException("Can not add a key with null value.");
            }

        }
        #endregion

        #region Private Methods
        private void MapTablesToEntities(IDbConnection dbconnection,  Assembly[] assembliesOfModelTypes = null)
        {
            Assembly[] assemblies;
            var sqlHelper = new SqlGeneratorHelper();
            var entityReader = new DefaultEntityReader();
            var service = PluralizationService.CreateService(CultureInfo.CurrentCulture);

           Tables = DefaultDatabaseMapper.MapDatabase(dbconnection, TargetDatabases);

            if (assembliesOfModelTypes != null)
                assemblies = assembliesOfModelTypes;
            else
                assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var entityTypes = entityReader.ReadEntities(assemblies).ToList();
            var typeDefault = default(Type);

            foreach (var dbObject in TargetDatabases)
            {
                var dbTables = Tables.Where(t => string.Equals(t.Database , dbObject.Name,
                                                StringComparison.InvariantCultureIgnoreCase))
                                     .ToArray();

                foreach (var tbl in dbTables)
                {
                    Type selectedType = null;
                    var typeName = "";

                    if (dbObject.TableNamesInPlural)
                        typeName = service.Singularize(tbl.Name);
                    else
                        typeName = tbl.Name;

                    if (!string.IsNullOrEmpty(dbObject.NameSpace))
                    {
                        selectedType = entityTypes.FirstOrDefault(t => string.Equals(t.Name, typeName,
                                                                   StringComparison.CurrentCultureIgnoreCase) &&
                                                                   string.Equals(t.Namespace, dbObject.NameSpace,
                                                                   StringComparison.CurrentCultureIgnoreCase));
                    }
                    else
                    {
                        selectedType = entityTypes.FirstOrDefault(t => string.Equals(t.Name, typeName,
                                                                   StringComparison.CurrentCultureIgnoreCase));
                    }

                    if (selectedType == typeDefault)
                    {
#if DEBUG
                        Debug.WriteLine(string.Format("Table {0}.{1}.{2} was not mapped", tbl.Database, tbl.Schema, tbl.Name));
#endif
                        continue;
                    }

                    var tableAlias = sqlHelper.GetTableAlias(selectedType);

                    MapEntity(selectedType, tbl.Name, tableAlias);
                    entityTypes.RemoveAt(entityTypes.IndexOf(selectedType));
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
