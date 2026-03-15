# Backend Workspace

This directory contains the PantryPlanner backend scaffold.

## Current State

- The backend centers on a single ASP.NET Core API project with a vertical-slice layout.
- Local email/password auth is implemented with signup, login, and `users/me` endpoints.
- PostgreSQL, EF Core migrations, JWT auth, ProblemDetails responses, and test coverage are wired up.
- Recipe, meal-plan, grocery-list, import, and media slices are still planned.
- `docs/` holds backend-specific implementation notes and agent-readable guidance.

## Solution Layout

```text
backend/
  PantryPlanner.sln
  docs/
    README.md
    local-development.md
  src/
    PantryPlanner.Api/
  tests/
    PantryPlanner.Api.UnitTests/
    PantryPlanner.Api.IntegrationTests/
      Support/
        Infrastructure/
        Seeding/
        Users/
```

## Useful Commands

Run from `backend/`:

- `dotnet restore PantryPlanner.sln`
- `dotnet build PantryPlanner.sln`
- `dotnet test PantryPlanner.sln`
- `dotnet test tests/PantryPlanner.Api.UnitTests/PantryPlanner.Api.UnitTests.csproj`
- `dotnet test tests/PantryPlanner.Api.IntegrationTests/PantryPlanner.Api.IntegrationTests.csproj --filter "FullyQualifiedName~AuthEndpointsTests"`
- `dotnet run --project src/PantryPlanner.Api/PantryPlanner.Api.csproj`
- `dotnet ef database update --project src/PantryPlanner.Api/PantryPlanner.Api.csproj --startup-project src/PantryPlanner.Api/PantryPlanner.Api.csproj`

Run from the repo root:

- `docker compose up --build`

## Read First

1. `docs/shared/README.md`
2. `docs/shared/product-scope.md`
3. `docs/shared/domain-model.md`
4. `docs/shared/auth-flow.md`
5. `docs/shared/api-contract.md`
6. `docs/backend/backend-architecture.md`
7. `docs/backend/backend-endpoints.md`
8. `docs/backend/security-and-auth.md`
9. `docs/coordination/BOARD.md`
10. `docs/WORKLOG.md`
11. `backend/docs/README.md`

## Notes

- Keep backend-only working notes under `backend/docs/`.
- Use `backend/docs/local-development.md` for runtime and testing details.
- Development OpenAPI JSON is exposed at `/openapi/v1.json` and the Scalar UI at `/docs`.
- Keep slice-local controllers, commands, validators, handlers, DTOs, entities, and EF configuration together under `backend/src/PantryPlanner.Api/Features/`.
