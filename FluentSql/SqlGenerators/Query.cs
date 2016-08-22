using Dapper;
using FluentSql.Mappers;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using FluentSql.Support.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FluentSql.SqlGenerators
{
    public class Query<TEntity> : IQuery<TEntity>
    {
        #region Protected Properties
        protected ExpressionHelper Predicate;
        protected Queue<dynamic> Joins = new Queue<dynamic>();
        protected List<SortOrderField> OrderByFields;
        protected int TopRows = 0;
        #endregion

        #region Public Properties
        public SqlGeneratorHelper ParameterNameGenerator { get; set; }
        public Type EntityType { get; protected set; }
        public string TableName { get; protected set; }
        public string TableAlias { get; set; }
        public string SchemaName { get; protected set; }
        public string DatabaseName { get; protected set; }
        public List<PropertyMap> Fields { get; protected set; }
        public string Verb { get; protected set; }
        public DynamicParameters Parameters { get; protected set; }
        public PredicateUnits PredicateParts { get; protected set; }        
        #endregion

        #region Constructor
        public Query()
        {
            Parameters = new DynamicParameters();
            Fields = new List<PropertyMap>();
            ParameterNameGenerator = new SqlGeneratorHelper();

            if (EntityMapper.EntityMap.ContainsKey(typeof(TEntity)))
            {
                var entityMap = EntityMapper.EntityMap[typeof(TEntity)];

                Fields = entityMap.Properties.Where(p => p.IsTableField).ToList();
                TableName = entityMap.TableName;
                TableAlias = entityMap.TableAlias;
                SchemaName = entityMap.SchemaName;
                DatabaseName = entityMap.Database;
                EntityType = typeof(TEntity);
            }
        }
        #endregion

        #region Virtual Methods

        #region Top Number of Rows
        public virtual IQuery<TEntity> GetTopRows(int topNumberOfRows)
        {
            TopRows = topNumberOfRows;
            return this;
        }
        #endregion

        #region Where clause
        public virtual IQuery<TEntity> Where(Expression<Func<TEntity, bool>> expression)
        {
            if (expression == null) return this;

            Predicate = new ExpressionHelper(expression, ParameterNameGenerator);

            if (Parameters.ParameterNames.Any())
                Parameters.AddDynamicParams(Predicate.QueryParameters);
            else
                Parameters = Predicate.QueryParameters;

            return this;
        }

        public IQuery<TEntity> Where<TRightEntity>(Expression<Func<TEntity, TRightEntity, bool>> expression) where TRightEntity : new()
        {
            if (expression == null) return this;

            Predicate = new ExpressionHelper(expression, ParameterNameGenerator);

            if (Parameters.ParameterNames.Any())
                Parameters.AddDynamicParams(Predicate.QueryParameters);
            else
                Parameters = Predicate.QueryParameters;

            return this;
        }

        public IQuery<TEntity> Where<T1, T2>(Expression<Func<T1, T2, bool>> expression) where T1 : new() where T2 : new()
        {
            if (expression == null) return this;

            Predicate = new ExpressionHelper(expression, ParameterNameGenerator);

            if (Parameters.ParameterNames.Any())
                Parameters.AddDynamicParams(Predicate.QueryParameters);
            else
                Parameters = Predicate.QueryParameters;

            return this;
        }

        public IQuery<TEntity> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression)
            where T1 : new()
            where T2 : new()
            where T3 : new()
        {
            if (expression == null) return this;

            Predicate = new ExpressionHelper(expression, ParameterNameGenerator);

            if (Parameters.ParameterNames.Any())
                Parameters.AddDynamicParams(Predicate.QueryParameters);
            else
                Parameters = Predicate.QueryParameters;

            return this;
        }

        internal IQuery<TEntity> Where(string leftOperand, ExpressionType predicateOperator, string rightOperand, bool isParametized = false, ExpressionType? linkingOperator = null)
        {
            if (this.PredicateParts == null)
                this.PredicateParts = new PredicateUnits();

            var parameterName = string.Empty;

            if (isParametized)
            {
                parameterName = this.ParameterNameGenerator.GetNextParameterName("param");

                Parameters.Add(parameterName, rightOperand);
                this.PredicateParts.Add(leftOperand, predicateOperator, parameterName, isParametized, linkingOperator);
            }
            else
            {
                this.PredicateParts.Add(leftOperand, predicateOperator, rightOperand, isParametized, linkingOperator);
            }

            return this;
        }

        #endregion

        #region Join
        public virtual IQuery<TEntity> JoinOn<TRightEntity>(Expression<Func<TEntity, TRightEntity, bool>> expression, JoinType joinType = JoinType.Inner)
        {
            if (expression == null && joinType != JoinType.Cross)
                throw new Exception("Join expression cannot be null.");

            var rightQuery = new Query<TRightEntity>();
            var queryJoin = EntityMapper.SqlGenerator.JoinOn<TEntity, TRightEntity>(this, rightQuery, joinType);

            queryJoin.On(expression);
            Joins.Enqueue(queryJoin);

            return Joins.Peek().LeftQuery;
        }

        public virtual IQuery<TEntity> JoinOn<T1, T2>(Expression<Func<T1, T2, bool>> expression, JoinType joinType = JoinType.Inner)
        {
            if (expression == null && joinType != JoinType.Cross)
                throw new Exception("Join expression cannot be null. Join expression can only be null if the JoinType is a Cross join.");

            var leftQuery = new Query<T1>();
            var rightQuery = new Query<T2>();

            var queryJoin = EntityMapper.SqlGenerator.JoinOn<T1, T2>(leftQuery, rightQuery, joinType);

            queryJoin.On(expression);
            Joins.Enqueue(queryJoin);

            return Joins.Peek().LeftQuery;
        }

        #endregion

        #region Order by
        public virtual IQuery<TEntity> OrderBy(Expression<Func<TEntity, object>> expression)
        {
            return SetOrderByClause(expression, SortOrder.Ascending);
        }

        public virtual IQuery<TEntity> OrderBy<T>(Expression<Func<T, object>> expression) where T : new()
        {
            return SetOrderByClause(expression, SortOrder.Ascending);
        }

        public virtual IQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object>> expression)
        {
            return SetOrderByClause(expression, SortOrder.Descending);
        }

        public virtual IQuery<TEntity> OrderByDescending<T>(Expression<Func<T, object>> expression) where T : new()
        {
            return SetOrderByClause(expression, SortOrder.Descending);
        }

        protected IQuery<TEntity> SetOrderByClause<T>(Expression<Func<T, object>> expression, SortOrder sortOrderDirection)
        {
            if (OrderByFields == null) OrderByFields = new List<SortOrderField>();

            if (expression == null || (expression.Body.NodeType != ExpressionType.MemberAccess &&
                expression.Body.NodeType != ExpressionType.Convert))
                throw new Exception("Incorrect sort order expression");

            string orderByFieldName = "";

            if (expression.Body.NodeType == ExpressionType.Convert)
                orderByFieldName = ExpressionHelper.GetPropertyName((UnaryExpression) expression.Body);
            else
                orderByFieldName = ExpressionHelper.GetPropertyName(expression.Body.ToString());

            OrderByFields.Add(new SortOrderField
            {
                FieldName = orderByFieldName,
                SortOrderDirection = sortOrderDirection,
                TableAlias = ResolveTableAlias(typeof(T))
            });

            return this;
        }

        public virtual IQuery<TEntity> OrderBy(params SortOrderField[] sortOrderArray)
        {
            if (sortOrderArray == null) return this;

            if (OrderByFields == null) OrderByFields = new List<SortOrderField>();

            OrderByFields.AddRange(sortOrderArray);

            return this;
        }

        public virtual IQuery<TEntity> OrderBy(List<SortOrderField> sortOrderFields)
        {
            if (sortOrderFields == null) return this;

            if (OrderByFields == null) OrderByFields = new List<SortOrderField>();

            OrderByFields.AddRange(sortOrderFields);

            return this;
        }
        #endregion

        public string ResolveTableAlias(Type type)
        {
            var alias = "";

            foreach (var join in Joins)
            {
                if (type == join.LeftJoinType)
                {
                    alias = join.LeftQuery.TableAlias;
                    break;
                }

                if (type == join.RightJoinType)
                {
                    alias = join.RightQuery.TableAlias;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(alias))
                return alias;

            if (EntityType == type)
                alias = TableAlias;

            return alias;
        }

        public virtual string ToSql()
        {
            return string.Format("{0} {1}", Verb, string.Join(", ", Fields.Where(f => f.IsTableField).ToList()));
        }

        public void Dispose()
        {
            Joins.Clear();
            Fields.Clear();
            Predicate = null;
        }
        #endregion

        #region Protected Methods

        protected virtual IQuery<TRightEntity> ProcessJoin<TRightEntity>(JoinType joinType = JoinType.Inner) where TRightEntity : new()
        {
            var rightQuery = new Query<TRightEntity>();
            var join = new Join<TEntity, TRightEntity>(this, rightQuery, joinType);

            Joins.Enqueue(join);

            return rightQuery;
        }        

        #endregion

    }
}
