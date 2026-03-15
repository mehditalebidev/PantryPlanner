namespace PantryPlanner.Api.IntegrationTests;

public sealed class IntegrationTestDataSeeder
{
    public async Task SeedBaselineAsync(IServiceProvider services)
    {
        await UserTestDataSeeder.SeedBaselineAsync(services);
    }
}
