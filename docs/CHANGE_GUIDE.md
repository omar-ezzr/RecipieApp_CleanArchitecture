# Change Guide

Last verified: 2026-07-26
Branch: `main`
Commit: `1de804620330d10f9ee6b493ecac423f6ab288b2`

Use this guide for coordinated changes. Always inspect current code and `git status --short` before editing.

## Backend Changes

| Change | Files normally changed | Coordinated checks | Common mistakes |
| --- | --- | --- | --- |
| Add backend endpoint | Controller under `API/Controller/`, service interface/implementation under `Core.Application/Interfaces/Services/` and `Core.Application/UseCases/`, repository if data access is needed, DTOs if request/response changes | Swagger route, auth attributes, DI registration, frontend service caller, tests | Returning EF entities, inconsistent status codes, missing cancellation tokens. |
| Add DTO | `Core.Application/DTO/...` | Controller action signatures, service mapping, Angular model | Exposing `PasswordHash`, `RefreshToken`, email where public display data is required. |
| Add validator | `Core.Application/Validators/`, `API/Program.cs` validator scanning | Model-state response shape, service relationship validation | Performing DB queries in sync validators without an established pattern. |
| Add service method | Service interface and implementation | Controller action, tests, repository method | Duplicating authorization only in controller. |
| Add repository query | Repository interface and implementation | EF indexes, `AsNoTracking`, filtering before pagination | Loading all rows before filtering, N+1 queries, missing page-size cap. |
| Add entity | `Core.Domain/Entities/`, `AppDbContext`, EF configuration, migration, seeder if needed | Delete behavior, indexes, DTOs, services, tests | Editing snapshot manually or relying on cascade delete by accident. |
| Add database field | Entity, configuration, DTOs, validators, service mapping, migration | Backfill existing rows if required | Adding non-null field without safe migration strategy. |
| Add relationship | Both entities if navigation needed, EF configuration, migration | Delete behavior, FK/indexes, repository includes/projections | Cascade delete causing data loss; shadow FKs. |
| Add seed data | `Infrastructure/Seed/DbSeeder.cs` | Startup call, idempotency, existing DB behavior | Global early return that prevents independent seed sections. |
| Change JWT claims | `AuthController.GenerateJwtToken`, `Program.cs` token validation, `AuthService` helpers, tests | Existing tokens/backward compatibility | Trusting frontend user IDs or parsing only one claim URI. |
| Change authorization | Controller attributes and service ownership/admin logic | Integration tests for 401/403/404 | Using Admin-only attributes when owner access is required. |
| Add recipe filters | `RecipeQueryParams`, `RecipeRepository.GetPagedAsync`, `RecipeService`, Angular `RecipeQuery`, recipes page | URL query params, cache key, indexes | Filtering after materialization or forgetting cache invalidation. |

## Frontend Changes

| Change | Files normally changed | Coordinated checks | Common mistakes |
| --- | --- | --- | --- |
| Add Angular model | `app/src/app/models/*.ts` | Backend DTO shape and JSON casing | Duplicating interfaces inside services when shared model is better. |
| Add service call | `app/src/app/services/*.ts` | `API_BASE_URL`, interceptors, tests, caller component | Hardcoding new base URLs. |
| Add page/component | `app/src/app/pages/...` or feature folder | Standalone imports, route entry, guards, specs | Adding NgModule patterns to standalone app. |
| Add protected route | `app/src/app/app.routes.ts`, guards | Backend endpoint authorization | Relying on frontend guard as security. |
| Add role-based UI | Component TS/HTML and `AuthService` role helper | Backend role enforcement | Hiding button without server-side rule. |
| Change API URLs | `app/src/app/app-api.config.ts` and affected tests | CORS/backend launch settings | Updating services but not specs. |
| Change pagination | Angular query model/component/service and backend query params | Cache key and URL query params | Reusing global cache for user-specific data. |
| Change cache behavior | `RecipeService` | Create/update/delete invalidation, user-specific queries | Caching `/me` data under shared key. |
| Change forms | Component TS/HTML/specs, backend DTO/validator | Required fields and error handling | Frontend-only validation without backend validation. |

## Frontend Redesign Guidance

Current design system entry points:

- Global tokens and app-wide primitives: `app/src/styles.css`
- Shell/footer: `app/src/app/app.component.html`, `app/src/app/app.component.css`
- Navbar: `app/src/app/shared/navbar/`
- Shared recipe card: `app/src/app/shared/components/recipe-card/`
- Shared loading/empty primitives: `app/src/app/shared/components/recipe-card-skeleton/`, `empty-state/`, `loading-spinner/`

When changing frontend visuals:

| Change | Files to inspect first | Required checks |
| --- | --- | --- |
| Change colors/fonts/buttons/forms | `app/src/styles.css` | Verify all redesigned pages still build and remain readable at 320px. |
| Change recipe card layout | `shared/components/recipe-card/*`, `pages/recipes/*`, `pages/my-recipes/*` | Preserve favorite, view, edit, delete outputs and accessible links/buttons. |
| Change Explore filters | `pages/recipes/*`, `RecipeService`, `RecipeQuery` model | Preserve URL query params, cache key behavior, pagination, cuisine-region reset. |
| Change recipe detail layout | `recipe-details/*`, `ReviewService`, `FavoriteService` | Preserve favorites, reviews, cuisine/region links, owner/Admin actions, ingredients, steps. |
| Change create recipe form | `pages/create-recipe/*`, `recipe.model.ts` | Preserve submit payload shape, dynamic ingredient/step behavior, cuisine/region relationship. |
| Change auth pages | `login/*`, `pages/register/*`, `AuthService` | Preserve token saving, autocomplete attributes, display-name registration. |
| Change Admin accounts UI | `pages/admin/accounts/*`, `UserManagementService` | Preserve Admin guard, role/status/delete/create actions, self-protection behavior. |

Common frontend redesign mistakes:

- Adding new hardcoded API origins instead of using `app/src/app/app-api.config.ts`.
- Resolving `/images/...` paths directly in templates instead of using `resolveAssetUrl`.
- Removing route guards because controls are visually hidden.
- Making owner/Admin controls visible without preserving backend authorization.
- Adding component CSS above the 4 kB `anyComponentStyle` error budget.
- Displaying raw review emails more prominently; mask them until backend returns safe review author data.

## Documentation Changes

| Change | Files |
| --- | --- |
| Update high-level context | `AI_CONTEXT.md`, `docs/PROJECT_OVERVIEW.md` |
| Update backend architecture | `docs/BACKEND_CONTEXT.md`, `docs/API_ENDPOINTS.md` |
| Update frontend architecture | `docs/FRONTEND_CONTEXT.md` |
| Update schema/migrations/seeding | `docs/DATABASE_CONTEXT.md` |
| Update feature status | `docs/FEATURES_CONTEXT.md` |
| Update risks/rules | `docs/KNOWN_ISSUES_AND_RULES.md` |
| Update recent repo state | `docs/RECENT_CHANGES_CONTEXT.md` |
| Update workflow guidance | `docs/CHANGE_GUIDE.md` |

Documentation must not include secrets. Use `[REDACTED]`.

## Validation Commands

Backend:

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

## Compatibility Risks

- Recipe-related changes must preserve `Recipie`, `RecipieDto`, `RecipieStep`, and `Recipies`.
- The controller path `API/Controller/RecipesController .cs` has a space before `.cs`.
- Existing migrations should not be edited.
- Startup migration behavior in `API/Program.cs` means running the API can change the configured database.
- `API/appsettings*.json` values are sensitive.
- The active dirty tree includes user work; avoid broad formatting or cleanup.
