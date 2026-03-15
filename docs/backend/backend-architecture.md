# Backend Architecture

## Current Architecture

The backend currently uses a single ASP.NET Core API project with a vertical-slice layout.

```text
PantryPlanner.Api/
  Common/
    Api/
    Behaviors/
    Persistence/
    Results/
    Security/
  Features/
    GroceryLists/
      Endpoints/
      GenerateGroceryList/
      GetGroceryList/
      Persistence/
      Shared/
      UpdateGroceryListItem/
    Ingredients/
      CreateIngredient/
      DeleteIngredient/
      Domain/
      Endpoints/
      GetIngredient/
      ListIngredients/
      Persistence/
      Shared/
      UpdateIngredient/
    MealPlans/
      CreateMealPlan/
      DeleteMealPlan/
      Domain/
      Endpoints/
      GetMealPlan/
      ListMealPlans/
      Persistence/
      Shared/
      UpdateMealPlan/
    Media/
      DeleteRecipeMedia/
      Endpoints/
      GetMediaContent/
      Shared/
      UploadRecipeMedia/
    RecipeImports/
      CreateRecipeImport/
      Domain/
      Endpoints/
      GetRecipeImport/
      Persistence/
      Shared/
    Recipes/
      CreateRecipe/
      DeleteRecipe/
      Domain/
      Endpoints/
      GetRecipe/
      ListRecipes/
      Persistence/
      Shared/
      UpdateRecipe/
    Units/
      Endpoints/
      ListUnits/
      Shared/
    Users/
      Domain/
      Endpoints/
      GetCurrentUser/
      Login/
      Persistence/
      Shared/
      Signup/
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

## Architectural Rules

- keep controllers thin
- put business logic in handlers and feature-local domain types
- keep validation close to commands and queries
- use `Common/` only for truly cross-cutting concerns
- keep user ownership explicit in every query and mutation
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
