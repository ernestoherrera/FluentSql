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

        public UpdateQuery(T entity) : base()
        {
            Verb = UPDATE;
            Entity = entity;

            if (Fields == null) return;
            
            Fields = Fields.Where(p => p.IsTableField &&
                                    !p.Ignored &&
                                    !p.IsReadOnly).ToList();
            
        }

        public override string ToSql()
        {
            if (Fields == null || !Fields.Any()) return string.Empty;

            SetClause = new SetClause<T>(this);

            var sqlBuilder = new StringBuilder();

            if (Predicate == null)
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
                                    Predicate.ToSql());
            }
            
            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
