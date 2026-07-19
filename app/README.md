# Recepes V2 Angular App

Angular 18 standalone frontend for the Recep V2 recipe application.

## Development Server

Run `ng serve` and open `http://localhost:4200/`.

## Local Dependencies

Bootstrap is installed through npm and imported from `src/styles.css`; no Bootstrap CDN is required.

## Build

Run:

```bash
npm run build
```

## Unit Tests

Run:

```bash
npm test -- --watch=false
```

Karma uses `karma-chrome-launcher`, so the local environment must provide a Chrome or Chromium binary.

## API Configuration

API service URLs currently target `http://localhost:5130/api/...`, matching the backend HTTP launch profile.

## Auth Behavior

Authentication uses `accessToken` and `refreshToken` in localStorage. Refresh is shared so concurrent `401` responses reuse a single refresh request.

Roles are `User`, `Operator`, and `Admin`. Operators can manage recipes. Admins can also access `/admin/accounts` for account management.
