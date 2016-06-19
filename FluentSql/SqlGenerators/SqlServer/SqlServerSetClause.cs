using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerSetClause<T> : SetClause<T>
    {
        public SqlServerSetClause(SqlServerUpdateQuery<T> parentQuery) : base(parentQuery)
        {  }

        public override string ToSql()
        {
            var setClause = new List<string>();
            var paramGen = ParentQuery.ParameterNameGenerator;

            foreach (var field in FieldsToUpate)
            {
                setClause.Add(string.Format(" [{0}] = {1} ", field.ColumnName, driverIndicator + paramGen.GetNextParameterName(field.ColumnName)));
            }

            return string.Join(",", setClause);
        }
    }
}
