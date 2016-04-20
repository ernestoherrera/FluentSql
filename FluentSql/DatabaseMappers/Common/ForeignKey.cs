using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.DatabaseMappers.Common
{
    /// <summary>
    /// Describes the relationship between tables
    /// </summary>
    public class ForeignKey
    {
        #region Public Properties
        /// <summary>
        /// Foreign key constraint name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Table Name that is referenced
        /// </summary>
        public string BaseTableName { get; private set; }

        /// <summary>
        /// Column name from the referenced table
        /// </summary>
        public Column ReferencingColumn { get; private set; }

        /// <summary>
        /// Calling table name
        /// </summary>
        public string ReferencedTableName { get; private set; }

        /// <summary>
        /// Column name in the referenced table
        /// </summary>
        public Column ReferencedColumn { get; private set; }

        #endregion
    }
}
