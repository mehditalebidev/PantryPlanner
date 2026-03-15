using Testcontainers.PostgreSql;

namespace PantryPlanner.Api.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("pantryplanner_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    private readonly IntegrationTestDataSeeder _dataSeeder = new();

    public TestWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();

        Factory = new TestWebApplicationFactory(_databaseContainer.GetConnectionString());

        _ = Factory.CreateClient();

        await _dataSeeder.SeedBaselineAsync(Factory.Services);
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await _databaseContainer.DisposeAsync();
    }

    public HttpClient CreateClient()
    {
        if (Factory is null)
        {
            throw new InvalidOperationException("The integration test fixture has not been initialized.");
        }

        return Factory.CreateClient();
    }
}
