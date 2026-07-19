# Project Overview

## What The Application Does

Recipe App V2 / Recep V2 is a full-stack recipe app. It lets authenticated users browse paged recipes, search and filter the list, view recipe details, mark favorites, and create reviews. Operator users can manage recipe content. Admin users can manage recipe content and user accounts.

## Users And Roles

- Anonymous users: can access Angular `/login` and `/register`; backend also allows `GET /api/Categories` and `GET /api/Reviews/recipe/{recipeId}`.
- Authenticated `User`: can browse recipes, view recipe details, favorite/unfavorite recipes, view own favorites, and create/update/delete own reviews.
- `Operator`: same as user, plus backend authorization and UI visibility for recipe create/update/delete.
- `Admin`: same as operator, plus account-management access at `/admin/accounts` and `/api/admin/users`.

Confirmed role storage: string property `Users.Role` in `Core.Domain/Entities/Users.cs`. Supported role constants are centralized in `Core.Domain/Constants/AppRoles.cs`. New registrations default to `User` in `API/Controller/AuthController.cs`.

## Main Pages And Workflows

- `/login`: login form in `app/src/app/login/`.
- `/register`: registration form in `app/src/app/pages/register/`.
- `/recipes`: guarded recipe listing in `app/src/app/pages/recipes/`; supports search, category, difficulty, sort, pagination, favorites, and Operator/Admin create/edit/delete controls.
- `/recipes/:id`: guarded recipe detail page in `app/src/app/recipe-details/`; shows image, metadata, ingredients, and reviews; allows submitting a review.
- `/admin/accounts`: guarded account-management page in `app/src/app/pages/admin/accounts/`; Admin only.
- Navbar: `app/src/app/shared/navbar/`; shows login/register or recipes/logout based on localStorage token presence and shows Accounts only for Admin.

## Layer Responsibilities

Backend:

- HTTP routing, auth, CORS, Swagger, startup migration/seed: `API/Program.cs`.
- Controllers: `API/Controller/`.
- Use-case logic: `Core.Application/UseCases/`.
- Data access contracts: `Core.Application/Interfaces/Repositories/`.
- Persistence: `Infrastructure/Persistence/`, `Infrastructure/Repositories/`.

Frontend:

- Standalone Angular app bootstrap: `app/src/main.ts`.
- Route definitions and guards: `app/src/app/app.routes.ts`, `app/src/app/guards/auth.guard.ts`.
- API services and interceptors: `app/src/app/services/`, `app/src/app/interceptors/`.
- UI components/pages: `app/src/app/login/`, `app/src/app/pages/`, `app/src/app/recipe-details/`, `app/src/app/shared/navbar/`.

Database:

- SQL Server via EF Core.
- Migration files under `Infrastructure/Migrations/`.
- Runtime migration and seed in `API/Program.cs`.
- Active seed process in `Infrastructure/Seed/DbSeeder.cs`.

## External Systems And Integrations

- SQL Server database via connection string named `DefaultConnection`.
- PerformancePlatform files and configuration were not present in the inspected tree during Phase 1; no Phase 1 changes added or removed that integration.
- External image URLs in seed data use `https://picsum.photos/400/300?random={i}`.

## Realtime Functionality

No SignalR hubs, websocket code, realtime notifications, or live update mechanisms were found.

## Boundaries And Missing Features

- No Docker or docker-compose files found.
- Backend tests exist under `tests/Core.Application.Tests` and `tests/API.IntegrationTests`.
- Angular unit specs compile with `tsconfig.spec.json`; Karma execution requires Chrome/Chromium, which was not available in this environment.
- No upload handling; recipes store image URLs only.
- Optional Admin bootstrap exists through `SeedAdmin:Email` and `SeedAdmin:Password`; committed config contains empty placeholders only.
- No standardized error response shape.
- Recipe creation DTO does not include ingredients or steps.
