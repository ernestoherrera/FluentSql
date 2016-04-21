using Dapper;
using FluentSql.EntityMappers;
using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.SqlGenerators
{
    public class Query<TEntity> : IQuery<TEntity>
    {
        #region Properties

        protected List<PredicateUnit> Predicates = new List<PredicateUnit>();

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

        public virtual IQuery<TEntity> Where(string propertyName, ExpressionType expressionType, dynamic value)
        {
            var op = EntityMapper.SqlGenerator.GetOperator(expressionType);

            var predicate = new PredicateUnit
            {
                Operand = propertyName,
                OperandType = typeof(TEntity),
                Operator = EntityMapper.SqlGenerator.GetOperator(expressionType),
                OperandValueType = value.GetType(),
                OperandValue = value,
                IsParameterized = true
            };

            Predicates.Add(predicate);
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

        public IQuery<TEntity> WhereOnKey<TEntity>(TEntity entity)
        {

            if (Fields == null || !Fields.Any()) return this;

            var keyFields = Fields.Where(f => f.IsPrimaryKey);

            foreach (var field in keyFields)
            {
                var predicate = new PredicateUnit
                {
                    Operand = field.ColumnName,
                    OperandType = typeof(TEntity),
                    Operator = EntityMapper.SqlGenerator.GetOperator(ExpressionType.Equal),
                    OperandValueType = field.PropertyInfo.GetType(),
                    OperandValue = field.PropertyInfo.GetValue(entity),
                    IsParameterized = true
                };

                Predicates.Add(predicate);
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
            Predicates.Clear();
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

        protected virtual string BuildPredicateSql()
        {
            var sqlBuilder = new StringBuilder();

            for (int i = 0; i < Predicates.Count(); i++)
            {
                var item = Predicates[i];

                if (item.IsParameterized)
                {
                    if (!string.IsNullOrEmpty(item.Link) && i > 0)
                        sqlBuilder.Append(string.Format("{1} {0}.{2} {3} @{2} ",
                                                            ResolveTableAlias(item.OperandType),
                                                            item.Link,
                                                            item.Operand,
                                                            item.Operator));
                    else
                        sqlBuilder.Append(string.Format("{0}.{1} {2} @{1} ",
                                                            ResolveTableAlias(item.OperandType),
                                                            item.Operand,
                                                            item.Operator));
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.Link) && i > 0)
                        sqlBuilder.Append(string.Format("{0}  {1}.{2} {3} {4}.{5} ",
                                                            item.Link,
                                                            ResolveTableAlias(item.OperandType),
                                                            item.Operand,
                                                            item.Operator,
                                                            ResolveTableAlias(item.OperandValueType),
                                                            item.OperandValue));
                    else
                        sqlBuilder.Append(string.Format("{0}.{1} {2} {3}.{4} ",
                                                            ResolveTableAlias(item.OperandType),
                                                            item.Operand,
                                                            item.Operator,
                                                            ResolveTableAlias(item.OperandValueType),
                                                            item.OperandValue));
                }
            }

            return sqlBuilder.ToString();
        }

        protected void CreateDynamicParameters()
        {
            var paramNames = Parameters.ParameterNames;

            foreach (var prd in Predicates)
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
