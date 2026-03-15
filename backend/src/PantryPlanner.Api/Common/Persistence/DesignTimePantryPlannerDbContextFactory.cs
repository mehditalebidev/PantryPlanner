using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PantryPlanner.Api.Common.Persistence;

public sealed class DesignTimePantryPlannerDbContextFactory : IDesignTimeDbContextFactory<PantryPlannerDbContext>
{
    public PantryPlannerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PantryPlannerDatabase")
            ?? "Host=localhost;Port=5432;Database=pantryplanner;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<PantryPlannerDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PantryPlannerDbContext(optionsBuilder.Options);
    }
}
