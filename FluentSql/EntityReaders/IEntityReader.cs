using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentSql.EntityReaders.Contracts
{
    public interface IEntityReader
    {
        IEnumerable<Type> ReadEntities(Assembly[] assemblySearch);

        void AfterEntityRead(IEnumerable<Type> entityTypes);
    }
}
