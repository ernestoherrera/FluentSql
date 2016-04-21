using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class PredicateUnit
    {
        /// <summary>
        /// Left side of the predicate
        /// </summary>
        public dynamic LeftOperand { get; set; }

        /// <summary>
        /// It determines the relationship between
        /// the left and righ operands
        /// </summary>
        public string Operator { get; set; }

        public dynamic RightOperand { get; set; }        

        /// <summary>
        /// The link to the next predicate unit
        /// usually "AND" or "OR"
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Indicates whether there is a parameter
        /// on either size of the predicate
        /// </summary>
        public bool IsParameterized { get; set; }
    }
}
