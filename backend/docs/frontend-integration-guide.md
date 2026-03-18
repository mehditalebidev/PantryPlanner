# Frontend Integration Guide

This guide is for frontend developers building against the PantryPlanner backend today. It explains what is implemented, how the entities relate to each other, which endpoint order to use, and which payload shapes the UI should send.

Use this guide together with `docs/shared/api-contract.md` when you need the full contract reference. This file focuses on practical frontend usage.

## What The Backend Supports Today

- auth: signup, login, current user
- ingredients: seeded starter catalog plus full CRUD
- units: shared lookup for supported measurement units
- recipes: full CRUD with reusable ingredients, ordered steps, structured step references, normalization, and media metadata
- recipe media: upload, authenticated content delivery, and delete
- meal plans: full CRUD with flexible slots and dated recipe scheduling
- grocery lists: generation from meal plans plus item checkoff
- recipe imports: URL-based starter draft generation plus persisted review lookup

## API Basics

### Base Rules

- base route prefix: `/api/v1`
- all authenticated requests require `Authorization: Bearer <token>`
- request and response bodies are JSON unless the endpoint explicitly uses multipart upload
- timestamps are ISO 8601 strings
- quantities are decimal-compatible JSON numbers

### Error Shape

Backend errors use ProblemDetails-style JSON.

Example:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Recipe was not found.",
  "status": 404,
  "detail": "The requested recipe does not exist.",
  "code": "recipe_not_found"
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

Frontend should always read:

- `status`
- `title`
- `detail`
- `code`
- `errors` for field-level validation cases

## Recommended Frontend HTTP Helpers

Use one JSON helper and one multipart helper.

