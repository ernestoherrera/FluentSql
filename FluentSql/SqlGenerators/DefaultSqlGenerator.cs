using FluentSql.SqlGenerators.Contracts;
using FluentSql.SqlGenerators.SqlServer;

namespace FluentSql.SqlGenerators
{
    public class DefaultSqlGenerator
    {
        public ISqlGenerator Generator { get; private set; }

        public DefaultSqlGenerator()
        {
        }

        public virtual void SetSqlGenerator()
        {
            Generator = new SqlServerSqlGenerator();
        }
    }
}
