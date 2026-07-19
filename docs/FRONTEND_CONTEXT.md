# Frontend Context

## Angular Version And Structure

- Angular dependencies are `18.2.x`; Angular CLI is `18.2.21`.
- Bootstrap: `app/src/main.ts` uses `bootstrapApplication(AppComponent, appConfig)`.
- Main providers: `app/src/app/app.config.ts`.
- App is primarily standalone components, not NgModule-based.
- `app/src/app/app.module.ts` is incomplete and not imported by `main.ts`; likely unused/misplaced.

## Routes

Defined in `app/src/app/app.routes.ts`:

- `/` redirects to `/recipes`.
- `/recipes`: `RecipesComponent`, protected by `authGuard`.
- `/login`: `LoginComponent`, public.
- `/register`: `RegisterComponent`, public.
- `/recipes/:id`: `RecipeDetailsComponent`, protected by `authGuard`.
- `/admin/accounts`: `AccountsComponent`, protected by `authGuard` and `adminGuard`.
- `**` redirects to `/recipes`.

## Components And Pages

- `AppComponent`: standalone root with navbar and router outlet; inline template used. `app.component.html` exists but is not used by the component decorator.
- `NavbarComponent`: login/register/recipes/logout nav based on `localStorage.accessToken`; Accounts link visible only for Admin.
- `LoginComponent`: posts email/password and stores returned tokens.
- `RegisterComponent`: posts email/password and redirects to login on success.
- `RecipesComponent`: paged recipe list, filters, sorting, Operator/Admin create/edit/delete UI, favorite toggles, cache clearing after recipe mutations.
- `RecipeDetailsComponent`: loads recipe details and reviews; submits new reviews.
- `AccountsComponent`: Admin-only account list/search/filter/pagination, account creation, role changes, activation/deactivation, and eligible deletion.

## Services And API Base URLs

All service URLs are hardcoded to `http://localhost:5130`:

- `app/src/app/services/auth.service.ts`: `/api/auth`.
- `app/src/app/services/recipe.service.ts`: `/api/recipes`.
- `app/src/app/services/category.service.ts`: `/api/categories`.
- `app/src/app/services/favorite.service.ts`: `/api/favorites`.
- `app/src/app/services/review.service.ts`: `/api/reviews`.
- `app/src/app/services/user-management.service.ts`: `/api/admin/users`.

These match the backend HTTP launch profile in `API/Properties/launchSettings.json`; CORS allows Angular dev server at `http://localhost:4200`.

## Models And Interfaces

- `app/src/app/models/category.model.ts`: `Category` has `id`, `name`; backend may also return `description` and `createdAt`, which the model omits.
- `app/src/app/models/recipe.model.ts`: `Recipe`, `CreateRecipe`, and `Difficulty`; difficulty values are string names `Easy`, `Medium`, `Hard`.
- `FavoriteRecipe` interface is defined inside `favorite.service.ts`.
- `Review`, `CreateReview`, `UpdateReview` interfaces are defined inside `review.service.ts`.
- `RecipeQuery` and `PagedRecipes` are defined inside `recipe.service.ts`.
- `app/src/app/models/user-account.model.ts`: `UserAccount`, account mutation request types, account query, and paged account response.

## Auth State

- Tokens are stored in localStorage keys `accessToken` and `refreshToken`.
- `AuthService.isLoggedIn()` checks only access-token presence.
- `authGuard` decodes `exp`; when the access token is expired and a refresh token exists it calls shared session restoration instead of clearing tokens immediately.
- `AuthService` reads `ClaimTypes.NameIdentifier` and the Microsoft role claim URI, and exposes `getCurrentUserId()`, `getCurrentRole()`, `hasRole()`, `isAdmin()`, `isOperator()`, and `canManageRecipes()`.

## Guards And Interceptors

