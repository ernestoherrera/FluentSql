using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.EntityReaders.Contracts
{
    public interface IEntityReader
    {
        IEnumerable<Type> ReadEntities(Assembly[] assemblySearch);

        void AfterEntityRead(IEnumerable<Type> entityTypes);
    }
}
