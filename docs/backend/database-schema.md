# Database Schema Notes

## Current Schema

The current persisted schema only needs the auth foundation.

### `users`

- `id` uuid primary key
- `email` varchar(200) unique not null
- `display_name` varchar(100) not null
- `password_hash` varchar(500) not null
- `created_at` timestamp with time zone not null
- `updated_at` timestamp with time zone not null

## Planned Tables

### `recipes`

Expected fields:

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

Expected fields:

- `id`
- `recipe_id`
- `name`
- `quantity`
- `unit`
- `preparation_note`
- `sort_order`

### `recipe_steps`

Expected fields:

- `id`
- `recipe_id`
- `instruction`
- `duration_minutes`
- `sort_order`

### `recipe_media_assets`

Expected fields:

- `id`
- `recipe_id`
- `kind`
- `storage_key`
- `url`
- `caption`
- `sort_order`

### `meal_plans`

Expected fields:

- `id`
- `user_id`
- `title`
- `start_date`
- `end_date`
- `created_at`
- `updated_at`

### `meal_slots`

Expected fields:

- `id`
- `meal_plan_id`
- `name`
- `sort_order`
- `is_default`

### `planned_meals`

Expected fields:

- `id`
- `meal_plan_id`
- `meal_slot_id`
- `planned_date`
- `recipe_id`
- `servings_override`
- `note`

### `grocery_lists`

Expected fields:

- `id`
- `user_id`
- `meal_plan_id`
- `start_date`
- `end_date`
- `generated_at`

### `grocery_list_items`

Expected fields:

- `id`
- `grocery_list_id`
- `name`
- `quantity`
- `unit`
- `is_checked`
- `source_count`

## Schema Rules

- all feature tables should remain user-scoped either directly or through a parent relationship
- quantity fields should be decimal-safe when exactness matters
- generated grocery data should be reproducible from plan and recipe state
