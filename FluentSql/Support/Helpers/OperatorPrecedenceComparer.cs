using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Support.Helpers
{
    internal class OperatorPrecedenceComparer : Comparer<ExpressionType>
    {
        public override int Compare(ExpressionType x, ExpressionType y)
        {
            var precendanceX = ExpressionPrecedence.GetOrdinal(x);
            var precendanceY = ExpressionPrecedence.GetOrdinal(y);
                        
            return precendanceX.CompareTo(precendanceY);
        }        
    }
}
