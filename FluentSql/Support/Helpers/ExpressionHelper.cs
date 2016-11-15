﻿using System;
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
        private Stack<dynamic> _predicateString = new Stack<dynamic>();
        private SqlGeneratorHelper _paramNameGenerator;
        private readonly string _SEPARATOR = " ";
        private readonly string _parameterName = "param_name";
        #endregion

        #region Public Properties
        public DynamicParameters QueryParameters = new DynamicParameters();
        #endregion

        #region Public Methods
        public string ToSql()
        {
            if (_predicateString.Count == 0) return string.Empty;

            var sqlBuilder = new StringBuilder();
            var predicate = _predicateString.ToArray();

            for (int i = predicate.Length - 1; i >= 0  ; i--)
            {
                var token = predicate[i];

                sqlBuilder.Append(_SEPARATOR + token);
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
                _predicateString.Push("(");
                this.Visit(exp.Left);
                _predicateString.Push(GetOperator(exp.NodeType));
                this.Visit(exp.Right);
                _predicateString.Push(")");
            }
            else
            {
                this.Visit(exp.Left);
                _predicateString.Push(GetOperator(exp.NodeType));
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
                var like = EntityMapper.SqlGenerator.GetStringComparisonOperator();
                var wildcard = EntityMapper.SqlGenerator.StringPatternMatchAny;
                var comparingArgument = string.Empty;

                like += _SEPARATOR;

                this.VisitMember((MemberExpression)methodObject);
                _predicateString.Push(like);

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

                    if (comparingArgument == null)
                    {
                        HandleNullParameterValue();
                        return methodCall;
                    }

                    if (methodName == "StartsWith")
                        comparingArgument += wildcard;
                    else
                        comparingArgument = wildcard + comparingArgument;

                    var paramName = _paramNameGenerator.GetNextParameterName(_parameterName);

                    QueryParameters.Add(paramName, comparingArgument);
                    _predicateString.Push(paramName);

                    return methodCall;
                }
                else
                {
                    throw new NotSupportedException(string.Format("{0} Method call supported", methodName));
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
            else
            {
                throw new NotSupportedException(string.Format("{0} Method call supported", methodName));
            }
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (SystemTypes.Numeric.Contains(p.Type))
            {
                var paramName = _paramNameGenerator.GetNextParameterName(_parameterName);

                QueryParameters.Add(paramName, p.ToString());
                _predicateString.Push(paramName);
            }

            return p;
        }

        protected override Expression VisitConstant(ConstantExpression exp)
        {
            var paramName = _paramNameGenerator.GetNextParameterName(_parameterName);

            QueryParameters.Add(paramName, exp.Value);
            _predicateString.Push(paramName);

            return base.VisitConstant(exp);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var propertyType = memberExpression.Type;
            var member = ((MemberExpression)memberExpression).Member;

            if (member.MemberType == MemberTypes.Field)
            {
                dynamic values = GetValue(memberExpression);

                if (values == null)
                {
                    HandleNullParameterValue();

                    return memberExpression;
                }

                Type valuesType = values.GetType();

                if (valuesType.GetIEnumerableImpl() != null && valuesType != typeof(string))
                {
                    var parameterNames = new List<string>();

                    foreach (var val in values)
                    {
                        var paramName = _paramNameGenerator.GetNextParameterName(_parameterName);

                        parameterNames.Add(paramName);
                        QueryParameters.Add(paramName, val);
                    }

                    var inClause = string.Format("IN ( {0} )", String.Join(",", parameterNames));

                    _predicateString.Push(inClause);
                }
                else
                {
                    var paramName = _paramNameGenerator.GetNextParameterName(_parameterName);

                    QueryParameters.Add(paramName, values);
                    _predicateString.Push(paramName);
                }

                return memberExpression;

            }
            else if (member.MemberType == MemberTypes.Property && propertyType == typeof(System.Boolean))
            {
                _predicateString.Push(memberExpression.ToString() + " = 1");
            }
            else if (member.MemberType == MemberTypes.Property && 
                    propertyType == typeof(System.DateTime) &&
                    memberExpression.Expression == null)
            {
                AddToPredicate(memberExpression);
            }
            else if (memberExpression.Expression != null &&
                     memberExpression.Expression.NodeType == ExpressionType.MemberAccess &&
                     ((MemberExpression)memberExpression.Expression).Member.MemberType == MemberTypes.Field)
            {
                AddToPredicate(memberExpression);
                return memberExpression;

            }
            else if (memberExpression.NodeType == ExpressionType.MemberAccess &&
                     memberExpression.Expression == null)
            {
                AddToPredicate(memberExpression);
                return memberExpression;
            }
            else if (memberExpression.NodeType == ExpressionType.MemberAccess &&
                        memberExpression.Expression != null &&
                        memberExpression.Expression.NodeType == ExpressionType.Constant)
            {
                AddToPredicate(memberExpression);
                return memberExpression;
            }
            else if (memberExpression.NodeType == ExpressionType.MemberAccess &&
                        memberExpression.Expression != null &&
                        memberExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                var propertyName = GetPropertyName(memberExpression.ToString());
                var token = GetFormattedField(memberExpression.Expression.Type, propertyName);

                _predicateString.Push(token);
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
                    _predicateString.Push(GetOperator(u.NodeType));
                }

                _predicateString.Push("(");
                operand = this.Visit(u.Operand);
                _predicateString.Push(")");
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

        #region Private Methods

        private void HandleNullParameterValue()
        {
            var previousToken = _predicateString.Peek();

            if (previousToken == "=")
            {
                var nullEquality = EntityMapper.SqlGenerator.GetNullEquality();

                _predicateString.Pop();
                _predicateString.Push(nullEquality);
            }
            else
            {
                _predicateString.Push(EntityMapper.SqlGenerator.Null);
            }
        }

        private void AddToPredicate(MemberExpression memberExpression)
        {
            var paramValue = GetValue(memberExpression);
            var paramName = _paramNameGenerator.GetNextParameterName(_parameterName);

            QueryParameters.Add(paramName, paramValue);
            _predicateString.Push(paramName);

        }
        #endregion
    }
}