- Guard: `app/src/app/guards/auth.guard.ts`.
- Admin guard: `app/src/app/guards/admin.guard.ts`; redirects non-admin authenticated users to `/recipes` and unauthenticated users to `/login`.
- Registered interceptors in `app/src/app/app.config.ts`:
  - `AuthInterceptor`: adds `Authorization: Bearer {accessToken}` when access token exists.
  - `ErrorInterceptor`: on 401, calls the shared refresh operation unless request is login/register/refresh; concurrent failures reuse one refresh request, store new tokens, and retry. On refresh failure, logs out and redirects to `/login`.
- Duplicate/misplaced interceptor file: `app/interceptors/auth.intercepro.ts` has a misspelled filename and is not registered.

## Error And Toast Handling

- Toastr is configured globally in `app/src/app/app.config.ts`.
- Login, recipe list, recipe mutations, category loading, and interceptor 403/500/network/session-expired cases use toasts.
- Register uses inline `message`/`error`.
- Recipe details review form uses inline success/error strings.
- Backend often returns plain strings, so Angular error parsing is inconsistent.

## Forms And Validation

- Template-driven forms via `FormsModule`.
- Login/register inputs use minimal HTML attributes but no comprehensive client validation.
- Recipe create/edit requires title, category, and difficulty.
- Account creation requires email, password, and role.
- Recipe detail review checks rating 1-5 before submit.
- Backend `CreateRecipeValidator` performs recipe validation; review validation is in service.

## Filtering, Sorting, Pagination, Caching

- `RecipesComponent` stores current filter state in route query params.
- Search is debounced by 400ms with `Subject`, `debounceTime`, and `distinctUntilChanged`.
- Query params sent to backend: `page`, `pageSize`, `search`, `difficulty`, `categoryId`, `sortBy`.
- Sorting values: empty/default newest, `title`, `time`, `difficulty`.
- `RecipeService.getPaged()` caches responses in an in-memory `Map` keyed by `JSON.stringify(query)`.
- Cache is cleared after create/update/delete; it is not persisted across page reloads.
- Favorite and review changes do not clear recipe list cache because they use separate API flows.

## Styling

- Global styles in `app/src/styles.css`; uses CSS variables for brand colors and imports `ngx-toastr/toastr`.
- Component styles under each component folder.
- Bootstrap is installed locally in `app/package.json` and imported once from `app/src/styles.css`; no Bootstrap CDN or local Bootstrap JS bundle is used.

## Environment Configuration

- No Angular `environment.ts` files found.
- API base URLs are hardcoded in services.

## Potentially Unused Or Misplaced Files

- `app/src/app/app.module.ts`: incomplete NgModule snippet and not used by standalone bootstrap.
- `app/src/app/app.component.html`: not referenced by `AppComponent`, which uses an inline template.
- `app/interceptors/auth.intercepro.ts`: duplicate auth interceptor outside `src/app`, misspelled `intercepro`, not registered.

## Commonly Modified Frontend Files

- `app/src/app/app.routes.ts`
- `app/src/app/app.config.ts`
- `app/src/app/services/*.ts`
- `app/src/app/models/*.ts`
- `app/src/app/guards/auth.guard.ts`
- `app/src/app/interceptors/*.ts`
- `app/src/app/pages/recipes/*`
- `app/src/app/recipe-details/*`
- `app/src/app/login/*`
- `app/src/app/pages/register/*`
- `app/src/app/shared/navbar/*`
- `app/src/styles.css`

## Known Frontend Risks

- Angular specs compile with `node node_modules/typescript/bin/tsc -p tsconfig.spec.json --noEmit`; Karma needs Chrome/Chromium in the environment.
- API URLs are hardcoded.
- Bootstrap dependency is present in `app/package.json`; JavaScript bundle is not included because no Bootstrap JS component usage was found.
- Recipe management UI checks use `AuthService.canManageRecipes()` for Admin and Operator; backend remains the security boundary.
- Navbar logout now delegates to `AuthService.logout()`, which removes only app auth tokens.
- Debug console statements were removed from auth, register, and recipe service paths.
