# Frontend Handoff

## Current Handoff State

- backend auth endpoints are implemented
- frontend scaffolding is not yet initialized
- recipe, meal-plan, grocery, import, and media flows are still planned

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

- recipes own ingredients, steps, and media metadata
- meal plans own flexible slots and dated entries
- grocery lists are generated snapshots and should be treated as derived state
- keep mock data realistic for quantities, units, servings, and planning windows

## Done Criteria For Initial Frontend Slice

- auth screens work against the real backend
- shared API client and token storage pattern exist
- route structure matches `docs/frontend/pages-and-routes.md`
- future feature folders for recipes and meal plans can be added without moving core app setup
