using FluentSql.SqlGenerators.Contracts;
using FluentSql.SqlGenerators.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
