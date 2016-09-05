using System;

namespace FluentSql.SqlGenerators.Contracts
{
    public interface IDeleteQuery<T> : IQuery<T>
    {
        T Entity { get; }
    }
}
