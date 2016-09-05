using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using FluentSql.Mappers;
using FluentSql.SqlGenerators;
using System.Reflection;
using FluentSql.Support.Extensions;
using Dapper;
using System.Text;

namespace FluentSql.Support.Helpers
{
    public class ExpressionHelper : ExpressionVisitor, IDisposable
    {
        #region Constructor
        public ExpressionHelper(Expression predicateExpression, SqlGeneratorHelper parameterNameGenerator)
        {
            if (predicateExpression == null)
                throw new Exception("Predicate expression can't be null.");

            if (parameterNameGenerator == null)
                parameterNameGenerator = new SqlGeneratorHelper();

            _paramNameGenerator = parameterNameGenerator;
            this.Visit(predicateExpression);
        }
        #endregion

        #region Private Properties
        private static readonly char[] _period = new char[] { '.' };
        private IComparer<ExpressionType> _comparer = new OperatorPrecedenceComparer();
        private Queue<dynamic> _predicateString = new Queue<dynamic>();
        private SqlGeneratorHelper _paramNameGenerator;
        private readonly string _SEPARATOR = " ";
        #endregion

        #region Public Properties
        public DynamicParameters QueryParameters = new DynamicParameters();
        #endregion

        #region Public Methods
        public string ToSql()
        {
            if (_predicateString.Count == 0) return string.Empty;

            var sqlBuilder = new StringBuilder();

            foreach (var token in _predicateString)
            {
                sqlBuilder.Append(_SEPARATOR + token.ToString());
            }

            sqlBuilder.Append(_SEPARATOR);

            return sqlBuilder.ToString();
        }

        public void Dispose()
        {
            _paramNameGenerator = null;
            _predicateString = null;
            _comparer = null;
        }
        #endregion

        #region Protected Overriden Methods
        protected override Expression VisitBinary(BinaryExpression exp)
        {
            if (_comparer.Compare(exp.Left.NodeType, exp.NodeType) < 0)
            {
                _predicateString.Enqueue("(");
                this.Visit(exp.Left);
                _predicateString.Enqueue(GetOperator(exp.NodeType));
                this.Visit(exp.Right);
                _predicateString.Enqueue(")");
            }
            else
            {
                this.Visit(exp.Left);
                _predicateString.Enqueue(GetOperator(exp.NodeType));
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
                _predicateString.Enqueue(like);

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

                    var paramName = _paramNameGenerator.GetNextParameterName(nameof(comparingArgument));

                    QueryParameters.Add(paramName, comparingArgument);
                    _predicateString.Enqueue(paramName);

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
            else if (methodObject.ToString().StartsWith("DateTime.Now"))
            {
                this.VisitMember((MemberExpression)methodObject);
                return methodCall;
            }
            return base.VisitMethodCall(methodCall);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (SystemTypes.Numeric.Contains(p.Type))
            {
                var paramName = _paramNameGenerator.GetNextParameterName(nameof(p));

                QueryParameters.Add(paramName, p.ToString());
                _predicateString.Enqueue(paramName);
            }

            return p;
        }

        protected override Expression VisitConstant(ConstantExpression exp)
        {
            var paramName = _paramNameGenerator.GetNextParameterName(nameof(exp));

            QueryParameters.Add(paramName, exp.Value);
            _predicateString.Enqueue(paramName);

            return base.VisitConstant(exp);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var propertyType = memberExpression.Type;
            var member = ((MemberExpression)memberExpression).Member;

            if (member.MemberType == MemberTypes.Field)
            {
                dynamic values = GetValue(memberExpression);
                Type valuesType = values.GetType();

                if (valuesType.GetIEnumerableImpl() != null && valuesType != typeof(string))
                {
                    var parameterNames = new List<string>();

                    foreach (var val in values)
                    {
                        var paramName = _paramNameGenerator.GetNextParameterName(nameof(val));

                        parameterNames.Add(paramName);
                        QueryParameters.Add(paramName, val);
                    }

                    var inClause = string.Format("IN ( {0} )", String.Join(",", parameterNames));

                    _predicateString.Enqueue(inClause);
                }
                else
                {
                    var paramName = _paramNameGenerator.GetNextParameterName(nameof(member.Name));

                    QueryParameters.Add(paramName, values);
                    _predicateString.Enqueue(paramName);
                }
                return memberExpression;

            }
            else if (member.MemberType == MemberTypes.Property && propertyType == typeof(System.Boolean))
            {
                _predicateString.Enqueue(memberExpression.ToString() + " = 1");
            }
            else if (member.MemberType == MemberTypes.Property && propertyType == typeof(System.DateTime))
            {
                var paramName = _paramNameGenerator.GetNextParameterName(nameof(member.Name));
                var propertyValue = GetValue(memberExpression);

                QueryParameters.Add(paramName, propertyValue);
                _predicateString.Enqueue(paramName);

            }
            else if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess
                     && ((MemberExpression)memberExpression.Expression).Member.MemberType == MemberTypes.Field)
            {
                var paramValue = GetValue(memberExpression);
                var paramName = _paramNameGenerator.GetNextParameterName(nameof(memberExpression));

                QueryParameters.Add(paramName, paramValue);
                _predicateString.Enqueue(paramName);

                return memberExpression;

            }
            else if (memberExpression.NodeType == ExpressionType.MemberAccess)
            {
                var propertyName = GetPropertyName(memberExpression.ToString());
                var token = GetFormattedField(memberExpression.Expression.Type, propertyName);

                _predicateString.Enqueue(token);
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            Expression operand;

            if (_comparer.Compare(u.Operand.NodeType, u.NodeType) < 0)
            {
                if (u.NodeType == ExpressionType.Not || u.NodeType == ExpressionType.Negate)
                {
                    _predicateString.Enqueue(GetOperator(u.NodeType));
                }

                _predicateString.Enqueue("(");
                operand = this.Visit(u.Operand);
                _predicateString.Enqueue(")");
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
            var propertyName = token.Split(_period)[1];

            return propertyName;
        }

        internal static string GetPropertyName(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return "";

            var propertyName = string.Empty;
            var expParts = expression.Split(_period);

            if (expParts.Length > 0)
                propertyName = expParts[1];

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

        internal static Expression<Func<TModel, TProperty>> CreateExpression<TModel, TProperty>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TModel), "m");

            return Expression.Lambda<Func<TModel, TProperty>>(Expression.PropertyOrField(param, propertyName), param);
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
