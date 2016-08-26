using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            {
                Properties = new List<PropertyMap>();
                Name = string.Empty;

                return;
            }

            Properties = entityType.GetProperties().Select(p => new PropertyMap(p)).ToList();
            Name = entityType.Name;
        }
    }
}
