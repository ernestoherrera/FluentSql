using System;
using System.Collections.Generic;
using System.Reflection;

using FluentSql.Support;
using FluentSql.EntityReaders.Contracts;

namespace FluentSql.EntityReaders
{
    internal class DefaultEntityReader : IEntityReader
    {
        #region IEntityReader implementation
        /// <summary>
        /// It finds all the entities that implement the given interface.
        /// </summary>
        /// <param name="entityInterface">This is the Interface that is implemented by the entity type</param>
        /// <param name="assemblySearch">Look for these entities in the given assemblies</param>
        /// <returns></returns>
        public IEnumerable<Type> ReadEntities(Type entityInterface, Assembly[] assemblySearch)
        {
            if (entityInterface == null || assemblySearch == null || assemblySearch.Length == 0)
                throw new ArgumentNullException("Entity Interface nor assembly array can not be null.");

            var entityTypes = new List<Type>();

            foreach (var lib in assemblySearch)
            {
                entityTypes.AddRange(lib.GetTypes(entityInterface));
            }

            AfterEnityMapping(entityTypes);

            return entityTypes;
        }

        #endregion
        /// <summary>
        /// This method can be overriden in order to modify the entity list
        /// before linking the Table object with The Entity Types.
        /// </summary>
        /// <param name="entityTypes"></param>
        public virtual void AfterEnityMapping(IEnumerable<Type> entityTypes)
        {

        }
    }
}
