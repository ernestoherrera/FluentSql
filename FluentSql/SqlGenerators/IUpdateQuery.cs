namespace FluentSql.SqlGenerators.Contracts
{
    public interface IUpdateQuery<T> : IQuery<T>
    {
        T Entity { get; }
    }
}
