using FluentSql.Mappers;
using FluentSql.Support.Helpers;
using System;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace FluentSql.SqlGenerators
{
    public class SortOrderField
    {
        public SortOrder SortOrderDirection { get; set; }
        public string FieldName { get; set; }
        internal string TableAlias { get; set; }

        public SortOrderField()
        { }

        public SortOrderField(Type entityType)
        {
            TableAlias = EntityMapper.EntityMap[entityType].TableAlias;
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