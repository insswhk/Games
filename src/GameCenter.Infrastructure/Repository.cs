using GameCenter.Application;
using GameCenter.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameCenter.Infrastructure;

public sealed class EfRepository<TEntity>(GameCenterDbContext dbContext) : IRepository<TEntity> where TEntity : Entity
{
    public IQueryable<TEntity> Query() => dbContext.Set<TEntity>().AsQueryable();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();

    public void Remove(TEntity entity)
    {
        entity.IsDeleted = true;
        dbContext.Set<TEntity>().Update(entity);
    }
}

public sealed class EfUnitOfWork(GameCenterDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfAppTransaction(transaction);
    }
}

public sealed class EfAppTransaction(IDbContextTransaction transaction) : IAppTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default) =>
        transaction.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
