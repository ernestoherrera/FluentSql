using System;
using System.Collections.Generic;
using System.Reflection;

using FluentSql.Support;
using FluentSql.EntityReaders.Contracts;
using FluentSql.Support.Extensions;

namespace FluentSql.EntityReaders
{
    internal class DefaultEntityReader : IEntityReader
    {
        #region IEntityReader implementation
        /// <summary>
        /// It finds all the entities that can be used with a given database table.
        /// </summary>
        /// <param name="entityInterface">This is the Interface that is implemented by the entity type</param>
        /// <param name="assemblySearch">Look for these entities in the given assemblies</param>
        /// <returns></returns>
        public IEnumerable<Type> ReadEntities(Assembly[] assemblySearch)
        {
            if (assemblySearch == null || assemblySearch.Length == 0)
                throw new ArgumentNullException("Entity Interface nor assembly array can not be null.");

            var entityTypes = new List<Type>();

            foreach (var lib in assemblySearch)
            {
                entityTypes.AddRange(lib.GetTypes());
            }

            AfterEntityRead(entityTypes);

            return entityTypes;
        }

        #endregion
        /// <summary>
        /// This method can be overriden in order to modify the entity list
        /// before linking the Table object with The Entity Types.
        /// </summary>
        /// <param name="entityTypes"></param>
        public virtual void AfterEntityRead(IEnumerable<Type> entityTypes)
        {  }
    }
}
