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

        IQuery<L> JoinOn<T1, T2>(Expression<Func<T1, T2, bool>> expression, JoinType joinType = JoinType.Inner);

        IQuery<L> Where(Expression<Func<L, bool>> expression);

        IQuery<L> Where<T>(Expression<Func<T, bool>> expression) where T : new();

        IQuery<L> Where<T1, T2>(Expression<Func<T1, T2, bool>> expression) where T1 : new() where T2 : new();

        IQuery<L> Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression) 
            where T1 : new() 
            where T2 : new()
            where T3 : new();

        IQuery<L> Where<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression)
            where T1 : new()
            where T2 : new()
            where T3 : new()
            where T4 : new();

        IQuery<L> GetTopRows(int topNumberOfRows);

        IQuery<L> OrderBy(Expression<Func<L, object>> expression);

        IQuery<L> OrderBy<T>(Expression<Func<T, object>> expression) where T : new();

        IQuery<L> OrderByDescending<T>(Expression<Func<T, object>> expression) where T : new();

        IQuery<L> OrderByDescending(Expression<Func<L, object>> expression);

        IQuery<L> OrderBy(List<SortOrderField> sortOrderFields);

        IQuery<L> OrderBy(params SortOrderField[] sortOrderArray);

        string ResolveTableAlias(Type type);
    }
}
