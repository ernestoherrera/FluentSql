using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerHelper
    {
        public static IEnumerable<string> BraketFieldNames(IEnumerable<string> fieldNames, string tableAlias)
        {
            var resultingFields = new List<string>();

            if (fieldNames == null) return resultingFields;

            foreach (var field in fieldNames)
            {
                resultingFields.Add(string.Format("{0}.[{1}]", tableAlias, field));
            }

            return resultingFields;
        }

        public static IEnumerable<string> BraketFieldNames(IEnumerable<string> fieldNames)
        {
            return BraketFieldNamesCustom(fieldNames, "[{0}]");
        }

        public static IEnumerable<string> BraketFieldNamesCustom(IEnumerable<string> fieldNames, string format)
        {
            var resultingFields = new List<string>();

            if (fieldNames == null) return resultingFields;

            foreach (var field in fieldNames)
            {
                resultingFields.Add(string.Format(format, field));
            }

            return resultingFields;
        }
    }
}
