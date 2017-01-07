using System;
using System.Collections.Generic;

namespace FluentSql.Support.Helpers
{
    class SystemTypes
    {
        private static List<Type> _numericTypes = new List<Type>
        {
            typeof(int),
            typeof(uint),
            typeof(short),
            typeof(ushort),
            typeof(ulong),
            typeof(long),
            typeof(double),
            typeof(decimal),
            typeof(float),
            typeof(object)
        };

        private static List<Type> _allTypes;

        public static List<Type> Numeric { get { return _numericTypes; } }

        public static List<Type> All
        {
            get
            {
                if (_allTypes == null)
                {
                    _allTypes = new List<Type> { typeof(DateTime), typeof(string), typeof(bool), typeof(char) };
                    _allTypes.AddRange(_numericTypes);
                }
                return _allTypes;
            }
        }
    }
}
