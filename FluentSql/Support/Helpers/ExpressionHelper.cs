using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using FluentSql.Mappers;
using FluentSql.SqlGenerators;


namespace FluentSql.Support.Helpers
{
    internal class ExpressionHelper
    {
        private static readonly char[] period = new char[] { '.' };

        /// <summary>
        /// Walks the expression tree in order to get the SQL precdicate string
        /// </summary>
        /// <param name="body"></param>
        /// <param name="linkingType"></param>
        /// <param name="queryPredicates"></param>
        internal static void WalkTree(BinaryExpression body, ExpressionType linkingType, ref List<PredicateUnit> queryPredicates)
        {
            //Josh: Avoid having to pass queryPredicates
            if (body.NodeType != ExpressionType.AndAlso && body.NodeType != ExpressionType.OrElse)
            {
                PredicateUnit predicate;

                if (body.Right.NodeType == ExpressionType.Constant)
                {
                    predicate = GetPredicateWithConstant((ConstantExpression)body.Right, body, linkingType);
                    queryPredicates.Add(predicate);
                    return;
                }

                if (body.Left.NodeType == ExpressionType.Constant)
                {
                    predicate = GetPredicateWithConstant((ConstantExpression)body.Left, body, linkingType, ExpressionDirection.Left);
                    queryPredicates.Add(predicate);
                    return;
                }

                MemberExpression memberRight = ((MemberExpression)(body.Right));
                MemberExpression memberLeft = ((MemberExpression)(body.Left));

                if (memberRight.Expression.NodeType == ExpressionType.Constant)
                {
                    predicate = GetPredicateWithMemberTypeField(body, linkingType);
                    queryPredicates.Add(predicate);
                    return;
                }

                if (memberLeft.Expression.NodeType == ExpressionType.Constant)
                {
                    predicate = GetPredicateWithMemberTypeField(body, linkingType, ExpressionDirection.Left);
                    queryPredicates.Add(predicate);
                    return;
                }

                if (memberRight.Expression.NodeType == ExpressionType.MemberAccess
                    && ((MemberExpression)memberRight.Expression).Member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    predicate = GetPredicateWithMemberTypeField(body, linkingType);
                    queryPredicates.Add(predicate);
                    return;
                }

                if (memberLeft.Expression.NodeType == ExpressionType.MemberAccess
                    && ((MemberExpression)memberLeft.Expression).Member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    predicate = GetPredicateWithMemberTypeField(body, linkingType, ExpressionDirection.Left);
                    queryPredicates.Add(predicate);
                    return;
                }
                else
                {
                    predicate = new PredicateUnit();

                    predicate.LeftOperand = GetPropertyName(memberLeft.ToString());
                    predicate.LeftOperandType = memberLeft.Expression.Type;
                    predicate.RightOperand = GetPropertyName(memberRight.ToString());
                    predicate.RightOperandType = memberRight.Expression.Type;
                    predicate.Operator = GetOperator(body.NodeType);
                    predicate.Link = GetOperator(linkingType);                    
                }

                queryPredicates.Add(predicate);
            }
            else
            {
                WalkTree((BinaryExpression)body.Left, body.NodeType, ref queryPredicates);
                WalkTree((BinaryExpression)body.Right, body.NodeType, ref queryPredicates);
            }
        }

        internal static PredicateUnit GetPredicateWithConstant(ConstantExpression constant, BinaryExpression body, ExpressionType linkingType, ExpressionDirection direction = ExpressionDirection.Right)
        {
            var predicate = new PredicateUnit();            

            if (direction == ExpressionDirection.Right)
            {
                predicate.RightOperand = constant.Value;
                predicate.RightOperandType = constant.Value.GetType();
                predicate.LeftOperand = GetPropertyName(body.Left.ToString());
                predicate.LeftOperandType = ((MemberExpression)body.Left).Expression.Type;
            }
            else
            {
                predicate.LeftOperand = constant.Value;
                predicate.LeftOperandType = constant.Value.GetType();
                predicate.RightOperand = GetPropertyName(body.Right.ToString());
                predicate.RightOperandType = ((MemberExpression)body.Right).Expression.Type;
            }

            predicate.Operator = GetOperator(body.NodeType);
            predicate.Link = GetOperator(linkingType);
            predicate.IsParameterized = true;

            return predicate;
        }

        internal static PredicateUnit GetPredicateWithMemberTypeField(BinaryExpression body, ExpressionType linkingType, ExpressionDirection direction = ExpressionDirection.Right)
        {
            var predicate = new PredicateUnit();
            MemberExpression memberRight = ((MemberExpression)(body.Right));
            MemberExpression memberLeft = ((MemberExpression)(body.Left));

            if (direction == ExpressionDirection.Right)
            {

                predicate.LeftOperand = GetPropertyName(memberLeft.ToString());
                predicate.LeftOperandType = memberLeft.Expression.Type;
                predicate.RightOperand = GetValue(memberRight);
                predicate.RightOperandType = memberRight.Expression.Type;
            }
            else
            {
                predicate.LeftOperand = GetPropertyName(memberRight.ToString());
                predicate.LeftOperandType = memberRight.Expression.Type;
                predicate.RightOperand = GetValue(memberLeft);
                predicate.RightOperandType = memberLeft.Expression.Type;
            }

            predicate.Operator = GetOperator(body.NodeType);
            predicate.Link = GetOperator(linkingType);
            predicate.IsParameterized = true;

            return predicate;
        }

        internal static object GetValue(MemberExpression member)
        {
            if (member == null) return new object();

            var objectMember = System.Linq.Expressions.Expression.Convert(member, typeof(object));

            var getterLambda = System.Linq.Expressions.Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        /// <summary>
        /// Gets the property name from a BinaryExpression.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>The property name for the property expression.</returns>
        internal static string GetPropertyName(string stringExpression)
        {
            string propertyName = stringExpression.Split(period)[1];

            return propertyName;
        }

        internal static string GetPropertyName(UnaryExpression body)
        {
            var token = body.Operand.ToString();

            var propertyName = token.Split(period)[1];

            return propertyName;
        }

        internal static string GetOperator(ExpressionType type)
        {
            return EntityMapper.SqlGenerator.GetOperator(type);
        }
    }
}
