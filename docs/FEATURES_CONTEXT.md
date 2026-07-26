# Features Context

Last verified: 2026-07-26
Branch: `main`
Commit: `1de804620330d10f9ee6b493ecac423f6ab288b2`

Status labels:

- Implemented: code path exists and is wired.
- Incomplete: code path exists but has notable gaps.
- Broken/inconsistent: confirmed mismatch or risk.
- Not found: searched repository and did not find a wired implementation.
- Requires verification: needs runtime/manual validation.

## Feature Matrix

| Feature | Status | Backend files | Frontend files | Database | Access rules | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Registration | Implemented | `AuthController.cs`, `RegisterDto.cs` | `pages/register/`, `AuthService` | `Users` | Public | Requires display name; validation partly in controller. |
| Login | Implemented | `AuthController.cs`, `PasswordService.cs` | `login/`, `AuthService` | `Users.RefreshToken` | Public | Returns access and refresh tokens. |
| JWT access tokens | Implemented | `Program.cs`, `AuthController.cs` | `AuthService`, interceptors, guards | None | Bearer | Includes ID, email, role claims. |
| Refresh tokens | Implemented | `AuthController.Refresh` | `ErrorInterceptor`, `AuthService.refreshSession` | `Users.RefreshToken`, expiry | Public refresh endpoint | Rotates refresh token. |
| Logout | Implemented frontend | None | `AuthService.logout`, navbar | None | Client-side | Removes auth keys only. |
| Recipe listing | Implemented | `RecipesController.GetPaged`, `RecipeService`, `RecipeRepository` | `pages/recipes/`, `RecipeService` | `Recipes` and related tables | Authenticated | Pagination and filters implemented. |
| Recipe search | Implemented | `RecipeRepository.GetPagedAsync` | recipes page | `Recipes.Title` | Authenticated | Uses `Contains`; may need index/full-text later. |
| Category filtering | Implemented | `RecipeQueryParams.CategoryId` | recipes page | `Categories`, `Recipes.CategoryId` | Authenticated | Category existence not validated for filters. |
| Difficulty filtering | Implemented | `RecipeQueryParams.Difficulty` | recipes page | `Recipes.Difficulty` | Authenticated | String filter parsed to enum. |
| Cuisine filtering | Implemented | `RecipeQueryParams.CuisineId` | recipes page | `Cuisines`, `Recipes.CuisineId` | Authenticated recipe list; public cuisines | New Phase 2 behavior. |
| Region filtering | Implemented | `RecipeQueryParams.RegionId` | recipes page | `Regions`, `Recipes.RegionId` | Authenticated recipe list; public region reads | Region/cuisine relationship validated on create/update. |
| Traditional-only filtering | Implemented | `RecipeQueryParams.IsTraditional` | recipes page | `Recipes.IsTraditional` | Authenticated | Boolean query param. |
| Sorting | Implemented | `RecipeRepository.GetPagedAsync` | recipes page | `Recipes` | Authenticated | Supports title/time/difficulty/default created date. |
| Pagination | Implemented | `PagedResult<T>`, repositories | recipes/My Recipes/admin users | `Recipes`, `Users` | Varies | Recipe page size capped at 100 in repository. |
| Recipe details | Implemented | `GET /api/Recipes/{id}` | `recipe-details/` | Recipes aggregate | Authenticated | Shows culture, ingredients, steps, reviews. |
| Recipe creation | Implemented | `RecipesController.Create`, `RecipeService.CreateAsync` | `create-recipe/`, recipes inline form | Recipes/ingredients/steps | Authenticated | Owner from JWT; includes cuisine/region. |
| Recipe editing | Implemented/incomplete | `RecipeService.UpdateAsync` | recipes inline edit | Recipes/children | Owner or Admin | Service replaces ingredients/steps; inline UI may not expose full child editing. |
| Recipe deletion | Implemented | `RecipeService.DeleteAsync` | recipes/My Recipes | Recipes | Owner or Admin | Backend source of truth. |
| My Recipes | Implemented | `GET /api/Recipes/me` | `pages/my-recipes/` | Recipes.UserId | Authenticated | Only current user's recipes. |
| Categories | Implemented/incomplete | `CategoriesController`, `CategoryConfiguration` | `CategoryService` | `Categories` | Public backend endpoint | No Admin category management UI found. |
| Favorites | Implemented | `FavoritesController`, `FavoriteService`, `FavoriteRepository` | recipes page, favorite service | `FavoriteRecipes` | Authenticated | Duplicate favorites rejected. |
| Reviews | Implemented/inconsistent | `ReviewsController`, `ReviewService`, `ReviewRepository` | details page, review service | `RecipeReviews` | Create/update/delete authenticated; list anonymous | Review DTO exposes email. |
| Admin authorization | Implemented | role constants, `[Authorize(Roles=...)]`, JWT validation | `adminGuard`, navbar | `Users.Role` | Admin | User management and culture writes. |
| Admin account management | Implemented | `AdminUsersController`, `UserManagementService` | `pages/admin/accounts/` | `Users` | Admin | Protects final Admin and self changes. |
| Admin seed | Implemented | `DbSeeder`, `AdminSeedOptions`, `Program.cs` | None | `Users` | Development/config gated | Requires configured password; never document value. |
| Cuisine/region management API | Implemented | Cuisines/Regions controllers/services/repos | Services only | `Cuisines`, `Regions` | Admin writes, anonymous reads | No Admin UI found. |
| Toast/error handling | Implemented/inconsistent | Mixed API errors | `ErrorInterceptor`, component error handlers | None | N/A | Contracts differ by endpoint. |
| External performance logging integration | Not found | Search found no wired `PerformancePlatform` code | Not found | Not found | N/A | If uncommitted integration appears, inspect before editing. |

