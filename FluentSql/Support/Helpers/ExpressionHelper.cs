using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using FluentSql.Mappers;
using FluentSql.SqlGenerators;
using System.Reflection;
using FluentSql.Support.Extensions;
using Dapper;
using System.Text;

namespace FluentSql.Support.Helpers
{
    public class ExpressionHelper : ExpressionVisitor , IDisposable
    {
        #region Constructor
        public ExpressionHelper(Expression predicateExpression, SqlGeneratorHelper parameterNameGenerator)
        {
            if (predicateExpression == null)
                throw new Exception("Predicate expression can't be null.");

            if (parameterNameGenerator == null)
                parameterNameGenerator = new SqlGeneratorHelper();

            this.paramNameGenerator = parameterNameGenerator;
            this.Visit(predicateExpression);
        }
        #endregion

        #region Private Properties
        private static readonly char[] period = new char[] { '.' };
        private IComparer<ExpressionType> comparer = new OperatorPrecedenceComparer();
        private Queue<dynamic> predicateString = new Queue<dynamic>();
        private SqlGeneratorHelper paramNameGenerator;
        #endregion

        #region Public Properties
        public DynamicParameters QueryParameters = new DynamicParameters();        
        #endregion

        #region Public Methods        
        public string ToSql()
        {
            if (predicateString.Count == 0) return string.Empty;

            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append("WHERE ");

            foreach (var token in predicateString)
            {
                sqlBuilder.Append(" " + token.ToString());
            }

            return sqlBuilder.ToString();
        }

        public void Dispose()
        {
            paramNameGenerator = null;
            predicateString = null;
            comparer = null;            
        }
        #endregion

        #region Protected Methods
        protected override Expression VisitBinary(BinaryExpression exp)
        {
            if ( comparer.Compare(exp.Left.NodeType, exp.NodeType) < 0)
            {
                predicateString.Enqueue("(");
                this.Visit(exp.Left);
                predicateString.Enqueue(GetOperator(exp.NodeType));
                this.Visit(exp.Right);
                predicateString.Enqueue(")");
            }
            else
            {
                this.Visit(exp.Left);
                predicateString.Enqueue(GetOperator(exp.NodeType));
                this.Visit(exp.Right);
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
                predicateString.Enqueue(like);

                if (methodCall.Arguments.Count > 0)
                {
                    var compareTo = methodCall.Arguments[0];

                    if (compareTo is ConstantExpression)
                    {
                        var lambdaExp = Expression.Lambda(compareTo).Compile();

                        comparingArgument = (string)lambdaExp.DynamicInvoke();
                                               
                    }
                    else if (((MemberExpression)compareTo).Member.MemberType == MemberTypes.Field)
                    {
                        comparingArgument = (string)GetValue((MemberExpression)compareTo);
                    }

                    if (methodName == "StartsWith")
                        comparingArgument += wildcard;
                    else
                        comparingArgument = wildcard + comparingArgument;

                    var paramName = paramNameGenerator.GetNextParameterName(nameof(comparingArgument));

                    QueryParameters.Add(paramName, comparingArgument);
                    predicateString.Enqueue(comparingArgument);

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
            {
                var paramName = paramNameGenerator.GetNextParameterName(nameof(p));

                QueryParameters.Add(paramName, p.ToString());
                predicateString.Enqueue(p.ToString());
            }

            return p;
        }

        protected override Expression VisitConstant(ConstantExpression exp)
        {
            var paramName = paramNameGenerator.GetNextParameterName(nameof(exp));

            QueryParameters.Add(paramName, exp.Value);
            predicateString.Enqueue(exp.Value.ToString());

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

                if (valuesType.GetIEnumerableImpl() != null)
                {
                    var parameterNames = new List<string>();

                    foreach (var val in values)
                    {
                        var paramName = paramNameGenerator.GetNextParameterName(nameof(val));

                        parameterNames.Add(paramName);
                        QueryParameters.Add(paramName, val);
                    }

                    var inClause = string.Format("IN ( {0} )", String.Join(",", parameterNames));

                    predicateString.Enqueue(inClause);
                }
                else
                {
                    predicateString.Enqueue(values.ToString());
                }
                return m;

            }
            else if (member.MemberType == MemberTypes.Property && propertyType == typeof(System.Boolean))
            {
                predicateString.Enqueue(m.ToString() + " = 1");
            }
            else
            {                
                var token = GetFormattedField(m.Expression.Type, m.ToString());
                
                predicateString.Enqueue(token);
            }

            return base.VisitMember(m);
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            Expression operand;

            if (comparer.Compare(u.Operand.NodeType, u.NodeType) < 0)
            {
                if (u.NodeType == ExpressionType.Not || u.NodeType == ExpressionType.Negate)
                {
                    predicateString.Enqueue(GetOperator(u.NodeType));
                }

                predicateString.Enqueue("(");
                operand = this.Visit(u.Operand);
                predicateString.Enqueue(")");
            }
            else
            {
                operand = this.Visit(u.Operand);
            }

            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }

            return u;
        }
        #endregion

        #region Internal Static Methods
        internal static object GetValue(MemberExpression member)
        {
            if (member == null) return new object();

            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

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

        internal static string GetFormattedField(Type type, string fieldName)
        {
            return EntityMapper.SqlGenerator.FormatFieldforSql(type, fieldName);
        }

        #endregion

        #region Override
        public override string ToString()
        {
            return ToSql();
        }       
        #endregion

    }
}
