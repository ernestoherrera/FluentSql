using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class SqlGeneratorHelper
    {
        private Random RandomGen = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Minute);
        private Dictionary<Type, string> Aliases = new Dictionary<Type, string>();
        private List<string> ParameterNames = new List<string>();
        private readonly string AT_SIGN = "@";

        public string GetTableAlias(Type type)
        {
            if (Aliases.ContainsKey(type))
                return Aliases[type];

            var result = "";

            do
            {
                var alias = new char[3];

                for (var i = 0; i < 3; i++)
                {
                    var ascii = RandomGen.Next(0, 26);
                    var letter = (char)('a' + ascii);
                    alias[i] = letter;
                }

                result = new string(alias);

            } while (Aliases.Values.FirstOrDefault(a => a == result) != null);

            Aliases.Add(type, result);

            return result;
        }

        public string GetNextParameterName(string paramBaseName)
        {
            var nextParamName = paramBaseName ?? "_";

            do
            {
                var nextNumber = RandomGen.Next(255, 1024);
                nextParamName = nextParamName + nextNumber;
            }
            while (ParameterNames.FirstOrDefault(p =>
                                string.Compare(p, nextParamName, StringComparison.CurrentCultureIgnoreCase) == 0) != null);

            ParameterNames.Add(nextParamName);

            return AT_SIGN + nextParamName;
        }

    }
}
