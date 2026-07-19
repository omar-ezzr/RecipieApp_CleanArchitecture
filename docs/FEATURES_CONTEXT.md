# Features Context

## Registration

- Does: creates a `Users` row with hashed password, default role `User`, and active status.
- Access: anonymous.
- Backend: `API/Controller/AuthController.cs`, `Infrastructure/services/PasswordService.cs`, `Core.Application/DTO/Auth/LoginDtos.cs`.
- Frontend: `app/src/app/pages/register/`, `app/src/app/services/auth.service.ts`.
- Database: `Users`.
- Flow: form -> `AuthService.register()` -> `POST /api/Auth/register` -> normalize email -> hash password -> save active `User`.
- Validation: duplicate email check only; no backend FluentValidation for auth DTO.
- Error handling: backend 400 string; frontend inline error.
- Limitations: no email validation, password policy, email confirmation, or Admin registration.

## Login

- Does: verifies credentials for active accounts, issues access and refresh tokens.
- Access: anonymous.
- Backend: `AuthController`, `PasswordService`, `Users`.
- Frontend: `LoginComponent`, `AuthService`.
- Flow: form -> `/api/Auth/login` -> BCrypt verify -> save refresh token/expiry -> return tokens -> localStorage.
- Error handling: backend returns generic 401 for invalid credentials or inactive account; frontend toast.

## JWT Access Token

- Does: signs JWT with user ID, email, and role claims.
- Backend: `API/Program.cs`, `AuthController.GenerateJwtToken`.
- Frontend: `AuthInterceptor`, `authGuard`, `AuthService.isAdmin()`.
- Claims: `ClaimTypes.NameIdentifier` user ID, `ClaimTypes.Name` email, `ClaimTypes.Role` role.
- `API/Program.cs` validates the current database account on every token: missing account, inactive account, or role mismatch rejects the token immediately.
- Limitations: issuer/audience validation disabled; local `Jwt:Key` must be provided via user secrets or environment variables.

## Refresh Token

- Does: rotates stored refresh token and issues new access token.
- Backend: `AuthController.Refresh`, `Users.RefreshToken`, `Users.RefreshTokenExpiryTime`.
- Frontend: `ErrorInterceptor`, `authGuard`, shared `AuthService.refreshSession()`.
- Flow: 401 or guarded navigation with expired access token -> shared refresh endpoint call -> store new tokens -> retry queued/original requests.
- Refresh-token expiry uses `Jwt:RefreshTokenDays`; inactive accounts cannot refresh.

## Logout

- Does: client-side token removal.
- Backend: no logout endpoint found.
- Frontend: `AuthService.logout()`, `NavbarComponent.logout()`.
- Limitation: server refresh token remains valid until expiry because no server-side logout endpoint exists.

## Recipe Listing

- Access: authenticated.
- Backend: `RecipesController.GetPaged`, `RecipeService.GetPagedAsync`, `RecipeRepository.GetPagedAsync`.
- Frontend: `RecipesComponent`, `RecipeService.getPaged()`.
- Database: `Recipies`, `Categories`, `Ingredients`, `RecipeSteps`.
- Response: paged object with recipe DTOs.

## Search

- Searches recipe title with `Title.Contains(parameters.Search)`.
- Frontend debounces search input and stores query param `search`.
- Limitation: no description/ingredient search.

## Category Filtering

- Uses query param `categoryId` and `Recipie.CategoryId`.
- Categories loaded from `/api/Categories`.

## Difficulty Filtering

- Backend parses `RecipeQueryParams.Difficulty` as `DifficultyLevel`, case-insensitive.
- Frontend sends `Easy`, `Medium`, or `Hard`.
- Invalid non-empty difficulty values return a validation error instead of silently acting as a valid filter.
- Database stores enum as int.

## Sorting

- Backend values: `title`, `time`, `difficulty`, default newest by `CreatedAt` descending.
- Frontend select sends matching values.

## Pagination

- Backend params: `Page`, `PageSize`; invalid values normalized, page size capped at 100.
- Frontend page size options: 10, 20, 30, 50.
- UI renders condensed visible page list.

## Recipe Details

