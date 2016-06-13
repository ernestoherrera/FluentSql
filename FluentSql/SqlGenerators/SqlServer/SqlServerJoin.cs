using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators.SqlServer
{
    public class SqlServerJoin<L, R> : Join<L, R>
    {
        #region Constructor        
        public SqlServerJoin(IQuery<L> leftQuery, IQuery<R> rightQuery, JoinType joinType = JoinType.Inner)
            : base(leftQuery, rightQuery, joinType)
        { }
        #endregion

    }
}
