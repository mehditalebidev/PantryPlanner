using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<IntegrationTestFixture>
{
    private const string ApiBasePath = "/api/v1";

    private readonly IntegrationTestFixture _fixture;

    public AuthEndpointsTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Signup_ReturnsAccessToken_ForNewUser()
    {
        var client = _fixture.CreateClient();
        var user = TestUserData.NewUser("signup");

        var signupResponse = await client.PostAsJsonAsync($"{ApiBasePath}/auth/signup", new
        {
            email = user.Email,
            displayName = user.DisplayName,
            password = user.Password
        });

        Assert.Equal(HttpStatusCode.OK, signupResponse.StatusCode);

        var signupPayload = await signupResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(signupPayload);
        Assert.False(string.IsNullOrWhiteSpace(signupPayload.AccessToken));
        Assert.Equal(user.Email, signupPayload.User.Email);
        Assert.Equal(user.DisplayName, signupPayload.User.DisplayName);
    }

    [Fact]
    public async Task Login_And_GetMe_Work_WithSeededUser()
    {
        var client = _fixture.CreateClient();

        var loginResponse = await client.PostAsJsonAsync($"{ApiBasePath}/auth/login", new
        {
            email = TestUserData.Seeded.Email,
            password = TestUserData.Seeded.Password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(loginPayload);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var meResponse = await client.GetAsync($"{ApiBasePath}/users/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePayload = await meResponse.Content.ReadFromJsonAsync<UserResponse>();

        Assert.NotNull(mePayload);
        Assert.Equal(TestUserData.Seeded.Email, mePayload.Email);
        Assert.Equal(TestUserData.Seeded.DisplayName, mePayload.DisplayName);
    }

    [Fact]
    public async Task Signup_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var client = _fixture.CreateClient();

        var duplicateResponse = await client.PostAsJsonAsync($"{ApiBasePath}/auth/signup", new
        {
            email = TestUserData.Seeded.Email,
            displayName = "Second User",
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        Assert.Equal("application/problem+json", duplicateResponse.Content.Headers.ContentType?.MediaType);

        var problem = await duplicateResponse.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal("Email is already in use.", problem.Title);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        var client = _fixture.CreateClient();

        var loginResponse = await client.PostAsJsonAsync($"{ApiBasePath}/auth/login", new
        {
            email = TestUserData.Seeded.Email,
            password = "WrongPass123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

        Assert.Equal("application/problem+json", loginResponse.Content.Headers.ContentType?.MediaType);
    }
}
