using FluentSql.Mappers;
using System.Linq.Expressions;

namespace FluentSql.SqlGenerators
{
    internal class PredicateUnit
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
            var linkingSql = "";

            if (LinkingOperator.HasValue)
                linkingSql = EntityMapper.SqlGenerator.GetOperator(LinkingOperator.Value);

            return string.Format("{3} ({0} {1} {2}) ", LeftOperand, EntityMapper.SqlGenerator.GetOperator(Operator),
                                                        RightOperand, linkingSql);
        }

        public override string ToString()
        {
            return ToSql();
        }
    }
}
