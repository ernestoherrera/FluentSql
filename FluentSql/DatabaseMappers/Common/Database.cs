using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.DatabaseMappers.Common
{
    public class Database
    {
        /// <summary>
        /// Database name as it appears in the server
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Are the tables names in plural
        /// </summary>
        public bool TableNamesInPlural { get; set; }

        /// <summary>
        /// The name of the Entities name space
        /// that will be associate with this database.        
        /// </summary>
        public string NameSpace { get; set; }
    }
}
