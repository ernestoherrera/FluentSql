using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using FluentSql.Mappers;
using FluentSql.SqlGenerators;
using System.Reflection;
using FluentSql.Support.Extensions;

namespace FluentSql.Support.Helpers
{
    internal class ExpressionHelper : ExpressionVisitor
    {
        private static readonly char[] period = new char[] { '.' };
        private IComparer<ExpressionType> comparer = new OperatorPrecedenceComparer();

        public Queue<dynamic> parents = new Queue<dynamic>();

        protected override Expression VisitBinary(BinaryExpression exp)
        {
            if ( comparer.Compare(exp.Left.NodeType, exp.NodeType) < 0)
            {
                parents.Enqueue("(");
                Visit(exp.Left);
                parents.Enqueue(GetOperator(exp.NodeType));
                Visit(exp.Right);
                parents.Enqueue(")");
            }
            else
            {
                Visit(exp.Left);
                parents.Enqueue(GetOperator(exp.NodeType));
                Visit(exp.Right);
            }

            Expression conversion = this.Visit(exp.Conversion);
            return exp;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            var allowedMethods = new string[] { "StartsWith", "Contains", "EndsWith" };
            var methodObject = methodCall.Object;
            var methodName = methodCall.Method.Name;            

            if (methodCall.Method.DeclaringType == typeof(string) && allowedMethods.Contains(methodName))
            {
                var like = "LIKE ";
                var wildcard = @"%";
                var comparingArgument = string.Empty;

                this.VisitMember((MemberExpression)methodObject);
                parents.Enqueue(like);

                if (methodCall.Arguments.Count > 0)
                {
                    var compareTo = methodCall.Arguments[0];

                    if (compareTo is ConstantExpression)
                    {
                        var expValue = Expression.Lambda(compareTo).Compile();
                        comparingArgument = (string)expValue.DynamicInvoke();
                    }
                    else if (((MemberExpression)compareTo).Member.MemberType == MemberTypes.Field)
                    {
                        comparingArgument = (string)GetValue((MemberExpression)compareTo);
                    }

                    if (methodName == "StartsWith")
                        comparingArgument += wildcard;
                    else
                        comparingArgument = wildcard + comparingArgument;

                    parents.Enqueue(comparingArgument);

                    return methodCall;
                }
                else
                {
                    throw new Exception("Method call arguments not supported");
                }
            }
            else if (methodObject.NodeType == ExpressionType.MemberAccess && methodName == "Contains")
            {
                if (methodCall.Arguments.Count > 0 && ((MemberExpression)methodCall.Arguments[0]).NodeType == ExpressionType.MemberAccess)
                {
                    this.VisitMember((MemberExpression)methodCall.Arguments[0]);
                    this.VisitMember((MemberExpression)methodObject);
                }
                else
                {
                    throw new Exception("Argument type not supported.");
                }

                return methodCall;
            }

            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {            
            if (SytemTypes.Numeric.Contains(p.Type))
                parents.Enqueue(p.ToString());

            return p;
        }

        protected override Expression VisitConstant(ConstantExpression exp)
        {
            parents.Enqueue(exp.Value.ToString());

            return base.VisitConstant(exp);
        }

        protected override Expression VisitMember(MemberExpression m)
        {            
            var propertyType = m.Type;
            var member = ((MemberExpression)m).Member;

            if (member.MemberType == MemberTypes.Field)
            {
                dynamic values = GetValue(m);
                Type valuesType = values.GetType();

                if ( valuesType.GetIEnumerableImpl() != null)
                {
                    var inClause = "IN (";

                    inClause += String.Join(",", values);
                    inClause += ")";

                    parents.Enqueue(inClause);
                }
                else
                {
                    parents.Enqueue(values.ToString());
                }
                return m;

            }
            else if (propertyType == typeof(System.Boolean))
                parents.Enqueue(m.ToString() + " = 1");
            else
                parents.Enqueue(m.ToString());

            return base.VisitMember(m);
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            Expression operand;

            if ( comparer.Compare(u.Operand.NodeType, u.NodeType) < 0)
            {
                if (u.NodeType == ExpressionType.Not || u.NodeType == ExpressionType.Negate)
                    parents.Enqueue(GetOperator(u.NodeType));
                parents.Enqueue("(");
                operand = this.Visit(u.Operand);
                parents.Enqueue(")");
            }
            else
                operand = this.Visit(u.Operand);

            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }
            return u;
        }

        internal static object GetValue(MemberExpression member)
        {
            if (member == null) return new object();

            var objectMember = System.Linq.Expressions.Expression.Convert(member, typeof(object));

            var getterLambda = System.Linq.Expressions.Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
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
