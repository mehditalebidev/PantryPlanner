# Local Backend Development

## Current Runtime Shape

The backend is a single ASP.NET Core Web API project using a vertical-slice layout inside `PantryPlanner.Api`.

Implemented now:

- local email/password signup
- local email/password login
- authenticated `users/me`
- PostgreSQL persistence and EF Core migrations
- ProblemDetails error responses
- unit and integration tests for the auth scaffold

Planned later:

- recipes
- meal plans
- grocery lists
- recipe imports
- media support

## Environment Variables

- `POSTGRES_DB`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_PORT`
- `API_PORT`
- `CONNECTIONSTRINGS__PANTRYPLANNERDATABASE`
- `JWT__ISSUER`
- `JWT__AUDIENCE`
- `JWT__SIGNINGKEY`
- `JWT__ACCESSTOKENMINUTES`

## Commands

Run from `backend/`:

- restore: `dotnet restore PantryPlanner.sln`
- build: `dotnet build PantryPlanner.sln`
- run API: `dotnet run --project src/PantryPlanner.Api/PantryPlanner.Api.csproj`
- run all tests: `dotnet test PantryPlanner.sln`
- run unit tests: `dotnet test tests/PantryPlanner.Api.UnitTests/PantryPlanner.Api.UnitTests.csproj`
- run integration tests: `dotnet test tests/PantryPlanner.Api.IntegrationTests/PantryPlanner.Api.IntegrationTests.csproj`
- run one test class: `dotnet test tests/PantryPlanner.Api.IntegrationTests/PantryPlanner.Api.IntegrationTests.csproj --filter "FullyQualifiedName~AuthEndpointsTests"`
- run one test method: `dotnet test tests/PantryPlanner.Api.IntegrationTests/PantryPlanner.Api.IntegrationTests.csproj --filter "Name~Login_And_GetMe_Work_WithSeededUser"`
- apply migrations: `dotnet tool restore && dotnet ef database update --project src/PantryPlanner.Api/PantryPlanner.Api.csproj --startup-project src/PantryPlanner.Api/PantryPlanner.Api.csproj`

Run from the repo root:

- `docker compose up --build`
- `docker compose down`

## Test Notes

- unit tests live in `backend/tests/PantryPlanner.Api.UnitTests/`
- integration tests live in `backend/tests/PantryPlanner.Api.IntegrationTests/`
- integration tests require Docker because PostgreSQL Testcontainers is used
- reusable integration support lives under `backend/tests/PantryPlanner.Api.IntegrationTests/Support/`

## Persistence Notes

- migrations live in `backend/src/PantryPlanner.Api/Common/Persistence/Migrations/`
- `backend/src/PantryPlanner.Api/Features/Users/` is the current reference slice for endpoint, handler, validator, and mapping patterns
