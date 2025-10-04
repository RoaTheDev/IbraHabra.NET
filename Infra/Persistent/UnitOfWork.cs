using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Infra.Repo;
using Microsoft.EntityFrameworkCore.Storage;

namespace IbraHabra.NET.Infra.Persistent;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repoDict;
    private IDbContextTransaction? _currentTransaction;
    public IDbContextTransaction? CurrentTransaction => _currentTransaction;
    private bool _disposed;
    public AppDbContext DbContext => _context; 
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        _repoDict = new Dictionary<Type, object>();
    }


    public IRepo<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class, IEntity<TKey>
    {
        Type entityType = typeof(TEntity);
        if (_repoDict.TryGetValue(entityType, out var repo))
            return (IRepo<TEntity, TKey>)repo;

        IRepo<TEntity, TKey> repository = new Repo<TEntity, TKey>(_context);
        _repoDict[entityType] = repository;
        return repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No transaction is currently active.");

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No transaction is currently active.");

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _currentTransaction?.Dispose();
            _repoDict.Clear();
            _context.Dispose();
            _disposed = true;
        }
    }
}