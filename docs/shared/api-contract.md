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

- implemented now: `POST /api/v1/auth/signup`, `POST /api/v1/auth/login`, `GET /api/v1/users/me`, ingredient CRUD, recipe CRUD, recipe media upload/content/delete, meal-plan CRUD, grocery-list generation/checkoff, recipe-import foundation, and `GET /api/v1/units`
- planned next: no additional API slices are currently planned
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

## Implemented Recipes Contract

Recipe creation and updates should support ingredient reuse, structured step references, and measurement normalization.

### Supported Units Shape

```json
{
  "code": "g",
  "displayName": "Gram",
  "abbreviation": "g",
  "family": "mass",
  "baseUnitCode": "g",
  "conversionFactor": 1.0,
  "isConvertible": true
}
```

### Ingredient Shape

```json
{
  "id": "uuid",
  "name": "Chicken thighs",
  "normalizedName": "chicken thighs",
  "createdAt": "2026-03-14T12:00:00Z",
  "updatedAt": "2026-03-14T12:00:00Z"
}
```

### Create Or Update Recipe Request

```json
{
  "title": "Sheet Pan Chicken",
  "description": "Weeknight dinner",
  "servings": 4,
  "prepTimeMinutes": 15,
  "cookTimeMinutes": 30,
  "sourceUrl": "https://example.com/recipe",
  "ingredients": [
    {
      "ingredientId": null,
      "name": "Chicken thighs",
      "referenceKey": "chicken-thighs",
      "quantity": 900.0,
      "unitCode": "g",
      "preparationNote": null,
      "sortOrder": 1
    }
  ],
  "steps": [
    {
      "instruction": "Pat the chicken thighs dry and season them.",
      "sortOrder": 1,
      "durationMinutes": 10,
      "ingredientReferenceKeys": ["chicken-thighs"]
    }
  ],
  "media": []
}
```

Notes:

- create requests commonly use `media: []` because uploaded media requires an existing recipe id
- recipe writes may still include externally hosted media rows when `storageKey` is omitted and `url` is an absolute URL
- backend-managed uploaded media rows are valid in recipe writes only after upload, using `storageKey`, a rooted relative backend `url`, and `contentType`

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
      "ingredientId": "uuid",
      "name": "Chicken thighs",
      "referenceKey": "chicken-thighs",
      "quantity": 2.0,
      "unitCode": "lb",
      "normalizedQuantity": 907.184,
      "normalizedUnitCode": "g",
      "preparationNote": null,
      "sortOrder": 1
    }
  ],
  "steps": [
    {
      "id": "uuid",
      "instruction": "Preheat the oven.",
      "sortOrder": 1,
      "durationMinutes": 10,
      "ingredientReferences": [
        {
          "recipeIngredientId": "uuid",
          "ingredientId": "uuid",
          "referenceKey": "chicken-thighs"
        }
      ]
    }
  ],
  "media": [
    {
      "id": "uuid",
      "kind": "image",
      "storageKey": "recipes/user-id/recipe-id/sheet-pan.jpg",
      "url": "/api/v1/media/recipes/user-id/recipe-id/sheet-pan.jpg",
      "contentType": "image/jpeg",
      "caption": "Finished dish",
      "sortOrder": 1
    }
  ],
  "createdAt": "2026-03-14T12:00:00Z",
  "updatedAt": "2026-03-14T12:00:00Z"
}
```

Implemented endpoints:

- `GET /api/v1/recipes`
- `POST /api/v1/recipes`
- `GET /api/v1/recipes/{id}`
- `PUT /api/v1/recipes/{id}`
- `DELETE /api/v1/recipes/{id}`
- `GET /api/v1/ingredients`
- `POST /api/v1/ingredients`
- `GET /api/v1/ingredients/{id}`
- `PUT /api/v1/ingredients/{id}`
- `DELETE /api/v1/ingredients/{id}`
- `GET /api/v1/units`

Implemented behavior:

- recipe requests may either reference an existing `ingredientId` or provide a new `name` for automatic ingredient creation within the authenticated user scope
- every newly created user receives a seeded starter ingredient catalog
- `referenceKey` must be unique within a recipe payload and is used to link steps to ingredient lines
- `unitCode` must come from the supported unit catalog
- normalized quantities are returned when the selected unit can be converted safely into that family base unit
- custom or non-convertible units are preserved as authored without guessed normalization
- ingredient deletion returns `409` when the ingredient is still referenced by at least one recipe
- recipe create and update are full-document writes for ingredients, steps, and media
- uploaded media URLs are backend-managed rooted relative paths when `storageKey` is present
- manually linked remote media may still use absolute URLs when `storageKey` is omitted

## Implemented Recipe Media Contract

### `POST /api/v1/recipes/{recipeId}/media`

Accepts `multipart/form-data`.

Fields:

- `kind`: `image` or `video`
- `caption`: optional text
- `sortOrder`: positive integer
- `file`: binary upload

Response:

```json
{
  "id": "uuid",
  "kind": "image",
  "storageKey": "recipes/user-id/recipe-id/uuid.jpg",
  "url": "/api/v1/media/recipes/user-id/recipe-id/uuid.jpg",
  "contentType": "image/jpeg",
  "caption": "Finished dish",
  "sortOrder": 1
}
```

### `GET /api/v1/media/{**storageKey}`

Returns the uploaded file content for the current user when the storage key belongs to one of that user's recipe media assets.

### `DELETE /api/v1/recipes/{recipeId}/media/{mediaId}`

Deletes the media asset metadata and the stored uploaded file.

Implemented behavior:

- uploads are user-scoped through the owning recipe
- the upload endpoint persists a recipe media asset immediately so the returned metadata can be reused in later recipe updates
- uploaded media responses include `storageKey`, `url`, and `contentType`
- content access stays authenticated and ownership-bound rather than using a public static-files path
- removing uploaded media through recipe updates or recipe deletion cleans up stored files

## Implemented Meal Plans Contract

### Create Or Update Meal Plan Request

```json
{
  "title": "Week of March 16",
  "startDate": "2026-03-16",
  "endDate": "2026-03-22",
  "slots": [
    {
      "referenceKey": "breakfast",
      "name": "Breakfast",
      "sortOrder": 1,
      "isDefault": true
    },
    {
      "referenceKey": "snack",
      "name": "Snack",
      "sortOrder": 2,
      "isDefault": false
    }
  ],
  "entries": [
    {
      "plannedDate": "2026-03-16",
      "mealSlotReferenceKey": "breakfast",
      "recipeId": "uuid",
      "servingsOverride": 6,
      "note": "Use leftovers"
    }
  ]
}
```

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
      "referenceKey": "breakfast",
      "name": "Breakfast",
      "sortOrder": 1,
      "isDefault": true
    },
    {
      "id": "uuid",
      "referenceKey": "snack",
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
      "mealSlotReferenceKey": "breakfast",
      "recipeId": "uuid",
      "recipeTitle": "Sheet Pan Chicken",
      "servingsOverride": 6,
      "note": "Use leftovers"
    }
  ]
}
```

