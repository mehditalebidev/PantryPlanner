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

### RecipeIngredient

Represents one ingredient line inside a recipe.

Key fields:

- `Id`
- `RecipeId`
- `Name`
- `Quantity`
- `Unit`
- `PreparationNote` nullable
- `SortOrder`

Rules:

- Quantity should use decimal-safe storage when persisted.
- Unit stays user-facing and flexible enough for common kitchen language.
- Sort order preserves recipe author intent.

### RecipeStep

Represents one ordered preparation step.

Key fields:

- `Id`
- `RecipeId`
- `Instruction`
- `SortOrder`
- `DurationMinutes` nullable

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
- `Name`
- `SortOrder`
- `IsDefault`

Rules:

- Users can keep defaults or add custom slots.
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
- `Name`
- `Quantity`
- `Unit`
- `IsChecked`
- `SourceCount`

## Future Entities

### ImportSource

Represents structured metadata about a recipe import origin.

### PantryItem

Represents ingredients already on hand and available for grocery adjustments.

## Business Rules To Preserve

- All reads and writes are scoped by authenticated user id.
- Decimal-safe quantities are preferred over floating point when exactness matters.
- Meal-slot flexibility matters more than forcing a fixed breakfast-lunch-dinner model.
- Grocery lists are generated projections, not permanently trusted source records.
- Import support should enrich recipes without replacing the local recipe model.
