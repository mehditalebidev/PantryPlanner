using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.Recipes;
using PantryPlanner.Api.Features.Units;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.IntegrationTests;

public sealed class RecipeEndpointsTests : IClassFixture<IntegrationTestFixture>
{
    private const string ApiBasePath = "/api/v1";

    private readonly IntegrationTestFixture _fixture;

    public RecipeEndpointsTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RecipeCrud_Works_WithStructuredIngredientReferences_AndNormalizedUnits()
    {
        var client = await CreateAuthenticatedClientForSeededUserAsync();
        var uniquePrefix = Guid.NewGuid().ToString("N");

        var createIngredientResponse = await client.PostAsJsonAsync($"{ApiBasePath}/ingredients", new
        {
            name = $"Paprika {uniquePrefix}"
        });

        Assert.Equal(HttpStatusCode.OK, createIngredientResponse.StatusCode);

        var ingredientPayload = await createIngredientResponse.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(ingredientPayload);

        var createRecipeResponse = await client.PostAsJsonAsync($"{ApiBasePath}/recipes", new
        {
            title = $"Sheet Pan Chicken {uniquePrefix}",
            description = "Weeknight dinner",
            servings = 4,
            prepTimeMinutes = 15,
            cookTimeMinutes = 30,
            sourceUrl = "https://example.com/recipe",
            ingredients = new object[]
            {
                new
                {
                    ingredientId = ingredientPayload.Id,
                    name = (string?)null,
                    referenceKey = "paprika",
                    quantity = 2.0m,
                    unitCode = "tbsp",
                    preparationNote = (string?)null,
                    sortOrder = 1
                },
                new
                {
                    ingredientId = (Guid?)null,
                    name = $"Chicken Thighs {uniquePrefix}",
                    referenceKey = "chicken",
                    quantity = 2.0m,
                    unitCode = "lb",
                    preparationNote = "skin-on",
                    sortOrder = 2
                }
            },
            steps = new object[]
            {
                new
                {
                    instruction = "Season the chicken with paprika.",
                    sortOrder = 1,
                    durationMinutes = 5,
                    ingredientReferenceKeys = new[] { "paprika", "chicken" }
                },
                new
                {
                    instruction = "Roast until done.",
                    sortOrder = 2,
                    durationMinutes = 30,
                    ingredientReferenceKeys = new[] { "chicken" }
                }
            },
            media = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.OK, createRecipeResponse.StatusCode);

        var createdRecipe = await createRecipeResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(createdRecipe);
        Assert.Equal(2, createdRecipe.Ingredients.Count);
        Assert.Equal(29.57353m, createdRecipe.Ingredients.Single(ingredient => ingredient.ReferenceKey == "paprika").NormalizedQuantity);
        Assert.Equal("ml", createdRecipe.Ingredients.Single(ingredient => ingredient.ReferenceKey == "paprika").NormalizedUnitCode);
        Assert.Equal(907.18474m, createdRecipe.Ingredients.Single(ingredient => ingredient.ReferenceKey == "chicken").NormalizedQuantity);
        Assert.Equal("g", createdRecipe.Ingredients.Single(ingredient => ingredient.ReferenceKey == "chicken").NormalizedUnitCode);
        Assert.Equal(2, createdRecipe.Steps.Single(step => step.SortOrder == 1).IngredientReferences.Count);

        var getRecipeResponse = await client.GetAsync($"{ApiBasePath}/recipes/{createdRecipe.Id}");
        Assert.Equal(HttpStatusCode.OK, getRecipeResponse.StatusCode);

        var recipeFromGet = await getRecipeResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipeFromGet);
        Assert.Equal(createdRecipe.Id, recipeFromGet.Id);

        var listRecipesResponse = await client.GetAsync($"{ApiBasePath}/recipes");
        Assert.Equal(HttpStatusCode.OK, listRecipesResponse.StatusCode);

        var recipeList = await listRecipesResponse.Content.ReadFromJsonAsync<RecipeResponse[]>();
        Assert.NotNull(recipeList);
        Assert.Contains(recipeList, recipe => recipe.Id == createdRecipe.Id);

        var updatedChickenIngredient = createdRecipe.Ingredients.Single(ingredient => ingredient.ReferenceKey == "chicken");

        var updateRecipeResponse = await client.PutAsJsonAsync($"{ApiBasePath}/recipes/{createdRecipe.Id}", new
        {
            title = $"Updated Sheet Pan Chicken {uniquePrefix}",
            description = "Updated",
            servings = 6,
            prepTimeMinutes = 20,
            cookTimeMinutes = 35,
            sourceUrl = "https://example.com/updated",
            ingredients = new object[]
            {
                new
                {
                    ingredientId = updatedChickenIngredient.IngredientId,
                    name = (string?)null,
                    referenceKey = "chicken",
                    quantity = 500.0m,
                    unitCode = "g",
                    preparationNote = "trimmed",
                    sortOrder = 1
                }
            },
            steps = new object[]
            {
                new
                {
                    instruction = "Cook the chicken.",
                    sortOrder = 1,
                    durationMinutes = 35,
                    ingredientReferenceKeys = new[] { "chicken" }
                }
            },
            media = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.OK, updateRecipeResponse.StatusCode);

        var updatedRecipe = await updateRecipeResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(updatedRecipe);
        Assert.Equal("Updated", updatedRecipe.Description);
        Assert.Single(updatedRecipe.Ingredients);
        Assert.Equal(500.0m, updatedRecipe.Ingredients.Single().NormalizedQuantity);
        Assert.Empty(updatedRecipe.Media);

        var deleteRecipeResponse = await client.DeleteAsync($"{ApiBasePath}/recipes/{createdRecipe.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRecipeResponse.StatusCode);

        var missingRecipeResponse = await client.GetAsync($"{ApiBasePath}/recipes/{createdRecipe.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingRecipeResponse.StatusCode);
    }

    [Fact]
    public async Task RecipeEndpoint_ReturnsNotFound_ForOtherAuthenticatedUser()
    {
        var seededClient = await CreateAuthenticatedClientForSeededUserAsync();
        var uniquePrefix = Guid.NewGuid().ToString("N");

        var createRecipeResponse = await seededClient.PostAsJsonAsync($"{ApiBasePath}/recipes", new
        {
            title = $"Private Recipe {uniquePrefix}",
            servings = 2,
            ingredients = new object[]
            {
                new
                {
                    ingredientId = (Guid?)null,
                    name = $"Salt {uniquePrefix}",
                    referenceKey = "salt",
                    quantity = 1.0m,
                    unitCode = "pinch",
                    sortOrder = 1
                }
            },
            steps = new object[]
            {
                new
                {
                    instruction = "Season to taste.",
                    sortOrder = 1,
                    ingredientReferenceKeys = new[] { "salt" }
                }
            },
            media = Array.Empty<object>()
        });

        var createdRecipe = await createRecipeResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(createdRecipe);

        var otherUser = TestUserData.NewUser("recipe-owner-boundary");
        var otherClient = await CreateAuthenticatedClientForNewUserAsync(otherUser);

        var getRecipeResponse = await otherClient.GetAsync($"{ApiBasePath}/recipes/{createdRecipe.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getRecipeResponse.StatusCode);
    }

    [Fact]
    public async Task IngredientAndUnitEndpoints_ReturnExpectedResponses()
    {
        var client = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("ingredient-catalog"));
        var uniquePrefix = Guid.NewGuid().ToString("N");

        var listSeededIngredientsResponse = await client.GetAsync($"{ApiBasePath}/ingredients");
        Assert.Equal(HttpStatusCode.OK, listSeededIngredientsResponse.StatusCode);

        var seededIngredients = await listSeededIngredientsResponse.Content.ReadFromJsonAsync<IngredientResponse[]>();
        Assert.NotNull(seededIngredients);
        Assert.True(seededIngredients.Length >= DefaultIngredientCatalog.All.Count);
        Assert.Contains(seededIngredients, ingredient => ingredient.Name == "Salt");
        Assert.Contains(seededIngredients, ingredient => ingredient.Name == "Olive oil");

        var seededSalt = seededIngredients.Single(ingredient => ingredient.Name == "Salt");
        var getSeededIngredientResponse = await client.GetAsync($"{ApiBasePath}/ingredients/{seededSalt.Id}");
        Assert.Equal(HttpStatusCode.OK, getSeededIngredientResponse.StatusCode);

        var seededSaltPayload = await getSeededIngredientResponse.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(seededSaltPayload);
        Assert.Equal("salt", seededSaltPayload.NormalizedName);

        var createIngredientResponse = await client.PostAsJsonAsync($"{ApiBasePath}/ingredients", new
        {
            name = $"Garlic {uniquePrefix}"
        });

        Assert.Equal(HttpStatusCode.OK, createIngredientResponse.StatusCode);

        var duplicateIngredientResponse = await client.PostAsJsonAsync($"{ApiBasePath}/ingredients", new
        {
            name = $" garlic {uniquePrefix} "
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicateIngredientResponse.StatusCode);

        var duplicateProblem = await duplicateIngredientResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(duplicateProblem);
        Assert.Equal("Ingredient name is already in use.", duplicateProblem.Title);

        var createdIngredient = await createIngredientResponse.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(createdIngredient);

        var getCreatedIngredientResponse = await client.GetAsync($"{ApiBasePath}/ingredients/{createdIngredient.Id}");
        Assert.Equal(HttpStatusCode.OK, getCreatedIngredientResponse.StatusCode);

        var updateIngredientResponse = await client.PutAsJsonAsync($"{ApiBasePath}/ingredients/{createdIngredient.Id}", new
        {
            name = $"Roasted Garlic {uniquePrefix}"
        });

        Assert.Equal(HttpStatusCode.OK, updateIngredientResponse.StatusCode);

        var updatedIngredient = await updateIngredientResponse.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(updatedIngredient);
        Assert.Equal($"Roasted Garlic {uniquePrefix}", updatedIngredient.Name);

        var deleteIngredientResponse = await client.DeleteAsync($"{ApiBasePath}/ingredients/{createdIngredient.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteIngredientResponse.StatusCode);

        var missingIngredientResponse = await client.GetAsync($"{ApiBasePath}/ingredients/{createdIngredient.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingIngredientResponse.StatusCode);

        var recipeIngredientResponse = await client.PostAsJsonAsync($"{ApiBasePath}/ingredients", new
        {
            name = $"Chili Crisp {uniquePrefix}"
        });

        Assert.Equal(HttpStatusCode.OK, recipeIngredientResponse.StatusCode);

        var recipeIngredient = await recipeIngredientResponse.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(recipeIngredient);

        var createRecipeResponse = await client.PostAsJsonAsync($"{ApiBasePath}/recipes", new
        {
            title = $"Crisp Eggs {uniquePrefix}",
            servings = 2,
            ingredients = new object[]
            {
                new
                {
                    ingredientId = recipeIngredient.Id,
                    name = (string?)null,
                    referenceKey = "chili-crisp",
                    quantity = 1.0m,
                    unitCode = "tbsp",
                    sortOrder = 1
                }
            },
            steps = new object[]
            {
                new
                {
                    instruction = "Spoon chili crisp over eggs.",
                    sortOrder = 1,
                    ingredientReferenceKeys = new[] { "chili-crisp" }
                }
            },
            media = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.OK, createRecipeResponse.StatusCode);

        var deleteProtectedIngredientResponse = await client.DeleteAsync($"{ApiBasePath}/ingredients/{recipeIngredient.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteProtectedIngredientResponse.StatusCode);

        var protectedProblem = await deleteProtectedIngredientResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(protectedProblem);
        Assert.Equal("Ingredient is still used by recipes.", protectedProblem.Title);

        var listIngredientsResponse = await client.GetAsync($"{ApiBasePath}/ingredients");
        Assert.Equal(HttpStatusCode.OK, listIngredientsResponse.StatusCode);

        var ingredients = await listIngredientsResponse.Content.ReadFromJsonAsync<IngredientResponse[]>();
        Assert.NotNull(ingredients);
        Assert.Contains(ingredients, ingredient => ingredient.Id == recipeIngredient.Id);

        var listUnitsResponse = await client.GetAsync($"{ApiBasePath}/units");
        Assert.Equal(HttpStatusCode.OK, listUnitsResponse.StatusCode);

        var units = await listUnitsResponse.Content.ReadFromJsonAsync<UnitDefinitionResponse[]>();
        Assert.NotNull(units);
        Assert.Contains(units, unit => unit.Code == "g" && unit.Family == "mass" && unit.IsConvertible);
        Assert.Contains(units, unit => unit.Code == "pinch" && !unit.IsConvertible);
    }

    private async Task<HttpClient> CreateAuthenticatedClientForSeededUserAsync()
    {
        var client = _fixture.CreateClient();
        var loginPayload = await LoginAsync(client, TestUserData.Seeded);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);
        return client;
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

    private static async Task<AuthResponse> LoginAsync(HttpClient client, TestUserData user)
    {
        var loginResponse = await client.PostAsJsonAsync($"{ApiBasePath}/auth/login", new
        {
            email = user.Email,
            password = user.Password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginPayload);
        return loginPayload;
    }
}
