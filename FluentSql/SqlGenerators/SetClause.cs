using FluentSql.Mappers;
using System.Collections.Generic;
using System.Linq;

namespace FluentSql.SqlGenerators
{
    public class SetClause<T>
    {        
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

            GenerateFieldParameterPairs();
            AddToQueryParameters();
        }       

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
            var setClause = new List<string>();
           
            foreach (var pair in this.FieldParameterPairs)
            {
                setClause.Add(string.Format(" {0} = {1} ", pair.Key.ColumnName, pair.Value ));
            }

            return string.Join(",", setClause);
        }

        public override string ToString()
        {
            return ToSql();
        }


    }
}
