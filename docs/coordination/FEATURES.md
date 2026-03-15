# Feature Briefs

## AUTH-001 - Preserve auth scaffold

- Suggested branch: `chore/auth-scaffold-hardening`
- Goal: keep signup, login, and current-user flows stable while PantryPlanner feature slices land.
- Done when:
  - auth endpoints continue to match the shared contract
  - JWT config, ProblemDetails responses, and user scoping remain stable
  - unit and integration tests stay green

## FE-001 - Initialize frontend shell

- Suggested branch: `feat/frontend-shell`
- Goal: create the real frontend scaffold and connect it to the existing auth backend.
- Done when:
  - Vite + React + TypeScript app exists under `frontend/`
  - router, providers, and API client structure match shared docs
  - login and signup screens work against the backend
  - basic authenticated layout exists for future recipe and planner pages

## REC-001 - Recipe library vertical slice

- Suggested branch: `feat/recipe-library`
- Goal: let an authenticated user create, edit, list, and view recipes.
- Done when:
  - recipe CRUD exists in backend and follows the shared contract
  - ingredient CRUD exists in backend and each user starts with a seeded ingredient catalog
  - recipes support ingredient entities, recipe-specific measurements, ordered steps, and media metadata
  - recipe steps can reference recipe ingredients through structured links
  - supported unit definitions and normalization rules are documented and exposed through a dedicated units slice
  - backend validation and ownership checks are covered by tests
  - frontend can create and browse recipes once the scaffold exists

## PLAN-001 - Meal-plan vertical slice

- Suggested branch: `feat/meal-plan-slice`
- Goal: let an authenticated user organize recipes into flexible plans.
- Done when:
  - meal plans support date ranges and default or custom meal slots
  - users can schedule recipes into dated entries
  - writes support stable slot references for nested create and update payloads
  - validation covers date-range and slot-name rules
  - contract and docs stay aligned with implementation

## GROC-001 - Grocery generation vertical slice

- Suggested branch: `feat/grocery-generation`
- Goal: generate a grocery list from a selected plan window.
- Done when:
  - backend aggregates ingredient requirements across planned recipes
  - quantities scale correctly for servings overrides and stay explicit about unit codes
  - grocery item checkoff state can be updated after generation
  - tests cover aggregation behavior and ownership boundaries

## IMP-001 - Recipe import foundation

- Suggested branch: `feat/recipe-import-foundation`
- Goal: let a user create recipes from an external source with an editable review step.
- Done when:
  - import request and result shapes are documented
  - backend exposes a persisted import workflow contract or stub
  - URL-based imports return a reviewable recipe draft that reuses normal recipe field names
  - imported data maps into the normal recipe model instead of a separate permanent shape
  - validation and ownership rules are covered by tests

## MEDIA-001 - Recipe media upload flow

- Suggested branch: `feat/recipe-media-flow`
- Goal: let an authenticated user upload and manage recipe media files without breaking recipe ownership rules.
- Done when:
  - backend exposes upload, protected content, and delete endpoints for recipe media assets
  - uploaded assets persist storage-backed metadata that recipe CRUD can reuse
  - recipe media responses include enough metadata to render uploaded content safely
  - removing media from a recipe or deleting a recipe cleans up stored files
  - validation and ownership behavior are covered by tests
