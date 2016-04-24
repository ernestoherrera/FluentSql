using System;

namespace FluentSql.SqlGenerators
{
    public class PredicateUnit 
    {
        /// <summary>
        /// Left side of the predicate
        /// </summary>
        public dynamic LeftOperand { get; set; }

        /// <summary>
        /// Describes one of the supported Entity Types
        /// or system types
        /// </summary>
        public Type LeftOperandType { get; set; }
        
        /// <summary>
        /// It determines the relationship between
        /// the left and righ operands
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Right side of the predicate
        /// </summary>
        public dynamic RightOperand { get; set; }
        
        /// <summary>
        /// Describes one of the supported Entity types
        /// or system types
        /// </summary>
        public Type RightOperandType { get; set; }    

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
