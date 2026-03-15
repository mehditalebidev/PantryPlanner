# Frontend Integration Guide

This guide explains the backend entities, endpoint relationships, and the API usage patterns the future frontend should follow.

## What Is Implemented Now

- auth: signup, login, current user
- ingredients: seeded starter catalog plus full CRUD
- recipes: full CRUD with ordered ingredients, ordered steps, media metadata, and structured step-to-ingredient references
- meal plans: full CRUD with flexible slots and dated recipe scheduling
- grocery lists: generation from meal plans plus item checkoff updates
- recipe imports: URL-based starter draft generation plus persisted review lookup
- units: lookup endpoint plus backend-owned normalization rules used by recipes

## Core Entities And Relationships

### User

- owns `Ingredient` and `Recipe`
- receives the default starter ingredient catalog on signup

### Ingredient

- reusable, user-scoped ingredient record
- fields: `id`, `name`, `normalizedName`, timestamps
- can exist before any recipe uses it
- may be referenced by many `RecipeIngredient` rows

### Recipe

- user-scoped aggregate root
- fields: `id`, `title`, `description`, `servings`, `prepTimeMinutes`, `cookTimeMinutes`, `sourceUrl`, timestamps
- owns ordered `RecipeIngredient`, `RecipeStep`, and `RecipeMediaAsset` collections
- cannot be deleted while any `PlannedMeal` still schedules it

### RecipeIngredient

- join-like recipe line item that points to a reusable `Ingredient`
- fields: `ingredientId`, `referenceKey`, `quantity`, `unitCode`, `normalizedQuantity`, `normalizedUnitCode`, `preparationNote`, `sortOrder`
- `referenceKey` is the frontend-safe identifier used inside one recipe payload before database ids exist for nested children

### RecipeStep

- ordered free-text instruction row inside a recipe
- fields: `instruction`, `sortOrder`, optional `durationMinutes`
- exposes `ingredientReferences` so the frontend can highlight or link mentioned ingredients without parsing the text itself

### RecipeStepIngredientReference

- structured link between one `RecipeStep` and one `RecipeIngredient`
- response fields: `recipeIngredientId`, `ingredientId`, `referenceKey`

### RecipeMediaAsset

- metadata-only media row for now
- fields: `kind`, `url`, optional `storageKey`, optional `caption`, `sortOrder`

### UnitDefinition

- shared reference data, not user-owned
- fields: `code`, `displayName`, `abbreviation`, `family`, `baseUnitCode`, `conversionFactor`, `isConvertible`
- backend validates `unitCode` values against this catalog and computes normalized quantities when conversion is safe

### MealPlan

- user-scoped planning aggregate
- fields: `id`, `title`, `startDate`, `endDate`, timestamps
- owns `MealSlot` and `PlannedMeal` collections

### MealSlot

- nested meal-plan slot row
- fields: `id`, `referenceKey`, `name`, `sortOrder`, `isDefault`
- `referenceKey` is the stable nested write identifier used by planned meals in create and update payloads

### PlannedMeal

- one scheduled recipe in one slot on one date
- fields: `plannedDate`, `mealSlotId`, `mealSlotReferenceKey`, `recipeId`, `recipeTitle`, optional `servingsOverride`, optional `note`
- backend enforces one recipe per meal slot per day within the same meal plan

### GroceryList

- generated snapshot derived from one meal plan
- fields: `id`, `mealPlanId`, `startDate`, `endDate`, `generatedAt`
- owns `GroceryListItem` rows

### GroceryListItem

- aggregated ingredient line for shopping
- fields: `id`, optional `ingredientId`, `name`, `quantity`, `unitCode`, `isChecked`, `sourceCount`
- quantity uses normalized units when safe aggregation is possible; otherwise it stays in the authored unit code

### RecipeImport

- user-scoped review artifact for imported recipe data
- fields: `id`, `sourceType`, `sourceUrl`, `status`, `draft`, `warnings`, timestamps
- `draft` uses recipe field names so the frontend can open the normal recipe form prefilled from the import result

## Why Normalization Stays In The Backend

- recipes persist normalized values that later planner and grocery features will aggregate on the server
- keeping normalization in the backend prevents client drift across web, mobile, imports, or scripts
- frontend should still use `GET /api/v1/units` to render unit choices and display unit metadata, but the backend remains the source of truth for persisted normalized values

## Endpoint Map

### Auth

- `POST /api/v1/auth/signup`
- `POST /api/v1/auth/login`
- `GET /api/v1/users/me`

Use signup or login to get a bearer token, then send `Authorization: Bearer <token>` on every ingredient, recipe, and unit request.

### Ingredients

- `GET /api/v1/ingredients`
- `GET /api/v1/ingredients/{id}`
- `POST /api/v1/ingredients`
- `PUT /api/v1/ingredients/{id}`
- `DELETE /api/v1/ingredients/{id}`

Behavior:

- every new user starts with a seeded ingredient catalog
- names are deduplicated per user by `normalizedName`
- deleting an ingredient fails with `409` if any recipe still references it

### Recipes

- `GET /api/v1/recipes`
- `GET /api/v1/recipes/{id}`
- `POST /api/v1/recipes`
- `PUT /api/v1/recipes/{id}`
- `DELETE /api/v1/recipes/{id}`

