# Backend Endpoints

## Implementation Status

Implemented now:

- `POST /api/v1/auth/signup`
- `POST /api/v1/auth/login`
- `GET /api/v1/users/me`

Planned next:

- recipe CRUD endpoints
- meal-plan CRUD endpoints
- grocery-list generation endpoints
- recipe-import endpoints
- recipe media support endpoints if needed

## Auth

### `POST /api/v1/auth/signup`

Creates a local user and returns an access token plus the current user payload.

### `POST /api/v1/auth/login`

Authenticates an existing user and returns an access token plus the current user payload.

### `GET /api/v1/users/me`

Returns the authenticated user.

## Planned Recipe Endpoints

- `GET /api/v1/recipes`
- `POST /api/v1/recipes`
- `GET /api/v1/recipes/{id}`
- `PUT /api/v1/recipes/{id}`
- `DELETE /api/v1/recipes/{id}`

Planned behavior:

- recipes are user-scoped
- recipe payloads include ingredient lines, ordered steps, and media metadata
- source URLs are optional because recipes can be manual or imported

## Planned Meal-Plan Endpoints

- `GET /api/v1/meal-plans`
- `POST /api/v1/meal-plans`
- `GET /api/v1/meal-plans/{id}`
- `PUT /api/v1/meal-plans/{id}`
- `DELETE /api/v1/meal-plans/{id}`

Planned behavior:

- meal plans support variable date ranges
- meal slots are configurable per plan
- scheduled recipes remain user-scoped

## Planned Grocery Endpoints

- `POST /api/v1/grocery-lists/generate`
- `GET /api/v1/grocery-lists/{id}`
- `PUT /api/v1/grocery-lists/{id}/items/{itemId}`

Planned behavior:

- grocery lists aggregate ingredients across planned recipes
- item quantities should stay explicit about units
- item checkoff state is mutable after generation
