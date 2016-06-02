using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Support.Extensions
{
    public static class CollectionExtensions
    {
        public static bool Contains(this string[] array, string value)
        {
            var isFound = false;
            foreach (var item in array)
            {
                if(string.Compare(item, value, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }

        public static bool Contains<T>(this T[] array, T value) where T : class
        {
            var isFound = false;
            foreach (var item in array)
            {
                if (item == value)
                {
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }
    }
}
