# AGENTS.md

## Purpose
- This file guides agentic coding agents working in this repository.
- The product direction is PantryPlanner: a meal-planning app for recipes, meal schedules, grocery generation, and rich recipe media.
- Keep the scaffold and docs workflow, but do not reintroduce legacy product concepts or names.
- Follow the planning docs first, then align implementation to them.

## Current State
- The repo is docs-first.
- `backend/` contains a runnable ASP.NET Core API with local auth, JWT issuance, PostgreSQL wiring, migrations, and tests.
- `frontend/` is still a placeholder and has no real scaffold or `package.json` yet.
- The backend currently implements auth, ingredient CRUD, recipe CRUD, meal-plan CRUD, grocery-list generation/checkoff, recipe-import foundation, unit lookup/normalization, PostgreSQL migrations, and tests.
- Recipe media upload flows are still planned.

## Extra Instruction Files
- Checked and not found: `.cursorrules`, `.cursor/rules/`, `.github/copilot-instructions.md`.
- If any of those files appear later, merge their guidance with this file.

## Read First
- `README.md`
- `docs/shared/README.md`
- `docs/shared/product-scope.md`
- `docs/shared/domain-model.md`
- `docs/shared/auth-flow.md`
- `docs/shared/api-contract.md`
- `docs/shared/repo-structure.md`
- `docs/coordination/README.md`
- `docs/coordination/OPERATING_MODEL.md`
- `docs/coordination/BOARD.md`
- `docs/WORKLOG.md`
- `docs/backend/backend-architecture.md`
- `docs/backend/backend-endpoints.md`
- `docs/backend/security-and-auth.md`
- `docs/frontend/frontend-architecture.md`
- `docs/frontend/pages-and-routes.md`
- `docs/frontend/ui-and-components.md`
- For backend work, also read `backend/README.md` and `backend/docs/`.
- For frontend work, also read `frontend/README.md` and `frontend/docs/` if present.

## Docs-First Rules
- Shared docs are the source of truth for product direction and contract shape.
- Update docs before or alongside implementation when routes, entities, or architecture change.
- Be explicit when something is planned rather than implemented.
- Do not describe recipe, meal-plan, grocery, import, or media functionality as implemented unless it exists in code.

## Coordination Workflow
- Planner-style work belongs in `docs/coordination/` and should not silently turn into code changes.
- Implementers should pick a `Ready` item from `docs/coordination/BOARD.md` unless the user explicitly overrides that.
- Update `docs/coordination/BOARD.md` and `docs/WORKLOG.md` as work moves through `Ready`, `In Progress`, `In Review`, `Blocked`, and `Done`.

## Repo Shape To Preserve
- Top level target: `docs/`, `frontend/`, `backend/`, `.env.example`, `docker-compose.yml`.
- Frontend target stack: React, TypeScript, Vite, TanStack Query, React Hook Form, Zod, Tailwind CSS.
- Backend target stack: ASP.NET Core Web API, EF Core, PostgreSQL, FluentValidation, JWT auth.
- Preserve the simple vertical-slice backend layout unless the docs are intentionally updated first.

## Git Workflow
- Do not work directly on `main` for meaningful changes.
- Start focused work from the latest `main` on a dedicated branch.
- Preferred prefixes: `feat/`, `fix/`, `docs/`, `chore/`.
- Keep one branch scoped to one coherent task.
- Commit, push, and open a PR targeting `main`.

## Command Policy
- Prefer the narrowest command that proves your change.
- For docs-only changes, no build is required.
- For backend-only changes, prefer targeted `dotnet test` commands before a full solution run.
- For frontend work, only run frontend commands if the scaffold actually exists.
- If a command is only planned, say that clearly.

## Backend Commands
- Run backend commands from `backend/`.
- Restore: `dotnet restore PantryPlanner.sln`
- Build: `dotnet build PantryPlanner.sln`
- Run API: `dotnet run --project src/PantryPlanner.Api/PantryPlanner.Api.csproj`
- Run all tests: `dotnet test PantryPlanner.sln`
- Run unit tests: `dotnet test tests/PantryPlanner.Api.UnitTests/PantryPlanner.Api.UnitTests.csproj`
- Run integration tests: `dotnet test tests/PantryPlanner.Api.IntegrationTests/PantryPlanner.Api.IntegrationTests.csproj`
- Run one test class: `dotnet test tests/PantryPlanner.Api.IntegrationTests/PantryPlanner.Api.IntegrationTests.csproj --filter "FullyQualifiedName~AuthEndpointsTests"`
- Run one test method: `dotnet test tests/PantryPlanner.Api.IntegrationTests/PantryPlanner.Api.IntegrationTests.csproj --filter "Name~Login_And_GetMe_Work_WithSeededUser"`
- Run one unit test method: `dotnet test tests/PantryPlanner.Api.UnitTests/PantryPlanner.Api.UnitTests.csproj --filter "Name~CreateAccessToken_ReturnsSignedTokenWithExpectedClaims"`
- Apply migrations locally: `dotnet tool restore && dotnet ef database update --project src/PantryPlanner.Api/PantryPlanner.Api.csproj --startup-project src/PantryPlanner.Api/PantryPlanner.Api.csproj`

