using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class PredicateUnit
    {
        /// <summary>
        /// Predicate's left side name
        /// </summary>
        public string LeftOperand { get; set; }        

        /// <summary>
        /// Predicate operator: equality, greater than, less than...
        /// </summary>
        public ExpressionType Operator { get; set; }

        /// <summary>
        /// Predicate's right side operand name
        /// </summary>
        public dynamic RightOperand { get; set; }
        
        /// <summary>
        /// Returns true if the right parameter needs to be parametized
        /// </summary>
        public bool IsRightOperandParameter { get; set; }
        /// <summary>
        /// Linking expression to other Predicate Unit objects.
        /// This would usually mean Or, And
        /// </summary>
        public ExpressionType? LinkingOperator { get; set; }

        public PredicateUnit()
        {
            IsRightOperandParameter = false;
        }

        public virtual string ToSql()
        {

        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
