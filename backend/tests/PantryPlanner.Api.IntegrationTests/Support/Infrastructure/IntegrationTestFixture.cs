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
    private string _mediaStorageRoot = string.Empty;

    public TestWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();

        _mediaStorageRoot = Path.Combine(Path.GetTempPath(), "pantryplanner-media-tests", Guid.NewGuid().ToString("N"));

        Factory = new TestWebApplicationFactory(_databaseContainer.GetConnectionString(), _mediaStorageRoot);

        _ = Factory.CreateClient();

        await _dataSeeder.SeedBaselineAsync(Factory.Services);
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        if (!string.IsNullOrWhiteSpace(_mediaStorageRoot) && Directory.Exists(_mediaStorageRoot))
        {
            Directory.Delete(_mediaStorageRoot, recursive: true);
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
