using FluentSql.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class InsertQuery<T> : Query<T>
    {
        protected readonly string INSERT = "INSERT";
        protected T Entity { get; set; }

        protected InsertQuery() : base()
        {
            Verb = INSERT;

            if (Fields == null) return;

            Fields = Fields.Where(p => p.IsTableField &&
                                        !p.IsAutoIncrement &&
                                        !p.Ignored &&
                                        !p.IsReadOnly)
                            .OrderBy(p => p.OrdinalPosition)
                            .ToList();
        }

        public InsertQuery(T entity) : this()
        {
            if (entity == null) return;

            Entity = entity;

            AddToQueryParameter();
        }

        public override string ToSql()
        {
            if (Fields == null || !Fields.Any()) return string.Empty;

            var sqlBuilder = new StringBuilder();

            sqlBuilder.AppendFormat("{0} INTO {1}.{2} ({3}) VALUES ({4}) ",
                                    Verb,
                                    SchemaName,
                                    TableName,
                                    string.Format("{0}", string.Join(",", Fields.Select(f => f.ColumnName))),
                                    string.Format("{0}", string.Join(",", Parameters.ParameterNames)));

            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }

        protected virtual void AddToQueryParameter()
        {
            foreach (var field in Fields)
            {
                var fieldValue = field.PropertyInfo.GetValue(Entity);
                var parameterName = EntityMapper.SqlGenerator.DriverParameterIndicator + field.ColumnName;

                Parameters.Add(parameterName, fieldValue);
            }
        }
    }
}
