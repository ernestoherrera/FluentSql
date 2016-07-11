using Dapper;
using FluentSql.Mappers;
using FluentSql.Support;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluentSql.SqlGenerators.Contracts
{
    public interface IQuery<L> : IToSql
    {
        string Verb { get; }

        SqlGeneratorHelper ParameterNameGenerator { get; }

        DynamicParameters Parameters { get; }

        List<PropertyMap> Fields { get; }

        string TableName { get; }

        string TableAlias { get; }

        string SchemaName { get; }

        string DatabaseName { get; }

        Type EntityType { get; }       

        IQuery<L> JoinOn<R>(Expression<Func<L, R, bool>> expression, JoinType joinType = JoinType.Inner);

        IQuery<L> Where(Expression<Func<L, bool>> expression);

        IQuery<L> Where<R>(Expression<Func<L, R, bool>> expression) where R : new();        

        string ResolveTableAlias(Type type);       
    }
}
