# RecepieV3 Angular Frontend

Angular 22 standalone frontend for the RecepieV3 social recipe platform.

The UI is designed as an editorial cooking product where authenticated users discover recipes by cuisine and region, publish recipes, manage their own recipe library, favorite recipes, review dishes, and access Admin account management when authorized.

## Stack

- Angular 22 standalone components.
- Angular Router.
- Angular HttpClient.
- JWT auth interceptor and refresh interceptor.
- @openng/ngx-toastr.
- Bootstrap installed through npm.
- Custom global design system in `src/styles.css`.

## Design System

Global visual primitives live in:

```text
src/styles.css
```

Current design direction:

- Warm paper background with subtle CSS texture.
- Forest green as the primary brand and action color.
- Serif editorial headings using Georgia/Times-style font stacks.
- Sans-serif UI labels, navigation, buttons, filters, metadata, and forms.
- Paper-light cards with green-tinted borders and soft shadows.
- Visible keyboard focus and reduced-motion support.

Shared UI components live under:

```text
src/app/shared/components/
```

Notable shared components:

- `recipe-card`
- `recipe-card-skeleton`
- `empty-state`
- `loading-spinner`
- `page-header`

## Routes

Configured in:

```text
src/app/app.routes.ts
```

Important routes:

- `/recipes`
- `/recipes/:id`
- `/create-recipe`
- `/my-recipes`
- `/login`
- `/register`
- `/admin/accounts`

Protected routes use the existing auth/admin guards. Frontend checks are only UI behavior; backend authorization remains authoritative.

## API Configuration

The API base URL is centralized in:

```text
src/app/app-api.config.ts
```

Do not hardcode additional API origins in components or services.

Recipe images may be absolute URLs or API-relative paths such as `/images/recipes/example.webp`. Resolve them with:

```text
src/app/core/utils/asset-url.util.ts
```

## Development

Install dependencies:

```bash
npm install
```

Start the Angular dev server:

```bash
npm start
```

Default local app URL:

```text
http://localhost:4203
```

During development, backend API calls use the Angular proxy through:

```text
/api
```

## Build

```bash
npm run build
```

Last verified on 2026-07-26: build passed with warnings for initial bundle size, component CSS budgets, and a Bootstrap selector parse warning.

## Tests

```bash
npm test -- --watch=false
```

Karma uses Chrome. In the current environment, the browser bundle compiled but the test run failed because no Chrome binary was available and `CHROME_BIN` was unset.

## Auth Behavior

Authentication uses `accessToken` and `refreshToken` in `localStorage`.

`AuthService.logout()` removes only:

- `accessToken`
- `refreshToken`

Supported UI role behavior:

- Authenticated users can publish recipes.
- Recipe owners can edit/delete their own recipes.
- Admins can manage any recipe and access `/admin/accounts`.

## Current Known Frontend Limits

- Browser tests require Chrome or Chromium.
- Review data from the backend still includes user email; the UI masks it on recipe details.
- Culture Admin services exist, but there is no dedicated Angular Admin cuisine/region page.
- Build warnings remain and should be reviewed before tightening budgets.
