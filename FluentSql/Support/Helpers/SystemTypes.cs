using System;
using System.Collections.Generic;

namespace FluentSql.Support.Helpers
{
    class SystemTypes
    {
        private static List<Type> _numericTypes = new List<Type> { typeof(int), typeof(long), typeof(double), typeof(decimal), typeof(float) };
        private static List<Type> _allTypes;

        public static List<Type> Numeric { get { return _numericTypes; } }

        public static List<Type> All
        {
            get
            {
                if (_allTypes == null)
                {
                    _allTypes = new List<Type> { typeof(DateTime), typeof(string), typeof(Boolean), typeof(Char) };
                    _allTypes.AddRange(_numericTypes);                    
                }
                return _allTypes;
            }
        }


    }
}