```ts
const API_BASE = "/api/v1";

type ApiError = {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  code?: string;
  errors?: Record<string, string[]>;
};

async function apiJson<T>(path: string, init: RequestInit = {}, token?: string): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init.headers ?? {})
    }
  });

  if (!response.ok) {
    throw (await response.json()) as ApiError;
  }

  return (await response.json()) as T;
}

async function apiForm<T>(path: string, form: FormData, token: string): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`
    },
    body: form
  });

  if (!response.ok) {
    throw (await response.json()) as ApiError;
  }

  return (await response.json()) as T;
}
```

## Suggested App Startup Flow

For a signed-in app shell, this is the most useful order:

1. login or restore token
2. call `GET /users/me`
3. call `GET /units`
4. call `GET /ingredients`
5. lazily load recipes, meal plans, grocery lists, or imports per route

Why this order:

- `users/me` confirms the token is still valid
- `units` powers recipe forms immediately
- `ingredients` is needed by recipe create and edit screens

## Core Domain Relationships

### User

- owns all user-scoped data
- receives a seeded starter ingredient catalog on signup

### Ingredient

- reusable user-scoped ingredient record
- can exist before any recipe uses it
- may be referenced by many recipe ingredient rows

Important fields:

- `id`
- `name`
- `normalizedName`
- `createdAt`
- `updatedAt`

### Recipe

- user-scoped aggregate root
- owns ingredients, steps, and media arrays
- may be scheduled in meal plans

Important fields:

- `id`
- `title`
- `description`
- `servings`
- `prepTimeMinutes`
- `cookTimeMinutes`
- `sourceUrl`
- `ingredients`
- `steps`
- `media`

### RecipeIngredient

- one ingredient line inside one recipe
- points to a reusable `Ingredient`
- stores both authored measurement data and normalized measurement data when conversion is safe

Important fields:

- `id`
- `ingredientId`
- `name`
- `referenceKey`
- `quantity`
- `unitCode`
- `normalizedQuantity`
- `normalizedUnitCode`
- `preparationNote`
- `sortOrder`

### RecipeStep

- ordered instruction row inside one recipe
- links to ingredient rows through structured references

Important fields:

- `id`
- `instruction`
- `sortOrder`
- `durationMinutes`
- `ingredientReferences`

### RecipeMediaAsset

- media metadata row attached to a recipe
- may point to backend-managed uploaded content through `storageKey`

Important fields:

- `id`
- `kind`
- `storageKey`
- `url`
- `contentType`
- `caption`
- `sortOrder`

### MealPlan

- user-scoped planning aggregate
- owns slots and entries

### MealSlot

- one configurable slot inside a plan, such as breakfast or dinner
- `referenceKey` is the stable nested write identifier the frontend should keep while editing

### PlannedMeal

- one recipe scheduled in one slot on one date
- may override servings for grocery generation

### GroceryList

- generated snapshot derived from one meal plan
- not a live view over recipes

### GroceryListItem

- aggregated shopping row
- checkoff state is mutable after generation

### RecipeImport

- review artifact, not a saved recipe
- returns a recipe-shaped `draft` the frontend can feed into the normal recipe form

## Units And Measurement Rules

`GET /units` returns the supported unit catalog.

Example:

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

Frontend rules:

- never hardcode the unit list
- use unit `code` in write payloads
- display `displayName` or `abbreviation` in the UI
- do not recalculate normalized quantities on the client
- use backend-returned `normalizedQuantity` and `normalizedUnitCode` as display-only canonical values

Backend rules:

- normalization only happens when the selected unit can safely convert within its family
- non-convertible units stay as authored
- grocery generation later uses those canonical normalized values

## Auth Endpoints

### `POST /auth/signup`

Use this to create a new user and get the first token.

Request:

```json
{
  "email": "jane@example.com",
  "displayName": "Jane Doe",
  "password": "Password123!"
}
```

Response:

```json
{
  "accessToken": "jwt",
  "expiresAt": "2026-03-15T12:00:00Z",
  "user": {
    "id": "uuid",
    "email": "jane@example.com",
    "displayName": "Jane Doe"
  }
}
```

Important behavior:

- signup also seeds the starter ingredient catalog for the user

### `POST /auth/login`

Use this to sign an existing user in.

Request:

```json
{
  "email": "jane@example.com",
  "password": "Password123!"
}
```

### `GET /users/me`

Use this to hydrate the current session from a stored token.

## Ingredient Endpoints

Routes:

- `GET /ingredients`
- `GET /ingredients/{id}`
- `POST /ingredients`
- `PUT /ingredients/{id}`
- `DELETE /ingredients/{id}`

### `GET /ingredients`

Use this for ingredient pickers, search, autocomplete, and library screens.

Example response:

```json
[
  {
    "id": "uuid",
    "name": "Salt",
    "normalizedName": "salt",
    "createdAt": "2026-03-15T10:00:00Z",
    "updatedAt": "2026-03-15T10:00:00Z"
  },
  {
    "id": "uuid",
    "name": "Olive oil",
    "normalizedName": "olive oil",
    "createdAt": "2026-03-15T10:00:00Z",
    "updatedAt": "2026-03-15T10:00:00Z"
  }
]
```

### `POST /ingredients`

Use this when the user wants a custom ingredient outside the seeded starter set.

Request:

```json
{
  "name": "Black garlic"
}
```

### `PUT /ingredients/{id}`

Use this for rename flows.

Request:

```json
{
  "name": "Roasted garlic"
}
```

### `DELETE /ingredients/{id}`

Use this only when the ingredient is not referenced by recipes.

Frontend behavior:

- show a clear confirmation before delete
- handle `409 ingredient_in_use_by_recipe` with a friendly message like "Remove this ingredient from recipes first"

## Recipe Endpoints

Routes:

- `GET /recipes`
- `GET /recipes/{id}`
- `POST /recipes`
- `PUT /recipes/{id}`
- `DELETE /recipes/{id}`

### How Recipe Writes Work

Recipe create and update requests are full document writes.

That means the frontend should send:

- all ingredient rows
- all step rows
- all media rows that should remain attached to the recipe

If an existing uploaded media item is omitted from a recipe update payload, the backend treats it as removed and cleans up the stored file.

### Create Recipe Example

```json
{
  "title": "Sheet Pan Chicken",
  "description": "Weeknight dinner",
  "servings": 4,
  "prepTimeMinutes": 15,
  "cookTimeMinutes": 30,
  "sourceUrl": "https://example.com/recipes/sheet-pan-chicken",
  "ingredients": [
    {
      "ingredientId": null,
      "name": "Chicken thighs",
      "referenceKey": "chicken",
      "quantity": 2.0,
      "unitCode": "lb",
      "preparationNote": null,
      "sortOrder": 1
    },
    {
      "ingredientId": null,
      "name": "Salt",
      "referenceKey": "salt",
      "quantity": 1.0,
      "unitCode": "tsp",
      "preparationNote": null,
      "sortOrder": 2
    }
  ],
  "steps": [
    {
      "instruction": "Season the chicken with salt.",
      "sortOrder": 1,
      "durationMinutes": 5,
      "ingredientReferenceKeys": ["chicken", "salt"]
    },
    {
      "instruction": "Roast until cooked through.",
      "sortOrder": 2,
      "durationMinutes": 30,
      "ingredientReferenceKeys": ["chicken"]
    }
  ],
  "media": []
}
```

Important notes:

- `referenceKey` only needs to be stable inside the current recipe payload
- use `ingredientId` when the user picked an existing ingredient
- use `name` when the user typed a new ingredient and wants the backend to create or reuse it
- `media` is often empty on create because uploads require an existing recipe id
- if you want to attach externally hosted media without uploading it through PantryPlanner, omit `storageKey`, use an absolute `url`, and leave `contentType` optional

### Create Recipe Response Example

```json
{
  "id": "recipe-uuid",
  "title": "Sheet Pan Chicken",
  "description": "Weeknight dinner",
  "servings": 4,
  "prepTimeMinutes": 15,
  "cookTimeMinutes": 30,
  "sourceUrl": "https://example.com/recipes/sheet-pan-chicken",
  "ingredients": [
    {
      "id": "recipe-ingredient-uuid",
      "ingredientId": "ingredient-uuid",
      "name": "Chicken thighs",
      "referenceKey": "chicken",
      "quantity": 2.0,
      "unitCode": "lb",
      "normalizedQuantity": 907.18474,
      "normalizedUnitCode": "g",
      "preparationNote": null,
      "sortOrder": 1
    }
  ],
  "steps": [
    {
      "id": "step-uuid",
      "instruction": "Season the chicken with salt.",
      "sortOrder": 1,
      "durationMinutes": 5,
      "ingredientReferences": [
        {
          "recipeIngredientId": "recipe-ingredient-uuid",
          "ingredientId": "ingredient-uuid",
          "referenceKey": "chicken"
        }
      ]
    }
  ],
  "media": [],
  "createdAt": "2026-03-15T12:00:00Z",
  "updatedAt": "2026-03-15T12:00:00Z"
}
```

### Update Recipe Example

Use `PUT /recipes/{id}` with the full desired final shape.

This is especially important for `media`.

Example update after one uploaded image already exists:

```json
{
  "title": "Sheet Pan Chicken",
  "description": "Weeknight dinner with lemon",
  "servings": 4,
  "prepTimeMinutes": 20,
  "cookTimeMinutes": 30,
  "sourceUrl": "https://example.com/recipes/sheet-pan-chicken",
  "ingredients": [
    {
      "ingredientId": "ingredient-uuid",
      "name": null,
      "referenceKey": "chicken",
      "quantity": 2.0,
      "unitCode": "lb",
      "preparationNote": null,
      "sortOrder": 1
    }
  ],
  "steps": [
    {
      "instruction": "Roast the chicken.",
      "sortOrder": 1,
      "durationMinutes": 30,
      "ingredientReferenceKeys": ["chicken"]
    }
  ],
  "media": [
    {
      "kind": "image",
      "storageKey": "recipes/user-id/recipe-id/file.jpg",
      "url": "/api/v1/media/recipes/user-id/recipe-id/file.jpg",
      "contentType": "image/jpeg",
      "caption": "Updated caption",
      "sortOrder": 1
    }
  ]
}
```

Important notes:

- when `storageKey` is present, `url` must be the rooted relative backend media URL
- when `storageKey` is present, `contentType` is required
- when `storageKey` is omitted, `url` must be an absolute URL
- if you omit a previously uploaded media row from this array, that media file is deleted

### Delete Recipe

Handle these cases:

- `409 recipe_in_use_by_meal_plan` if the recipe is still scheduled
- uploaded files are also deleted when recipe deletion succeeds

## Recipe Media Endpoints

Routes:

- `POST /recipes/{recipeId}/media`
- `GET /media/{**storageKey}`
- `DELETE /recipes/{recipeId}/media/{mediaId}`

### Recommended Media Flow

Use this sequence:

1. create or load the recipe
2. upload media through `POST /recipes/{recipeId}/media`
3. use the returned media object to update local recipe form state
4. when saving the recipe later, include all media rows you want to keep in the recipe `PUT` payload

### Upload Media Example

```ts
async function uploadRecipeImage(recipeId: string, file: File, token: string) {
  const form = new FormData();
  form.append("kind", "image");
  form.append("caption", "Finished dish");
  form.append("sortOrder", "1");
  form.append("file", file);

  return apiForm<RecipeMediaAssetResponse>(`/recipes/${recipeId}/media`, form, token);
}
```

Example response:

```json
{
  "id": "media-uuid",
  "kind": "image",
  "storageKey": "recipes/user-id/recipe-id/uuid.jpg",
  "url": "/api/v1/media/recipes/user-id/recipe-id/uuid.jpg",
  "contentType": "image/jpeg",
  "caption": "Finished dish",
  "sortOrder": 1
}
```

### Render Uploaded Media

Because `GET /media/{**storageKey}` is authenticated, normal public-image assumptions do not apply.

The safest frontend approach is to fetch the file with auth and create an object URL.

```ts
async function loadProtectedMedia(url: string, token: string): Promise<string> {
  const response = await fetch(url, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  if (!response.ok) {
    throw new Error("Unable to load media");
  }

  const blob = await response.blob();
  return URL.createObjectURL(blob);
}
```

### Delete Uploaded Media

Use `DELETE /recipes/{recipeId}/media/{mediaId}` when the UI explicitly removes one media item.

If you instead remove the media row from the recipe `PUT` payload, the backend also cleans it up.

## Meal Plan Endpoints

Routes:

- `GET /meal-plans`
- `GET /meal-plans/{id}`
- `POST /meal-plans`
- `PUT /meal-plans/{id}`
- `DELETE /meal-plans/{id}`

### How Meal Plan Writes Work

Meal-plan writes are also full document writes.

The frontend should send:

- all slots
- all entries

Entries do not refer to slot ids during writes. They refer to `mealSlotReferenceKey`.

### Create Meal Plan Example

```json
{
  "title": "Week 1",
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
      "referenceKey": "dinner",
      "name": "Dinner",
      "sortOrder": 2,
      "isDefault": true
    }
  ],
  "entries": [
    {
      "plannedDate": "2026-03-16",
      "mealSlotReferenceKey": "dinner",
      "recipeId": "recipe-uuid",
      "servingsOverride": 2,
      "note": "Use leftovers for lunch"
    }
  ]
}
```

Frontend rules:

- keep slot `referenceKey` stable while editing
- do not use returned slot ids as write keys
- prevent duplicate slot names, duplicate sort orders, and duplicate slot/date entries in the form if possible before submit

## Grocery Endpoints

Routes:

- `POST /grocery-lists/generate`
- `GET /grocery-lists/{id}`
- `PUT /grocery-lists/{id}/items/{itemId}`

### Generate Grocery List Example

Request:

```json
{
  "mealPlanId": "meal-plan-uuid"
}
```

Response:

```json
{
  "id": "grocery-list-uuid",
  "mealPlanId": "meal-plan-uuid",
  "startDate": "2026-03-16",
  "endDate": "2026-03-22",
  "generatedAt": "2026-03-15T12:00:00Z",
  "items": [
    {
      "id": "item-uuid",
      "ingredientId": "ingredient-uuid",
      "name": "Chicken thighs",
      "quantity": 907.18474,
      "unitCode": "g",
      "isChecked": false,
      "sourceCount": 2
    }
  ]
}
```

Frontend rules:

- treat grocery lists as snapshots, not live computed state
- never recompute aggregation in the UI
- render `quantity` and `unitCode` exactly as returned

### Checkoff Update Example

```json
{
  "isChecked": true
}
```

## Recipe Import Endpoints

Routes:

- `POST /recipe-imports`
- `GET /recipe-imports/{id}`

### Import Flow

The current import feature is intentionally a foundation, not a full parser.

It works like this:

1. submit a recipe URL
2. backend creates a review artifact
3. backend returns a recipe-shaped `draft`
4. frontend opens the normal recipe editor prefilled with that draft
5. user reviews and completes the recipe
6. frontend saves through the normal `POST /recipes` endpoint

### Create Import Example

Request:

```json
{
  "sourceUrl": "https://example.com/recipes/sheet-pan-chicken"
}
```

Response:

```json
{
  "id": "import-uuid",
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
  "createdAt": "2026-03-15T12:00:00Z",
  "updatedAt": "2026-03-15T12:00:00Z"
}
```

Frontend rules:

- show the warnings prominently
- do not treat the import as a saved recipe
- map `draft` directly into recipe form state
- use `GET /recipe-imports/{id}` if the user resumes the review flow later

## Typical Screen-by-Screen Data Flow

### Auth Shell

1. login or signup
2. store token securely
3. call `GET /users/me`
4. prefetch `GET /units` and `GET /ingredients`

### Ingredient Library Screen

1. call `GET /ingredients`
2. optionally filter on the client for now
3. create custom ingredients with `POST /ingredients`
4. rename with `PUT /ingredients/{id}`
5. delete with `DELETE /ingredients/{id}` and handle `409`

### Recipe Create Screen

1. load `GET /ingredients`
2. load `GET /units`
3. build local ingredient rows with client-generated `referenceKey` values
4. build local step rows with `ingredientReferenceKeys`
5. submit `POST /recipes`
6. after create succeeds, allow media uploads against the returned recipe id

### Recipe Edit Screen

1. load `GET /recipes/{id}`
2. populate form with returned ordered arrays
3. upload media separately as needed
4. when saving, send the full desired recipe document through `PUT /recipes/{id}`
5. include every media row that should remain attached

### Meal Plan Screen

1. load `GET /recipes`
2. load `GET /meal-plans/{id}` or start from an empty form
3. keep slot `referenceKey` values stable locally
4. save through `POST` or `PUT`

### Grocery Screen

1. generate with `POST /grocery-lists/generate`
2. render returned snapshot
3. toggle item state with `PUT /grocery-lists/{id}/items/{itemId}`

### Recipe Import Screen

1. submit URL through `POST /recipe-imports`
2. open returned `draft` in the recipe editor
3. save final reviewed content through recipe create

## Common Frontend Mistakes To Avoid

- do not hardcode unit options
- do not compute normalized values in the client
- do not treat recipe and meal-plan `PUT` requests as partial patches
- do not omit existing media rows from a recipe update unless you want them deleted
- do not assume media URLs are public; they are authenticated backend routes
- do not use slot ids as write keys for meal-plan entries; use `mealSlotReferenceKey`
- do not treat recipe imports as recipes; they are review artifacts only

## Common Error Cases To Handle Well

- `401 unauthorized`: token missing or expired
- `404 ingredient_not_found`: stale ingredient route or cross-user access
- `404 recipe_not_found`: stale recipe route or cross-user access
- `404 meal_plan_not_found`: stale meal-plan route or cross-user access
- `404 grocery_list_not_found`: stale grocery-list route or cross-user access
- `404 recipe_import_not_found`: stale import route or cross-user access
- `404 recipe_media_not_found` or `404 media_content_not_found`: removed media or cross-user access
- `409 ingredient_name_in_use`: duplicate ingredient create or rename
- `409 ingredient_in_use_by_recipe`: delete blocked by recipe usage
- `409 recipe_in_use_by_meal_plan`: delete blocked by schedule usage
- `400 validation_failed`: invalid field values, duplicate `referenceKey`, bad unit code, invalid media payload, or other request-shape issues

## Seeded Ingredient Catalog

Every new user receives a large starter ingredient catalog.

Examples include:

- `Salt`
- `Olive oil`
- `Garlic`
- `Chicken thighs`
- `Rice`
- `Paprika`
- `Soy sauce`
- `Tomatoes`

Frontend implication:

- autocomplete and ingredient pickers are useful immediately after signup
- custom ingredient creation should still remain available
