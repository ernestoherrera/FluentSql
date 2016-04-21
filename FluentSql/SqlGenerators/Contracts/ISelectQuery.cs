using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.Contracts
{
    public interface ISelectQuery<T> : IQuery<T>
    {
        ISelectQuery<T> OrderBy(Expression<Func<T, object>> expression);

        ISelectQuery<T> OrderByDescending(Expression<Func<T, object>> expression);

        ISelectQuery<T> OrderBy(List<SortOrderField<T>> sortOrderFields);

        ISelectQuery<T> OrderBy(params SortOrderField<T>[] sortOrderArray);

    }
}
