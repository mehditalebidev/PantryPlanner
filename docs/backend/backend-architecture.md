# Backend Architecture

## Current Architecture

The backend currently uses a single ASP.NET Core API project with a vertical-slice layout.

```text
PantryPlanner.Api/
  Common/
    Api/
    Behaviors/
    Persistence/
      Configurations/
      Migrations/
    Results/
    Security/
  Domain/
    GroceryList.cs
    GroceryListItem.cs
    Ingredient.cs
    MealPlan.cs
    MealSlot.cs
    PlannedMeal.cs
    Recipe.cs
    RecipeImport.cs
    RecipeIngredient.cs
    RecipeMediaAsset.cs
    RecipeStep.cs
    RecipeStepIngredientReference.cs
    User.cs
  Features/
    GroceryLists/
      GenerateGroceryList.cs
      GetGroceryList.cs
      GroceryListsController.cs
      Shared/
      UpdateGroceryListItem.cs
    Ingredients/
      CreateIngredient.cs
      DeleteIngredient.cs
      GetIngredient.cs
      IngredientsController.cs
      ListIngredients.cs
      Shared/
      UpdateIngredient.cs
    MealPlans/
      CreateMealPlan.cs
      DeleteMealPlan.cs
      GetMealPlan.cs
      ListMealPlans.cs
      MealPlansController.cs
      Shared/
      UpdateMealPlan.cs
    Media/
      Endpoints/
      DeleteRecipeMedia.cs
      GetMediaContent.cs
      Shared/
      UploadRecipeMedia.cs
    RecipeImports/
      CreateRecipeImport.cs
      GetRecipeImport.cs
      RecipeImportsController.cs
      Shared/
    Recipes/
      CreateRecipe.cs
      DeleteRecipe.cs
      GetRecipe.cs
      ListRecipes.cs
      RecipesController.cs
      Shared/
      UpdateRecipe.cs
    Units/
      ListUnits.cs
      Shared/
    Users/
      Endpoints/
      GetCurrentUser.cs
      Login.cs
      Shared/
      Signup.cs
  Program.cs
```

## Current Technical Choices

- ASP.NET Core Web API
- MediatR for request handling
- FluentValidation for request validation
- EF Core with PostgreSQL
- JWT bearer auth
- ProblemDetails responses
- Scalar/OpenAPI in development
- direct `PantryPlannerDbContext` injection instead of a thin generic repository wrapper

## Architectural Rules

- keep route attributes and shared controller setup in thin partial controller shells; use an `Endpoints/` folder only when a slice has more than one controller shell
- keep each endpoint as a single `.cs` file directly under its feature folder with the action, request, validator, handler, and request-local mapping extensions together
- put business logic in handlers and keep domain entities under the top-level `Domain/` folder
- keep validation close to commands and queries
- use `Common/` only for truly cross-cutting concerns
- keep user ownership explicit in every query and mutation
- inject `PantryPlannerDbContext` directly unless a dedicated shared abstraction adds real behavior
- keep EF entity configuration under `Common/Persistence/Configurations/` instead of feature folders
- keep backend-owned unit normalization in the dedicated `Units` slice so recipe persistence, future planner aggregation, and future grocery generation all share one source of truth
- keep seeded ingredient catalog behavior inside the `Ingredients` slice rather than inside auth or recipes
- keep recipe-import drafts mapped to recipe field names so import review can flow into standard recipe creation instead of a separate save path
- keep binary media storage and protected content delivery inside a dedicated `Media` slice while recipe metadata remains part of the `Recipes` aggregate

## Planned Slices

No additional near-term backend slices are planned beyond the current PantryPlanner MVP set.

## Testing Layout

```text
backend/tests/
  PantryPlanner.Api.UnitTests/
  PantryPlanner.Api.IntegrationTests/
```

- `PantryPlanner.Api.UnitTests`: fast coverage for domain behavior, validation, handlers, security helpers, and result mapping
- `PantryPlanner.Api.IntegrationTests`: endpoint wiring, auth flows, middleware behavior, and PostgreSQL-backed runtime paths
