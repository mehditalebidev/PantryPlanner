# Domain Model

## Core Principles

- Every record belongs to exactly one local application user unless collaboration is intentionally added later.
- Recipes, meal plans, grocery lists, imports, and media are separate concepts even when they reference one another.
- Ingredient quantities should preserve exactness where practical.
- Meal planning should stay flexible enough for custom slots and variable planning windows.

## Phase 1 Entities

### User

Represents the local application account owner.

Key fields:

- `Id`
- `Email`
- `DisplayName`
- `PasswordHash`
- `CreatedAt`
- `UpdatedAt`

Rules:

- Email is unique and normalized for login.
- Password hashes are stored only for local email/password auth.
- A user owns recipes, meal plans, grocery lists, imports, and media assets.

### Recipe

Represents a cookable item the user wants to save or plan.

Key fields:

- `Id`
- `UserId`
- `Title`
- `Description`
- `Servings`
- `PrepTimeMinutes`
- `CookTimeMinutes`
- `SourceUrl` nullable
- `CreatedAt`
- `UpdatedAt`

Rules:

- Recipe titles should be meaningful within a user account.
- `SourceUrl` is optional because recipes can be manual or imported.
- A recipe owns ordered ingredients, steps, and media references.
- Recipe ingredients keep both authored measurement data and normalized measurement data when the selected unit is convertible.

### Ingredient

Represents a reusable ingredient concept owned by a user.

Key fields:

- `Id`
- `UserId`
- `Name`
- `NormalizedName`
- `CreatedAt`
- `UpdatedAt`

Rules:

- Ingredients are user-scoped and can be reused across recipes.
- `NormalizedName` supports case-insensitive matching and deduplication within one user account.
- Ingredient identity is separate from the amount used in a specific recipe.
- New users receive a seeded starter ingredient catalog that they can later edit and extend.

### UnitDefinition

Represents an application-level measurement definition used for validation and conversion.

Key fields:

- `Code`
- `DisplayName`
- `Abbreviation`
- `Family`
- `BaseUnitCode`
- `ConversionFactor`
- `IsConvertible`

Rules:

- Unit definitions are shared reference data rather than user-owned records.
- Unit definitions are exposed through a dedicated backend slice and endpoint.
- Automatic conversion is only supported within the same family such as mass, volume, or count.
- Cross-family conversion is out of scope until ingredient-specific density data exists.
- Custom or ambiguous kitchen units may be valid for recipes but may not produce normalized quantities.

### RecipeIngredient

Represents one ingredient line inside a recipe.

Key fields:

- `Id`
- `RecipeId`
- `IngredientId`
- `ReferenceKey`
- `Quantity`
- `UnitCode`
- `NormalizedQuantity` nullable
- `NormalizedUnitCode` nullable
- `PreparationNote` nullable
- `SortOrder`

Rules:

- Quantity should use decimal-safe storage when persisted.
- Each recipe ingredient points to a reusable `Ingredient` entity.
- `ReferenceKey` is stable within a recipe and lets steps reference ingredients before database ids exist on the client.
- `UnitCode` should come from the supported unit definition catalog.
- `NormalizedQuantity` and `NormalizedUnitCode` are stored only when conversion is safe and deterministic.
- Sort order preserves recipe author intent.

### RecipeStep

Represents one ordered preparation step.

Key fields:

- `Id`
- `RecipeId`
- `Instruction`
- `SortOrder`
- `DurationMinutes` nullable

Rules:

- Steps remain free-text instructions.
- Steps can reference one or more recipe ingredients through structured links.

### RecipeStepIngredientReference

Represents a structured link from a recipe step to a recipe ingredient.

Key fields:

- `Id`
- `RecipeStepId`
- `RecipeIngredientId`

Rules:

- Links let the client know which ingredients are mentioned in a step without forcing the instruction text into a rigid token format.
- A step should not link to the same recipe ingredient more than once.

### RecipeMediaAsset

Represents an image or video attached to a recipe.

Key fields:

- `Id`
- `RecipeId`
- `Kind`
- `StorageKey`
- `Url`
- `Caption` nullable
- `SortOrder`

Rules:

- Media metadata should be tracked even if binary upload handling evolves later.
- `Kind` starts simple with values such as `image` and `video`.

### MealPlan

Represents a planning window.

Key fields:

- `Id`
- `UserId`
- `Title`
- `StartDate`
- `EndDate`
- `CreatedAt`
- `UpdatedAt`

Rules:

- A meal plan can represent a week, two weeks, a month, or another range.
- `EndDate` must be on or after `StartDate`.
- A meal plan owns slots and planned meals for that range.

### MealSlot

Represents a named section within a day, such as breakfast or snack.

Key fields:

- `Id`
- `MealPlanId`
- `ReferenceKey`
- `Name`
- `SortOrder`
- `IsDefault`

Rules:

- Users can keep defaults or add custom slots.
- `ReferenceKey` is stable within a meal plan payload and lets planned meals reference slots before database ids exist on the client.
- Slot names should be unique within a meal plan.

### PlannedMeal

Represents a recipe scheduled into a meal slot for a day.

Key fields:

- `Id`
- `MealPlanId`
- `MealSlotId`
- `PlannedDate`
- `RecipeId`
- `ServingsOverride` nullable
- `Note` nullable

Rules:

- Planned dates must fall inside the owning meal plan range.
- Planned meals reference one recipe and one meal slot.
- A meal plan should not schedule more than one recipe into the same slot on the same date.

### GroceryList

Represents a generated shopping snapshot for a selected planning window.

Key fields:

- `Id`
- `UserId`
- `MealPlanId` nullable
- `StartDate`
- `EndDate`
- `GeneratedAt`

### GroceryListItem

Represents one aggregated ingredient entry.

Key fields:

- `Id`
- `GroceryListId`
- `IngredientId` nullable
- `Name`
- `Quantity`
- `UnitCode`
- `IsChecked`
- `SourceCount`

Rules:

- Grocery aggregation should group by `IngredientId` when compatible measurements can be normalized into the same unit family.
- When a normalized quantity exists, grocery output should prefer the normalized base unit code for deterministic aggregation.
- When automatic conversion is not safe, separate grocery entries should be preserved instead of guessing.

## Future Entities

### ImportSource

Represents structured metadata about a recipe import origin.

### PantryItem

Represents ingredients already on hand and available for grocery adjustments.

## Business Rules To Preserve

- All reads and writes are scoped by authenticated user id.
- Decimal-safe quantities are preferred over floating point when exactness matters.
- Reusable ingredient identity should stay separate from recipe-specific quantities and preparation notes.
- Unit conversion should only happen when the backend can prove it is safe within one measurement family.
- Meal-slot flexibility matters more than forcing a fixed breakfast-lunch-dinner model.
- Grocery lists are generated projections, not permanently trusted source records.
- Import support should enrich recipes without replacing the local recipe model.
