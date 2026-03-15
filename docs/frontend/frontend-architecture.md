# Frontend Architecture

## Target Stack

- React
- TypeScript
- Vite
- TanStack Query
- React Hook Form
- Zod
- Tailwind CSS

## Planned Structure

- `app/` for bootstrap, providers, and routing
- `api/` for HTTP client and API modules
- `components/` for shared UI
- `features/` for domain-specific UI and state
- `pages/` for route-level composition
- `lib/` and `types/` for utilities and local models

## Planned Feature Areas

- `auth`
- `recipes`
- `meal-plans`
- `grocery-lists`
- `recipe-imports`
- `pantry`

## State Rules

- use TanStack Query for server state
- use React Hook Form for form state
- use Zod at request and form boundaries
- keep API clients centralized under `api/`
- avoid route files that mix fetching, schema logic, and heavy presentation in one place
