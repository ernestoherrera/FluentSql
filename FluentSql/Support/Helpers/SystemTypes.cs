using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Support.Helpers
{
    class SystemTypes
    {
        private static List<Type> numericTypes = new List<Type> { typeof(int), typeof(long), typeof(double), typeof(decimal), typeof(float) };
        private static List<Type> allTypes;

        public static List<Type> Numeric { get { return numericTypes; } }

        public static List<Type> All
        {
            get
            {
                if (allTypes == null)
                {
                    allTypes = new List<Type> { typeof(DateTime), typeof(string), typeof(Boolean), typeof(Char) };
                    allTypes.AddRange(numericTypes);                    
                }
                return allTypes;
            }
        }


    }
}
