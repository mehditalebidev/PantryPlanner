# PantryPlanner

PantryPlanner is a meal-planning app focused on recipes, flexible meal schedules, and grocery list generation.

## Current State

- This repository is docs-first and mid-pivot into PantryPlanner.
- The backend workspace contains a runnable ASP.NET Core API with local auth, JWT issuance, PostgreSQL wiring, migrations, and tests.
- The frontend workspace is still a placeholder and does not yet contain a real app scaffold.
- Product docs under `docs/` now describe the PantryPlanner direction and should be treated as the source of truth.

## Product Direction

PantryPlanner should support:

- creating recipes manually
- importing recipes from other places
- storing ingredients, quantities, units, and preparation steps
- attaching images and videos to recipes
- planning meals across flexible ranges such as one week, two weeks, or a month
- using default meal slots like breakfast, lunch, dinner, and snacks while allowing custom slots
- generating grocery lists from the selected meal-plan range

## Planned Stack

- Frontend: React, TypeScript, Vite, TanStack Query, React Hook Form, Zod, Tailwind CSS
- Backend: ASP.NET Core Web API, EF Core, PostgreSQL, JWT auth

## Repository Layout

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

## Docs To Read First

1. `docs/shared/README.md`
2. `docs/shared/product-scope.md`
3. `docs/shared/domain-model.md`
4. `docs/shared/auth-flow.md`
5. `docs/shared/api-contract.md`
6. `docs/shared/repo-structure.md`
7. `docs/coordination/BOARD.md`
8. `docs/WORKLOG.md`
9. `backend/README.md` or `frontend/README.md` depending on the area you are changing

## Notes

- Keep the repo honest about what is implemented versus planned.
- Treat the current backend as an auth-first scaffold for PantryPlanner, not as a completed product backend.
- Update shared docs before changing contracts, routes, or cross-workspace architecture.
- Use `docs/coordination/` and `docs/WORKLOG.md` to keep planner and implementer work aligned.
