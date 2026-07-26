# Frontend Context

Last verified: 2026-07-26
Branch: `main`
Commit: `1de804620330d10f9ee6b493ecac423f6ab288b2`

## Angular Setup

| Item | Evidence |
| --- | --- |
| Angular | `app/package.json` uses Angular `^18.2.x` and CLI `^18.2.21`. |
| Bootstrap | `app/src/main.ts` calls `bootstrapApplication(AppComponent, appConfig)`. |
| Standalone components | Routes import standalone components directly in `app/src/app/app.routes.ts`. |
| Providers | `app/src/app/app.config.ts` registers router, HttpClient with DI interceptors, animations, and ngx-toastr. |
| API URL | `app/src/app/app-api.config.ts` exports `API_BASE_URL = 'http://localhost:5130/api'`. |
| Styling | `app/src/styles.css`, component CSS files, Bootstrap dependency. |
| Tests | Karma/Jasmine configured in `app/angular.json`; Chrome required by `karma-chrome-launcher`. |

`app/src/app/app.module.ts` exists but the app bootstraps standalone components, so the module appears unused.

## Routes

| Route | Component | Guard | Access |
| --- | --- | --- | --- |
| `/` | redirect to `/recipes` | None | Redirect |
| `/recipes` | `RecipesComponent` | `authGuard` | Authenticated |
| `/login` | `LoginComponent` | None | Public |
| `/register` | `RegisterComponent` | None | Public |
| `/recipes/:id` | `RecipeDetailsComponent` | `authGuard` | Authenticated |
| `/create-recipe` | `CreateRecipeComponent` | `authGuard` | Authenticated |
| `/my-recipes` | `MyRecipesComponent` | `authGuard` | Authenticated |
| `/admin/accounts` | `AccountsComponent` | `authGuard`, `adminGuard` | Admin |
| `**` | redirect to `/recipes` | None | Redirect |

## Services

| Service | Path | Base URL | Main methods | Cache |
| --- | --- | --- | --- | --- |
| `AuthService` | `app/src/app/services/auth.service.ts` | `${API_BASE_URL}/auth` | login, register, refresh, token helpers, role/user helpers, logout | Shares in-flight refresh request. |
| `RecipeService` | `app/src/app/services/recipe.service.ts` | `${API_BASE_URL}/recipes` | getAll, getById, create, update, delete, getMine, getPaged | Map cache for global paged queries; invalidates on create/update/delete. |
| `CategoryService` | `app/src/app/services/category.service.ts` | `${API_BASE_URL}/categories` | getAll | None. |
| `FavoriteService` | `app/src/app/services/favorite.service.ts` | `${API_BASE_URL}/favorites` | add, remove, getMine, check | None. |
| `ReviewService` | `app/src/app/services/review.service.ts` | `${API_BASE_URL}/reviews` | getByRecipe, create, update, delete | None. |
| `UserManagementService` | `app/src/app/services/user-management.service.ts` | `${API_BASE_URL}/admin/users` | list/get/create/role/status/delete | None. |
| `CuisineService` | `app/src/app/services/cuisine.service.ts` | `${API_BASE_URL}/cuisines` | getAll, getById, getRegions, create, update, delete | None. |
| `RegionService` | `app/src/app/services/region.service.ts` | `${API_BASE_URL}/regions` | getById, create, update, delete | None. |

Tests currently still assert the concrete URL `http://localhost:5130/api/...` even though services import the centralized constant.

## Auth Frontend

Files:

- `app/src/app/services/auth.service.ts`
- `app/src/app/interceptors/auth.interceptor.ts`
- `app/src/app/interceptors/error.interceptor.ts`
- `app/src/app/guards/auth.guard.ts`
- `app/src/app/guards/admin.guard.ts`

Behavior:

- Tokens are saved under `accessToken` and `refreshToken`.
- Logout removes those two keys only.
- `AuthInterceptor` attaches bearer tokens.
- `ErrorInterceptor` attempts token refresh on 401.
- `AuthService.getCurrentUserId()` supports ASP.NET claim URI and normal claim names.
- `AuthService.isAdmin()` reads the role claim.

## Components

| Component | Files | Responsibilities | Backend calls |
| --- | --- | --- | --- |
| `AppComponent` | `app/src/app/app.component.ts`, `.html`, `.css`, `.spec.ts` | Shell with navbar and router outlet. | None. |
| `NavbarComponent` | `app/src/app/shared/navbar/` | Auth-aware links: Recipes, Create Recipe, My Recipes, Admin Accounts, Logout. | AuthService only. |
| `LoginComponent` | `app/src/app/login/` | Login form, token storage, navigate to recipes. | `POST /api/Auth/login` |
| `RegisterComponent` | `app/src/app/pages/register/` | Register form with display name. | `POST /api/Auth/register` |
| `RecipesComponent` | `app/src/app/pages/recipes/` | Explore/list page, search, category/cuisine/region/difficulty/traditional filters, pagination, favorites, inline create/edit/delete. | Recipes, categories, cuisines, favorites. |
| `CreateRecipeComponent` | `app/src/app/pages/create-recipe/` | Complete recipe publishing form with cultural fields, dynamic ingredients/steps. | Recipes, categories, cuisines/regions. |
| `MyRecipesComponent` | `app/src/app/pages/my-recipes/` | Current user's recipes with pagination and delete/view/edit actions. | `GET /api/Recipes/me`, delete. |
| `RecipeDetailsComponent` | `app/src/app/recipe-details/` | Recipe detail, cultural fields, ingredients, steps, reviews. | Recipe details and reviews. |
| `AccountsComponent` | `app/src/app/pages/admin/accounts/` | Admin account list/create/role/status/delete. | Admin users API. |

