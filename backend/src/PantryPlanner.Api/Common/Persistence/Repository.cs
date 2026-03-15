using Microsoft.EntityFrameworkCore;

namespace PantryPlanner.Api.Common.Persistence;

public sealed class Repository : IRepository
{
    public Repository(PantryPlannerDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public PantryPlannerDbContext DbContext { get; }

    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
    {
        return DbContext.Set<TEntity>();
    }

    public Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : class
    {
        return DbContext.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        DbContext.Set<TEntity>().Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }
}
