using System;
using System.Collections.Generic;

namespace FluentSql.Mappers
{
    public class EntityMap
    {
        public string Database { get; internal set; }
        public string SchemaName { get; internal set; }
        public string TableName { get; internal set; }
        public string TableAlias { get; internal set; }
        public List<PropertyMap> Properties { get; internal set; }
        public Type EntityType { get; private set; }
        public string Name { get; private set; }

        public EntityMap(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException("Entity type can not be null");

            Properties = new List<PropertyMap>();
            Name = entityType.Name;
            EntityType = entityType;
        }
    }
}