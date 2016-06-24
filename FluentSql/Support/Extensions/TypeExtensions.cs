using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Support.Extensions
{
    internal static class TypeExtensions
    {
        internal static bool IsIEnumerable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        internal static Type GetIEnumerableImpl(this Type type)
        {

            if (IsIEnumerable(type))
                return type;

            Type[] t = type.FindInterfaces((m, o) => IsIEnumerable(m), null);

            if (t.Length > 0)
                return t[0];
            else
                return null;
        }

        public static bool IsAnonymous(this Type type)
        {
            if (type == null)
                return false;

            return type.Namespace == null;
        }
    }
}
