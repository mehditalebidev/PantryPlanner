# Concrete Folder Structure

This is the current repository shape plus the near-term target structure as implementation continues.

## Top-Level Layout

```text
PantryPlanner/
  docs/
    shared/
    coordination/
    frontend/
    backend/
    WORKLOG.md
  frontend/
    README.md
    docs/
    src/
  backend/
    README.md
    docs/
    src/
    tests/
  .env.example
  docker-compose.yml
  AGENTS.md
  CONTRIBUTING.md
  README.md
```

## Planned Frontend Structure

```text
frontend/
  README.md
  docs/
  src/
    app/
      providers/
      router/
    api/
    components/
      forms/
      layout/
      feedback/
      media/
    features/
      auth/
      recipes/
      meal-plans/
      grocery-lists/
      recipe-imports/
      pantry/
    hooks/
    lib/
    pages/
    styles/
    types/
  public/
  index.html
  package.json
  tsconfig.json
  vite.config.ts
```

## Current Backend Structure

```text
backend/
  README.md
  docs/
    README.md
    local-development.md
  src/
    PantryPlanner.Api/
      Common/
        Api/
        Behaviors/
        Persistence/
        Results/
        Security/
      Features/
        Users/
      Program.cs
  tests/
    PantryPlanner.Api.UnitTests/
      Common/
      Features/
      Support/
    PantryPlanner.Api.IntegrationTests/
      Support/
        Infrastructure/
        Seeding/
        Users/
  dotnet-tools.json
  PantryPlanner.sln
```

## Backend Responsibility Split

- `PantryPlanner.Api/Common`: shared API helpers, persistence, validation behavior, security, and result models
- `PantryPlanner.Api/Features/*`: slice-local endpoints, handlers, validators, DTOs, entities, and EF configuration
- `tests/PantryPlanner.Api.UnitTests`: fast unit coverage for shared helpers and slice logic
- `tests/PantryPlanner.Api.IntegrationTests`: end-to-end API tests plus shared test support and seeding helpers

## Planned Backend Slices

- `Users` for auth and current-user profile
- `Recipes` for recipe CRUD, ingredients, steps, and media metadata
- `MealPlans` for planning ranges, slots, and scheduled recipes
- `GroceryLists` for generated shopping projections and item state
- `RecipeImports` for source parsing and import status
- `PantryItems` if pantry support becomes part of the near-term roadmap

## Ownership Guidance

- frontend work lives in `frontend/` plus `docs/frontend/`
- backend work lives in `backend/` plus `docs/backend/`
- planner and coordinator work lives in `docs/coordination/` plus `docs/WORKLOG.md`
- contract changes must be updated first in `docs/shared/api-contract.md`
- frontend structure remains planned until the frontend scaffold is initialized