- Access: authenticated route.
- Backend: `GET /api/Recipes/{id}` includes category, ingredients, steps.
- Frontend: `RecipeDetailsComponent`.
- Limitations: no edit/delete reviews UI despite service methods.

## Account Management

- Access: `Admin` only.
- Backend: `API/Controller/AdminUsersController.cs`, `IUserManagementService`, `UserManagementService`, `IUserRepository`, `UserRepository`.
- Frontend: `/admin/accounts`, `AccountsComponent`, `adminGuard`, `UserManagementService`.
- Capabilities: list/search accounts, filter by role/status, create accounts, change role, activate/deactivate, delete accounts with no related favorites/reviews.
- Roles: `User`, `Operator`, `Admin` from `Core.Domain/Constants/AppRoles.cs`.
- Safety: Admin cannot self-demote, self-deactivate, or self-delete. Last active Admin cannot be demoted, deactivated, or deleted by service rules. Role/status changes clear refresh tokens.
- Error handling: new endpoints return JSON `{ "message": "..." }`.

## Recipe Creation

- Backend: `POST /api/Recipes`, `CreateRecipeDto`, `CreateRecipeValidator`; Admin-or-Operator.
- Frontend: create form is visible when `AuthService.canManageRecipes()` is true.
- Database: creates `Recipie` only; no ingredients/steps from DTO.
- Difficulty: request sends string value `Easy`, `Medium`, or `Hard`; backend parses to `DifficultyLevel` and rejects missing/invalid values.
- Authorization: backend and UI require Admin or Operator.
- Cache: recipe list cache cleared after successful create.

## Recipe Editing

- Backend: `PUT /api/Recipes/{id}` requires `Admin` or `Operator`.
- Frontend: edit action shown when `AuthService.canManageRecipes()` is true.
- Cache: cleared after success.
- Difficulty is initialized from the existing recipe and preserved unless changed.
- Limitation: edit form does not include ingredients or steps.

## Recipe Deletion

- Backend: `DELETE /api/Recipes/{id}` requires `Admin` or `Operator`; cascades ingredients, steps, favorites, and reviews.
- Frontend: optimistic removal with rollback on error; cache cleared on success.

## Category Loading

- Backend: `CategoriesController.Get` returns EF category entities.
- Frontend: `CategoryService.getAll()` used by recipe page for create/filter selects.
- Access: anonymous backend route, but frontend page is guarded.

## Roles And Authorization

- Backend source of truth: `Core.Domain/Constants/AppRoles.cs`.
- `User`: recipes, favorites, own reviews.
- `Operator`: User permissions plus recipe create/update/delete.
- `Admin`: Operator permissions plus `/api/admin/users` and `/admin/accounts`.
- Frontend uses decoded JWT role claim for navigation/UI only; backend authorization remains authoritative.

## Toast And Error Handling

- Toastr configured in `app.config.ts`.
- Used by login, recipe page, interceptors.
- Inline errors in register and review form.
- Backend uses plain string errors, not standardized JSON.

## Seed Data

- Active seeder creates 3 categories and 1,000 recipes with ingredients/steps and external image URLs.
- Admin bootstrap is independent from recipe seeding and runs only when `SeedAdmin:Email` and `SeedAdmin:Password` are configured and no Admin exists.
- Recipe seeding still creates 3 categories and 1,000 recipes only when recipes table is empty.

## Favorites

- Access: authenticated.
- Backend: `FavoritesController`, `FavoriteService`, `FavoriteRepository`.
- Frontend: `FavoriteService`, favorite button in recipe cards.
- Database: `FavoriteRecipes` unique per user/recipe.
- UI behavior: optimistic favorite toggle with rollback on failed API call.

## Reviews

- Access: public read by recipe; authenticated write/update/delete.
- Backend: `ReviewsController`, `ReviewService`, `ReviewRepository`.
- Frontend: `ReviewService`, create/list UI in `RecipeDetailsComponent`.
- Database: `RecipeReviews`, unique per user/recipe.
- Validation: rating 1-5; duplicate review blocked.
- Limitation: update/delete service exists but no UI controls found.

## Requested But Absent

- Dashboards: not found.
- Logs UI: not found.
- Charts: not found.
- Realtime/SignalR: not found.
- Notifications: not found.
- Uploads: not found.
