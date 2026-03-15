# Contributing to PantryPlanner

## Working Agreement

- Keep changes focused and small.
- Update docs when architecture, contracts, or product scope change.
- Preserve the docs-first workflow while the project is still taking shape.
- Be explicit about what is implemented today versus what is still planned.

## Recommended Workflow

1. Read the relevant shared docs before coding.
2. Check `docs/coordination/BOARD.md` for the active or ready item.
3. Create a focused branch for one task.
4. Implement the change in the smallest sensible slice.
5. Run the narrowest commands that prove the change.
6. Update `docs/WORKLOG.md` and the coordination board.
7. Open a pull request to `main`.

## Branch Naming

- `feat/...` for feature work
- `fix/...` for bug fixes
- `docs/...` for documentation work
- `chore/...` for maintenance work

## Commands

Run backend commands from `backend/`:

- `dotnet restore PantryPlanner.sln`
- `dotnet build PantryPlanner.sln`
- `dotnet test PantryPlanner.sln`
- `dotnet test tests/PantryPlanner.Api.UnitTests/PantryPlanner.Api.UnitTests.csproj`
- `dotnet test tests/PantryPlanner.Api.IntegrationTests/PantryPlanner.Api.IntegrationTests.csproj --filter "FullyQualifiedName~AuthEndpointsTests"`
- `dotnet run --project src/PantryPlanner.Api/PantryPlanner.Api.csproj`

Run local infrastructure from the repo root:

- `docker compose up --build`
- `docker compose down`

## Pull Request Notes

- Keep PRs scoped to one feature slice or one documentation update.
- Mention any new commands, migrations, or contract changes in the PR description.
- If the change is docs-only, say that clearly.
- If the frontend command is only planned because the scaffold does not exist yet, say that clearly.