Behavior:

- each recipe payload contains nested ingredients, steps, and media arrays
- a nested recipe ingredient may either reference an existing `ingredientId` or create/reuse an ingredient via `name`
- `ingredientReferenceKeys` on each step must point to ingredient `referenceKey` values from the same request
- the response returns authored quantity/unit plus normalized quantity/unit when conversion is safe
- deleting a recipe fails with `409` while a meal plan still references it

### Meal Plans

- `GET /api/v1/meal-plans`
- `GET /api/v1/meal-plans/{id}`
- `POST /api/v1/meal-plans`
- `PUT /api/v1/meal-plans/{id}`
- `DELETE /api/v1/meal-plans/{id}`

Behavior:

- each write submits the full meal plan document with nested `slots` and `entries`
- entries refer to slots through `mealSlotReferenceKey` during writes
- slot `referenceKey`, slot `name`, and slot `sortOrder` must each be unique within one meal plan payload
- planned dates must stay inside the meal plan range
- a slot can only contain one recipe per day

### Grocery Lists

- `POST /api/v1/grocery-lists/generate`
- `GET /api/v1/grocery-lists/{id}`
- `PUT /api/v1/grocery-lists/{id}/items/{itemId}`

Behavior:

- grocery generation starts from an existing meal plan id
- servings overrides rescale ingredient quantities before aggregation
- normalized ingredients aggregate by `ingredientId` plus normalized unit code
- non-normalized ingredients only aggregate when `ingredientId` and authored `unitCode` still match exactly
- grocery lists are stored snapshots and item checkoff state is mutable after generation

### Recipe Imports

- `POST /api/v1/recipe-imports`
- `GET /api/v1/recipe-imports/{id}`

Behavior:

- create an import by providing a source URL
- the backend stores a reviewable import result instead of creating a recipe immediately
- the returned `draft` should be treated as recipe-form starter data and submitted later through `POST /api/v1/recipes`
- current foundation behavior infers only starter values from the URL and returns warnings that the user should review the draft

### Units

- `GET /api/v1/units`

Use this to build unit pickers and display unit metadata. Do not hardcode the supported unit list in the frontend.

## Recommended Frontend Data Flow

### Ingredient Library Screens

- load `GET /ingredients` for browse and search
- use `POST /ingredients` for custom additions beyond the seeded catalog
- use `PUT /ingredients/{id}` for rename flows
- disable destructive delete UI or show a strong warning when delete returns `ingredient_in_use_by_recipe`

### Recipe Create And Edit Screens

- load `GET /ingredients` and `GET /units` before opening the form
- let the user either pick an existing ingredient or type a new one
- assign a stable client-side `referenceKey` for each recipe ingredient row
- when a step mentions ingredients, store those links as `ingredientReferenceKeys`
- submit the full recipe document on create and update
- trust the backend response as the canonical saved recipe because normalization and ingredient auto-reuse happen server-side

### Meal Plan Screens

- load `GET /recipes` before plan editing so the user can pick recipes
- submit the full plan shape with nested slots and entries on both create and update
- keep slot `referenceKey` stable on the client while editing one meal plan form
- use the returned `mealSlotId` values for display only; keep `mealSlotReferenceKey` as the nested write key

### Grocery Screens

- call `POST /grocery-lists/generate` after the user picks or confirms a meal plan
- render `quantity` plus `unitCode` exactly as returned
- treat the generated grocery list as a snapshot and update item state through `PUT /grocery-lists/{id}/items/{itemId}`
- do not recalculate grocery aggregation in the client

### Recipe Import Screens

- submit a source URL through `POST /recipe-imports`
- route the returned `draft` directly into the normal recipe create/edit form state
- surface `warnings` clearly because the current foundation intentionally returns partial drafts that require user review
- use `GET /recipe-imports/{id}` to reopen a saved review flow

### Recipe Detail Screens

- render ordered `ingredients`, `steps`, and `media`
- use `ingredientReferences` from each step if the UI wants chips, highlighting, or deep links back to the ingredient list

## Common Error Cases The Frontend Should Handle

- `401 unauthorized`: token missing or expired
- `404 ingredient_not_found` or `404 recipe_not_found`: stale route or deleted resource
- `404 meal_plan_not_found` or `404 grocery_list_not_found`: stale route or cross-user access
- `404 recipe_import_not_found`: stale route or cross-user access
- `409 ingredient_name_in_use`: duplicate ingredient rename/create
- `409 ingredient_in_use_by_recipe`: attempted delete while referenced by at least one recipe
- `409 recipe_in_use_by_meal_plan`: attempted recipe delete while still scheduled
- `400 validation_failed`: invalid payload, unsupported unit code, duplicate `referenceKey`, missing step reference target, or invalid sort order

## Current Seeded Ingredient Catalog

- the backend seeds a broad starter catalog of common pantry staples, produce, proteins, dairy, herbs, spices, sauces, grains, and baking ingredients
- examples include `Salt`, `Olive oil`, `Garlic`, `Chicken thighs`, `Rice`, `Paprika`, `Soy sauce`, and `Tomatoes`
- users can still create their own custom ingredients beyond the starter set
