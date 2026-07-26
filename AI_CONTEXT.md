# RecepieV3 AI Context

Last verified: 2026-07-26
Branch: `main`
Commit: `1de804620330d10f9ee6b493ecac423f6ab288b2`
Working tree: dirty. Many backend/frontend/test changes for recipe ownership and cuisine/region support are uncommitted. Treat them as active work and do not overwrite them.

This document is the entry point for future AI agents and developers. It describes the repository as inspected from code, not from product intentions. Documentation must reflect implemented code. Planned behavior must be labeled as planned or recommended.

## Project Identity

RecepieV3 / Recep V2 is a full-stack social recipe application. Current code supports authenticated users publishing recipes, favoriting recipes, rating/reviewing recipes, and exploring recipes by category, difficulty, cuisine, region, and traditional metadata.

Important naming rules:

- Preserve `Recipie`, `RecipieDto`, `RecipieStep`, `Recipies`, `Recep`, and `Recepes`.
- Preserve the existing controller filename `API/Controller/RecipesController .cs`; it contains a space before `.cs`.
- Do not rename domain, DTO, migration, route, or Angular symbols casually.

## Current Architecture

| Area | Evidence | Notes |
| --- | --- | --- |
| Solution | `Recep.sln` | Projects: `API`, `Core.Application`, `Core.Domain`, `Infrastructure`, and test projects under `tests/`. |
| Backend API | `API/Program.cs`, `API/Controller/` | ASP.NET Core Web API with controllers, JWT bearer auth, Swagger, CORS, EF migrations/seeding at startup outside Testing. |
| Domain | `Core.Domain/Entities/`, `Core.Domain/Enums/` | Entities are simple EF-oriented classes; `BaseEntity` contains `Id` and `CreatedAt`. |
| Application | `Core.Application/UseCases/`, `Core.Application/Interfaces/`, `Core.Application/DTO/`, `Core.Application/Validators/` | Service layer, repository interfaces, DTOs, FluentValidation, `Result` and `ServiceResult` conventions. |
| Infrastructure | `Infrastructure/Persistence/`, `Infrastructure/Repositories/`, `Infrastructure/Seed/` | EF Core SQL Server context, configurations, migrations, repository implementations, seed data. |
| Frontend | `app/src/app/` | Angular 18 standalone components with route guards, interceptors, services, and template-driven forms. |
| Tests | `tests/`, Angular `*.spec.ts` | Backend xUnit/SQLite tests exist. Angular Karma tests compile but require Chrome at runtime. |

Dependency direction mostly follows `API -> Core.Application -> Core.Domain` and `Infrastructure -> Core.Application/Core.Domain`. Some controllers still use `AppDbContext` directly, notably `API/Controller/AuthController.cs` and `API/Controller/CategoriesController.cs`.

## Repository Tree

```text
API/                         ASP.NET Core API, controllers, startup, options, responses
Core.Application/            DTOs, interfaces, services/use cases, validators, common result types
Core.Domain/                 Entities, enums, constants, BaseEntity
Infrastructure/              EF DbContext, configurations, migrations, repositories, seeders
app/                         Angular 18 standalone frontend
tests/                       Backend unit and integration tests
docs/                        Context documentation
README.md                    General repository readme
Recep.sln                    .NET solution
```

Generated/dependency directories such as `bin/`, `obj/`, `node_modules/`, `dist/`, `.angular/`, and `.git/` should not be used as source evidence except for build diagnostics.

## Backend Summary

JWT authentication is configured in `API/Program.cs`. Tokens are generated in `API/Controller/AuthController.cs` and include `ClaimTypes.NameIdentifier`, `ClaimTypes.Name`, and `ClaimTypes.Role`.

Recipe publishing is implemented in:

- `API/Controller/RecipesController .cs`
- `Core.Application/UseCases/Recipes/RecipeService.cs`
- `Infrastructure/Repositories/RecipeRepository.cs`
- `Core.Domain/Entities/Recipie.cs`

