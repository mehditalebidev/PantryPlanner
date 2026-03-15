# Pages And Routes

## Public Routes

### `/login`

- local email/password login
- links to signup

### `/signup`

- account creation form
- immediate transition into authenticated app shell after success

## Authenticated Routes

### `/recipes`

- recipe library list
- search, filters, and quick actions when implemented

### `/recipes/new`

- recipe creation form
- ingredient, step, and media sections

### `/recipes/:id`

- recipe detail view
- ingredients, steps, metadata, and media

### `/recipes/:id/edit`

- recipe editing form

### `/meal-plans`

- list of planning windows

### `/meal-plans/:id`

- planner view for one range
- slot-based schedule for each day

### `/meal-plans/:id/edit`

- plan metadata and slot configuration

### `/grocery-lists/:id`

- aggregated shopping list
- item checkoff state

### `/imports/new`

- submit recipe source to import
- review and edit parsed result before saving
