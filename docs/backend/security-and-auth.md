# Security And Auth

## Current Auth Model

- local email/password signup and login
- backend-issued JWT access tokens
- authenticated `users/me` lookup
- user ownership enforced in handlers and queries

## Security Rules

- keep JWT claims minimal
- never trust client-provided ownership fields
- validate request payloads at the boundary
- return ProblemDetails responses for expected failures
- avoid logging secrets, tokens, or password material

## Current JWT Settings

- issuer: `PantryPlanner`
- audience: `PantryPlanner.Client`
- symmetric signing key from configuration
- expiration controlled by `Jwt:AccessTokenMinutes`

## Future Security Considerations

- refresh-token support only if the UX needs it
- media upload permissions should remain user-scoped
- recipe import endpoints should sanitize and validate external payloads before persistence
