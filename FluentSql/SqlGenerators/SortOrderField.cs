using FluentSql.Mappers;
using System;
using System.Data.SqlClient;

namespace FluentSql.SqlGenerators
{
    public class SortOrderField
    {
        public SortOrder SortOrderDirection { get; internal set; }
        public string FieldName { get; internal set; }
        internal string TableAlias { get; set; }

        internal SortOrderField()
        { }

        public SortOrderField(Type entityType, string fieldName, SortOrder sortDirection = SortOrder.Ascending)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new Exception("Order by clause requires a field name. FieldName can not be null.");

            if (entityType == null)
                throw new Exception("Order by clause requires an entity type. Type can not be null.");

            TableAlias = EntityMapper.Entities[entityType].TableAlias;
            FieldName = fieldName;
            SortOrderDirection = sortDirection;
        }

        public string SortOrderSql()
        {
            return EntityMapper.SqlGenerator.GetSortOrderToken(SortOrderDirection);
        }

        public virtual string ToSql()
        {
            return string.Format("{0}.{1} {2}",
                                    TableAlias,
                                    FieldName,
                                    SortOrderSql());
        }

        public override string ToString()
        {
            return this.ToSql();
        }
    }
}