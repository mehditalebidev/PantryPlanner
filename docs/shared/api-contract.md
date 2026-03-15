# MVP API Contract

This document defines the backend contract the frontend can build against.

## API Conventions

- base route prefix: `/api/v{version}`
- current implemented version: `v1`
- JSON request and response bodies
- authenticated routes require bearer token
- timestamps use ISO 8601 strings
- exact ingredient quantities should use decimal-compatible JSON numbers

## Implementation Status

- implemented now: `POST /api/v1/auth/signup`, `POST /api/v1/auth/login`, `GET /api/v1/users/me`
- planned next: recipe CRUD, meal-plan CRUD, grocery-list generation, import workflows, and recipe media support
- frontend code may mock planned endpoints from this document, but should not assume backend availability until the corresponding coordination item lands

## Error Shape

All API errors should use ProblemDetails-style JSON.

Non-validation example:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Recipe was not found.",
  "status": 404,
  "detail": "The requested recipe does not exist.",
  "code": "resource_not_found"
}
```

Validation example:

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "See the errors property for details.",
  "code": "validation_failed",
  "errors": {
    "title": ["Title is required."],
    "ingredients": ["At least one ingredient is required."]
  }
}
```

## Authentication

### `POST /api/v1/auth/signup`

Creates a local user and returns an access token.

Request:

```json
{
  "email": "user@example.com",
  "displayName": "Jane Doe",
  "password": "Password123!"
}
```

Response:

```json
{
  "accessToken": "string",
  "expiresAt": "2026-03-14T12:00:00Z",
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "displayName": "Jane Doe"
  }
}
```

### `POST /api/v1/auth/login`

Logs an existing user in and returns an access token.

### `GET /api/v1/users/me`

Returns the authenticated user.

```json
{
  "id": "uuid",
  "email": "user@example.com",
  "displayName": "Jane Doe"
}
```

## Planned Recipes Contract

### Recipe Shape

```json
{
  "id": "uuid",
  "title": "Sheet Pan Chicken",
  "description": "Weeknight dinner",
  "servings": 4,
  "prepTimeMinutes": 15,
  "cookTimeMinutes": 30,
  "sourceUrl": "https://example.com/recipe",
  "ingredients": [
    {
      "id": "uuid",
      "name": "Chicken thighs",
      "quantity": 2.0,
      "unit": "lb",
      "preparationNote": null,
      "sortOrder": 1
    }
  ],
  "steps": [
    {
      "id": "uuid",
      "instruction": "Preheat the oven.",
      "sortOrder": 1,
      "durationMinutes": 10
    }
  ],
  "media": [
    {
      "id": "uuid",
      "kind": "image",
      "url": "https://cdn.example.com/recipes/sheet-pan.jpg",
      "caption": "Finished dish",
      "sortOrder": 1
    }
  ],
  "createdAt": "2026-03-14T12:00:00Z",
  "updatedAt": "2026-03-14T12:00:00Z"
}
```

Planned endpoints:

- `GET /api/v1/recipes`
- `POST /api/v1/recipes`
- `GET /api/v1/recipes/{id}`
- `PUT /api/v1/recipes/{id}`
- `DELETE /api/v1/recipes/{id}`

## Planned Meal Plans Contract

### Meal Plan Shape

```json
{
  "id": "uuid",
  "title": "Week of March 16",
  "startDate": "2026-03-16",
  "endDate": "2026-03-22",
  "slots": [
    {
      "id": "uuid",
      "name": "Breakfast",
      "sortOrder": 1,
      "isDefault": true
    },
    {
      "id": "uuid",
      "name": "Snack",
      "sortOrder": 2,
      "isDefault": false
    }
  ],
  "entries": [
    {
      "id": "uuid",
      "plannedDate": "2026-03-16",
      "mealSlotId": "uuid",
      "recipeId": "uuid",
      "recipeTitle": "Sheet Pan Chicken",
      "servingsOverride": 6,
      "note": "Use leftovers"
    }
  ]
}
```

Planned endpoints:

- `GET /api/v1/meal-plans`
- `POST /api/v1/meal-plans`
- `GET /api/v1/meal-plans/{id}`
- `PUT /api/v1/meal-plans/{id}`
- `DELETE /api/v1/meal-plans/{id}`

## Planned Grocery Lists Contract

### Grocery List Shape

```json
{
  "id": "uuid",
  "startDate": "2026-03-16",
  "endDate": "2026-03-22",
  "generatedAt": "2026-03-15T18:00:00Z",
  "items": [
    {
      "id": "uuid",
      "name": "Chicken thighs",
      "quantity": 4.0,
      "unit": "lb",
      "isChecked": false,
      "sourceCount": 2
    }
  ]
}
```

Planned endpoints:

- `POST /api/v1/grocery-lists/generate`
- `GET /api/v1/grocery-lists/{id}`
- `PUT /api/v1/grocery-lists/{id}/items/{itemId}`

## Planned Import Contract

Planned endpoints:

- `POST /api/v1/recipe-imports`
- `GET /api/v1/recipe-imports/{id}`

## Frontend Integration Notes

- build query hooks around auth, recipes, meal plans, grocery lists, imports, and media
- treat field names here as contract names unless changed jointly
- keep mocked recipe, meal-plan, and grocery data realistic
