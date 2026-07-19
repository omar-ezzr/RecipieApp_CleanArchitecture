# Change Guide

## Add A Backend Endpoint

Files normally changed: `API/Controller/*.cs`, service interface in `Core.Application/Interfaces/Services/`, service in `Core.Application/UseCases/`, repository interface/implementation if data access is needed.

Required coordinated changes: DTOs, validators, DI registration in `API/Program.cs` or `Infrastructure/DependencyInjection.cs`, Angular service call, `docs/API_ENDPOINTS.md`.

Validate: `dotnet build Recep.sln`, `dotnet test Recep.sln`, `cd app && npm run build`.

Common mistakes: wrong `[Route]` token, missing `[Authorize]`, forgetting Angular interceptor auth behavior, returning a shape not modeled in TypeScript.

Compatibility risks: changing route casing is usually tolerated, but changing route segments, status codes, or response JSON names breaks frontend callers.

## Add A DTO

Files: `Core.Application/DTO/**`.

Coordinate with controllers, services, validators, Angular models.

Validate with backend build and frontend build.

Common mistakes: adding nullable mismatches, enum/string mismatch, missing validation.

## Add A Service Method

Files: `Core.Application/Interfaces/Services/*.cs`, `Core.Application/UseCases/**/*.cs`.

Coordinate with repository interfaces and DI if adding a new service class.

Validate: `dotnet build Recep.sln`.

Common mistakes: bypassing `Result` convention for service failures.

## Add A Repository Query

Files: `Core.Application/Interfaces/Repositories/*.cs`, `Infrastructure/Repositories/*.cs`.

Coordinate with EF includes, DTO mapping, pagination/filter params.

Validate: `dotnet build Recep.sln`.

Common mistakes: missing `Include` for navigation data required by DTO mapping; unbounded queries.

## Add A Database Entity

Files: `Core.Domain/Entities/*.cs`, `Infrastructure/Persistence/AppDbContext.cs`, `Infrastructure/Persistence/Configurations/*.cs`, `Infrastructure/Migrations/`.

Coordinate with repository, DTOs, services, seed data if needed.

Validate: `dotnet build Recep.sln`.

Common mistakes: missing DbSet, incorrect cascade delete, shadow FK from ambiguous navigation.

## Add A Database Field

Files: entity, EF configuration, DTOs/mappers, Angular model if exposed, migration.

Migration command:

```bash
dotnet ef migrations add <Name> --project Infrastructure --startup-project API
```

Common mistakes: changing non-nullable fields without default/backfill; missing frontend form binding.

## Add Or Modify Seed Data

Files: active `Infrastructure/Seed/DbSeeder.cs`; older `Infrastructure/Persistence/DataSeeder.cs` is unused.

Coordinate enum values and required fields.

Validate: `dotnet build Recep.sln`.

Common mistakes: expecting seed to rerun when recipes already exist; not seeding Admin if admin features are needed.

## Add An Angular Model

Files: `app/src/app/models/*.ts` or service-local interface if only used by one service.

Coordinate with backend DTO response/request shape.

Validate: `cd app && npm run build`.

Common mistakes: using string where backend expects enum number or vice versa; omitting required API properties used by templates.

## Add An Angular Service Call

Files: `app/src/app/services/*.ts`.

Coordinate with `docs/API_ENDPOINTS.md`, backend route attributes, auth requirement.

Validate frontend build and a manual API smoke test.

Common mistakes: hardcoding a new base URL instead of centralizing; forgetting cache invalidation after mutations.

## Add A Page Or Component

Files: new folder under `app/src/app/`, route in `app/src/app/app.routes.ts`, optional styles/template/spec.

Coordinate standalone `imports` array.

Validate: `cd app && npm run build`.

Common mistakes: adding NgModule patterns into this standalone app; forgetting `CommonModule` or `FormsModule`.

## Add A Route Or Protected Route

Files: `app/src/app/app.routes.ts`, guard if needed.

Protected route: add `canActivate: [authGuard]`.

Coordinate backend auth and navigation links.

Common mistakes: relying only on frontend guards for security; backend must enforce sensitive actions.

## Add Role-Based Authorization

Backend files: controller attributes, service ownership checks, `AuthController.GenerateJwtToken` if claim changes.

Frontend files: `AuthService.isAdmin()`, components that show/hide controls.

Validate login token payload and protected endpoint behavior.

Common mistakes: changing claim names without updating Angular decoding; not creating/seeding users with matching roles.

## Change JWT Claims

Files: `API/Controller/AuthController.cs`, Angular `AuthService.isAdmin()`, `auth.guard.ts` if expiry handling changes.

Compatibility risk: existing tokens become incompatible; refresh flow may need logout fallback.

## Change API URLs

Files: all services in `app/src/app/services/*.ts`, backend `API/Properties/launchSettings.json`, CORS in `API/Program.cs`.

Recommended improvement: introduce Angular environment config.

Validate browser calls and CORS.

## Modify Recipe Filters, Sorting, Pagination

Backend files: `Core.Application/DTO/RecipeQueryParams.cs`, `Infrastructure/Repositories/RecipeRepository.cs`, `RecipesController`.

Frontend files: `RecipeService`, `RecipesComponent` TS/HTML.

Common mistakes: sending query param names backend does not bind; failing to reset page on filter changes; cache key missing new filter property.

## Modify Frontend Cache Behavior

Files: `app/src/app/services/recipe.service.ts`, callers in `RecipesComponent`.

Coordinate invalidation after create/update/delete or new mutations.

Common mistakes: caching authenticated/user-specific data under a global key; stale list after mutation.

## Add Tests

Backend: add a test project to `Recep.sln`; none exists now.

Frontend: update specs under `app/src/app/**/*.spec.ts`.

Validate:

```bash
dotnet test Recep.sln
cd app && npm test -- --watch=false
```

Common mistakes: stale generated specs, missing providers for standalone components using Router/HttpClient/Toastr.

## Update Documentation

Files: `AI_CONTEXT.md`, `docs/*.md`.

Required: update endpoint table, known issues, and feature context when behavior changes.

Common mistakes: documenting intended behavior instead of code-confirmed behavior; exposing secrets.
