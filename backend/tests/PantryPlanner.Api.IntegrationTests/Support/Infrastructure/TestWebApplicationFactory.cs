using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PantryPlanner.Api.IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _mediaStorageRoot;

    public TestWebApplicationFactory(string connectionString, string mediaStorageRoot)
    {
        _connectionString = connectionString;
        _mediaStorageRoot = mediaStorageRoot;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PantryPlannerDatabase"] = _connectionString,
                ["Media:StorageRoot"] = _mediaStorageRoot
            });
        });
    }
}
