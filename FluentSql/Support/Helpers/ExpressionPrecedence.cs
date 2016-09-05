using System;
using System.Linq.Expressions;

namespace FluentSql.Support.Helpers
{
    class ExpressionPrecedence
    {
        public static long GetOrdinal(ExpressionType expressionType)
        {
            long precedanceOrdinal = 1;

            switch (expressionType)
            {
                case ExpressionType.Negate:
                    precedanceOrdinal = precedanceOrdinal << 64;
                    break;
                case ExpressionType.NegateChecked:
                    precedanceOrdinal = precedanceOrdinal << 63;
                    break;
                case ExpressionType.Not:
                    precedanceOrdinal = precedanceOrdinal << 62;
                    break;
                case ExpressionType.Convert:
                    precedanceOrdinal = precedanceOrdinal << 61;
                    break;
                case ExpressionType.ConvertChecked:
                    precedanceOrdinal = precedanceOrdinal << 60;
                    break;
                case ExpressionType.ArrayLength:
                    precedanceOrdinal = precedanceOrdinal << 59;
                    break;
                case ExpressionType.Quote:
                    precedanceOrdinal = precedanceOrdinal << 58;
                    break;
                case ExpressionType.TypeAs:
                    precedanceOrdinal = precedanceOrdinal << 57;
                    break;
                case ExpressionType.Add:
                    precedanceOrdinal = precedanceOrdinal << 56;
                    break;
                case ExpressionType.AddChecked:
                    precedanceOrdinal = precedanceOrdinal << 55;
                    break;
                case ExpressionType.Subtract:
                    precedanceOrdinal = precedanceOrdinal << 54;
                    break;
                case ExpressionType.SubtractChecked:
                    precedanceOrdinal = precedanceOrdinal << 53;
                    break;
                case ExpressionType.Multiply:
                    precedanceOrdinal = precedanceOrdinal << 52;
                    break;
                case ExpressionType.MultiplyChecked:
                    precedanceOrdinal = precedanceOrdinal << 51;
                    break;
                case ExpressionType.Divide:
                    precedanceOrdinal = precedanceOrdinal << 50;
                    break;
                case ExpressionType.Modulo:
                    precedanceOrdinal = precedanceOrdinal << 49;
                    break;
                case ExpressionType.And:
                    precedanceOrdinal = precedanceOrdinal << 48;
                    break;
                case ExpressionType.AndAlso:
                    precedanceOrdinal = precedanceOrdinal << 47;
                    break;
                case ExpressionType.Or:
                    precedanceOrdinal = precedanceOrdinal << 46;
                    break;
                case ExpressionType.OrElse:
                    precedanceOrdinal = precedanceOrdinal << 45;
                    break;
                case ExpressionType.LessThan:
                    precedanceOrdinal = precedanceOrdinal << 44;
                    break;
                case ExpressionType.LessThanOrEqual:
                    precedanceOrdinal = precedanceOrdinal << 43;
                    break;
                case ExpressionType.GreaterThan:
                    precedanceOrdinal = precedanceOrdinal << 42;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    precedanceOrdinal = precedanceOrdinal << 41;
                    break;
                case ExpressionType.Equal:
                    precedanceOrdinal = precedanceOrdinal << 40;
                    break;
                case ExpressionType.NotEqual:
                    precedanceOrdinal = precedanceOrdinal << 39;
                    break;
                case ExpressionType.Coalesce:
                    precedanceOrdinal = precedanceOrdinal << 38;
                    break;
                case ExpressionType.ArrayIndex:
                    precedanceOrdinal = precedanceOrdinal << 37;
                    break;
                case ExpressionType.RightShift:
                    precedanceOrdinal = precedanceOrdinal << 36;
                    break;
                case ExpressionType.LeftShift:
                    precedanceOrdinal = precedanceOrdinal << 35;
                    break;
                case ExpressionType.ExclusiveOr:
                    precedanceOrdinal = precedanceOrdinal << 34;
                    break;
                case ExpressionType.TypeIs:
                    precedanceOrdinal = precedanceOrdinal << 33;
                    break;
                case ExpressionType.Conditional:
                    precedanceOrdinal = precedanceOrdinal << 32;
                    break;

                case ExpressionType.Constant:
                    precedanceOrdinal = precedanceOrdinal << 31;
                    break;

                case ExpressionType.Parameter:
                    precedanceOrdinal = precedanceOrdinal << 30;
                    break;

                case ExpressionType.MemberAccess:
                    precedanceOrdinal = precedanceOrdinal << 29;
                    break;

                case ExpressionType.Call:
                    precedanceOrdinal = precedanceOrdinal << 28;
                    break;

                case ExpressionType.Lambda:
                    precedanceOrdinal = precedanceOrdinal << 27;
                    break;

                case ExpressionType.New:
                    precedanceOrdinal = precedanceOrdinal << 26;
                    break;

                case ExpressionType.NewArrayInit:
                    precedanceOrdinal = precedanceOrdinal << 25;
                    break;
                case ExpressionType.NewArrayBounds:
                    precedanceOrdinal = precedanceOrdinal << 24;
                    break;

                case ExpressionType.Invoke:
                    precedanceOrdinal = precedanceOrdinal << 23;
                    break;

                case ExpressionType.MemberInit:
                    precedanceOrdinal = precedanceOrdinal << 22;
                    break;

                case ExpressionType.ListInit:
                    precedanceOrdinal = precedanceOrdinal << 21;
                    break;

                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", expressionType));
            }
            return precedanceOrdinal;
        }
    }
}
