# Known Issues And Rules

## Confirmed Bugs

No confirmed application bugs remain from the fixed difficulty/auth/test issues. Remaining risks are listed below.

## Security Risks

| Severity | File path | Evidence | Impact | Recommended correction | Likely affected files |
|---|---|---|---|---|---|
| Medium | `API/appsettings.json`, `API/appsettings.Development.json` | Connection string and JWT key are empty placeholders. | API startup fails until local secrets are configured. | Set `ConnectionStrings__DefaultConnection` and `Jwt__Key` through user secrets or environment variables. | local/deployment config |
| Medium | `API/Program.cs` | JWT issuer/audience validation disabled. | Tokens signed with key are accepted regardless of issuer/audience. | Configure and validate issuer/audience for non-local environments. | `API/Program.cs`, appsettings |

## Backend/Frontend Mismatches

| Severity | File path | Evidence | Impact | Recommended correction | Likely affected files |
|---|---|---|---|---|---|
| Low | `app/src/app/models/category.model.ts`, `Core.Domain/Entities/Category.cs` | Angular model omits `description`/`createdAt`. | Safe now, but incomplete if UI later needs fields. | Extend model when consuming fields. | category model/service/components |

## Database Risks

| Severity | File path | Evidence | Impact | Recommended correction | Likely affected files |
|---|---|---|---|---|---|
| High | `Core.Domain/Entities/RecipeImage.cs`, `Infrastructure/Migrations/AppDbContextModelSnapshot.cs` | Snapshot uses nullable shadow `RecipieId`; entity has `RecipeId`; no DbSet. | Images relationship can persist incorrectly and is hard to query. | Explicitly configure `RecipeImage.RecipeId` FK/table and add migration if feature is needed. | entity, DbContext, configuration, migrations |
| Medium | `Infrastructure/Persistence/Configurations/IngredientConfiguration.cs` | File defines `RecipieConfiguration : IEntityTypeConfiguration<Recipie>`, not ingredient config. | Duplicate recipe config and misleading file/class name. | Rename/fix when doing DB cleanup; avoid casual runtime changes. | configuration files, migrations |
| Medium | `API/Program.cs` | `db.Database.Migrate()` runs at startup. | App startup can mutate whichever DB connection string points to. | Keep for local only or gate by environment/explicit flag. | `Program.cs`, deployment config |
| Low | migration designer files | Early designers show EF ProductVersion `9.0.0`, current packages/snapshot are EF `8.0.0`. | Tooling confusion if regenerating migrations. | Use consistent SDK/packages for future migrations. | csproj, migrations |

## Build Or Configuration Risks

| Severity | File path | Evidence | Impact | Recommended correction | Likely affected files |
|---|---|---|---|---|---|
| Medium | `app/package-lock.json` | `npm audit` reports 52 remaining vulnerabilities after safe `npm audit fix`; remaining fixes require `npm audit fix --force` and Angular/build-tool major upgrades. | Development/build tooling and Angular dependencies remain on vulnerable ranges. | Plan a controlled Angular/tooling upgrade instead of force-upgrading blindly. | `app/package.json`, `app/package-lock.json`, Angular source/tests |
| Low | `app/src/styles.css`, `app/angular.json` | Bootstrap CSS is imported locally and initial bundle is about 705 kB; budget warning is now 750 kB. Build also reports one Bootstrap selector optimizer warning. | Build passes, but Bootstrap contributes most CSS size. | Trim Bootstrap imports if bundle size becomes a product concern. | `app/angular.json`, `app/src/styles.css` |
| Low | `app/src/app/app.module.ts` | Incomplete `@NgModule` snippet; not imported by `main.ts`. | Confusing unused file. | Remove or complete only during cleanup. | `app.module.ts` |

## Naming Inconsistencies

- Preserve `Recipie`, `RecipieDto`, `RecipieStep`, `Recipies`.
- Exact odd path: `API/Controller/RecipesController .cs`.
- `app/interceptors/auth.intercepro.ts` has misspelled filename and duplicates auth interceptor.
- Frontend brand text uses `Recepes V2`.

## Potentially Unused Files

- `Core.Domain/Class1.cs`
- `Core.Application/Class1.cs`
- `Infrastructure/Class1.cs`
- `Infrastructure/Persistence/DataSeeder.cs`
- `app/src/app/app.module.ts`
- `app/src/app/app.component.html`
- `app/interceptors/auth.intercepro.ts`

## Missing Tests

- Backend application tests exist in `tests/Core.Application.Tests`; API recipe authorization integration tests exist in `tests/API.IntegrationTests`.
- Angular specs cover core standalone behavior and compile, but runtime execution in this environment is blocked without a Chrome/Chromium binary. CI workflow runs Chrome Headless.

## Performance Risks

- `GET /api/Recipes` delegates to capped paged behavior with page size 100.
- Recipe search uses `Title.Contains`, with no confirmed index.
- Seed creates 1,000 recipes at startup when database is empty.

## Maintainability Risks

- Mixed direct DbContext controller usage and service/repository pattern.
- Hardcoded API URLs in Angular services.
- Plain string error responses vary by endpoint.
- Feature code and docs have spelling variants: Recipe/Recipie/Recep/Recepes.

## Areas Requiring Verification

- Whether the current database already contains an Admin user.
- Whether the project should stay on Angular 18 despite unresolved npm audit findings requiring major upgrades.
- Whether startup migration is intended outside local development.

## Files That Must Not Be Changed Casually

- `Infrastructure/Migrations/**`
- `Infrastructure/Persistence/AppDbContext.cs`
- `Infrastructure/Persistence/Configurations/**`
- `API/Program.cs`
- `API/appsettings*.json`
- `Core.Domain/Entities/**`
- `app/src/app/services/auth.service.ts`
- `app/src/app/interceptors/error.interceptor.ts`
- `app/src/app/guards/auth.guard.ts`

## Route, Angular, Styling, And Secret Rules

- API route convention is `[Route("api/[controller]")]`; document actual controller token routes.
- Sensitive backend actions must use backend `[Authorize]`/role checks, not only Angular UI checks.
- Angular uses standalone bootstrap; add standalone imports locally.
- Keep API URLs coordinated with backend launch settings and CORS.
- Do not expose connection strings, JWT keys, API keys, tokens, or password hashes. In docs use `[REDACTED]`.
- Store local secrets in user secrets or environment variables; store deployment secrets in platform secret management.