## Models

Main model files:

- `app/src/app/models/recipe.model.ts`
- `app/src/app/models/category.model.ts`
- `app/src/app/models/cuisine.model.ts`
- `app/src/app/models/region.model.ts`
- `app/src/app/models/user-account.model.ts`

Confirmed model alignment:

- `DifficultyLevel` is a numeric enum matching backend `Easy = 1`, `Medium = 2`, `Hard = 3`.
- `Recipe` includes author, cuisine, region, traditional fields, ingredients, and steps.
- `CreateRecipe` includes required cuisine, difficulty, ingredients, and steps.
- Review interfaces are declared in `review.service.ts`, not a shared model file.

Known mismatch/risk:

- Backend `ReviewDto` exposes `UserEmail`; frontend review templates display it.
- `Category` frontend model contains only `id` and `name`; backend entity also has `description`.
- Culture Admin endpoints exist but no Angular Admin UI for them.

## Security Notes

Frontend route guards and button visibility are only UI behavior. Server-side authorization is enforced in controllers/services. Owner checks in templates use `AuthService.getCurrentUserId()` plus `recipe.author.id`, while backend checks owner/Admin in `RecipeService`.

## Dependencies

Confirmed dependencies in `app/package.json`:

- Angular 18
- Bootstrap 5.3.8
- ngx-toastr
- RxJS
- Karma/Jasmine test stack

No frontend upload, charting, state-management, real-time, or i18n library was found.

## Frontend Redesign Context

Last verified: 2026-07-26 after the frontend redesign task.

The Angular application now uses a custom editorial cooking design system instead of relying on default Bootstrap visuals.

Design system files:

- `app/src/styles.css`: global design tokens, paper background, typography scale, button variants, form styling, badges, pagination, focus states, and reduced-motion behavior.
- `app/src/app/app.component.html`: skip link, navbar, main landmark, router outlet, and product footer.
- `app/src/app/app.component.css`: footer layout.

Confirmed visual direction:

- Warm off-white paper background with subtle CSS texture.
- Forest green as the primary brand/action color.
- Georgia/Times-style serif for editorial headings and recipe reading.
- Inter/Arial/Helvetica fallback stack for navigation, controls, labels, metadata, and forms.
- Paper-light cards with thin green-tinted borders and soft shadows.

Shared UI components added under `app/src/app/shared/components/`:

| Component | Role |
| --- | --- |
| `recipe-card/` | Reusable recipe card with image, cuisine/region, title, description, author, time, difficulty, favorite, and owner/Admin actions. |
| `recipe-card-skeleton/` | Loading skeleton for recipe grids. |
| `empty-state/` | Reusable empty state with optional action link/event. |
| `page-header/` | Small shared page-header primitive, available for future consolidation. |
| `loading-spinner/` | Small loading primitive, available for future consolidation. |

Redesigned pages:

| Page | Files | Notes |
| --- | --- | --- |
| Application shell | `app.component.*` | Adds skip link, `main#main-content`, and footer. |
| Navbar | `shared/navbar/*` | Sticky responsive paper navbar with RECIPIE brand, auth-aware links, mobile menu, Escape close, and logout. |
| Explore recipes | `pages/recipes/*` | Editorial hero, dominant search, cuisine atlas, responsive filters, URL-query preservation, shared cards, loading/empty states, pagination, inline owner edit. |
| Recipe details | `recipe-details/*` | Article-style recipe page with large image, cultural metadata, ingredients, steps, favorite, owner/Admin actions, reviews, and masked review email display. |
| Create recipe | `pages/create-recipe/*` | Publishing form split into story sections with cultural fields, dynamic ingredients/steps, preview, and unchanged submit payload. |
| My Recipes | `pages/my-recipes/*` | Account-library layout using shared recipe cards, empty state, pagination, edit/delete actions. |
| Login/Register | `login/*`, `pages/register/*` | Editorial auth layout with accessible labels and autocomplete attributes. |
| Admin accounts | `pages/admin/accounts/*` | Responsive operational page with filters, create form, status/role controls, and mobile table strategy. |

Image handling:

- Recipe card/detail/create preview image paths use `app/src/app/core/utils/asset-url.util.ts`.
- API-relative paths such as `/images/recipes/...` resolve against `API_BASE_URL` without hardcoding a new origin.
- Missing images fall back to `/assets/recipe-placeholder.webp`.

Accessibility notes:

- Global skip link targets `#main-content`.
- Navbar mobile menu uses `aria-expanded` and `aria-controls`.
- Focus-visible states are globally defined.
- Forms use visible labels rather than placeholder-only controls.
- Recipe detail ingredients use optional client-only checkboxes that do not mutate backend data.
- Review email display is masked in `RecipeDetailsComponent`; backend still returns `UserEmail` and should be corrected later.

Validation on 2026-07-26:

- `npm run build`: passed.
- `npm test -- --watch=false`: browser bundle compiled, then failed because Karma could not find a Chrome binary (`CHROME_BIN` unset).
- Build warnings remain for initial bundle size, component CSS budgets, and a Bootstrap selector parse warning from `.form-floating>~label`.
