using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace IbraHabra.NET.Domain.SharedKernel.Interface;

public interface IRepo<TEntity, in TKey>
{
    Task<TEntity?> GetViaIdAsync(TKey id);
    Task<TEntity?> GetViaConditionAsync(Expression<Func<TEntity, bool>> predicate);

    Task<TProjection?> GetViaConditionAsync<TProjection>(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProjection>> projection);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity> AddAsync(TEntity entity);

    Task<int> UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression);

    Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate);

    Task BatchAddAsync(IEnumerable<TEntity> entities);

    Task<int> BatchUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression);

    Task<(IEnumerable<TProjection> Items, string? NextCursor)> GetByCursorAsync<TProjection>(
        string? cursor,
        int pageSize,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, TProjection>> projection,
        bool ascending = true);

    Task<int> BatchDeleteAsync(Expression<Func<TEntity, bool>> predicate);

    Task<(IEnumerable<TProjection> Items, string? NextCursor)> FindByCursorAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        string? cursor,
        int pageSize,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, TProjection>> projection,
        bool ascending = true);


    Task<(IEnumerable<TProjection> Items, string? NextCursor)> FindWithIncludesCursorAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        string? cursor,
        int pageSize,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, TProjection>> projection,
        bool ascending = true,
        params Expression<Func<TEntity, object>>[] includes);
}