Culture discovery is implemented in:

- `Core.Domain/Entities/Cuisine.cs`
- `Core.Domain/Entities/Region.cs`
- `API/Controller/CuisinesController.cs`
- `API/Controller/RegionsController.cs`
- `Core.Application/UseCases/Cuisines/CuisineService.cs`
- `Core.Application/UseCases/Regions/RegionService.cs`

Admin account management is implemented in `API/Controller/AdminUsersController.cs` and `Core.Application/UseCases/Users/UserManagementService.cs`.

## Frontend Summary

Angular bootstrap uses standalone APIs in `app/src/main.ts` and `app/src/app/app.config.ts`. Routes are declared in `app/src/app/app.routes.ts`.

Implemented pages include:

- Recipes explore/list: `app/src/app/pages/recipes/`
- Recipe details: `app/src/app/recipe-details/`
- Create recipe: `app/src/app/pages/create-recipe/`
- My Recipes: `app/src/app/pages/my-recipes/`
- Login/register: `app/src/app/login/`, `app/src/app/pages/register/`
- Admin accounts: `app/src/app/pages/admin/accounts/`

API base URL is centralized in `app/src/app/app-api.config.ts` as `http://localhost:5130/api`.

Frontend redesign status verified on 2026-07-26:

- `app/src/styles.css` defines the editorial paper/forest-green design system.
- `app/src/app/app.component.*` provides the skip link, main landmark, footer, and shell.
- `app/src/app/shared/navbar/*` provides the responsive RECIPIE navbar.
- `app/src/app/shared/components/` contains reusable recipe card, skeleton, empty-state, loading, and page-header primitives.
- Explore, recipe details, create recipe, My Recipes, login/register, and Admin accounts pages were redesigned while preserving the existing routes and service calls.
- `npm run build` passes with budget warnings; `npm test -- --watch=false` compiles but cannot launch Chrome in the current environment.

## Database Summary

Active context: `Infrastructure/Persistence/AppDbContext.cs`.

Provider registration: `Infrastructure/DependencyInjection.cs` uses SQL Server with `ConnectionStrings:DefaultConnection`.

Configured tables include `Users`, `Recipes`, `Categories`, `Ingredients`, `RecipeSteps`, `FavoriteRecipes`, `RecipeReviews`, `Cuisines`, and `Regions`.

Uncommitted migrations currently present:

- `Infrastructure/Migrations/20260726135722_AddRecipeOwnershipAndUserDisplayName.cs`
- `Infrastructure/Migrations/20260726145257_AddCuisineAndRegionSupport.cs`

Do not create or modify migrations during documentation-only work.

## Authentication Summary

Registration (`POST /api/Auth/register`) creates active `User` accounts with a public `DisplayName`. Login (`POST /api/Auth/login`) validates password hashes via `Infrastructure/services/PasswordService.cs`, stores a refresh token on `Users`, and returns access/refresh tokens. Refresh (`POST /api/Auth/refresh`) rotates refresh tokens.

Frontend tokens are stored in `localStorage` under `accessToken` and `refreshToken` by `app/src/app/services/auth.service.ts`. `AuthInterceptor` adds bearer tokens and `ErrorInterceptor` attempts refresh on 401.

## Main Workflows

| Workflow | Implemented path |
| --- | --- |
| Register/login | `AuthController`, `AuthService`, login/register components |
| Browse recipes | `/recipes`, `RecipesController.GetPaged`, `RecipeService.GetPagedAsync` |
| Create recipe | `/create-recipe`, `RecipesController.Create`, owner from JWT |
| Manage own recipes | `/my-recipes`, `GET /api/Recipes/me`, owner/Admin update/delete |
| Favorite recipes | `FavoritesController`, `FavoriteService`, frontend favorite buttons |
| Review recipes | `ReviewsController`, `ReviewService`, recipe details review form |
| Manage accounts | `/admin/accounts`, `AdminUsersController`, Admin role only |
| Explore culture | cuisine/region endpoints and recipe filters |