## Infra And Frontend Commands
- Run infra commands from repo root: `docker compose up --build`, `docker compose down`.
- The frontend scaffold is still planned, so these commands are not currently runnable: `npm install`, `npm run dev`, `npm run build`, `npm run lint`, `npm run typecheck`, `npm run test`.
- Planned frontend single-test examples: `npm run test -- src/features/recipes/components/RecipeForm.test.tsx` and `npm run test -- --testNamePattern="submits valid recipe"`.

## Architecture And Structure Rules
- Keep controllers and route handlers thin.
- Keep business rules in slice handlers and feature-local domain types.
- Keep cross-cutting concerns under `Common/` only when they are truly shared.
- Keep `Features/*` as the main home for backend business logic.
- Keep validation close to commands and queries and run it through the MediatR pipeline.
- Keep user scoping explicit in every query and mutation.
- Shared UI belongs in `components/`; feature-specific code belongs in `features/*`; route wiring belongs in `app/router/`; providers belong in `app/providers/`; API clients and token handling belong in `api/`.
- Do not let page files become a dumping ground for API logic, schema logic, and state orchestration.

## Code Style
- Group imports consistently: external first, then internal.
- Keep import lists stable and clean, and remove unused imports.
- Use repository tooling once formatter and lint config exist; until then, follow conventional language defaults.
- Keep lines readable and prefer one statement per line when clarity matters.
- Use file-scoped namespaces in C# when consistent with surrounding code.
- Prefer explicit types and DTOs over `any`; prefer `unknown` for untrusted input.
- Use Zod schemas at form and request boundaries.
- Keep server state in TanStack Query and form state in React Hook Form.
- Use strong C# request/response DTOs and nullable reference types consistently.
- Prefer `sealed record` request and response models when that matches existing patterns.
- Return `Result` or `Result<T>` for expected application failures instead of throwing.
- Keep domain entities framework-agnostic where practical.
- Use `decimal` when persisted ingredient quantities or computed grocery totals require exact precision.

## Naming Conventions
- Prefer descriptive domain names over abbreviations.
- Name by business meaning, not UI position.
- Use PantryPlanner language: recipe, ingredient, unit, meal plan, meal slot, grocery list, import source, media asset.
- React components use PascalCase; hooks use `useX`; TypeScript variables and functions use camelCase; TypeScript types and interfaces use PascalCase.
- C# types, methods, and properties use PascalCase; locals and parameters use camelCase.
- Tests should use `Method_ExpectedBehavior_WhenCondition`.

## Error Handling, Security, And Testing
- Validate input at system boundaries.
- Frontend forms should validate before submit; backend requests should validate through FluentValidation.
- Return consistent ProblemDetails-style error responses and preserve the stable `code` extension in backend problem responses.
- Never leak secrets, internal exception details, or raw tokens.
- Backend owns auth implementation details; the current auth bootstrap is local email/password with backend-issued JWTs.
- Keep JWT claims minimal and app-specific, keep user ownership explicit in all reads and writes, and never trust client-provided ownership fields.
- Test behavior, not implementation trivia.
- Add or update tests for bug fixes when practical.
- Prefer unit tests for validators, handlers, and shared helpers; add integration tests for endpoint wiring and auth-sensitive flows.
- Keep seed and fixture data realistic for recipes, units, servings, meal slots, and grocery aggregation.
- If only auth is implemented, keep tests scoped to auth until feature slices land.

## Documentation And Working Style
- `docs/shared/api-contract.md` is the API contract source of truth.
- Keep planning docs and implementation aligned.
- Update docs when changing endpoints, auth flow, ownership rules, or repo structure.
- Preserve the documentation structure even when the product scope changes.
- Do not reintroduce legacy product names anywhere in the repo.
- Read relevant docs before large edits.
- Prefer targeted edits over broad rewrites.
- Preserve user changes you did not make.
- If code and docs conflict, resolve the conflict deliberately and update both.
- When implementing a new PantryPlanner feature, also check whether readmes, shared docs, coordination docs, and local dev docs should change in the same slice.
