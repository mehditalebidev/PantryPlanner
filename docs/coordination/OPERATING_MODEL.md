# Operating Model

## Roles

### Planner

- refines product intent into implementable slices
- updates `FEATURES.md`, `ROADMAP.md`, and `BOARD.md`
- keeps scope tight and coherent
- does not silently turn planning work into implementation work

### Implementer

- picks a ready item unless the user overrides that rule
- reads the linked docs before coding
- updates status in `BOARD.md` and `docs/WORKLOG.md`
- keeps code and docs aligned as the slice lands

## Item Naming

Use short prefixes tied to PantryPlanner concepts:

- Auth: `AUTH-###`
- Frontend/setup: `FE-###`, `SETUP-###`
- Recipes: `REC-###`
- Meal plans: `PLAN-###`
- Grocery lists: `GROC-###`
- Imports: `IMP-###`
- Media: `MEDIA-###`
- Quality/hardening: `QUAL-###`

## Default Loop

1. read the ready item on `BOARD.md`
2. read any linked shared or workspace docs
3. create a focused branch such as `feat/recipe-library` or `fix/auth-problem-details`
4. implement the slice with the smallest proving command set
5. update `BOARD.md` and `docs/WORKLOG.md`
6. open a PR to `main`

## Documentation Expectations

- shared docs define product shape and contract
- backend docs define runtime and implementation details
- frontend docs define route, UI, and state-management intent
- coordination docs define what happens next
