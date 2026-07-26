# API Endpoint Inventory

Last verified: 2026-07-26
Branch: `main`
Commit: `1de804620330d10f9ee6b493ecac423f6ab288b2`

Swagger is configured in `API/Program.cs` and is available only in Development. Bearer auth format is `Authorization: Bearer {token}`.

## Endpoints

| Method | Exact route | Controller/action | Authentication | Role/policy | Route parameters | Query parameters | Request DTO | Response | Status codes | Purpose | Frontend caller |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| POST | `/api/Auth/login` | `AuthController.Login` | Anonymous | None | None | None | `LoginDto` | `{ accessToken, refreshToken }` | 200, 401 | Login | `AuthService.login`, login page |
| POST | `/api/Auth/register` | `AuthController.Register` | Anonymous | None | None | None | `RegisterDto` | string or `ProblemDetails` | 200, 400, 409 | Register user | `AuthService.register`, register page |
| POST | `/api/Auth/refresh` | `AuthController.Refresh` | Anonymous | None | None | None | `TokenRequestDto` | `{ accessToken, refreshToken }` | 200, 401 | Rotate refresh token | `AuthService.refreshSession`, `ErrorInterceptor` |
| GET | `/api/Categories` | `CategoriesController.Get` | Anonymous | None | None | None | None | `Category[]` | 200 | List categories | `CategoryService.getAll` |
| GET | `/api/Recipes` | `RecipesController.Get` | Bearer | Any authenticated | None | None | None | `PagedResult<RecipieDto>` | 200, 401 | First 100 recipes | `RecipeService.getAll` |
| GET | `/api/Recipes/paged` | `RecipesController.GetPaged` | Bearer | Any authenticated | None | `Page`, `PageSize`, `Search`, `Difficulty`, `CategoryId`, `UserId`, `CuisineId`, `RegionId`, `IsTraditional`, `SortBy` | None | `PagedResult<RecipieDto>` | 200, 400, 401 | Filtered recipe list | `RecipeService.getPaged`, recipes page |
| GET | `/api/Recipes/me` | `RecipesController.GetMine` | Bearer | Any authenticated | None | Same as paged; service overwrites `UserId` | None | `PagedResult<RecipieDto>` | 200, 400, 401 | Current user's recipes | `RecipeService.getMine`, My Recipes page |
| GET | `/api/Recipes/{id}` | `RecipesController.GetById` | Bearer | Any authenticated | `id` | None | None | `RecipieDto` | 200, 401, 404 | Recipe details | `RecipeService.getById`, details/edit pages |
| POST | `/api/Recipes` | `RecipesController.Create` | Bearer | Any authenticated | None | None | `CreateRecipeDto` | `RecipieDto` | 201, 400, 401 | Create owned recipe | `RecipeService.create`, create/edit pages |
| PUT | `/api/Recipes/{id}` | `RecipesController.Update` | Bearer | Owner or Admin in service | `id` | None | `CreateRecipeDto` | `RecipieDto` | 200, 400, 401, 403, 404 | Update recipe | `RecipeService.update`, recipes page |
| DELETE | `/api/Recipes/{id}` | `RecipesController.Delete` | Bearer | Owner or Admin in service | `id` | None | None | empty | 204, 401, 403, 404 | Delete recipe | `RecipeService.delete`, recipes/My Recipes pages |
| POST | `/api/Favorites/{recipeId}` | `FavoritesController.Add` | Bearer | Any authenticated | `recipeId` | None | None | empty | 200, 401, 409 | Favorite recipe | `FavoriteService.add` |
| DELETE | `/api/Favorites/{recipeId}` | `FavoritesController.Remove` | Bearer | Any authenticated | `recipeId` | None | None | empty/string error | 200, 401, 404 | Remove favorite | `FavoriteService.remove` |
| GET | `/api/Favorites/me` | `FavoritesController.GetMine` | Bearer | Any authenticated | None | None | None | `FavoriteRecipeDto[]` | 200, 401 | User favorites | `FavoriteService.getMine` |
| GET | `/api/Favorites/check/{recipeId}` | `FavoritesController.Check` | Bearer | Any authenticated | `recipeId` | None | None | `{ isFavorite }` | 200, 401 | Check favorite | `FavoriteService.check` |
| POST | `/api/Reviews` | `ReviewsController.Create` | Bearer | Any authenticated | None | None | `CreateReviewDto` | empty | 200, 401, 409 | Add review | `ReviewService.create` |
| GET | `/api/Reviews/recipe/{recipeId}` | `ReviewsController.GetByRecipe` | Anonymous | None | `recipeId` | None | None | `ReviewDto[]` | 200 | List reviews by recipe | `ReviewService.getByRecipe`, details page |
| PUT | `/api/Reviews/{id}` | `ReviewsController.Update` | Bearer | Review owner in service | `id` | None | `UpdateReviewDto` | empty/string error | 200, 400, 401 | Update review | `ReviewService.update` |
| DELETE | `/api/Reviews/{id}` | `ReviewsController.Delete` | Bearer | Review owner or Admin in service | `id` | None | None | empty/string error | 200, 400, 401 | Delete review | `ReviewService.delete` |
| GET | `/api/admin/users` | `AdminUsersController.GetPaged` | Bearer | Admin | None | `Page`, `PageSize`, `Search`, `Role`, `IsActive` | None | `PagedUsersDto` | 200, 401, 403, 400 | Manage users list | `UserManagementService.getPaged`, admin accounts page |
| GET | `/api/admin/users/{id}` | `AdminUsersController.GetById` | Bearer | Admin | `id` | None | None | `UserAccountDto` | 200, 401, 403, 404 | User details | `UserManagementService.getById` |
| POST | `/api/admin/users` | `AdminUsersController.Create` | Bearer | Admin | None | None | `CreateUserAccountDto` | `UserAccountDto` | 201, 400, 401, 403, 409 | Create account | admin accounts page |
| PUT | `/api/admin/users/{id}/role` | `AdminUsersController.UpdateRole` | Bearer | Admin | `id` | None | `UpdateUserRoleDto` | `UserAccountDto` | 200, 400, 401, 403, 404, 409 | Change role | admin accounts page |
| PUT | `/api/admin/users/{id}/status` | `AdminUsersController.UpdateStatus` | Bearer | Admin | `id` | None | `UpdateUserStatusDto` | `UserAccountDto` | 200, 400, 401, 403, 404, 409 | Activate/deactivate account | admin accounts page |
| DELETE | `/api/admin/users/{id}` | `AdminUsersController.Delete` | Bearer | Admin | `id` | None | None | empty | 204, 400, 401, 403, 404, 409 | Delete account | admin accounts page |
| GET | `/api/Cuisines` | `CuisinesController.Get` | Anonymous | None | None | None | None | `CuisineDto[]` | 200 | List active cuisines | `CuisineService.getAll`, recipes/create pages |
| GET | `/api/Cuisines/{id}` | `CuisinesController.GetById` | Anonymous | None | `id` | None | None | `CuisineDto` | 200, 404 | Cuisine details | `CuisineService.getById` |
| GET | `/api/Cuisines/{id}/regions` | `CuisinesController.GetRegions` | Anonymous | None | `id` | None | None | `RegionDto[]` | 200 | Regions for cuisine | `CuisineService.getRegions`, recipe forms/filters |
| POST | `/api/Cuisines` | `CuisinesController.Create` | Bearer | Admin | None | None | `CreateCuisineDto` | `CuisineDto` | 201, 400, 401, 403, 409 | Create cuisine | No Angular Admin UI found |
| PUT | `/api/Cuisines/{id}` | `CuisinesController.Update` | Bearer | Admin | `id` | None | `UpdateCuisineDto` | `CuisineDto` | 200, 400, 401, 403, 404, 409 | Update cuisine | No Angular Admin UI found |
| DELETE | `/api/Cuisines/{id}` | `CuisinesController.Delete` | Bearer | Admin | `id` | None | None | empty | 204, 401, 403, 404, 409 | Delete cuisine | No Angular Admin UI found |
| GET | `/api/Regions/{id}` | `RegionsController.GetById` | Anonymous | None | `id` | None | None | `RegionDto` | 200, 404 | Region details | `RegionService.getById` |
| POST | `/api/Regions` | `RegionsController.Create` | Bearer | Admin | None | None | `CreateRegionDto` | `RegionDto` | 201, 400, 401, 403, 404, 409 | Create region | No Angular Admin UI found |
| PUT | `/api/Regions/{id}` | `RegionsController.Update` | Bearer | Admin | `id` | None | `UpdateRegionDto` | `RegionDto` | 200, 400, 401, 403, 404, 409 | Update region | No Angular Admin UI found |
| DELETE | `/api/Regions/{id}` | `RegionsController.Delete` | Bearer | Admin | `id` | None | None | empty | 204, 401, 403, 404, 409 | Delete region | No Angular Admin UI found |

## Response Shapes

Paged recipes and users use:

```json
{
  "items": [],
  "total": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

Recipe DTOs include safe recipe author data:

```json
{
  "author": {
    "id": "guid",
    "displayName": "Public name"
  }
}
```

Validation errors for model-state failures use `API/Responses/ApiErrorResponse.cs`:

```json
{
  "code": "validation_failed",
  "message": "The request is invalid.",
  "errors": {},
  "traceId": "..."
}
```

Older endpoints still use mixed string and `ProblemDetails` responses.

## Known API Mismatches And Risks

- Recipe read endpoints require authentication; culture GET endpoints are anonymous.
- `RecipeQueryParams.UserId` is public-bindable on `/api/Recipes/paged`; `GET /api/Recipes/me` overwrites it server-side.
- `ReviewDto` exposes `UserEmail`; recipe author DTOs do not expose email.
- `CategoriesController` returns EF entities directly.
- Error response contract is not uniform across all controllers.
