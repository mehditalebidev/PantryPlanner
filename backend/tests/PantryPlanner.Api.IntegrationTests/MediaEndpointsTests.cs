using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.Recipes;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.IntegrationTests;

public sealed class MediaEndpointsTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string ApiBasePath = "/api/v1";
    private readonly IntegrationTestFixture _fixture = fixture;

    [Fact]
    public async Task RecipeMediaEndpoints_Upload_Get_Delete_And_EnforceOwnership()
    {
        var ownerClient = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("media-owner"));
        var recipe = await CreateRecipeAsync(ownerClient, "media-owner");

        var uploadResponse = await ownerClient.PostAsync(
            $"{ApiBasePath}/recipes/{recipe.Id}/media",
            CreateUploadContent("image", "plated", 1, "dish.jpg", "image/jpeg", new byte[] { 1, 2, 3, 4 }));

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var media = await uploadResponse.Content.ReadFromJsonAsync<RecipeMediaAssetResponse>();
        Assert.NotNull(media);
        Assert.Equal("image", media.Kind);
        Assert.Equal("image/jpeg", media.ContentType);
        Assert.NotNull(media.StorageKey);
        Assert.StartsWith("recipes/", media.StorageKey, StringComparison.Ordinal);
        Assert.Equal($"/api/v1/media/{media.StorageKey}", media.Url);

        var getRecipeResponse = await ownerClient.GetAsync($"{ApiBasePath}/recipes/{recipe.Id}");
        Assert.Equal(HttpStatusCode.OK, getRecipeResponse.StatusCode);

        var storedRecipe = await getRecipeResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(storedRecipe);
        var storedMedia = Assert.Single(storedRecipe.Media);
        Assert.Equal(media.Id, storedMedia.Id);

        var mediaContentResponse = await ownerClient.GetAsync(media.Url);
        Assert.Equal(HttpStatusCode.OK, mediaContentResponse.StatusCode);
        Assert.Equal("image/jpeg", mediaContentResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, await mediaContentResponse.Content.ReadAsByteArrayAsync());

        var otherClient = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("media-other"));

        var forbiddenGetResponse = await otherClient.GetAsync(media.Url);
        Assert.Equal(HttpStatusCode.NotFound, forbiddenGetResponse.StatusCode);

        var forbiddenDeleteResponse = await otherClient.DeleteAsync($"{ApiBasePath}/recipes/{recipe.Id}/media/{media.Id}");
        Assert.Equal(HttpStatusCode.NotFound, forbiddenDeleteResponse.StatusCode);

        var deleteResponse = await ownerClient.DeleteAsync($"{ApiBasePath}/recipes/{recipe.Id}/media/{media.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedContentResponse = await ownerClient.GetAsync(media.Url);
        Assert.Equal(HttpStatusCode.NotFound, deletedContentResponse.StatusCode);
    }

    [Fact]
    public async Task UploadedMedia_IsCleanedUp_WhenRecipeUpdateOrDeleteRemovesIt()
    {
        var client = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("media-cleanup"));
        var recipe = await CreateRecipeAsync(client, "media-cleanup");
        var ingredient = Assert.Single(recipe.Ingredients);
        var step = Assert.Single(recipe.Steps);

        var firstUploadResponse = await client.PostAsync(
            $"{ApiBasePath}/recipes/{recipe.Id}/media",
            CreateUploadContent("image", "first", 1, "first.jpg", "image/jpeg", new byte[] { 9, 8, 7 }));

        Assert.Equal(HttpStatusCode.OK, firstUploadResponse.StatusCode);

        var firstMedia = await firstUploadResponse.Content.ReadFromJsonAsync<RecipeMediaAssetResponse>();
        Assert.NotNull(firstMedia);

        var updateResponse = await client.PutAsJsonAsync($"{ApiBasePath}/recipes/{recipe.Id}", new
        {
            title = recipe.Title,
            description = recipe.Description,
            servings = recipe.Servings,
            prepTimeMinutes = recipe.PrepTimeMinutes,
            cookTimeMinutes = recipe.CookTimeMinutes,
            sourceUrl = recipe.SourceUrl,
            ingredients = new object[]
            {
                new
                {
                    ingredientId = ingredient.IngredientId,
                    name = (string?)null,
                    referenceKey = ingredient.ReferenceKey,
                    quantity = ingredient.Quantity,
                    unitCode = ingredient.UnitCode,
                    preparationNote = ingredient.PreparationNote,
                    sortOrder = ingredient.SortOrder
                }
            },
            steps = new object[]
            {
                new
                {
                    instruction = step.Instruction,
                    sortOrder = step.SortOrder,
                    durationMinutes = step.DurationMinutes,
                    ingredientReferenceKeys = step.IngredientReferences.Select(reference => reference.ReferenceKey).ToArray()
                }
            },
            media = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedRecipe = await updateResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(updatedRecipe);
        Assert.Empty(updatedRecipe.Media);

        var removedContentResponse = await client.GetAsync(firstMedia.Url);
        Assert.Equal(HttpStatusCode.NotFound, removedContentResponse.StatusCode);

        var secondUploadResponse = await client.PostAsync(
            $"{ApiBasePath}/recipes/{recipe.Id}/media",
            CreateUploadContent("image", "second", 1, "second.jpg", "image/jpeg", new byte[] { 4, 5, 6 }));

        Assert.Equal(HttpStatusCode.OK, secondUploadResponse.StatusCode);

        var secondMedia = await secondUploadResponse.Content.ReadFromJsonAsync<RecipeMediaAssetResponse>();
        Assert.NotNull(secondMedia);

        var deleteRecipeResponse = await client.DeleteAsync($"{ApiBasePath}/recipes/{recipe.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRecipeResponse.StatusCode);

        var deletedRecipeContentResponse = await client.GetAsync(secondMedia.Url);
        Assert.Equal(HttpStatusCode.NotFound, deletedRecipeContentResponse.StatusCode);
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

    private async Task<RecipeResponse> CreateRecipeAsync(HttpClient client, string uniquePrefix)
    {
        var ingredientResponse = await client.PostAsJsonAsync($"{ApiBasePath}/ingredients", new
        {
            name = $"Chicken {uniquePrefix}"
        });

        Assert.Equal(HttpStatusCode.OK, ingredientResponse.StatusCode);

        var ingredient = await ingredientResponse.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(ingredient);

        var createRecipeResponse = await client.PostAsJsonAsync($"{ApiBasePath}/recipes", new
        {
            title = $"Chicken {uniquePrefix}",
            description = "Media test recipe",
            servings = 4,
            prepTimeMinutes = 10,
            cookTimeMinutes = 20,
            sourceUrl = "https://example.com/recipes/media-test",
            ingredients = new object[]
            {
                new
                {
                    ingredientId = ingredient.Id,
                    name = (string?)null,
                    referenceKey = "chicken",
                    quantity = 1.0m,
                    unitCode = "lb",
                    preparationNote = "cubed",
                    sortOrder = 1
                }
            },
            steps = new object[]
            {
                new
                {
                    instruction = "Cook the chicken.",
                    sortOrder = 1,
                    durationMinutes = 20,
                    ingredientReferenceKeys = new[] { "chicken" }
                }
            },
            media = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.OK, createRecipeResponse.StatusCode);

        var recipe = await createRecipeResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        return recipe;
    }

    private static MultipartFormDataContent CreateUploadContent(
        string kind,
        string caption,
        int sortOrder,
        string fileName,
        string contentType,
        byte[] bytes)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(kind), "kind");
        content.Add(new StringContent(caption), "caption");
        content.Add(new StringContent(sortOrder.ToString()), "sortOrder");

        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        content.Add(fileContent, "file", fileName);

        return content;
    }
}
