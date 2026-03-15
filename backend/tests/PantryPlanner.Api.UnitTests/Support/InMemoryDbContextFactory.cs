using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.UnitTests.Support;

internal static class InMemoryDbContextFactory
{
    public static PantryPlannerDbContext Create()
    {
        var options = new DbContextOptionsBuilder<PantryPlannerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PantryPlannerDbContext(options);
    }
}
