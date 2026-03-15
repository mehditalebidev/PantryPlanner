using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Features.GroceryLists;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.MealPlans;
using PantryPlanner.Api.Features.Recipes;
using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.IntegrationTests;

public sealed class MealPlanAndGroceryEndpointsTests : IClassFixture<IntegrationTestFixture>
{
    private const string ApiBasePath = "/api/v1";

    private readonly IntegrationTestFixture _fixture;

    public MealPlanAndGroceryEndpointsTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MealPlanCrud_Works_And_BlocksRecipeDeletionWhileScheduled()
    {
        var client = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("meal-plan-crud"));
        var uniquePrefix = Guid.NewGuid().ToString("N");

        var ingredientResponse = await client.PostAsJsonAsync($"{ApiBasePath}/ingredients", new
        {
            name = $"Chicken {uniquePrefix}"
        });
        Assert.Equal(HttpStatusCode.OK, ingredientResponse.StatusCode);

        var ingredient = await ingredientResponse.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(ingredient);

        var recipeResponse = await client.PostAsJsonAsync($"{ApiBasePath}/recipes", new
        {
            title = $"Roast Chicken {uniquePrefix}",
            servings = 4,
            ingredients = new object[]
            {
                new
                {
                    ingredientId = ingredient.Id,
                    name = (string?)null,
                    referenceKey = "chicken",
                    quantity = 2.0m,
                    unitCode = "lb",
                    sortOrder = 1
                }
            },
            steps = new object[]
            {
                new
                {
                    instruction = "Roast the chicken.",
                    sortOrder = 1,
                    ingredientReferenceKeys = new[] { "chicken" }
                }
            },
            media = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.OK, recipeResponse.StatusCode);

        var recipe = await recipeResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);

        var createMealPlanResponse = await client.PostAsJsonAsync($"{ApiBasePath}/meal-plans", new
        {
            title = $"Week of March 16 {uniquePrefix}",
            startDate = "2026-03-16",
            endDate = "2026-03-22",
            slots = new object[]
            {
                new
                {
                    referenceKey = "dinner",
                    name = "Dinner",
                    sortOrder = 1,
                    isDefault = true
                },
                new
                {
                    referenceKey = "snack",
                    name = "Snack",
                    sortOrder = 2,
                    isDefault = false
                }
            },
            entries = new object[]
            {
                new
                {
                    plannedDate = "2026-03-16",
                    mealSlotReferenceKey = "dinner",
                    recipeId = recipe.Id,
                    servingsOverride = 6,
                    note = "Use leftovers"
                }
            }
        });
        Assert.Equal(HttpStatusCode.OK, createMealPlanResponse.StatusCode);

        var createdMealPlan = await createMealPlanResponse.Content.ReadFromJsonAsync<MealPlanResponse>();
        Assert.NotNull(createdMealPlan);
        Assert.Equal(2, createdMealPlan.Slots.Count);
        Assert.Single(createdMealPlan.Entries);
        Assert.Equal("dinner", createdMealPlan.Entries.Single().MealSlotReferenceKey);
        Assert.Equal(recipe.Title, createdMealPlan.Entries.Single().RecipeTitle);

        var getMealPlanResponse = await client.GetAsync($"{ApiBasePath}/meal-plans/{createdMealPlan.Id}");
        Assert.Equal(HttpStatusCode.OK, getMealPlanResponse.StatusCode);

        var listMealPlansResponse = await client.GetAsync($"{ApiBasePath}/meal-plans");
        Assert.Equal(HttpStatusCode.OK, listMealPlansResponse.StatusCode);

        var mealPlans = await listMealPlansResponse.Content.ReadFromJsonAsync<MealPlanResponse[]>();
        Assert.NotNull(mealPlans);
        Assert.Contains(mealPlans, mealPlan => mealPlan.Id == createdMealPlan.Id);

