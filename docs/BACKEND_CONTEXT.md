# Backend Context

## Project Structure

```text
API/
  Controller/
  Middleware/
  Services/
  Program.cs
  appsettings*.json
Core.Application/
  Common/
  DTO/
  Interfaces/
  UseCases/
  Validators/
Core.Domain/
  Common/
  Entities/
  Enums/
Infrastructure/
  Persistence/
  Repositories/
  Seed/
  services/
  Migrations/
```

## Controllers And Endpoints

- `API/Controller/AuthController.cs`: registration, login, refresh token creation, JWT claims.
- `API/Controller/RecipesController .cs`: recipe list/detail/create/update/delete. Exact filename includes a space before `.cs`.
- `API/Controller/AdminUsersController.cs`: Admin-only account management at route `api/admin/users`.
- `API/Controller/CategoriesController.cs`: category list.
- `API/Controller/FavoritesController.cs`: current-user favorites.
- `API/Controller/ReviewsController.cs`: review create/list/update/delete.

Complete endpoint table is in `docs/API_ENDPOINTS.md`.

## Controller Responsibilities And Dependencies

- `AuthController`: depends on `AppDbContext`, `IPasswordService`, `IConfiguration`; hashes/verifies passwords, issues JWT access tokens, stores refresh tokens on `Users`.
- `RecipesController`: depends on `IRecipeService`; delegates recipe reads and mutations.
- `CategoriesController`: depends directly on `AppDbContext`; returns all categories.
- `FavoritesController`: depends on `IFavoriteService`; resolves current user from `ClaimTypes.NameIdentifier`.
- `ReviewsController`: depends on `IReviewService`; resolves current user from `ClaimTypes.NameIdentifier`, delegates review ownership/admin checks to service.
- `AdminUsersController`: depends on `IUserManagementService`; maps service validation/not-found/conflict outcomes to JSON `{ "message": "..." }`.

## DTOs And Validation

- Auth: `Core.Application/DTO/Auth/LoginDtos.cs`, `TokenRequestDto.cs`.
- Recipe: `Core.Application/DTO/Recipe/CreateRecipeDto.cs`, `RecipieDto.cs`, `Core.Application/DTO/RecipeQueryParams.cs`.
- Favorites: `Core.Application/DTO/Favorites/FavoriteRecipeDto.cs`.
- Reviews: `Core.Application/DTO/Reviews/CreateReviewDto.cs`, `ReviewDto.cs`, `UpdateReviewDto.cs`.
- User accounts: `Core.Application/DTO/Users/UserAccountDto.cs`, `CreateUserAccountDto.cs`, `UpdateUserRoleDto.cs`, `UpdateUserStatusDto.cs`, `UserQueryParameters.cs`, `PagedUsersDto.cs`.
- FluentValidation: `Core.Application/Validators/CreateRecipeValidator.cs` validates title, description, difficulty string (`Easy`, `Medium`, `Hard`), positive preparation time, and non-empty category ID for `CreateRecipeDto`.
- Account validators: `CreateUserAccountValidator`, `UpdateUserRoleValidator`, `UpdateUserStatusValidator`.
- Review rating validation is implemented manually in `Core.Application/UseCases/Reviews/ReviewService.cs`.

## Interfaces And Implementations

- `IRecipeService` -> `RecipeService`.
- `IFavoriteService` -> `FavoriteService`.
- `IReviewService` -> `ReviewService`.
- `IUserManagementService` -> `UserManagementService`.
- `IPasswordService` -> `PasswordService`.
- `IRecipeRepository` -> `RecipeRepository`.
- `IFavoriteRepository` -> `FavoriteRepository`.
- `IReviewRepository` -> `ReviewRepository`.
- `IUserRepository` -> `UserRepository`.

## Application Services

- `Core.Application/UseCases/Recipes/RecipeService.cs`: maps `Recipie` to `RecipieDto`, creates/updates/deletes recipe entities, delegates paging/filtering to repository.
- `Core.Application/UseCases/Favorites/FavoriteService.cs`: verifies recipe existence, prevents duplicate favorites, maps favorites to DTOs.
- `Core.Application/UseCases/Reviews/ReviewService.cs`: validates rating, prevents duplicate user review per recipe, enforces owner/admin delete and owner-only update.
- `Core.Application/UseCases/Users/UserManagementService.cs`: normalizes account emails, hashes Admin-created passwords, enforces role/status/delete safety rules, invalidates refresh tokens on role/status changes.

