using FluentSql.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerInsertQuery<T> : InsertQuery<T>
    {

        public SqlServerInsertQuery(T entity) : base(entity)
        {

        }
        public override string ToSql()
        {
            if (Fields == null || !Fields.Any()) return string.Empty;

            var sqlBuilder = new StringBuilder();
            
            var formattedFields = SqlServerHelper.BraketFieldNames(Fields.Select(f => f.ColumnName));
            var autoIncrementField = EntityMapper.EntityMap[typeof(T)].Properties.Where(p => p.IsAutoIncrement).FirstOrDefault();
            var parameterSign = EntityMapper.SqlGenerator.DriverParameterIndicator;

            if (autoIncrementField != null)
            {
                sqlBuilder.Append("DECLARE @insertedIds TABLE (Id INT);");
                sqlBuilder.AppendFormat("INSERT INTO {0}[{1}].[{2}] ({3}) {4} VALUES ({5}); {6};",
                                        EntityMapper.SqlGenerator.IncludeDbNameInQuery ? string.Format("[{0}].",DatabaseName): "",
                                        SchemaName,
                                        TableName,
                                        string.Format("{0}", string.Join(",", formattedFields)),
                                        string.Format("OUTPUT inserted.{0} INTO @insertedIds", autoIncrementField.Name),
                                        string.Format("{0}", string.Join(",", Fields.Select(i => parameterSign + i.ColumnName))),
                                        string.Format("SELECT TOP 1 {[0]} FROM @insertedIds", autoIncrementField.Name));
            }
            else
            {
                sqlBuilder.AppendFormat("INSERT INTO {0}[{1}].[{2}] ({3}) VALUES ({4});",
                                        EntityMapper.SqlGenerator.IncludeDbNameInQuery ? string.Format("[{0}].", DatabaseName) : "",
                                        SchemaName,
                                        TableName,
                                        string.Format("{0}", string.Join(",", formattedFields)),
                                        string.Format("{0}", string.Join(",", Fields.Select(i => parameterSign + i.ColumnName))));
            }

            return sqlBuilder.ToString();
        }

    }
}