        var updateMealPlanResponse = await client.PutAsJsonAsync($"{ApiBasePath}/meal-plans/{createdMealPlan.Id}", new
        {
            title = $"Updated Week {uniquePrefix}",
            startDate = "2026-03-16",
            endDate = "2026-03-23",
            slots = new object[]
            {
                new
                {
                    referenceKey = "dinner",
                    name = "Dinner",
                    sortOrder = 1,
                    isDefault = true
                }
            },
            entries = new object[]
            {
                new
                {
                    plannedDate = "2026-03-17",
                    mealSlotReferenceKey = "dinner",
                    recipeId = recipe.Id,
                    servingsOverride = 2,
                    note = "Scaled down"
                }
            }
        });
        Assert.Equal(HttpStatusCode.OK, updateMealPlanResponse.StatusCode);

        var updatedMealPlan = await updateMealPlanResponse.Content.ReadFromJsonAsync<MealPlanResponse>();
        Assert.NotNull(updatedMealPlan);
        Assert.Equal($"Updated Week {uniquePrefix}", updatedMealPlan.Title);
        Assert.Single(updatedMealPlan.Slots);
        Assert.Single(updatedMealPlan.Entries);
        Assert.Equal("Scaled down", updatedMealPlan.Entries.Single().Note);

        var deleteRecipeWhileScheduledResponse = await client.DeleteAsync($"{ApiBasePath}/recipes/{recipe.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteRecipeWhileScheduledResponse.StatusCode);

        var recipeConflict = await deleteRecipeWhileScheduledResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(recipeConflict);
        Assert.Equal("Recipe is still used by meal plans.", recipeConflict.Title);

        var deleteMealPlanResponse = await client.DeleteAsync($"{ApiBasePath}/meal-plans/{createdMealPlan.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteMealPlanResponse.StatusCode);

        var missingMealPlanResponse = await client.GetAsync($"{ApiBasePath}/meal-plans/{createdMealPlan.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingMealPlanResponse.StatusCode);

        var deleteRecipeResponse = await client.DeleteAsync($"{ApiBasePath}/recipes/{recipe.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRecipeResponse.StatusCode);
    }

    [Fact]
    public async Task GroceryGeneration_Works_WithNormalization_Checkoff_AndOwnershipBoundaries()
    {
        var client = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("grocery-generation"));
        var uniquePrefix = Guid.NewGuid().ToString("N");

        var chickenIngredient = await CreateIngredientAsync(client, $"Chicken thighs {uniquePrefix}");
        var saltIngredient = await CreateIngredientAsync(client, $"Salt flakes {uniquePrefix}");

        var firstRecipe = await CreateRecipeAsync(
            client,
            $"Chicken Traybake {uniquePrefix}",
            4,
            new object[]
            {
                new
                {
                    ingredientId = chickenIngredient.Id,
                    name = (string?)null,
                    referenceKey = "chicken",
                    quantity = 2.0m,
                    unitCode = "lb",
                    sortOrder = 1
                },
                new
                {
                    ingredientId = saltIngredient.Id,
                    name = (string?)null,
                    referenceKey = "salt",
                    quantity = 1.0m,
                    unitCode = "pinch",
                    sortOrder = 2
                }
            });

        var secondRecipe = await CreateRecipeAsync(
            client,
            $"Chicken Rice Bowl {uniquePrefix}",
            2,
            new object[]
            {
                new
                {
                    ingredientId = chickenIngredient.Id,
                    name = (string?)null,
                    referenceKey = "chicken",
                    quantity = 500.0m,
                    unitCode = "g",
                    sortOrder = 1
                }
            });

        var mealPlanResponse = await client.PostAsJsonAsync($"{ApiBasePath}/meal-plans", new
        {
            title = $"Meal Plan {uniquePrefix}",
            startDate = "2026-03-16",
            endDate = "2026-03-22",
            slots = new object[]
            {
                new
                {
                    referenceKey = "lunch",
                    name = "Lunch",
                    sortOrder = 1,
                    isDefault = true
                },
                new
                {
                    referenceKey = "dinner",
                    name = "Dinner",
                    sortOrder = 2,
                    isDefault = true
                }
            },
            entries = new object[]
            {
                new
                {
                    plannedDate = "2026-03-16",
                    mealSlotReferenceKey = "dinner",
                    recipeId = firstRecipe.Id,
                    servingsOverride = 2,
                    note = "Half batch"
                },
                new
                {
                    plannedDate = "2026-03-17",
                    mealSlotReferenceKey = "lunch",
                    recipeId = secondRecipe.Id,
                    servingsOverride = (int?)null,
                    note = (string?)null
                }
            }
        });
        Assert.Equal(HttpStatusCode.OK, mealPlanResponse.StatusCode);

        var mealPlan = await mealPlanResponse.Content.ReadFromJsonAsync<MealPlanResponse>();
        Assert.NotNull(mealPlan);

        var generateGroceryListResponse = await client.PostAsJsonAsync($"{ApiBasePath}/grocery-lists/generate", new
        {
            mealPlanId = mealPlan.Id
        });
        Assert.Equal(HttpStatusCode.OK, generateGroceryListResponse.StatusCode);

        var groceryList = await generateGroceryListResponse.Content.ReadFromJsonAsync<GroceryListResponse>();
        Assert.NotNull(groceryList);
        Assert.Equal(mealPlan.Id, groceryList.MealPlanId);

        var chickenItem = Assert.Single(groceryList.Items, item => item.IngredientId == chickenIngredient.Id);
        Assert.Equal(953.59237m, chickenItem.Quantity);
        Assert.Equal("g", chickenItem.UnitCode);
        Assert.Equal(2, chickenItem.SourceCount);

        var saltItem = Assert.Single(groceryList.Items, item => item.IngredientId == saltIngredient.Id);
        Assert.Equal(0.5m, saltItem.Quantity);
        Assert.Equal("pinch", saltItem.UnitCode);
        Assert.False(saltItem.IsChecked);

        var getGroceryListResponse = await client.GetAsync($"{ApiBasePath}/grocery-lists/{groceryList.Id}");
        Assert.Equal(HttpStatusCode.OK, getGroceryListResponse.StatusCode);

        var updateItemResponse = await client.PutAsJsonAsync($"{ApiBasePath}/grocery-lists/{groceryList.Id}/items/{saltItem.Id}", new
        {
            isChecked = true
        });
        Assert.Equal(HttpStatusCode.OK, updateItemResponse.StatusCode);

        var updatedGroceryList = await updateItemResponse.Content.ReadFromJsonAsync<GroceryListResponse>();
        Assert.NotNull(updatedGroceryList);
        Assert.True(updatedGroceryList.Items.Single(item => item.Id == saltItem.Id).IsChecked);

        var otherClient = await CreateAuthenticatedClientForNewUserAsync(TestUserData.NewUser("grocery-boundary"));
        var otherUserGetResponse = await otherClient.GetAsync($"{ApiBasePath}/grocery-lists/{groceryList.Id}");
        Assert.Equal(HttpStatusCode.NotFound, otherUserGetResponse.StatusCode);
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

    private static async Task<IngredientResponse> CreateIngredientAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync($"{ApiBasePath}/ingredients", new { name });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ingredient = await response.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(ingredient);
        return ingredient;
    }

    private static async Task<RecipeResponse> CreateRecipeAsync(HttpClient client, string title, int servings, object[] ingredients)
    {
        var response = await client.PostAsJsonAsync($"{ApiBasePath}/recipes", new
        {
            title,
            servings,
            ingredients,
            steps = new object[]
            {
                new
                {
                    instruction = "Cook everything.",
                    sortOrder = 1,
                    ingredientReferenceKeys = ingredients.Select(ingredient => (string)ingredient.GetType().GetProperty("referenceKey")!.GetValue(ingredient)!).ToArray()
                }
            },
            media = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var recipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        return recipe;
    }
}
