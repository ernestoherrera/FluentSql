using FluentSql.Mappers;
using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerInsertQuery<T> : InsertQuery<T>
    {
        private List<string> _unsupportedTypes = new List<string> { "text", "image" };
        private readonly string _tempTableName = "@targetInserted";

        public SqlServerInsertQuery(T entity) : base(entity)
        {

        }
        public override string ToSql()
        {
            if (Fields == null || !Fields.Any()) return string.Empty;

            var sqlBuilder = new StringBuilder();
            var insertableFields = GetInsertableFields();

            // This should never happen
            if (insertableFields == null) return string.Empty;

            insertableFields = insertableFields.Where(fld => !_unsupportedTypes.Contains(fld.ColumnDataType));

            var parameterSign = EntityMapper.SqlGenerator.DriverParameterIndicator;
            var insertableFieldNames = insertableFields.Select(fld => fld.ColumnName);
            var valuesParameters = string.Format("{0}", string.Join(",", insertableFields.Select(i => parameterSign + i.ColumnName)));
            var formattedFields = SqlServerHelper.BraketFieldNames(insertableFieldNames);
            var autoIncrementField = EntityMapper.EntityMap[typeof(T)].Properties.Where(p => p.IsAutoIncrement).FirstOrDefault();

            if (autoIncrementField != null)
            {
                sqlBuilder.Append(GenerateTempTableSql());

                sqlBuilder.AppendFormat("INSERT INTO {0}[{1}].[{2}] ({3}) {4} ",
                                        EntityMapper.SqlGenerator.IncludeDbNameInQuery ? string.Format("[{0}].", DatabaseName) : "",
                                        SchemaName,
                                        TableName,
                                        string.Format("{0}", string.Join(",", formattedFields)),
                                        string.Format("OUTPUT {0} INTO {1}", GetAllFields(useInsertedPrefix: true), _tempTableName));

                var valuesClause = "VALUES ({0}); {1};";

                var returnEntity = string.Format("SELECT TOP 1 {0} FROM {1}", GetAllFields(), _tempTableName);

                sqlBuilder.AppendFormat(valuesClause, valuesParameters, returnEntity);

            }
            else
            {
                sqlBuilder.AppendFormat("INSERT INTO {0}[{1}].[{2}] ({3}) VALUES ({4});",
                                        EntityMapper.SqlGenerator.IncludeDbNameInQuery ? string.Format("[{0}].", DatabaseName) : "",
                                        SchemaName,
                                        TableName,
                                        string.Format("{0}", string.Join(",", formattedFields)),
                                        string.Format("{0}", string.Join(",", valuesParameters)));

                var query = new SqlServerSelectQuery<T>();
                query = EntityStore.GetQueryByKey<T>(this.Entity, query) as SqlServerSelectQuery<T>;
                query.GetTopRows(1);

                Parameters.AddDynamicParams(query.Parameters);
                sqlBuilder.Append(query.ToSql());
            }

            return sqlBuilder.ToString();
        }

        private string GenerateTempTableSql()
        {
            var sqlBuilder = new StringBuilder();
            var fieldsToInsert = EntityMapper.EntityMap[typeof(T)].Properties
                                        .Where(p => !_unsupportedTypes.Contains(p.ColumnDataType))
                                        .ToList();
            var fieldCount = fieldsToInsert.Count();

            sqlBuilder.AppendFormat("DECLARE {0} TABLE (", _tempTableName);

            for (var i = 0; i < fieldCount; i++)
            {
                sqlBuilder.AppendFormat("[{0}] {1} ", fieldsToInsert[i].ColumnName, GetDataTypeSql(fieldsToInsert[i]));

                if (i < (fieldCount - 1))
                    sqlBuilder.Append(",");
            }

            sqlBuilder.Append("); ");
            return sqlBuilder.ToString();
        }

        private string GetDataTypeSql(PropertyMap field)
        {
            switch (field.ColumnDataType.ToLower())
            {
                case "varchar":
                case "nvarchar":
                case "char":
                case "nchar":

                    if (!field.Size.HasValue)
                        throw new Exception("varchar, nvarchar, char, nchar data type cannot have null size.");

                    return string.Format("{0} ({1})", field.ColumnDataType ,
                                    ((field.Size.Value) == -1 ? "max" : field.Size.Value.ToString()));

                case "decimal":

                    if (!field.NumericPrecision.HasValue)
                        throw new Exception("Decimal data type cannot have null presicion.");

                    return string.Format("{0} ({1},{2})", field.ColumnDataType, field.NumericPrecision.Value ,
                                                            field.NumericScale ?? 0);

                case "float":
                case "numeric":

                    if (!field.NumericPrecision.HasValue)
                        throw new Exception("float data type cannot have null presicion.");

                    return string.Format("{0} ({1})", field.ColumnDataType, field.NumericPrecision.Value);

                default:
                    return string.Format("{0}", field.ColumnDataType);
            }
        }

        private string GetAllFields(bool useInsertedPrefix = false)
        {

            var allColumns = EntityMapper.EntityMap[typeof(T)].Properties
                                        .Where(p => ! _unsupportedTypes.Contains(p.ColumnDataType))
                                        .Select(p => string.Format("{0}[{1}]",
                                         (useInsertedPrefix ? "inserted." : "") , p.ColumnName));

            return string.Join(",", allColumns);
        }
    }
}