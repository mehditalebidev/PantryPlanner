# Backend Endpoints

## Implementation Status

Implemented now:

- `POST /api/v1/auth/signup`
- `POST /api/v1/auth/login`
- `GET /api/v1/users/me`
- `GET /api/v1/ingredients`
- `GET /api/v1/ingredients/{id}`
- `POST /api/v1/ingredients`
- `PUT /api/v1/ingredients/{id}`
- `DELETE /api/v1/ingredients/{id}`
- `GET /api/v1/recipes`
- `GET /api/v1/recipes/{id}`
- `POST /api/v1/recipes`
- `PUT /api/v1/recipes/{id}`
- `DELETE /api/v1/recipes/{id}`
- `GET /api/v1/units`
- `GET /api/v1/meal-plans`
- `POST /api/v1/meal-plans`
- `GET /api/v1/meal-plans/{id}`
- `PUT /api/v1/meal-plans/{id}`
- `DELETE /api/v1/meal-plans/{id}`
- `POST /api/v1/grocery-lists/generate`
- `GET /api/v1/grocery-lists/{id}`
- `PUT /api/v1/grocery-lists/{id}/items/{itemId}`

Planned next:

- recipe-import endpoints
- recipe media support endpoints if needed

## Auth

### `POST /api/v1/auth/signup`

Creates a local user and returns an access token plus the current user payload.

### `POST /api/v1/auth/login`

Authenticates an existing user and returns an access token plus the current user payload.

### `GET /api/v1/users/me`

Returns the authenticated user.

## Recipe Endpoints

- `GET /api/v1/recipes`
- `POST /api/v1/recipes`
- `GET /api/v1/recipes/{id}`
- `PUT /api/v1/recipes/{id}`
- `DELETE /api/v1/recipes/{id}`

Implemented behavior:

- recipes are user-scoped
- recipe payloads include ingredient identity, authored measurements, normalized measurements when available, ordered steps, and media metadata
- steps can link to recipe ingredients through structured references
- source URLs are optional because recipes can be manual or imported
- deleting a recipe is blocked while any meal plan still schedules it

## Ingredient Endpoints

- `GET /api/v1/ingredients`
- `GET /api/v1/ingredients/{id}`
- `POST /api/v1/ingredients`
- `PUT /api/v1/ingredients/{id}`
- `DELETE /api/v1/ingredients/{id}`

Implemented behavior:

- ingredients are user-scoped reusable records
- each new user receives a seeded starter ingredient catalog
- ingredient names are deduplicated by normalized name per user
- recipe creation and editing may create missing ingredients automatically
- deleting an ingredient is blocked while any recipe still references it

## Unit Endpoints

- `GET /api/v1/units`

Implemented behavior:

- unit definitions are shared reference data
- units expose their family and conversion metadata
- automatic conversion only works within a compatible family such as mass, volume, or count
- normalization stays in the backend so persisted recipe data is consistent for future planner and grocery aggregation

## Meal-Plan Endpoints

- `GET /api/v1/meal-plans`
- `POST /api/v1/meal-plans`
- `GET /api/v1/meal-plans/{id}`
- `PUT /api/v1/meal-plans/{id}`
- `DELETE /api/v1/meal-plans/{id}`

Implemented behavior:

- meal plans support variable date ranges
- meal slots are configurable per plan and are referenced during writes by a stable `referenceKey`
- scheduled recipes remain user-scoped
- planned meals must stay inside the plan date range and cannot duplicate the same slot/date pair

## Grocery Endpoints

- `POST /api/v1/grocery-lists/generate`
- `GET /api/v1/grocery-lists/{id}`
- `PUT /api/v1/grocery-lists/{id}/items/{itemId}`

Implemented behavior:

- grocery lists aggregate ingredients across planned recipes
- grocery generation scales recipe quantities when a planned meal overrides servings
- normalized ingredients aggregate into deterministic base unit codes
- non-normalized ingredients only aggregate when their authored unit code still matches exactly
- item checkoff state is mutable after generation
