using FluentSql.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentSql.SqlGenerators
{
    public class SqlGeneratorHelper
    {
        private Random _randomGen = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Minute);
        private Dictionary<Type, string> _aliases = new Dictionary<Type, string>();
        private List<string> _parameterNames = new List<string>();
        private string AT_SIGN = string.Empty;

        public SqlGeneratorHelper()
        {
            AT_SIGN = EntityMapper.SqlGenerator.DriverParameterIndicator;
        }

        public string GetTableAlias(Type type)
        {
            if (_aliases.ContainsKey(type))
                return _aliases[type];

            var result = "";

            do
            {
                var alias = new char[3];

                for (var i = 0; i < 3; i++)
                {
                    var ascii = _randomGen.Next(0, 26);
                    var letter = (char)('a' + ascii);
                    alias[i] = letter;
                }

                result = new string(alias);

            } while (_aliases.Values.FirstOrDefault(a => a == result) != null);

            _aliases.Add(type, result);

            return result;
        }

        public string GetNextParameterName(string paramBaseName)
        {
            var nextParamName = paramBaseName ?? "_";

            do
            {
                var nextNumber = _randomGen.Next(255, 1024);
                nextParamName = nextParamName + nextNumber;
            }
            while (_parameterNames.FirstOrDefault(p =>
                                string.Compare(p, nextParamName, StringComparison.CurrentCultureIgnoreCase) == 0) != null);

            _parameterNames.Add(nextParamName);

            return AT_SIGN + nextParamName;
        }

    }
}
