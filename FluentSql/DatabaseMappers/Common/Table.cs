using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.DatabaseMappers.Common
{
    /// <summary>
    /// Represent a Table in a Database
    /// </summary>
    public class Table
    {
        #region Public Properties
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<ForeignKey> ForeignKeys { get; set; }

        public string Database { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }
        public bool IsView { get; set; }
        public bool Ignore { get; set; }

        public List<Column> PrimaryKeys
        {
            get { return Columns.Where(c => c.IsPrimaryKey).ToList(); }
        }

        public Column GetColumn(string columnName)
        {
            return Columns.Single(c => string.Compare(c.ColumnName, columnName, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        public Column this[string columnName]
        {
            get
            {
                return GetColumn(columnName);
            }
        }
        #endregion
    }
}