## Repository Layer

- `Infrastructure/Repositories/RecipeRepository.cs`: projects paged recipe lists without loading ingredients/steps, loads detailed collections for detail reads, applies search, category, difficulty, sorting, and page-size cap of 100.
- `Infrastructure/Repositories/FavoriteRepository.cs`: queries unique `(UserId, RecipeId)` favorites and includes recipe for favorite DTO mapping.
- `Infrastructure/Repositories/ReviewRepository.cs`: includes user for review DTO email.
- `Infrastructure/Repositories/UserRepository.cs`: paged account query, lookup, active Admin count, related favorite/review existence checks, and account persistence.

## Dependency Injection

`Infrastructure/DependencyInjection.cs` registers:

- `AppDbContext` using SQL Server and `DefaultConnection`.
- `IRecipeRepository`, `IFavoriteRepository`, `IReviewRepository`, `IUserRepository`.

`API/Program.cs` registers:

- Controllers and Swagger.
- Infrastructure.
- `IRecipeService`, `IFavoriteService`, `IReviewService`, `IUserManagementService`.
- `IPasswordService`.
- FluentValidation auto-validation and `CreateRecipeValidator`.

## Middleware Pipeline

Confirmed order in `API/Program.cs`:

1. Swagger/SwaggerUI only in development.
2. HTTPS redirection.
3. CORS policy `AllowAngular`.
4. Authentication.
5. Authorization.
6. Map controllers.
7. Create scope, `db.Database.Migrate()`, `DbSeeder.SeedAsync(db, configuration, passwordService)`.

## Authentication And Authorization

- JWT bearer scheme configured in `API/Program.cs`.
- Issuer and audience validation disabled.
- Lifetime and signing key validation enabled.
- `ClockSkew = TimeSpan.Zero`.
- JWT key from `Jwt:Key`; app throws if missing.
- Claims in `AuthController.GenerateJwtToken`:
  - `ClaimTypes.NameIdentifier`: user ID.
  - `ClaimTypes.Name`: user email.
  - `ClaimTypes.Role`: user role.
- `OnTokenValidated` in `API/Program.cs` rejects tokens when the account no longer exists, is inactive, or has a different current database role.
- Role names are centralized in `Core.Domain/Constants/AppRoles.cs`.
- Recipe mutations use Admin-or-Operator authorization.
- Account-management endpoints use Admin authorization.

## CORS And Swagger

- CORS policy name `AllowAngular` allows origin `http://localhost:4200`, any header, any method.
- Swagger document `v1` with JWT bearer security definition is enabled only when `app.Environment.IsDevelopment()`.

## Database Startup And Seed

- Startup applies migrations automatically using `db.Database.Migrate()`.
- Startup calls active seeder `Infrastructure/Seed/DbSeeder.cs`.
- Optional first Admin bootstrap reads `SeedAdmin:Email` and `SeedAdmin:Password` from configuration/user secrets/environment and only runs when no Admin exists.
- Older seeder `Infrastructure/Persistence/DataSeeder.cs` exists but is not called.

## Logging

- Standard ASP.NET logging in appsettings.
- Standard ASP.NET Core logging remains. The previous external performance middleware and sensitive connection-string console output were removed.

## Commonly Modified Backend Files

- `API/Controller/*.cs`
- `API/Program.cs`
- `Core.Application/DTO/**`
- `Core.Application/Interfaces/**`
- `Core.Application/UseCases/**`
- `Core.Application/Validators/CreateRecipeValidator.cs`
- `Core.Domain/Entities/**`
- `Core.Domain/Enums/DifficultyLevel.cs`
- `Infrastructure/Persistence/AppDbContext.cs`
- `Infrastructure/Persistence/Configurations/**`
- `Infrastructure/Repositories/**`
- `Infrastructure/Seed/DbSeeder.cs`
- `Infrastructure/Migrations/**`

## Known Backend Risks

- `API/appsettings*.json` contain empty placeholders; use user secrets or environment variables for `ConnectionStrings__DefaultConnection` and `Jwt__Key`.
- Runtime migrations can change databases at API startup.
- Backend application tests exist in `tests/Core.Application.Tests`.
- Duplicate/confusing recipe EF configurations.
- `RecipeImage` mapping is suspicious and not exposed by API.
