using System.Linq.Expressions;
using FastExpressionCompiler;
using IbraHabra.NET.Domain.SharedKernel.Interface;
using IbraHabra.NET.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace IbraHabra.NET.Infra.Repo;

public class Repo<TEntity, TKey> : IRepo<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    private readonly DbSet<TEntity> _dbSet;

    public Repo(AppDbContext context)
    {
        _dbSet = context.Set<TEntity>();
    }

    // WRITE : SECTION
    public async Task<TEntity> AddAsync(TEntity entity)
    {
        return (await _dbSet.AddAsync(entity)).Entity;
    }

    public async Task<int> UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression)
    {
        return await _dbSet.Where(predicate).Take(1).ExecuteUpdateAsync(updateExpression);
    }

    public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).Take(1).ExecuteDeleteAsync();
    }

    public async Task BatchAddAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public async Task<int> BatchUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression)
    {
        return await _dbSet.Where(predicate).ExecuteUpdateAsync(updateExpression);
    }

    public async Task<int> BatchDeleteAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ExecuteDeleteAsync();
    }


    // READ : SECTION
    public async Task<TEntity?> GetViaIdAsync(TKey id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<TEntity?> GetViaConditionAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    public async Task<(IEnumerable<TProjection> Items, string? NextCursor)> GetByCursorAsync<TProjection>(
        string? cursor,
        int pageSize,
        Expression<Func<TEntity, object>> orderBy, Expression<Func<TEntity, TProjection>> projection,
        bool ascending = true)
    {
        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));


        if (orderBy.Body is not MemberExpression and not UnaryExpression)
            throw new ArgumentException("The orderBy expression must be a simple property access.", nameof(orderBy));

        var query = CreateCursorQuery(ascending, cursor, orderBy);

        var items = await query.Take(pageSize + 1).ToListAsync();

        string? nextCursor = null;

        if (items.Count > pageSize)
        {
            var lastItem = items[pageSize];
            var lastValue = orderBy.CompileFast()(lastItem);
            nextCursor = lastValue?.ToString();
            items = items.Take(pageSize).ToList();
        }

        var projectedItems = items.AsQueryable().Select(projection).ToList();

        return (projectedItems, nextCursor);
    }

    public async Task<(IEnumerable<TProjection> Items, string? NextCursor)> FindByCursorAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate, string? cursor, int pageSize,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, TProjection>> projection, bool ascending = true)
    {
        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        if (orderBy.Body is not MemberExpression and not UnaryExpression)
            throw new ArgumentException("The orderBy expression must be a simple property access.", nameof(orderBy));

        var query = CreateCursorQueryWithPredicate(predicate, ascending, cursor, orderBy);
        var items = await query.Take(pageSize + 1).ToListAsync();

        string? nextCursor = null;

        if (items.Count > pageSize)
        {
            var lastItem = items[pageSize];
            var lastValue = orderBy.CompileFast()(lastItem);
            nextCursor = lastValue?.ToString();
            items = items.Take(pageSize).ToList();
        }

        var projectedItems = items.AsQueryable().Select(projection).ToList();

        return (projectedItems, nextCursor);
    }

    public async Task<(IEnumerable<TProjection> Items, string? NextCursor)> FindWithIncludesCursorAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate, string? cursor, int pageSize,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, TProjection>> projection, bool ascending = true,
        params Expression<Func<TEntity, object>>[] includes)
    {
        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        if (orderBy.Body is not MemberExpression and not UnaryExpression)
            throw new ArgumentException("The orderBy expression must be a simple property access.", nameof(orderBy));

        var query = CreateCursorQueryWithPredicateAndIncludes(predicate, ascending, cursor, orderBy, includes);
        var items = await query.Take(pageSize + 1).ToListAsync();

        string? nextCursor = null;
        if (items.Count > pageSize)
        {
            var lastItem = items[pageSize];
            var lastValue = orderBy.CompileFast()(lastItem);
            nextCursor = lastValue?.ToString();
            items = items.Take(pageSize).ToList();
        }

        var projectedItems = items.AsQueryable().Select(projection).ToList();

        return (projectedItems, nextCursor);
    }


    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().AnyAsync(predicate);
    }

    public async Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().LongCountAsync(predicate);
    }

    private IQueryable<TEntity> CreateCursorQuery(bool ascending, string? cursor,
        Expression<Func<TEntity, object>> orderBy)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        query = ascending
            ? query.OrderBy(orderBy).ThenBy(e => e.Id)
            : query.OrderByDescending(orderBy).ThenByDescending(e => e.Id);

        if (string.IsNullOrEmpty(cursor)) return query;

        query = ApplyCursorFilter(query, cursor, orderBy, ascending);

        return query;
    }

    private IQueryable<TEntity> CreateCursorQueryWithPredicate(
        Expression<Func<TEntity, bool>> predicate,
        bool ascending,
        string? cursor,
        Expression<Func<TEntity, object>> orderBy)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking().Where(predicate);

        query = ascending
            ? query.OrderBy(orderBy).ThenBy(e => e.Id)
            : query.OrderByDescending(orderBy).ThenByDescending(e => e.Id);

        if (string.IsNullOrEmpty(cursor)) return query;

        query = ApplyCursorFilter(query, cursor, orderBy, ascending);

        return query;
    }

    private IQueryable<TEntity> CreateCursorQueryWithPredicateAndIncludes(
        Expression<Func<TEntity, bool>> predicate,
        bool ascending,
        string? cursor,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet.Where(predicate);

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        query = ascending
            ? query.OrderBy(orderBy).ThenBy(e => e.Id)
            : query.OrderByDescending(orderBy).ThenByDescending(e => e.Id);

        if (string.IsNullOrEmpty(cursor)) return query;

        query = ApplyCursorFilter(query, cursor, orderBy, ascending);

        return query;
    }

    private IQueryable<TEntity> ApplyCursorFilter(
        IQueryable<TEntity> query,
        string cursor,
        Expression<Func<TEntity, object>> orderBy,
        bool ascending)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");

        Expression property = orderBy.Body;

        object convertedCursor = ConvertCursor(cursor, property.Type);
        ConstantExpression constant = Expression.Constant(convertedCursor, property.Type);

        BinaryExpression comparison = ascending
            ? Expression.GreaterThan(property, constant)
            : Expression.LessThan(property, constant);

        var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);

        return query.Where(lambda);
    }

    private static object ConvertCursor(string cursor, Type targetType)
    {
        try
        {
            return Convert.ChangeType(cursor, targetType);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                $"Invalid cursor format: {cursor} cannot be converted to {targetType.Name}", nameof(cursor), ex);
        }
    }
}