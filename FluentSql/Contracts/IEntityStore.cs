using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Contracts
{
    public interface IEntityStore
    {
        ISqlGenerator SqlGenerator { get; }

        IEnumerable<T> Get<T>(Expression<Func<T, bool>> setFilterExpression);
    }
}
