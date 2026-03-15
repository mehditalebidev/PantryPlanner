# Product Scope

## Project Summary

Build a practical meal-planning application that helps a user collect recipes, organize meals across flexible schedules, and generate grocery lists from those plans.

## Product Goals

- create recipes manually with ingredients, quantities, and preparation steps
- import recipes from external sources
- attach images and videos to recipes
- build meal plans for ranges such as one week, two weeks, or a month
- support default and custom meal slots such as breakfast, lunch, dinner, and snacks
- generate grocery lists by aggregating planned recipe ingredients
- keep the scope small enough to ship iteratively

## MVP Scope

The first usable version includes:

- local email/password signup and login
- authenticated current-user profile lookup
- recipe CRUD with ingredients, units, and ordered steps
- recipe media metadata support
- meal-plan CRUD for configurable date ranges and meal slots
- grocery-list generation from a selected meal-plan range
- responsive web UI for the core flows

## Explicitly Deferred From MVP

- household collaboration
- pantry inventory tracking beyond a lightweight placeholder
- nutrition analysis
- shopping price comparison
- barcode scanning
- calendar sync
- offline support
- push notifications

## Phase Breakdown

### Phase 1 - Core Planning

- authentication with backend-issued JWTs
- recipe library management
- meal-plan creation for flexible ranges and slots
- grocery-list aggregation from plans
- validation and basic error handling

### Phase 2 - Productivity

- richer recipe import workflows
- pantry item tracking
- grocery list editing, substitutions, and checkoff state
- recipe search, filtering, and tagging
- media upload hardening and cleanup rules

### Phase 3 - Polish

- sharing and collaboration groundwork
- saved planning templates
- analytics around frequently used recipes and ingredients
- deployment and observability hardening
- broader testing depth

## Non-Goals

- nutrition-grade clinical planning
- restaurant inventory management
- point-of-sale workflows
- warehouse logistics
- marketplace features in v1

## Success Criteria

- a user can authenticate and only access their own data
- a user can manage recipes with realistic ingredient and step data
- a user can build meal plans with flexible slots and date ranges
- a user can generate a grocery list that reflects the selected plan window
- the architecture remains easy to extend with imports, pantry items, and media
