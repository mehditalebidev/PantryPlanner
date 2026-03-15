using Microsoft.EntityFrameworkCore;

namespace PantryPlanner.Api.Common.Persistence;

public interface IRepository
{
    PantryPlannerDbContext DbContext { get; }

    IQueryable<TEntity> Query<TEntity>() where TEntity : class;

    Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) where TEntity : class;

    void Remove<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
