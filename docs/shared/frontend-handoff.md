# Frontend Handoff

## Current Handoff State

- backend auth, ingredient CRUD, recipe CRUD, meal-plan CRUD, grocery generation/checkoff, and unit lookup endpoints are implemented
- frontend scaffolding is not yet initialized
- import and media upload flows are still planned
- `backend/docs/frontend-integration-guide.md` is the backend-to-frontend implementation guide for current entities and endpoint relationships

## First Frontend Goals

- bootstrap the app shell, router, and providers
- implement auth pages and token handling against the current backend
- prepare shared form patterns for recipes and meal planning
- establish a layout that can scale to recipe library, planner, and grocery views

## Priority Pages

- auth screens
- recipe library and recipe detail pages
- recipe create and edit flows
- meal-plan calendar or planner views
- grocery-list review and checkoff page

## Data Modeling Guidance

- ingredients are reusable user-scoped entities and each new user starts with a seeded starter catalog
- recipes own recipe-specific ingredient lines, steps, and media metadata
- recipe steps reference recipe ingredients through structured `ingredientReferences`
- meal plans own flexible slots and dated entries
- grocery lists are generated snapshots and should be treated as derived state
- keep mock data realistic for quantities, units, servings, and planning windows

## Done Criteria For Initial Frontend Slice

- auth screens work against the real backend
- shared API client and token storage pattern exist
- route structure matches `docs/frontend/pages-and-routes.md`
- future feature folders for recipes and meal plans can be added without moving core app setup
