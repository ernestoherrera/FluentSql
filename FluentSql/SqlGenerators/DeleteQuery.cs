using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class DeleteQuery<T> : Query<T>
    {
        #region Protected Properties
        protected readonly string DELETE = "DELETE";
        #endregion

        public DeleteQuery() : base()
        {
            this.Verb = DELETE;
        }
    }
}
