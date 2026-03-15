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

## Planned Slices

Near-term slices should follow the same structure:

- `Recipes`
- `MealPlans`
- `GroceryLists`
- `RecipeImports`
- `Media` if it becomes large enough to justify its own slice

## Testing Layout

```text
backend/tests/
  PantryPlanner.Api.UnitTests/
  PantryPlanner.Api.IntegrationTests/
```

- `PantryPlanner.Api.UnitTests`: fast coverage for domain behavior, validation, handlers, security helpers, and result mapping
- `PantryPlanner.Api.IntegrationTests`: endpoint wiring, auth flows, middleware behavior, and PostgreSQL-backed runtime paths
