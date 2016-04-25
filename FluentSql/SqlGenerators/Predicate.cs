using FluentSql.SqlGenerators.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace FluentSql.SqlGenerators
{
    public class Predicate<T> : IToSql , IDisposable
    {
        protected List<PredicateUnit> Predicates;

        protected IQuery<T> ParentQuery;

        public Predicate(IQuery<T> parentQuery) 
        {
            Predicates = new List<PredicateUnit>();
            ParentQuery = parentQuery;
        }

        public void Add(PredicateUnit predicate)
        {
            Predicates.Add(predicate);
        }

        public int Count()
        {
            return Predicates.Count();
        }

        public PredicateUnit this[int index]
        {
            get { return Predicates[index]; }
        }

        public IEnumerator<PredicateUnit> GetEnumerator()
        {
            return Predicates.GetEnumerator();
        }

        #region ITSql Implementation
        public virtual string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var iter = Predicates.GetEnumerator();

            while(iter.MoveNext())
            {
                var item = iter.Current as PredicateUnit;

                if(item.LeftOperandType.IsValueType)
                {
                    sqlBuilder.AppendFormat("{0} @{1} ", string.IsNullOrEmpty(item.Link) ? "" : item.Link,
                                                            item.LeftOperand);
                }
                else
                {
                    sqlBuilder.AppendFormat("{0} {1}.{2} ", string.IsNullOrEmpty(item.Link) ? "" : item.Link,
                                                            ParentQuery.ResolveTableAlias(item.LeftOperandType),
                                                            item.LeftOperand);
                }

                sqlBuilder.AppendFormat("{0}", item.Operator);

                if (item.RightOperandType.IsValueType)
                {
                    sqlBuilder.AppendFormat(" @{1} ", item.RightOperand);
                }
                else
                {
                    sqlBuilder.AppendFormat("{1}.{2} ", ParentQuery.ResolveTableAlias(item.RightOperandType),
                                                        item.RightOperand);
                }                
            }

            return sqlBuilder.ToString();
        }

        public void Clear()
        {
            Predicates.Clear();
        }

        public void Dispose()
        {
            Clear();            
        }

        #endregion
    }
}
