using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.Contracts
{
    public interface IInsertQuery<T> : IQuery<T>
    {
        T Entity { get; }
    }
}
