# Auth Flow

## Current Auth Strategy

PantryPlanner currently uses local email/password auth with backend-issued JWT access tokens.

## Signup

1. User submits email, display name, and password.
2. Backend validates input and checks for duplicate email.
3. Backend creates the local user record.
4. Backend returns an access token plus the current user payload.

## Login

1. User submits email and password.
2. Backend verifies the password hash.
3. Backend returns an access token plus the current user payload.

## Authenticated Requests

1. Frontend stores the access token using the future frontend auth module.
2. Frontend sends `Authorization: Bearer <token>`.
3. Backend validates the token and resolves the current user id.
4. Handlers scope all reads and writes by that user id.

## Current Endpoints

- `POST /api/v1/auth/signup`
- `POST /api/v1/auth/login`
- `GET /api/v1/users/me`

## Planned Extensions

- refresh-token support if session length becomes a real UX issue
- external provider linking only after local auth flows are stable
- route guards in the future frontend scaffold

## Security Notes

- keep JWT claims minimal
- never trust client-supplied ownership fields
- return ProblemDetails responses for auth failures
- do not leak password or token secrets in logs
