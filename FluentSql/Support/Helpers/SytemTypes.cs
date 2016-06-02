using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Support.Helpers
{
    class SytemTypes
    {
        private static List<Type> numericTypes = new List<Type> { typeof(int), typeof(long), typeof(double), typeof(decimal), typeof(float) };

        public static List<Type> Numeric { get { return numericTypes; } }

    }
}
