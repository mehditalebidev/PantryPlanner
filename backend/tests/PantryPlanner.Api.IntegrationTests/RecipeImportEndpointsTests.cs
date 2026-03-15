using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryPlanner.Api.Features.RecipeImports;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.IntegrationTests;

public sealed class RecipeImportEndpointsTests : IClassFixture<IntegrationTestFixture>
{
    private const string ApiBasePath = "/api/v1";

    private readonly IntegrationTestFixture _fixture;

    public RecipeImportEndpointsTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RecipeImportEndpoints_CreateAndGet_ReturnReviewableDraft()
    {
        var client = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("recipe-import"));
        const string sourceUrl = "https://example.com/recipes/sheet-pan-chicken";

        var createResponse = await client.PostAsJsonAsync($"{ApiBasePath}/recipe-imports", new
        {
            sourceUrl
        });

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var createdImport = await createResponse.Content.ReadFromJsonAsync<RecipeImportResponse>();
        Assert.NotNull(createdImport);
        Assert.Equal(RecipeImportSourceTypes.Url, createdImport.SourceType);
        Assert.Equal(RecipeImportStatuses.NeedsReview, createdImport.Status);
        Assert.Equal(sourceUrl, createdImport.SourceUrl);
        Assert.Equal("Sheet Pan Chicken", createdImport.Draft.Title);
        Assert.Equal(sourceUrl, createdImport.Draft.SourceUrl);
        Assert.Empty(createdImport.Draft.Ingredients);
        Assert.Empty(createdImport.Draft.Steps);
        Assert.Empty(createdImport.Draft.Media);
        Assert.Contains(RecipeImportWarnings.ReviewRequired, createdImport.Warnings);

        var getResponse = await client.GetAsync($"{ApiBasePath}/recipe-imports/{createdImport.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedImport = await getResponse.Content.ReadFromJsonAsync<RecipeImportResponse>();
        Assert.NotNull(fetchedImport);
        Assert.Equal(createdImport.Id, fetchedImport.Id);
        Assert.Equal("Sheet Pan Chicken", fetchedImport.Draft.Title);
    }

    [Fact]
    public async Task RecipeImportEndpoint_ReturnsNotFound_ForOtherAuthenticatedUser()
    {
        var ownerClient = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("recipe-import-owner"));

        var createResponse = await ownerClient.PostAsJsonAsync($"{ApiBasePath}/recipe-imports", new
        {
            sourceUrl = "https://example.com/recipes/tomato-soup"
        });

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var createdImport = await createResponse.Content.ReadFromJsonAsync<RecipeImportResponse>();
        Assert.NotNull(createdImport);

        var otherClient = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("recipe-import-boundary"));
        var getResponse = await otherClient.GetAsync($"{ApiBasePath}/recipe-imports/{createdImport.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClientForNewUserAsync(TestUserData user)
    {
        var client = _fixture.CreateClient();
        var signupResponse = await client.PostAsJsonAsync($"{ApiBasePath}/auth/signup", new
        {
            email = user.Email,
            displayName = user.DisplayName,
            password = user.Password
        });

        Assert.Equal(HttpStatusCode.OK, signupResponse.StatusCode);

        var signupPayload = await signupResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(signupPayload);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signupPayload.AccessToken);
        return client;
    }
}
