using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.Common.Persistence;

public sealed class PantryPlannerDbContext : DbContext
{
    public PantryPlannerDbContext(DbContextOptions<PantryPlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PantryPlannerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
