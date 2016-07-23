using FluentSql.Mappers;
using FluentSql.Support.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentSql.SqlGenerators
{
    public class SetClause<T>
    {
        protected List<string> SetClauseParts;

        protected UpdateQuery<T> ParentQuery { get; set; }

        protected IEnumerable<PropertyMap> FieldsToUpate;

        protected List<KeyValuePair<PropertyMap, string>> FieldParameterPairs;
               
        public SetClause(UpdateQuery<T> parentQuery)
        {
            this.ParentQuery = parentQuery;

            FieldsToUpate = ParentQuery.Fields.Where(f => !f.IsAutoIncrement &&
                                                    !f.IsReadOnly &&
                                                    !f.Ignored &&
                                                    f.IsTableField)
                                        .OrderBy(f => f.OrdinalPosition);

            if (parentQuery.Entity != null)
            {
                GenerateFieldParameterPairs();
                AddToQueryParameters();
            }            
        }
        
        public SetClause(UpdateQuery<T> parentQuery, params Expression<Func<T, bool>>[] setExpressions) : 
            this(parentQuery)
        {
            SetClauseParts = new List<string>(setExpressions.Length);
            ExpressionHelper setClauseExpression = null;

            foreach (var setExp in setExpressions)
            {
                if (setClauseExpression == null)
                    setClauseExpression = new ExpressionHelper(setExp, ParentQuery.ParameterNameGenerator);
                else
                    setClauseExpression.Visit(setExp);

                SetClauseParts.Add(setClauseExpression.ToSql());
                ParentQuery.Parameters.AddDynamicParams(setClauseExpression.QueryParameters);

                setClauseExpression.Reset();
            }            
        }

        /// <summary>
        /// Generates a list of all updateable entity fields with its 
        /// corresponding parameter name represented by the KeyValue pair.
        /// KeyValuePair of Field, parameterName
        /// </summary>
        protected virtual void GenerateFieldParameterPairs()
        {
            FieldParameterPairs = new List<KeyValuePair<PropertyMap, string>>();
            var paramGen = this.ParentQuery.ParameterNameGenerator;

            foreach (var field in this.FieldsToUpate)
            {
                var parameterName = paramGen.GetNextParameterName(field.ColumnName);

                FieldParameterPairs.Add(new KeyValuePair<PropertyMap, string>(field, parameterName));
            }
        }

        protected virtual void AddToQueryParameters()
        {            
            foreach (var pair in this.FieldParameterPairs)
            {
                var fieldValue = pair.Key.PropertyInfo.GetValue(this.ParentQuery.Entity);

                this.ParentQuery.Parameters.Add(pair.Value, fieldValue);
            }
        }            

        public virtual string ToSql()
        {
            if (SetClauseParts != null)            
                return String.Join(",", SetClauseParts);

            SetClauseParts = new List<string>();
           
            foreach (var pair in this.FieldParameterPairs)
            {
                SetClauseParts.Add(string.Format(" {0} = {1} ", pair.Key.ColumnName, pair.Value ));
            }

            return string.Join(",", SetClauseParts);
        }

        public override string ToString()
        {
            return ToSql();
        }


    }
}
