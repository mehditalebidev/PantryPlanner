# Database Schema Notes

## Current Schema

The current persisted schema includes auth plus the recipe library foundation.

### `users`

- `id` uuid primary key
- `email` varchar(200) unique not null
- `display_name` varchar(100) not null
- `password_hash` varchar(500) not null
- `created_at` timestamp with time zone not null
- `updated_at` timestamp with time zone not null

## Current Recipe Tables

The recipe library uses reusable ingredients plus application-defined unit definitions. Unit definitions remain seeded reference data in backend code for MVP, while recipe rows persist normalized quantities so later planner and grocery slices can reuse them.

### `ingredients`

Current fields:

- `id`
- `user_id`
- `name`
- `normalized_name`
- `created_at`
- `updated_at`

Notes:

- unique index on (`user_id`, `normalized_name`)

### `recipes`

Current fields:

- `id`
- `user_id`
- `title`
- `description`
- `servings`
- `prep_time_minutes`
- `cook_time_minutes`
- `source_url`
- `created_at`
- `updated_at`

### `recipe_ingredients`

Current fields:

- `id`
- `recipe_id`
- `ingredient_id`
- `reference_key`
- `quantity`
- `unit_code`
- `normalized_quantity`
- `normalized_unit_code`
- `preparation_note`
- `sort_order`

Notes:

- `quantity` and `normalized_quantity` should use a decimal-safe numeric type
- `reference_key` should be unique within one recipe
- normalized fields stay nullable for non-convertible units

### `recipe_steps`

Current fields:

- `id`
- `recipe_id`
- `instruction`
- `duration_minutes`
- `sort_order`

### `recipe_step_ingredient_references`

Current fields:

- `id`
- `recipe_step_id`
- `recipe_ingredient_id`

Notes:

- unique index on (`recipe_step_id`, `recipe_ingredient_id`)

### `recipe_media_assets`

Current fields:

- `id`
- `recipe_id`
- `kind`
- `storage_key`
- `url`
- `caption`
- `sort_order`

## Seeded Reference Data

- the backend seeds a large starter ingredient catalog for each user
- unit definitions are stored in backend code and exposed through `GET /api/v1/units`

## Current Meal Planning And Grocery Tables

### `meal_plans`

Current fields:

- `id`
- `user_id`
- `title`
- `start_date`
- `end_date`
- `created_at`
- `updated_at`

### `meal_slots`

Current fields:

- `id`
- `meal_plan_id`
- `reference_key`
- `name`
- `sort_order`
- `is_default`

Notes:

- `reference_key`, `name`, and `sort_order` should each be unique within one meal plan

### `planned_meals`

Current fields:

- `id`
- `meal_plan_id`
- `meal_slot_id`
- `planned_date`
- `recipe_id`
- `servings_override`
- `note`

Notes:

- unique index on (`meal_plan_id`, `planned_date`, `meal_slot_id`)

### `grocery_lists`

Current fields:

- `id`
- `user_id`
- `meal_plan_id`
- `start_date`
- `end_date`
- `generated_at`

### `grocery_list_items`

Current fields:

- `id`
- `grocery_list_id`
- `ingredient_id`
- `name`
- `quantity`
- `unit_code`
- `is_checked`
- `source_count`

## Schema Rules

- all feature tables should remain user-scoped either directly or through a parent relationship
- quantity fields should be decimal-safe when exactness matters
- reusable ingredient identity should be stored separately from recipe-specific measurement data
- normalized quantities should only be persisted when the backend can convert safely inside one unit family
- generated grocery data should be reproducible from plan and recipe state
