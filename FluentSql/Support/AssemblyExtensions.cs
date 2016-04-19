using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentSql.Support
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            var loadableTypes = new List<Type>();

            if (assembly == null) return loadableTypes;

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }

        public static List<Type> GetTypes(this Assembly assembly, Type type)
        {

            var loadableTypes = new List<Type>();

            if (type == null) return loadableTypes;

            foreach (var libType in assembly.GetLoadableTypes())
            {
                if (libType.GetInterfaces().Contains(type))
                {
                    loadableTypes.Add(libType);
                }
            }

            return loadableTypes;
        }
    }
}
