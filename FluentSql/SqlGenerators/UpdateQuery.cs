using FluentSql.Support.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentSql.SqlGenerators
{
    public class UpdateQuery<T> : Query<T>
    {
        protected readonly string UPDATE = "UPDATE";

        protected SetClause<T> SetClause;        

        public T Entity { get; set; }

        protected UpdateQuery() : base()
        {
            Verb = UPDATE;
        }

        public UpdateQuery(T entity) : base()
        {
            Verb = UPDATE;
            Entity = entity;

            if (Fields == null) return;
            
            Fields = Fields.Where(p => p.IsTableField &&
                                    !p.Ignored &&
                                    !p.IsReadOnly).ToList();

            SetClause = new SetClause<T>(this);
            
        }

        public override string ToSql()
        {
            if (Fields == null || !Fields.Any()) return string.Empty;           

            var sqlBuilder = new StringBuilder();
            var predicateSql = string.Empty;

            if (PredicateParts != null)
                predicateSql = PredicateParts.ToSql();
            else
                predicateSql = Predicate == null ? "" : Predicate.ToSql();

            if (Predicate == null && PredicateParts == null)
            {
                sqlBuilder.AppendFormat("{0} {1}.{2} SET {3} ",
                                    Verb,
                                    SchemaName,
                                    TableName,
                                    SetClause.ToSql());
            }
            else
            {
                sqlBuilder.AppendFormat("{0} {1}.{2} SET {3} WHERE {4} ",
                                    Verb,
                                    SchemaName,
                                    TableName,
                                    SetClause.ToSql(),
                                    predicateSql);
            }
            
            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
