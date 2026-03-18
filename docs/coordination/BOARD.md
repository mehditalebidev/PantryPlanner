# Coordination Board

This board is the current source of truth for what is ready, active, blocked, or complete.

Update it whenever work changes status.

## Ready

| ID | Title | Notes |
| --- | --- | --- |
| FE-001 | Initialize frontend shell | Set up the real frontend scaffold with routing, providers, and auth shell wiring. |

## Backlog

No backlog items currently queued.

## In Progress

| ID | Title | Notes |
| --- | --- | --- |
| AUTH-001 | Preserve auth scaffold during PantryPlanner pivot | Keep signup, login, `users/me`, JWT wiring, and tests healthy while new slices are introduced. |
| REC-001 | Build recipe library vertical slice | Recipe CRUD, ingredient CRUD, seeded starter ingredients, structured step references, and dedicated backend units support are implemented locally; branch/PR still pending. |
| PLAN-001 | Build meal-plan vertical slice | Backend meal-plan CRUD with flexible slots, stable slot references, and recipe scheduling is implemented locally; branch/PR still pending. |
| GROC-001 | Build grocery-list generation slice | Backend grocery generation, snapshot persistence, and item checkoff are implemented locally; branch/PR still pending. |
| IMP-001 | Add recipe import foundation | Backend recipe-import foundation is implemented locally with URL-based draft creation and persisted review payloads; branch/PR still pending. |
| MEDIA-001 | Harden recipe media upload flow | Backend recipe media upload flow is implemented locally with protected content serving, storage-backed uploads, and cleanup on recipe changes; branch/PR still pending. |
| QUAL-001 | Consolidate backend endpoint slice files | Flat one-file endpoint modules, command-local mapping helpers, direct `PantryPlannerDbContext` usage, and domain/persistence files moved out of `Features/` are implemented locally; branch/PR still pending. |

## In Review

No coordination-tracked product item is currently in review.

## Blocked

No blocked items yet.

## Done

| ID | Title | Outcome |
| --- | --- | --- |
| SETUP-001 | Rename repo scaffold to PantryPlanner | Renamed solution, projects, namespaces, env/config keys, and docs away from the previous product identity. |
| SETUP-002 | Reset shared planning docs for PantryPlanner | Rewrote product, domain, API, roadmap, and workspace docs around recipes, meal plans, groceries, imports, and media. |

## Board Maintenance Rules

- keep only actionable items in `Ready`
- before starting a new item, review entries in `In Review` and move merged work to `Done`
- move an item to `In Progress` as soon as an implementer starts work on it
- include the PR link in `In Review` once a branch is pushed and reviewed
- move items to `Done` only after a human merges the PR
