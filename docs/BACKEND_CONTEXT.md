# Backend Context

Last verified: 2026-07-26
Branch: `main`
Commit: `1de804620330d10f9ee6b493ecac423f6ab288b2`
Working tree: dirty; includes uncommitted Phase 1/2 source and test changes.

## Startup And Middleware

Configured in `API/Program.cs`.

Middleware and startup order:

1. `AddControllers()`
2. Custom `ApiBehaviorOptions.InvalidModelStateResponseFactory` returning `ApiErrorResponse`
3. Swagger/OpenAPI with bearer definition
4. `AddInfrastructure(builder.Configuration)`
5. Application service registrations
6. CORS policy `AllowAngular` for `http://localhost:4200`
7. JWT bearer authentication
8. FluentValidation auto-validation
9. `app.UseSwagger()` and `UseSwaggerUI()` in Development
10. `UseHttpsRedirection()`
11. `UseCors("AllowAngular")`
12. `UseAuthentication()`
13. `UseAuthorization()`
14. `MapControllers()`
15. Outside `Testing`, `db.Database.Migrate()` then `DbSeeder.SeedAsync(...)`

Risk: API startup mutates the configured database outside Testing.

## Dependency Injection

| Registration | File |
| --- | --- |
| `AppDbContext` SQL Server | `Infrastructure/DependencyInjection.cs` |
| Repositories | `Infrastructure/DependencyInjection.cs` |
| `IPasswordService -> PasswordService` | `API/Program.cs` |
| `IRecipeService -> RecipeService` | `API/Program.cs` |
| `IFavoriteService -> FavoriteService` | `API/Program.cs` |
| `IReviewService -> ReviewService` | `API/Program.cs` |
| `IUserManagementService -> UserManagementService` | `API/Program.cs` |
| `ICuisineService -> CuisineService` | `API/Program.cs` |
| `IRegionService -> RegionService` | `API/Program.cs` |
| FluentValidation validators | `API/Program.cs` |
| `AdminSeedOptions` | `API/Program.cs`, `API/Options/AdminSeedOptions.cs` |

## Authentication

Files:

- `API/Controller/AuthController.cs`
- `API/Program.cs`
- `Infrastructure/services/PasswordService.cs`
- `Core.Domain/Entities/Users.cs`
- `Core.Domain/Constants/AppRoles.cs`

Implemented flow:

- Register normal users through `POST /api/Auth/register`.
- Login through `POST /api/Auth/login`.
- Refresh through `POST /api/Auth/refresh`.
- Password hashing uses BCrypt in `PasswordService`.
- Refresh tokens are stored on `Users.RefreshToken` with expiry.
- JWT claims include:
  - `ClaimTypes.NameIdentifier`
  - `ClaimTypes.Name`
  - `ClaimTypes.Role`
- JWT validation in `Program.cs` also checks that the user exists, is active, and still has the token role.

Issuer and audience validation are disabled in `Program.cs`; lifetime and signing key validation are enabled.

## Controllers

| Controller | File | Route prefix | Auth | Dependencies | Notes |
| --- | --- | --- | --- | --- | --- |
| `AuthController` | `API/Controller/AuthController.cs` | `/api/Auth` | Login/refresh/register allow anonymous by action or absence of class auth | `AppDbContext`, `IPasswordService`, `IConfiguration` | Direct DbContext usage. |
| `RecipesController` | `API/Controller/RecipesController .cs` | `/api/Recipes` | Controller and actions use `[Authorize]` | `IRecipeService` | Filename includes space. Owner/Admin enforcement is in service. |
| `CategoriesController` | `API/Controller/CategoriesController.cs` | `/api/Categories` | No auth attribute | `AppDbContext` | Direct DbContext usage; returns EF categories. |
| `FavoritesController` | `API/Controller/FavoritesController.cs` | `/api/Favorites` | `[Authorize]` | `IFavoriteService` | Current user from `ClaimTypes.NameIdentifier`. |
| `ReviewsController` | `API/Controller/ReviewsController.cs` | `/api/Reviews` | Create/update/delete authorized; get by recipe anonymous | `IReviewService` | Review DTO exposes `UserEmail`. |
| `AdminUsersController` | `API/Controller/AdminUsersController.cs` | `/api/admin/users` | `[Authorize(Roles = AppRoles.Admin)]` | `IUserManagementService` | Admin account management. |
| `CuisinesController` | `API/Controller/CuisinesController.cs` | `/api/Cuisines` | GET anonymous; write Admin | `ICuisineService` | Returns DTOs, not EF entities. |
| `RegionsController` | `API/Controller/RegionsController.cs` | `/api/Regions` | GET anonymous; write Admin | `IRegionService` | Returns DTOs, not EF entities. |

