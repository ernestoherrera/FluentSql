using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.Contracts
{
    public interface IPredicate<T> : IToSql , IDisposable
    {
        void Add(PredicateUnit predicate);

        int Count();

        bool Any();

        IEnumerator<PredicateUnit> GetEnumerator();

        void Clear();       
    }
}