## Requested Or Expected Features Not Found

| Feature | Status | Evidence |
| --- | --- | --- |
| Social feed | Not found | No feed entities/controllers/routes/components found. |
| Follows/followers | Not found | No follow entity or endpoints found. |
| Comments separate from reviews | Not found | Reviews exist; no comment entity/controller. |
| Notifications | Not found | No notification model/service/routes. |
| Realtime updates | Not found | No SignalR/WebSocket setup. |
| Uploads/media storage | Not found | Image URLs exist; no upload controller/service. |
| Localization/i18n | Not found | No Angular i18n setup or translation files. |
| Admin cuisine/region UI | Not found | Backend services/controllers exist; no routed admin culture component. |
| Redis/cache backend | Not found | No Redis packages/config. |
| RabbitMQ/Kafka | Not found | No messaging packages/config. |
| GraphQL | Not found | No GraphQL packages/config. |
| Microservices/load balancing | Not found | Single solution/web API. |
| Country entity | Not found | Cuisine has `CountryCode`; no Country table/entity. |

## Cache Behavior

- `RecipeService` frontend caches `getPaged` responses by JSON query key and clears cache after create/update/delete.
- `getMine` does not use the global recipe-list cache.
- Backend does not use memory/distributed cache.

## Missing Tests Or Gaps

- Angular Karma cannot run without Chrome in the current environment.
- No dedicated backend test was found for category endpoints.
- No Admin UI tests for cuisine/region management because no UI exists.
- Existing tests cover many Phase 1/2 behaviors but are uncommitted.

## Frontend Experience Status

Last verified: 2026-07-26 after frontend redesign.

| Feature surface | Status | Frontend files | Notes |
| --- | --- | --- | --- |
| Recipe-first discovery | Implemented | `app/src/app/pages/recipes/*`, `shared/components/recipe-card/*` | Editorial hero, prominent search, cuisine tiles, filters, result count, cards, and pagination. |
| Cultural exploration UI | Implemented | `pages/recipes/*`, `recipe-details/*` | Cuisines and regions are visible on cards/details and link back into filtered recipe search. |
| Recipe publishing UI | Implemented | `pages/create-recipe/*` | Sectioned publishing form supports identity, cultural origin, ingredients, steps, image, preview, and existing submit contract. |
| User recipe library | Implemented | `pages/my-recipes/*` | Shows current user's recipes from `GET /api/Recipes/me` with create/edit/delete/view actions. |
| Owner/Admin controls | Implemented frontend UI | `recipe-card/*`, `pages/recipes/*`, `recipe-details/*`, `pages/my-recipes/*` | Visibility uses JWT user ID/Admin role, but backend remains authoritative. |
| Favorites UI | Implemented | `recipe-card/*`, `pages/recipes/*`, `recipe-details/*` | Favorite toggles use existing `FavoriteService`. |
| Reviews UI | Implemented/incomplete | `recipe-details/*`, `ReviewService` | Review list/form redesigned; backend still exposes reviewer email, which frontend masks. |
| Auth pages | Implemented | `login/*`, `pages/register/*` | Redesigned editorial login/register forms with labels and autocomplete. |
| Admin account UI | Implemented | `pages/admin/accounts/*` | Responsive table/card behavior, create, role/status, delete actions preserved. |

Known frontend limitations after redesign:

- Angular browser tests still require Chrome/`CHROME_BIN`.
- `npm run build` passes but reports CSS and bundle budget warnings.
- Culture Admin API exists, but no Admin culture-management route was added during this redesign.
- Review privacy is only mitigated in the UI; backend should eventually return safe review author data instead of email.
