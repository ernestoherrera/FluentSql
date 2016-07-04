using Dapper;
using FluentSql.Mappers;
using FluentSql.SqlGenerators.Contracts;
using FluentSql.Support;
using FluentSql.Support.Helpers;
using System;
using System.Collections.Generic;
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
        public virtual IQuery<TEntity> Where(Expression<Func<TEntity, bool>> expression)
        {
            if (expression == null) return this;

            Predicate = new ExpressionHelper(expression, ParameterNameGenerator);
            Parameters = Predicate.QueryParameters;

            return this;
        }

        public IQuery<TEntity> Where<TRightEntity>(Expression<Func<TEntity, TRightEntity, bool>> expression) where TRightEntity : new()
        {
            if (expression == null) return this;

            Predicate = new ExpressionHelper(expression, ParameterNameGenerator);
            Parameters = Predicate.QueryParameters;

            return this;
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

        public IQuery<TEntity> Where(string leftOperand, ExpressionType predicateOperator, string rightOperand, bool isParametized = false, ExpressionType? linkingOperator = null)
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

    }
}