## Services

| Interface | Implementation | Responsibilities | Authorization |
| --- | --- | --- | --- |
| `IRecipeService` | `Core.Application/UseCases/Recipes/RecipeService.cs` | Recipe create/update/delete/list/details, owner assignment, ingredients/steps mapping, category/cuisine/region validation, DTO mapping. | Enforces owner-or-Admin on update/delete. |
| `ICuisineService` | `Core.Application/UseCases/Cuisines/CuisineService.cs` | List/get/create/update/delete cuisines, slug generation, duplicate checks, blocked deletion. | Controller restricts writes to Admin. |
| `IRegionService` | `Core.Application/UseCases/Regions/RegionService.cs` | Get/create/update/delete regions, parent cuisine validation, slug uniqueness inside cuisine, blocked deletion. | Controller restricts writes to Admin. |
| `IFavoriteService` | `Core.Application/UseCases/Favorites/FavoriteService.cs` | Add/remove/list/check favorites. | Controller requires authenticated user; service uses supplied user ID. |
| `IReviewService` | `Core.Application/UseCases/Reviews/ReviewService.cs` | Add/update/delete/list reviews, duplicate review/rating validation. | Owner update/delete; Admin can delete other reviews. |
| `IUserManagementService` | `Core.Application/UseCases/Users/UserManagementService.cs` | Admin user list/create/role/status/delete. | Controller Admin-only; service protects final Admin and self changes. |

Result conventions:

- Older favorite/review methods use `Core.Application/Common/Result.cs`.
- Newer recipe/culture/user management methods use `Core.Application/Common/ServiceResult.cs` with `ServiceErrorType`.

## Repositories

| Interface | Implementation | Entity | Query behavior |
| --- | --- | --- | --- |
| `IRecipeRepository` | `Infrastructure/Repositories/RecipeRepository.cs` | `Recipie` | Details include user/category/cuisine/region/ingredients/steps; paged list uses `AsNoTracking`, filters before count/pagination, then materializes entities. |
| `ICuisineRepository` | `Infrastructure/Repositories/CuisineRepository.cs` | `Cuisine` | Public list projects `CuisineDto` with counts; duplicate checks use `AnyAsync`; deletes blocked by service. |
| `IRegionRepository` | `Infrastructure/Repositories/RegionRepository.cs` | `Region` | Lists by cuisine with `AsNoTracking`, projections and counts. |
| `IFavoriteRepository` | `Infrastructure/Repositories/FavoriteRepository.cs` | `FavoriteRecipe` | Unique user/recipe checks; favorites include `Recipe` only. |
| `IReviewRepository` | `Infrastructure/Repositories/ReviewRepository.cs` | `RecipeReview` | Reviews include `User`; list is unpaged per recipe. |
| `IUserRepository` | `Infrastructure/Repositories/UserRepository.cs` | `Users` | Paged user management with `AsNoTracking`; role/status/search filters. |

Potential risks:

- `RecipeRepository.GetPagedAsync` materializes entities with includes rather than projecting only DTO fields.
- `ReviewRepository.GetByRecipeIdAsync` returns all reviews for a recipe without pagination.
- `CuisineRepository.ExistsByNameAsync` lowercases the column expression.

## Validation

`CreateRecipeValidator` in `Core.Application/Validators/CreateRecipeValidator.cs` validates:

- title/description required and max lengths
- preparation time range
- defined `DifficultyLevel`
- non-empty category and cuisine IDs
- image URL absolute HTTP/HTTPS when supplied
- cultural metadata lengths
- ingredient and step counts/field lengths
- unique, sequential step numbers

User management validators are in `Core.Application/Validators/`.

Registration display-name validation is currently implemented directly in `AuthController`, not a FluentValidation validator.

## Error Handling

Model validation returns `ApiErrorResponse` from `API/Responses/ApiErrorResponse.cs` for invalid model state. Recipe and culture controllers map `ServiceResult` to status codes. Older auth/favorite/review endpoints still return a mix of strings, `ProblemDetails`, `BadRequest`, `Conflict`, and `Unauthorized`.

## External Calls

No confirmed runtime PerformancePlatform middleware or HTTP integration was found. Seed data references external image URLs but does not fetch them during seeding.
