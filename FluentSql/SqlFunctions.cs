using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql
{
    public class SqlFunctions
    {
        public static DateTime AddYears(DateTime fieldName, int numberOfYears)
        {
            if (fieldName == null )
                throw new ArgumentNullException("Arguements can not be null.");

            var dateFunction = "DATEPART({0}, {1})";
            //var tableAlias = EntityMapper.Entities[entityType].TableAlias;
            //var verifiedField = EntityMapper.Entities[entityType].Properties.FirstOrDefault(p => p.Name == fieldName);

            //if (verifiedField == null)
            //    throw new Exception(string.Format("Could not find field {0} in type {1}", fieldName, entityType));

            //var formattedField = string.Format("[{0}].[{1}]", tableAlias, fieldName);
            return DateTime.Now;
        }

        public static DateTime AddYears(DateTime? fieldName, int numberOfYears)
        {
            return AddYears(fieldName.Value, numberOfYears);
        }
    }
}
