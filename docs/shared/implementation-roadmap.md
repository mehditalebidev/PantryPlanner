# Implementation Roadmap

## High-Level Sequence

1. finish the PantryPlanner pivot and keep docs aligned with the new product direction
2. keep the backend auth scaffold healthy while the frontend app shell is initialized
3. implement recipe CRUD with ingredients, steps, and media metadata
4. implement meal-plan ranges, slots, and recipe scheduling
5. implement grocery-list generation from selected planning windows
6. add recipe import workflows and media upload hardening

## Delivery Principles

- prefer thin vertical slices over broad rewrites
- keep docs and contracts ahead of implementation
- keep the backend runnable after each slice
- do not invent frontend commands before the scaffold exists
- add targeted tests as each backend slice lands

## Near-Term Focus

- `AUTH-001`: preserve and harden auth foundation
- `FE-001`: initialize frontend shell
- `REC-001`: recipe library vertical slice
- `PLAN-001`: meal-plan vertical slice
- `GROC-001`: grocery generation vertical slice

## After MVP Stability

- richer import parsing and editing workflows
- media upload lifecycle improvements
- planning templates and reuse
- collaboration groundwork if the project still needs it
