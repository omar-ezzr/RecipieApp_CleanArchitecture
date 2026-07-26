# RecepieV3

RecepieV3 is a full-stack social recipe platform built with ASP.NET Core, EF Core, SQL Server, and Angular 18 standalone components.

The product is a public cooking network, not an Admin-only recipe catalog. Authenticated users publish their own recipes, manage the recipes they created, browse cultural cooking styles, favorite recipes, and write reviews. Admins moderate the platform and can manage any recipe or user account when authorized.

## Current Capabilities

- JWT authentication with access and refresh tokens.
- User registration with a public display name.
- Owner-based recipe publishing and management.
- Admin moderation for recipes and account management.
- Recipe listing, search, filtering, sorting, and pagination.
- Cuisine, region, and traditional-recipe discovery.
- Recipe details with ingredients, ordered steps, cultural origin metadata, favorites, and reviews.
- `GET /api/Recipes/me` and Angular `/my-recipes` for the current user's recipes.
- Angular `/create-recipe` publishing form with ingredients, steps, cuisine, region, difficulty, and cultural fields.
- Safe recipe author display data through `author.id` and `author.displayName`.
- Local recipe image support through API static files and frontend asset URL resolution.
- Backend unit and integration test projects.

## Architecture

The backend is layered, with some legacy direct `DbContext` usage still present.

```text
API                ASP.NET Core controllers, JWT, Swagger, startup
Core.Domain        Entities, enums, constants
Core.Application   DTOs, interfaces, services/use cases, validators
Infrastructure     EF Core DbContext, configurations, repositories, migrations, seeders
tests/             xUnit unit and integration tests
app/               Angular 18 standalone frontend
```

Main request flow:

```text
Angular -> API Controller -> Application Service -> Repository -> EF Core -> SQL Server
```

Important naming is intentionally preserved across the repository:

- `Recipie`
- `RecipieDto`
- `RecipieStep`
- `Recipies`
- `API/Controller/RecipesController .cs`

## Frontend

The Angular app lives in `app/`.

The frontend uses:

- Angular 18 standalone components.
- Angular Router guards and HTTP interceptors.
- ngx-toastr.
- Bootstrap as a dependency, with a custom editorial design system layered on top.
- Centralized API base URL in `app/src/app/app-api.config.ts`.
- Centralized API-relative image URL handling in `app/src/app/core/utils/asset-url.util.ts`.

Current redesigned pages:

- `/recipes` - recipe-first discovery with cuisine/region/category/difficulty/traditional filters.
- `/recipes/:id` - editorial recipe detail page.
- `/create-recipe` - authenticated recipe publishing form.
- `/my-recipes` - current user's recipe library.
- `/login` and `/register` - product-styled auth screens.
- `/admin/accounts` - Admin account management.

## Backend

The API project lives in `API/`.

Core backend behavior:

- JWT bearer authentication is configured in `API/Program.cs`.
- Token generation happens in `API/Controller/AuthController.cs`.
- Recipe ownership is enforced in `Core.Application/UseCases/Recipes/RecipeService.cs`.
- EF Core context is `Infrastructure/Persistence/AppDbContext.cs`.
- Active seeding is under `Infrastructure/Seed/`.

Do not put secrets in committed configuration. Use user secrets or environment variables for connection strings, JWT keys, and seed credentials.

## Getting Started

From the repository root:

```bash
dotnet restore
dotnet build Recep.sln
dotnet test Recep.sln
dotnet run --project API
```

Frontend:

```bash
cd app
npm install
npm start
```

Default local URLs:

- API HTTP launch profile: `http://localhost:5130`
- Angular dev server: `http://localhost:4200`
- Swagger is available in Development when the API is running.

## Validation Status

Last verified in this working tree on 2026-07-26:

- `dotnet restore`: passed.
- `dotnet build Recep.sln`: passed.
- `dotnet test Recep.sln`: passed, 57 backend tests.
- `cd app && npm install`: passed; npm reported existing vulnerabilities.
- `cd app && npm run build`: passed with budget warnings.
- `cd app && npm test -- --watch=false`: browser bundle compiled, but Karma could not launch because Chrome was unavailable and `CHROME_BIN` was unset.

## Known Limitations

- Angular browser tests require a Chrome or Chromium binary.
- Frontend build currently reports bundle/component CSS budget warnings.
- Review responses still expose reviewer email from the backend; the redesigned frontend masks it, but the backend DTO should be corrected later.
- Cuisine/region Admin APIs exist, but no dedicated Angular Admin culture-management page is currently implemented.
- Startup migration behavior in `API/Program.cs` can mutate the configured database outside Testing.

## Documentation

Detailed project context is maintained in:

- `AI_CONTEXT.md`
- `docs/PROJECT_OVERVIEW.md`
- `docs/BACKEND_CONTEXT.md`
- `docs/FRONTEND_CONTEXT.md`
- `docs/DATABASE_CONTEXT.md`
- `docs/API_ENDPOINTS.md`
- `docs/FEATURES_CONTEXT.md`
- `docs/KNOWN_ISSUES_AND_RULES.md`
- `docs/RECENT_CHANGES_CONTEXT.md`
- `docs/CHANGE_GUIDE.md`

Documentation must describe implemented code, not intended behavior. Never include secrets; use `[REDACTED]` for sensitive values.

## Author

Omar Ezzr
