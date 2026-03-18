# Worklog

Record meaningful project changes here in reverse chronological order.

## Entries

- 2026-03-18 | QUAL-001 | in_progress | local build | Consolidated backend endpoints so each endpoint folder has one `.cs` file with the action, request, validator, and handler, removed the thin generic repository layer, updated docs, and verified the backend solution build/tests pass.
- 2026-03-18 | QUAL-001 | in_progress | local build | Flattened single-file endpoint folders, moved response mapping extensions onto request types, relocated entities to `src/PantryPlanner.Api/Domain/` and EF configurations to `src/PantryPlanner.Api/Common/Persistence/Configurations/`, and kept backend build/tests green.
- 2026-03-15 | MEDIA-001 | in_progress | local build | Started backend recipe-media flow work for storage-backed uploads, protected content access, and cleanup behavior.
- 2026-03-15 | MEDIA-001 | in_progress | local build | Implemented the backend recipe-media upload/content/delete flow, added migrations/tests, and verified the full backend test suite passes.
- 2026-03-15 | IMP-001 | in_progress | local build | Started backend recipe-import foundation work for URL-based draft generation, persisted review state, and frontend-ready import contract docs.
- 2026-03-15 | IMP-001 | in_progress | local build | Implemented the backend recipe-import foundation with persisted URL-based review drafts, added migration/tests, and verified the backend test suite passes.
- 2026-03-15 | GROC-001 | in_progress | local build | Implemented backend grocery-list generation and item checkoff, added migrations, and verified backend tests pass.
- 2026-03-15 | PLAN-001 | in_progress | local build | Implemented backend meal-plan CRUD with nested slot references, recipe scheduling, and verified backend tests pass.
- 2026-03-15 | PLAN-001 | in_progress | local build | Started backend meal-plan slice work, including nested slot references, scheduling rules, and grocery-generation contract updates.
- 2026-03-15 | REC-001 | in_progress | local build | Split ingredients and units into dedicated backend slices, added seeded starter ingredients plus ingredient CRUD, finished recipe CRUD, and verified backend tests pass.
- 2026-03-15 | REC-001 | in_progress | local build | Started backend recipe slice work, including ingredient entities, structured step references, and unit conversion planning/docs.
- 2026-03-15 | SETUP-002 | done | local pivot | Rewrote shared, coordination, backend, frontend, and agent docs around the PantryPlanner product direction.
- 2026-03-15 | SETUP-001 | done | local pivot | Renamed the backend solution, projects, namespaces, config keys, and workspace files from the previous product identity to PantryPlanner.
- 2026-03-15 | AUTH-001 | in_progress | local pivot | Preserved local email/password auth, JWT issuance, `users/me`, migrations, and tests as the initial PantryPlanner backend scaffold.
