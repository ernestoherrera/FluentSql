using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.DatabaseMappers.Common
{
    /// <summary>
    /// Represents a Colum in a database
    /// </summary>
    public class Column : IComparable
    {
        #region Public Properties 
        /// <summary>
        /// Database Column Name
        /// </summary>
        public string ColumnName;
        /// <summary>
        /// Database type
        /// </summary>
        public string DataType;
        /// <summary>
        /// C# database type
        /// </summary>
        public DbType DatabaseType;
        /// <summary>
        /// Returns true if Column is a Primary Key
        /// </summary>
        public bool IsPrimaryKey;
        /// <summary>
        /// Returns true if Column is Nullable
        /// </summary>
        public bool IsNullable;
        /// <summary>
        /// Return true if Column is AutoIncrement or Identity
        /// </summary>
        public bool IsAutoIncrement;
        /// <summary>
        /// Return true if Column has a default value on insert
        /// </summary>
        public bool HasDefault;
        /// <summary>
        /// Return Database Column size
        /// </summary>
        public int Size;
        /// <summary>
        /// User specific field used to ignore the column
        /// </summary>
        public bool Ignore;
        /// <summary>
        /// Return true if Column is the column is computed by the DB engine
        /// </summary>
        public bool IsComputed;
        /// <summary>
        /// Return true if Column is readonly
        /// </summary>
        public bool IsReadOnly;
        /// <summary>
        /// Column Ordinal Position within the database table
        /// </summary>
        public int OrdinalPosition;
        /// <summary>
        /// Returns the table name where the column is part of
        /// </summary>
        public string TableName;
        #endregion

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var columnIn = obj as Column;

            if (columnIn != null)
                return this.OrdinalPosition.CompareTo(columnIn.OrdinalPosition);
            else
                throw new ArgumentException("Object is not a Column");
            
        }
        #endregion
    }
}
