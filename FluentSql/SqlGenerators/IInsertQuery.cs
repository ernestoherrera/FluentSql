using System;

namespace FluentSql.SqlGenerators.Contracts
{
    public interface IInsertQuery<T> : IQuery<T>
    {
        T Entity { get; }
    }
}
