# Recepie

RecepieV3 is a full-stack social recipe platform built with ASP.NET Core, EF Core, SQL Server, and Angular 18 standalone components.

The product is a public cooking network, not an Admin-only recipe catalog. Authenticated users publish their own recipes, manage the recipes they created, browse cultural cooking styles, follow cooks, react to recipes with Likes, save recipes privately, comment, and write reviews. Admins moderate the platform and can manage any recipe or user account when authorized.

## Current Capabilities

- JWT authentication with access and refresh tokens.
- User registration with a public display name.
- Owner-based recipe publishing and management.
- Admin moderation for recipes and account management.
- Recipe listing, search, filtering, sorting, and pagination.
- Cuisine, region, and traditional-recipe discovery.
- Public user profiles with follower/following counts and paged recipe grids.
- Follow and unfollow social relationships.
- Personalized `/feed` containing recipes from followed cooks.
- Public recipe Likes with count/current-user state on Feed, Explore, and recipe details.
- Private favorites/saved recipes kept separate from Likes.
- Recipe comments kept separate from reviews/ratings.
- Database-backed notifications for follows, Likes, and comments.
- Recipe details with ingredients, ordered steps, cultural origin metadata, Likes, favorites, comments, and reviews.
- `GET /api/Recipes/me` and Angular `/my-recipes` for the current user's recipes.
- Angular `/create-recipe` publishing form with ingredients, steps, cuisine, region, difficulty, and cultural fields.
- Safe recipe/review/comment author display data through public author DTOs.
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

- `/feed` - authenticated following feed with Like and Save actions.
- `/recipes` - recipe-first discovery with cuisine/region/category/difficulty/traditional filters.
- `/recipes/:id` - editorial recipe detail page.
- `/create-recipe` - authenticated recipe publishing form.
- `/users/:id` - public user profile.
- `/profile/edit` - authenticated current-user profile editor.
- `/saved` - authenticated private saved/favorite recipes.
- `/notifications` - authenticated social notifications.
- `/my-recipes` - current user's recipe library.
- `/login` and `/register` - product-styled auth screens.
- `/admin/accounts` - Admin account management.

## Backend

The API project lives in `API/`.

Core backend behavior:

- JWT bearer authentication is configured in `API/Program.cs`.
- Token generation happens in `API/Controller/AuthController.cs`.
- Recipe ownership is enforced in `Core.Application/UseCases/Recipes/RecipeService.cs`.
- Social features are implemented through focused controllers/services/repositories for profiles, follows, Likes, comments, feed, and notifications.
- Like endpoints are:
  - `POST /api/recipes/{recipeId}/likes`
  - `DELETE /api/recipes/{recipeId}/likes`
  - `GET /api/recipes/{recipeId}/likes`
  - `GET /api/recipes/{recipeId}/likes/status`
- Recipe list/detail DTOs include `likeCount` and `isLikedByCurrentUser`, populated server-side from the authenticated JWT identity.
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
- Angular dev server: `http://localhost:4203`
- Swagger is available in Development when the API is running.

## Validation Status

Last verified in this working tree on 2026-08-09:

- `dotnet restore`: passed.
- `dotnet build Recep.sln`: passed.
- `dotnet test Recep.sln`: passed, 60 backend tests.
- `cd app && npm run build`: passed with budget warnings.
- `cd app && npm test -- --watch=false`: Karma bundle compilation succeeded, but browser execution could not start because Chrome/`CHROME_BIN` is unavailable.

## Known Limitations

- Angular browser tests require a Chrome or Chromium binary.
- Frontend build currently reports bundle/component CSS budget warnings.
- Cuisine/region Admin APIs exist, but no dedicated Angular Admin culture-management page is currently implemented.
- Startup migration behavior in `API/Program.cs` can mutate the configured database outside Testing.



## Author

Omar Ezzr
