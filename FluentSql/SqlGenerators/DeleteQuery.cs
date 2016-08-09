using FluentSql.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class DeleteQuery<T> : Query<T>
    {
        #region Protected Properties
        protected readonly string DELETE = "DELETE";
        #endregion

        public DeleteQuery() : base()
        {
            this.Verb = DELETE;
        }

        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder(Verb);

            if (EntityMapper.SqlGenerator.IncludeDbNameInQuery)
                sqlBuilder.Append(string.Format("FROM {0}.{1}.{2} {3} ", DatabaseName, SchemaName, TableName, TableAlias));
            else
                sqlBuilder.Append(string.Format("FROM {0}.{1} {2} ", SchemaName, TableName, TableAlias));

            foreach (var join in Joins)
            {
                sqlBuilder.Append(join.ToSql());
            }

            if (PredicateParts != null && PredicateParts.Any())
                sqlBuilder.Append(string.Format("WHERE {0} ", PredicateParts.ToSql()));
            else if (Predicate != null)
                sqlBuilder.Append(string.Format("WHERE {0} ", Predicate.ToSql()));

            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