## Implemented Features

- JWT login, refresh, and route guarding.
- Registration with `DisplayName`.
- Recipe list, search, filtering, sorting, and pagination.
- Recipe details with ingredients, steps, favorites, and reviews.
- User-owned recipe create/update/delete with Admin override.
- My Recipes page.
- Cuisine and region entities, public read endpoints, Admin write endpoints.
- Cultural recipe metadata and filters.
- Admin account management.
- Backend xUnit tests for auth, ownership, cuisine/region, favorites, reviews, and difficulty behavior.

## Missing Or Incomplete Features

- No social feeds, follows, notifications, sharing workflow, uploads, localization, real-time features, Redis, RabbitMQ, Kafka, GraphQL, microservices, or load balancing were found.
- No Angular Admin UI for cuisine/region management was found; backend endpoints exist.
- `RecipeReviewDto` exposes `UserEmail`; recipe author DTOs avoid email.
- `RecipeImage` exists but has incomplete/ambiguous EF mapping and no active DbSet or UI workflow.
- Angular browser tests require Chrome; compilation can pass while Karma cannot launch without `CHROME_BIN`.
- `app/src/app/app.module.ts` exists but standalone bootstrap is used, so it appears unused.

## Critical Known Issues

- `API/appsettings.json` and `API/appsettings.Development.json` are modified in the working tree. Never copy their values into docs or logs; use `[REDACTED]`.
- `Program.cs` runs `db.Database.Migrate()` during startup outside Testing. This can mutate the configured database when the API starts.
- `CategoriesController` and `AuthController` bypass the service/repository pattern and use `AppDbContext` directly.
- Review responses expose user email via `Core.Application/DTO/Reviews/ReviewDto.cs`.
- The working tree includes untracked archives `back.zip` and `front.zip` and an `images/` directory.

## Security Rules

- Never expose connection strings, JWT keys, passwords, password hashes, refresh tokens, API keys, or user secrets.
- Replace any sensitive value in docs with `[REDACTED]`.
- Backend authorization is the source of truth; frontend button visibility is UI only.
- Ownership-sensitive code must use `ClaimTypes.NameIdentifier`, not frontend-supplied ownership.
- Do not log complete configuration or connection strings.

## Files Not To Change Casually

- `API/Controller/RecipesController .cs`
- `Core.Domain/Entities/Recipie.cs`
- `Core.Application/DTO/Recipe/RecipieDto.cs`
- `Infrastructure/Migrations/`
- `Infrastructure/Migrations/AppDbContextModelSnapshot.cs`
- `API/appsettings.json`
- `API/appsettings.Development.json`
- Any uncommitted PerformancePlatform-related work if it appears later.

## Build And Test Commands

From repository root:

```bash
dotnet restore
dotnet build Recep.sln
dotnet test Recep.sln
```

Frontend:

```bash
cd app
npm install
npm run build
npm test -- --watch=false
```

Documentation inventory:

```bash
find . -maxdepth 2 -type f \( -name "AI_CONTEXT.md" -o -path "./docs/*.md" \) | sort
```

## Documentation Index

- `docs/PROJECT_OVERVIEW.md`
- `docs/BACKEND_CONTEXT.md`
- `docs/FRONTEND_CONTEXT.md`
- `docs/DATABASE_CONTEXT.md`
- `docs/API_ENDPOINTS.md`
- `docs/FEATURES_CONTEXT.md`
- `docs/KNOWN_ISSUES_AND_RULES.md`
- `docs/RECENT_CHANGES_CONTEXT.md`
- `docs/CHANGE_GUIDE.md`

## Instructions For Future Agents

1. Inspect the current working tree before editing.
2. Treat uncommitted source changes as active user work.
3. Do not modify app behavior while doing documentation tasks.
4. Verify every architecture or feature statement against source files.
5. Keep docs synchronized with implemented code, not intended behavior.
6. Do not create migrations unless explicitly asked.
7. Preserve misspelled established names.
