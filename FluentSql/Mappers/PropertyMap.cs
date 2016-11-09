using System;
using System.Reflection;

using FluentSql.Mappers.Contracts;

namespace FluentSql.Mappers
{
    public class PropertyMap : IPropertyMap, IComparable
    {       
        public string Name { get; internal set; }
       
        public string ColumnName { get; internal set; }

        public string ColumnDataType { get; internal set; }

        public bool IsPrimaryKey { get; internal set; }

        public bool IsAutoIncrement { get; internal set; }
       
        public bool HasDefault { get; internal set; }

        public PropertyInfo PropertyInfo { get; internal set; }
       
        public int? Size { get; internal set; }

        public int? NumericPrecision { get; internal set; }

        public int? NumericScale { get; internal set; }

        public bool Ignored { get; internal set; }

        public bool IsComputed { get; internal set; }

        public bool IsReadOnly { get; internal set; }

        public bool IsNullable { get; internal set; }

        public bool IsTableField { get; set; }

        public string ColumnAlias { get; internal set; }

        public int OrdinalPosition { get; internal set; }

        public string TableName { get; internal set; }

        public PropertyMap(PropertyInfo prop)
        {
            PropertyInfo = prop;
            Name = prop.Name;
        }

        public int CompareTo(object obj)
        {
            var propertyMap = obj as PropertyMap;

            if (propertyMap == null) return 1;

            return OrdinalPosition.CompareTo(propertyMap.OrdinalPosition);
        }
    }
}
