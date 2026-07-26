# Project Overview

Last verified: 2026-07-26
Branch: `main`
Commit: `1de804620330d10f9ee6b493ecac423f6ab288b2`
Working tree: dirty with active uncommitted backend, frontend, migration, test, generated, and documentation changes.

## Product

RecepieV3 / Recep V2 is implemented as a recipe-sharing web application. The current code supports authenticated user accounts, JWT authentication, recipe publishing, favorites, reviews, categories, difficulty filters, Admin account management, and cultural discovery through cuisines and regions.

The codebase now points toward a social cooking network rather than an Admin-only recipe catalog. Recipe ownership is represented in `Core.Domain/Entities/Recipie.cs` with `UserId` and `User`, and public recipe responses expose safe author display data via `Core.Application/DTO/Users/AuthorDto.cs`.

## Solution Projects

| Project | Path | Role |
| --- | --- | --- |
| API | `API/API.csproj` | ASP.NET Core Web API, controllers, auth, startup, Swagger, app configuration. |
| Core.Domain | `Core.Domain/Core.Domain.csproj` | Entities, enums, constants, base entity. |
| Core.Application | `Core.Application/Core.Application.csproj` | DTOs, service interfaces, repository interfaces, use cases, validators, result types. |
| Infrastructure | `Infrastructure/Infrastructure.csproj` | EF Core DbContext, configurations, migrations, repositories, seed data, password hashing. |
| Core.Application.Tests | `tests/Core.Application.Tests/Core.Application.Tests.csproj` | Application/repository-focused xUnit tests. |
| API.IntegrationTests | `tests/API.IntegrationTests/API.IntegrationTests.csproj` | Older API integration tests. |
| Recep.UnitTests | `tests/Recep.UnitTests/Recep.UnitTests.csproj` | Unit tests, including Admin seeding. |
| Recep.IntegrationTests | `tests/Recep.IntegrationTests/Recep.IntegrationTests.csproj` | API behavior integration tests using SQLite in-memory. |
| Angular app | `app/package.json` | Angular 18 standalone frontend. |

## Architecture Summary

The backend is layered but not perfectly strict:

- `API/` depends on application services and also directly uses `AppDbContext` in `AuthController` and `CategoriesController`.
- `Core.Application/` defines DTOs, interfaces, use cases, validators, and result types.
- `Core.Domain/` contains EF-oriented entities and constants.
- `Infrastructure/` implements repositories, EF configuration, migrations, seeding, and password hashing.

Frontend architecture is Angular standalone:

- Bootstrap: `app/src/main.ts`
- Providers: `app/src/app/app.config.ts`
- Routes: `app/src/app/app.routes.ts`
- Interceptors: `app/src/app/interceptors/`
- Services: `app/src/app/services/`
- Pages/components: `app/src/app/pages/`, `app/src/app/recipe-details/`, `app/src/app/shared/`

## Frontend Design System

Last verified: 2026-07-26 after frontend redesign.

The Angular frontend now presents RecepieV3 as an editorial cultural cooking product instead of a generic Bootstrap dashboard.

Confirmed implementation:

- `app/src/styles.css` defines the global paper texture, forest-green palette, serif heading scale, sans-serif UI stack, button variants, forms, focus states, and reduced-motion rule.
- `app/src/app/app.component.*` provides the skip link, main landmark, navbar, router outlet, and footer.
- `app/src/app/shared/navbar/*` provides a sticky responsive RECIPIE navbar.
- `app/src/app/shared/components/recipe-card/` is the shared recipe card used by Explore and My Recipes.
- `app/src/app/pages/recipes/*` is the recipe-first discovery surface with cuisine tiles and filters.
- `app/src/app/recipe-details/*` is the article-style recipe reading page.
- `app/src/app/pages/create-recipe/*` is the sectioned recipe publishing form.
- `app/src/app/pages/my-recipes/*`, `app/src/app/login/*`, `app/src/app/pages/register/*`, and `app/src/app/pages/admin/accounts/*` were restyled into the same system.

Frontend build status: `npm run build` passed on 2026-07-26 with budget warnings. Browser tests compiled but could not execute because Chrome was unavailable.

## Main Implemented Workflows

| Workflow | Backend files | Frontend files | Status |
| --- | --- | --- | --- |
| Registration | `API/Controller/AuthController.cs`, `Core.Application/DTO/Auth/RegisterDto.cs` | `app/src/app/pages/register/` | Implemented; requires display name. |
| Login/refresh | `AuthController`, `Program.cs`, `PasswordService.cs` | `AuthService`, interceptors, guards | Implemented. |
| Recipe listing | `RecipesController .cs`, `RecipeService.cs`, `RecipeRepository.cs` | `pages/recipes/`, `recipe.service.ts` | Implemented with filters and pagination. |
| Recipe creation | same recipe backend files | `pages/create-recipe/` | Implemented; authenticated users only. |
| Recipe ownership | `Recipie.cs`, `RecipeService.cs`, `ClaimsPrincipalExtensions.cs` | `AuthService`, recipe templates | Implemented server-side with owner/Admin rules. |
| My Recipes | `GET /api/Recipes/me` | `pages/my-recipes/` | Implemented. |
| Favorites | `FavoritesController`, `FavoriteService`, `FavoriteRepository` | `favorite.service.ts`, recipe list/details | Implemented. |
| Reviews | `ReviewsController`, `ReviewService`, `ReviewRepository` | `review.service.ts`, recipe details | Implemented; review DTO exposes email. |
| Admin accounts | `AdminUsersController`, `UserManagementService` | `pages/admin/accounts/` | Implemented for Admin role. |
| Cuisines/regions | `CuisinesController`, `RegionsController`, culture services/repos/entities | `cuisine.service.ts`, `region.service.ts`, recipes/details pages | Implemented; no Admin culture UI. |

## External Integrations

No confirmed runtime PerformancePlatform integration was found by searching source identifiers. The code uses no Redis, RabbitMQ, Kafka, GraphQL, Elasticsearch, external upload provider, or real-time transport. Seed data references external image URLs in `Infrastructure/Seed/DbSeeder.cs`.

## Configuration

Configuration is read through ASP.NET Core configuration in `API/Program.cs`, `Infrastructure/DependencyInjection.cs`, `Infrastructure/Persistence/AppDbContextFactory.cs`, and `Infrastructure/Seed/DbSeeder.cs`.

Sensitive values in `API/appsettings.json`, `API/appsettings.Development.json`, user secrets, and environment variables must be treated as `[REDACTED]`.

## Current Limitations

- Startup applies EF migrations automatically outside Testing.
- Some controllers bypass services/repositories.
- Review responses expose `UserEmail`.
- Culture management has backend endpoints but no Angular Admin UI.
- `RecipeImage` mapping is incomplete/ambiguous.
- Angular Karma tests need Chrome.
- Documentation and feature work are currently uncommitted.