Implemented endpoints:

- `GET /api/v1/meal-plans`
- `POST /api/v1/meal-plans`
- `GET /api/v1/meal-plans/{id}`
- `PUT /api/v1/meal-plans/{id}`
- `DELETE /api/v1/meal-plans/{id}`

Implemented behavior:

- meal plans are user-scoped
- `slots` must use unique `referenceKey`, unique `name`, and unique `sortOrder` values within the payload
- entries reference slots by `mealSlotReferenceKey` during writes and return both the saved slot id and reference key in responses
- entry dates must stay inside the plan range
- a slot can only have one planned recipe per day

## Implemented Grocery Lists Contract

### Generate Grocery List Request

```json
{
  "mealPlanId": "uuid"
}
```

### Grocery List Shape

```json
{
  "id": "uuid",
  "mealPlanId": "uuid",
  "startDate": "2026-03-16",
  "endDate": "2026-03-22",
  "generatedAt": "2026-03-15T18:00:00Z",
  "items": [
    {
      "id": "uuid",
      "ingredientId": "uuid",
      "name": "Chicken thighs",
      "quantity": 1814.36948,
      "unitCode": "g",
      "isChecked": false,
      "sourceCount": 2
    }
  ]
}
```

Implemented endpoints:

- `POST /api/v1/grocery-lists/generate`
- `GET /api/v1/grocery-lists/{id}`
- `PUT /api/v1/grocery-lists/{id}/items/{itemId}`

### Update Grocery List Item Request

```json
{
  "isChecked": true
}
```

Implemented behavior:

- grocery generation is user-scoped and starts from an existing meal plan
- recipe ingredient quantities scale by `servingsOverride` when present
- normalized recipe measurements aggregate by `ingredientId` and normalized unit code
- non-normalized measurements only aggregate when the ingredient and authored unit code match exactly
- generated grocery lists are snapshots that can track item checkoff state after generation
- recipe deletion returns `409` when the recipe is still scheduled in at least one meal plan

## Implemented Import Contract

### Create Recipe Import Request

```json
{
  "sourceUrl": "https://example.com/recipes/sheet-pan-chicken"
}
```

### Recipe Import Shape

```json
{
  "id": "uuid",
  "sourceType": "url",
  "sourceUrl": "https://example.com/recipes/sheet-pan-chicken",
  "status": "needs_review",
  "draft": {
    "title": "Sheet Pan Chicken",
    "description": null,
    "servings": null,
    "prepTimeMinutes": null,
    "cookTimeMinutes": null,
    "sourceUrl": "https://example.com/recipes/sheet-pan-chicken",
    "ingredients": [],
    "steps": [],
    "media": []
  },
  "warnings": [
    "This import foundation only infers a starter draft from the source URL. Review and complete the recipe before saving it."
  ],
  "createdAt": "2026-03-15T18:00:00Z",
  "updatedAt": "2026-03-15T18:00:00Z"
}
```

Implemented endpoints:

- `POST /api/v1/recipe-imports`
- `GET /api/v1/recipe-imports/{id}`

Implemented behavior:

- recipe imports are user-scoped
- the current foundation accepts a source URL and creates a reviewable draft instead of creating a recipe immediately
- the returned draft uses recipe field names so the frontend can hydrate the normal recipe create/edit flow
- the foundation infers only starter values and may leave recipe fields incomplete for review

## Frontend Integration Notes

- build query hooks around auth, recipes, meal plans, grocery lists, imports, and media
- treat field names here as contract names unless changed jointly
- keep mocked recipe, meal-plan, and grocery data realistic
