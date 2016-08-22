using FluentSql.Mappers;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class SelectQuery<T> : Query<T> , IDisposable
    {
        #region Properties
        protected readonly string SELECT = "SELECT";
        #endregion

        #region Constructors
        public SelectQuery() : base()
        {
            Verb = SELECT;
        }
        #endregion

        #region ToString methods
        public override string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var selectFields = new List<string>();
            var sqlJoinBuilder = new StringBuilder();

            selectFields.AddRange(Fields.Select(f => string.Format("{0}.{1}", TableAlias, f.ColumnName)));

            foreach (var join in Joins)
            {
                var rightTableAlias = join.RightQuery.TableAlias;

                foreach (var field in join.RightQuery.Fields)
                {
                    selectFields.Add(string.Format("{0}.{1}", rightTableAlias, field.ColumnName));
                }

                sqlJoinBuilder.Append(join.ToSql());
            }

            if (TopRows > 0)
                sqlBuilder.Append(string.Format("{0} {1} {2} {3} ", Verb, EntityMapper.SqlGenerator.Top, TopRows, string.Join(",", selectFields)));
            else
                sqlBuilder.Append(string.Format("{0} {1} ", Verb, string.Join(",", selectFields)));

            sqlBuilder.Append(string.Format("FROM {0} {1} ", TableName, TableAlias));
            sqlBuilder.Append(sqlJoinBuilder.ToString());

            if (PredicateParts != null && PredicateParts.Any())
                sqlBuilder.Append(string.Format("WHERE {0} ", PredicateParts.ToSql()));
            else if (Predicate != null)
                sqlBuilder.Append(string.Format("WHERE {0} ", Predicate.ToSql()));

            if (OrderByFields != null)
                sqlBuilder.Append(string.Format("ORDER BY {0}", string.Join(",", OrderByFields.Select(f => f.ToSql()))));

            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }

        #endregion
    }
}
