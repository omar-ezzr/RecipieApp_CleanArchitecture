# API Endpoints

Controller route attributes use `[Route("api/[controller]")]`; effective controller tokens are `Auth`, `Recipes`, `Categories`, `Favorites`, and `Reviews`. Frontend calls lowercase paths such as `/api/auth`; ASP.NET Core routing is case-insensitive by default.

| HTTP method | Exact route | Controller | Action method | Authentication requirement | Required role or policy | Route parameters | Query parameters | Request DTO | Response type | Expected status codes | Purpose | Frontend caller |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| POST | `/api/Auth/login` | `AuthController` | `Login` | Anonymous | None | None | None | `LoginDto` | `{ accessToken, refreshToken }` | 200, 401 | Authenticate user and store refresh token | `AuthService.login()` |
| POST | `/api/Auth/refresh` | `AuthController` | `Refresh` | Anonymous | None | None | None | `TokenRequestDto` | `{ accessToken, refreshToken }` | 200, 401 | Rotate refresh token and issue new access token | `AuthService.refreshToken()`, `ErrorInterceptor` |
| POST | `/api/Auth/register` | `AuthController` | `Register` | Anonymous by absence of controller `[Authorize]` | None | None | None | `LoginDto` | Plain string `"User created"` | 200, 400 | Create user with default `User` role | `AuthService.register()` |
| GET | `/api/Recipes` | `RecipesController` | `Get` | Required | Any authenticated user | None | None | None | `PagedResult<RecipieDto>` | 200, 401 | Safe first-page recipe result capped at 100 | `RecipeService.getAll()`; no current page caller found |
| GET | `/api/Recipes/paged` | `RecipesController` | `GetPaged` | Required | Any authenticated user | None | `Page`, `PageSize`, `Search`, `Difficulty`, `CategoryId`, `SortBy` | None | `PagedResult<RecipieDto>` | 200, 400, 401 | Paged/filterable recipe list | `RecipeService.getPaged()`, `RecipesComponent` |
| GET | `/api/Recipes/{id}` | `RecipesController` | `GetById` | Required | Any authenticated user | `id: Guid` | None | None | `RecipieDto` | 200, 401, 404 | Recipe details | `RecipeService.getById()`, `RecipeDetailsComponent` |
| POST | `/api/Recipes` | `RecipesController` | `Create` | Required | Role `Admin` or `Operator` | None | None | `CreateRecipeDto` | Empty body | 200, 400, 401, 403 | Create recipe | `RecipeService.create()`, `RecipesComponent` management UI |
| PUT | `/api/Recipes/{id}` | `RecipesController` | `Update` | Required | Role `Admin` or `Operator` | `id: Guid` | None | `CreateRecipeDto` | Empty body | 200, 400, 401, 403 | Update recipe | `RecipeService.update()`, `RecipesComponent` |
| DELETE | `/api/Recipes/{id}` | `RecipesController` | `Delete` | Required | Role `Admin` or `Operator` | `id: Guid` | None | None | Empty body or error string | 200, 401, 403, 404 | Delete recipe | `RecipeService.delete()`, `RecipesComponent` |
| GET | `/api/admin/users` | `AdminUsersController` | `GetPaged` | Required | Role `Admin` | None | `page`, `pageSize`, `search`, `role`, `isActive` | None | `PagedUsersDto` | 200, 400, 401, 403 | List/search accounts | `UserManagementService.getPaged()`, `AccountsComponent` |
| GET | `/api/admin/users/{id}` | `AdminUsersController` | `GetById` | Required | Role `Admin` | `id: Guid` | None | None | `UserAccountDto` | 200, 401, 403, 404 | View account | `UserManagementService.getById()` |
| POST | `/api/admin/users` | `AdminUsersController` | `Create` | Required | Role `Admin` | None | None | `CreateUserAccountDto` | `UserAccountDto` | 201, 400, 401, 403, 409 | Create account | `UserManagementService.create()`, `AccountsComponent` |
| PUT | `/api/admin/users/{id}/role` | `AdminUsersController` | `UpdateRole` | Required | Role `Admin` | `id: Guid` | None | `UpdateUserRoleDto` | `UserAccountDto` | 200, 400, 401, 403, 404, 409 | Change account role | `UserManagementService.updateRole()`, `AccountsComponent` |
| PUT | `/api/admin/users/{id}/status` | `AdminUsersController` | `UpdateStatus` | Required | Role `Admin` | `id: Guid` | None | `UpdateUserStatusDto` | `UserAccountDto` | 200, 400, 401, 403, 404, 409 | Activate/deactivate account | `UserManagementService.updateStatus()`, `AccountsComponent` |
| DELETE | `/api/admin/users/{id}` | `AdminUsersController` | `Delete` | Required | Role `Admin` | `id: Guid` | None | None | Empty body | 204, 400, 401, 403, 404, 409 | Delete eligible account | `UserManagementService.delete()`, `AccountsComponent` |
| GET | `/api/Categories` | `CategoriesController` | `Get` | Anonymous | None | None | None | None | `Category[]` entity shape | 200 | Load categories | `CategoryService.getAll()`, `RecipesComponent` |
| POST | `/api/Favorites/{recipeId}` | `FavoritesController` | `Add` | Required | Any authenticated user | `recipeId: Guid` | None | Empty object from frontend | Empty body | 200, 400, 401 | Favorite a recipe | `FavoriteService.add()`, `RecipesComponent` |
| DELETE | `/api/Favorites/{recipeId}` | `FavoritesController` | `Remove` | Required | Any authenticated user | `recipeId: Guid` | None | None | Empty body or error string | 200, 401, 404 | Remove favorite | `FavoriteService.remove()`, `RecipesComponent` |
| GET | `/api/Favorites/me` | `FavoritesController` | `GetMine` | Required | Any authenticated user | None | None | None | `FavoriteRecipeDto[]` | 200, 401 | Current user's favorites | `FavoriteService.getMine()`, `RecipesComponent` |
| GET | `/api/Favorites/check/{recipeId}` | `FavoritesController` | `Check` | Required | Any authenticated user | `recipeId: Guid` | None | None | `{ isFavorite }` | 200, 401 | Check favorite status | `FavoriteService.check()`; no current caller found |
| POST | `/api/Reviews` | `ReviewsController` | `Create` | Required | Any authenticated user | None | None | `CreateReviewDto` | Empty body or error string | 200, 400, 401 | Add recipe review | `ReviewService.create()`, `RecipeDetailsComponent` |
| GET | `/api/Reviews/recipe/{recipeId}` | `ReviewsController` | `GetByRecipe` | Anonymous | None | `recipeId: Guid` | None | None | `ReviewDto[]` | 200 | List reviews for recipe | `ReviewService.getByRecipe()`, `RecipeDetailsComponent` |
| PUT | `/api/Reviews/{id}` | `ReviewsController` | `Update` | Required | Review owner enforced in service | `id: Guid` | None | `UpdateReviewDto` | Empty body or error string | 200, 400, 401 | Update own review | `ReviewService.update()`; no current UI caller found |
| DELETE | `/api/Reviews/{id}` | `ReviewsController` | `Delete` | Required | Owner or `Admin` enforced in service | `id: Guid` | None | None | Empty body or error string | 200, 400, 401 | Delete own review or Admin delete | `ReviewService.delete()`; no current UI caller found |

