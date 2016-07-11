using FluentSql.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class PredicateUnits
    {
        private IList<PredicateUnit> predicateUnits;        
            
        #region Constructor
        public PredicateUnits()
        {
            predicateUnits = new List<PredicateUnit>();
        }
        #endregion

        #region Public Methods
        public void Add(string leftOperand, ExpressionType optr, string rightOperand, bool isParametized = false, ExpressionType? linkingOperator = null )
        {
            var unit = new PredicateUnit
            {
                LeftOperand = leftOperand,
                Operator = optr,
                RightOperand = rightOperand,
                IsRightOperandParameter = isParametized,
                LinkingOperator = linkingOperator
            };

            predicateUnits.Add(unit);
        }

        public bool Any()
        {
            return predicateUnits.Any();
        }

        public string ToSql()
        {
            var sqlBuilder = new StringBuilder();

            foreach (var unit in predicateUnits)
            {
                sqlBuilder.Append(unit.ToSql());
            }

            return sqlBuilder.ToString();
        }

        public override string ToString()
        {
            return ToSql();
        }
        #endregion
    }
}
