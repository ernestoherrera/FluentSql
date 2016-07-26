using FluentSql.Mappers;
using FluentSql.Support.Extensions;
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

        protected List<KeyValuePair<string, PropertyMap>> FieldParameterPairs;
               
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
                GenerateFieldParameterPairs(FieldsToUpate);
                AddToQueryParameters();
            }
        }
        
        public SetClause(UpdateQuery<T> parentQuery, object setFields) : 
            this(parentQuery)
        {
            var setFieldsType = setFields.GetType();

            if (!string.IsNullOrEmpty(setFieldsType.Namespace) || setFieldsType.IsIEnumerable())
                throw new Exception("Set clause only supports anonymous parameters.");

            var entityMap = EntityMapper.EntityMap[typeof(T)];
            var setFieldsProps = setFieldsType.GetProperties();
            var propMapDefault = default(PropertyMap);
            var paramGen = ParentQuery.ParameterNameGenerator;
            FieldParameterPairs = new List<KeyValuePair<string, PropertyMap>>();

            foreach (var setFieldPropInfo in setFieldsProps)
            {
                var propMap = entityMap.Properties.FirstOrDefault(p => p.Name == setFieldPropInfo.Name);

                if (propMap == propMapDefault)
                    throw new Exception(string.Format("Property {0} does not exists for entity {1}", setFieldPropInfo.Name, typeof(T).ToString()));

                var paramName = paramGen.GetNextParameterName(propMap.Name);
                var paramValue = setFieldsType.GetProperty(propMap.Name).GetValue(setFields);

                ParentQuery.Parameters.Add(paramName, paramValue);

                FieldParameterPairs.Add(new KeyValuePair<string, PropertyMap>(paramName, propMap));
            }
        }

        /// <summary>
        /// Generates a list of all updateable entity fields with its 
        /// corresponding parameter name represented by the KeyValue pair.
        /// KeyValuePair of Field, parameterName
        /// </summary>
        protected virtual void GenerateFieldParameterPairs(IEnumerable<PropertyMap> fieldList)
        {
            FieldParameterPairs = new List<KeyValuePair<string, PropertyMap>>();
            var paramGen = this.ParentQuery.ParameterNameGenerator;

            foreach (var field in fieldList)
            {
                var parameterName = paramGen.GetNextParameterName(field.ColumnName);

                FieldParameterPairs.Add(new KeyValuePair<string, PropertyMap>(parameterName, field));
            }
        }

        protected virtual void AddToQueryParameters()
        {            
            foreach (var pair in this.FieldParameterPairs)
            {
                var fieldValue = pair.Value.PropertyInfo.GetValue(ParentQuery.Entity);

                ParentQuery.Parameters.Add(pair.Key, fieldValue);
            }
        }

        public virtual string ToSql()
        {
            if (FieldParameterPairs == null)
                return string.Empty;

            SetClauseParts = new List<string>();
           
            foreach (var pair in FieldParameterPairs)
            {
                SetClauseParts.Add(string.Format(" {0} = {1} ", pair.Value.ColumnName, pair.Key ));
            }

            return string.Join(",", SetClauseParts);
        }

        public override string ToString()
        {
            return ToSql();
        }


    }
}
