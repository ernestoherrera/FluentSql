using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerSortOrderField<T> : SortOrderField<T>
    {
        public override string ToSql()
        {
            return string.Format("{0}.[{1}] {2}", TableAlias, FieldName, SortOrderSql());
        }
    }
}
