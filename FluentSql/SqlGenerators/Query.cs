using Dapper;
using FluentSql.Mappers;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FluentSql.SqlGenerators
{
    public class Query<TEntity> : IQuery<TEntity>
    {
        #region Properties

        protected Predicate<TEntity> Predicate;

        protected Queue<dynamic> Joins = new Queue<dynamic>();

        public Type EntityType { get; protected set; }
        public string TableName { get; protected set; }
        public string SchemaName { get; protected set; }
        public string Database { get; protected set; }
        public List<PropertyMap> Fields { get; protected set; }
        public string Verb { get; protected set; }
        public DynamicParameters Parameters { get; protected set; }
        public string TableAlias { get; set; }
        #endregion

        #region Constructor
        public Query()
        {
            Parameters = new DynamicParameters();
            Fields = new List<PropertyMap>();
            Predicate = new Predicate<TEntity>(this);

            if (EntityMapper.EntityMap.ContainsKey(typeof(TEntity)))
            {
                var entityMap = EntityMapper.EntityMap[typeof(TEntity)];
                
                Fields = entityMap.Properties.Where(p => p.IsTableField).ToList();
                TableName = entityMap.TableName;
                TableAlias = entityMap.TableAlias;
                SchemaName = entityMap.SchemaName;
                Database = entityMap.Database;
                EntityType = typeof(TEntity);
            }
        }
        #endregion

        #region Virtual Methods
        public virtual IQuery<TEntity> Where(Expression<Func<TEntity, bool>> expression)
        {
            ProcessExpression(expression);
            CreateDynamicParameters();

            return this;
        }

        public virtual IQuery<TEntity> Where(string propertyName, ExpressionType expressionType, dynamic value, string linkToNextPredicate)
        {
            if (string.IsNullOrEmpty(propertyName) || value == null )
                throw new Exception("Property name can not be empty or null and value can not be null for use in the where clause");                
            
            var op = EntityMapper.SqlGenerator.GetOperator(expressionType);

            var predicateUnit = new PredicateUnit
            {
                LeftOperand = typeof(TEntity).GetProperty(propertyName),
                LeftOperandType = typeof(TEntity),
                Operator = EntityMapper.SqlGenerator.GetOperator(expressionType),
                RightOperandType = value.GetType(),
                RightOperand = value,
                Link = linkToNextPredicate
            };

            Predicate.Add(predicateUnit);
            CreateDynamicParameters();

            return this;
        }

        public IQuery<TEntity> Where<TRightEntity>(Expression<Func<TEntity, TRightEntity, bool>> expression) where TRightEntity : new()
        {
            if (expression == null) return this;

            ProcessExpression<TRightEntity>(expression);
            CreateDynamicParameters();

            return this;
        }

        public IQuery<TEntity> WhereOnKey<R>(R entity)
        {

            if (Fields == null || !Fields.Any()) return this;

            var keyFields = Fields.Where(f => f.IsPrimaryKey);
            var fieldCount = Fields.Count();

            for (var i = 0 ; i < fieldCount; i++)
            {
                var field = Fields[i];
                var predicateUnit = new PredicateUnit
                {
                    LeftOperand = field.ColumnName,
                    LeftOperandType = typeof(TEntity),
                    Operator = EntityMapper.SqlGenerator.GetOperator(ExpressionType.Equal),
                    RightOperandType = field.PropertyInfo.GetType(),
                    RightOperand = field.PropertyInfo.GetValue(entity),
                    Link = ((i + 1) >= fieldCount ? "" : EntityMapper.SqlGenerator.And)
                };

                Predicate.Add(predicateUnit);
            }

            CreateDynamicParameters();
            return this;
        }

        public virtual IQuery<TEntity> JoinOnKey<TRightEntity>() where TRightEntity : new()
        {
            var rightQuery = new Query<TRightEntity>();
            var join = new Join<TEntity, TRightEntity>(this, rightQuery);

            Joins.Enqueue(join);

            return join.OnKey();
        }

        public virtual IQuery<TEntity> JoinOn<TRightEntity>(Expression<Func<TEntity, TRightEntity, bool>> expression, JoinType joinType = JoinType.Inner)
        {
            var rightQuery = new Query<TRightEntity>();
            var join = new Join<TEntity, TRightEntity>(this, rightQuery, joinType);

            Joins.Enqueue(join);

            return join.On(expression);
        }

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
            Predicate.Clear();
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

        protected void ProcessExpression<TRightEntity>(Expression<Func<TEntity, TRightEntity, bool>> expression) where TRightEntity : new()
        {
            if (expression == null) return;

            ExpressionHelper.WalkTree((BinaryExpression)expression.Body, ExpressionType.Default, ref Predicates);

            return;
        }

        protected void ProcessExpression(Expression<Func<TEntity, bool>> expression)
        {
            if (expression == null) return;

            ExpressionHelper.WalkTree((BinaryExpression)expression.Body, ExpressionType.Default, ref Predicates);
        }       

        protected void CreateDynamicParameters()
        {
            var paramNames = Parameters.ParameterNames;

            foreach (var prd in Predicate)
            {
                if (prd.IsParameterized && paramNames.FirstOrDefault(p =>
                                    string.Compare(p, prd.Operand, StringComparison.CurrentCultureIgnoreCase) != 0) == null)
                {
                    Parameters.Add("@" + prd.Operand, prd.OperandValue);
                }

            }
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