## Swagger

- Available only in development.
- Configured in `API/Program.cs` with title `API`, version `v1`, and bearer security definition.

## Authorization Header

```text
Authorization: Bearer {accessToken}
```

Angular adds this header in `app/src/app/interceptors/auth.interceptor.ts`.

## Pagination Response Shape

Source: `PagedResult<RecipieDto>` from `RecipesController.GetPaged`.

```json
{
  "items": [],
  "total": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

`page` and `pageSize` are the normalized values used by the query. Rules: minimum page `1`, minimum page size `1`, maximum page size `100`.

## Error Response Conventions

- Transitioning conventions: invalid difficulty and invalid refresh token use problem-details style responses.
- Auth login still returns `Unauthorized("Invalid credentials")`.
- Service failures are usually returned as plain strings via `BadRequest(result.Error)` or `NotFound(result.Error)`.
- Some success responses are empty `Ok()`.

## Backend/Frontend Mismatches

- Recipe mutations are Admin-or-Operator in backend and UI.
- Recipe difficulty request/filter values are string names: `Easy`, `Medium`, `Hard`.
- Angular category model omits backend `description` and `createdAt`, which is safe for current UI but incomplete.
- `FavoriteService.check()` and review update/delete service methods have backend routes but no current UI caller found.
- `RecipeService.getAll()` has a backend route but no current UI caller found.
