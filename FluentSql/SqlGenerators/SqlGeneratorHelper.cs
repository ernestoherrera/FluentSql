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
    }
}
