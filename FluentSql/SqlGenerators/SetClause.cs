using FluentSql.Mappers;
using System.Collections.Generic;
using System.Linq;

namespace FluentSql.SqlGenerators
{
    public class SetClause<T>
    {
        protected string driverIndicator = EntityMapper.SqlGenerator.DriverParameterIndicator;

        protected UpdateQuery<T> ParentQuery { get; set; }

        protected IEnumerable<PropertyMap> FieldsToUpate;
               
        public SetClause(UpdateQuery<T> parentQuery)
        {
            this.ParentQuery = parentQuery;

            FieldsToUpate = ParentQuery.Fields.Where(f => !f.IsAutoIncrement &&
                                                    !f.IsReadOnly &&
                                                    !f.Ignored &&
                                                    f.IsTableField)
                                        .OrderBy(f => f.OrdinalPosition);

            AddToQueryParameter();
        }

        public SetClause(UpdateQuery<T> parentQuery, IEnumerable<PropertyMap> fieldsToUpdate )
        {
            this.ParentQuery = parentQuery;
            this.FieldsToUpate = fieldsToUpdate.OrderBy(f => f.OrdinalPosition);

            AddToQueryParameter();
        }

        protected virtual void AddToQueryParameter()
        {            
            foreach (var field in FieldsToUpate)
            {
                var fieldValue = field.PropertyInfo.GetValue(ParentQuery.Entity);

                ParentQuery.Parameters.Add(EntityMapper.SqlGenerator.DriverParameterIndicator + field.ColumnName + field.OrdinalPosition, fieldValue);
            }
        }            

        public virtual string ToSql()
        {            
            var setClause = new List<string>();
            var paramGen = ParentQuery.ParameterNameGenerator;

            foreach (var field in FieldsToUpate)
            {
                setClause.Add(string.Format(" {0} = {1} ", field.ColumnName, driverIndicator + paramGen.GetNextParameterName(field.ColumnName) ));
            }

            return string.Join(",", setClause);
        }

        public override string ToString()
        {
            return ToSql();
        }


    }
}
