# Known Issues And Rules

Last verified: 2026-07-26
Branch: `main`
Commit: `1de804620330d10f9ee6b493ecac423f6ab288b2`

## Non-Negotiable Rules

| Rule | Reason |
| --- | --- |
| Do not expose secrets; use `[REDACTED]`. | `API/appsettings*.json`, user secrets, and environment variables may contain sensitive values. |
| Preserve `Recipie`, `RecipieDto`, `RecipieStep`, `Recipies`, `Recep`, `Recepes`. | Existing code and migrations depend on these names. |
| Preserve `API/Controller/RecipesController .cs`. | Filename contains a space and is part of the current tree. |
| Do not edit migrations manually. | EF migrations and snapshot must remain coordinated. |
| Do not overwrite dirty source files. | Working tree contains active uncommitted feature work. |
| Backend authorization is authoritative. | Frontend guards/buttons are UI only. |

## Confirmed Bugs / Inconsistencies

| Severity | File | Evidence | Impact | Recommended correction |
| --- | --- | --- | --- | --- |
| High | `Core.Application/DTO/Reviews/ReviewDto.cs` | DTO exposes `UserEmail`; details page displays reviews. | Public email exposure through anonymous review list. | Replace with safe public author DTO/display name. |
| High | `API/Program.cs` | Runs `db.Database.Migrate()` at startup outside Testing. | Starting API can mutate configured DB unexpectedly. | Move migration to explicit deployment step or gated dev-only path. |
| Medium | `API/Controller/CategoriesController.cs` | Uses `AppDbContext` directly and returns entities. | Breaks service/repository pattern and leaks entity shape. | Add category service/repository/DTO if category behavior grows. |
| Medium | `API/Controller/AuthController.cs` | Uses `AppDbContext` directly; errors are mixed strings/ProblemDetails. | Inconsistent architecture and error contract. | Introduce auth service and unified responses. |
| Medium | `Core.Domain/Entities/RecipeImage.cs` | Entity exists without clear DbSet/config; `Recipie.Images` exists. | Possible shadow FK/migration confusion. | Add explicit mapping or remove in coordinated migration. |
| Low | `app/src/app/app.module.ts` | Standalone bootstrap uses `main.ts`; module remains. | Maintainer confusion. | Remove only in a coordinated cleanup if verified unused. |

## Security Risks

| Severity | Area | Evidence | Risk | Recommendation |
| --- | --- | --- | --- | --- |
| Critical | Secrets | `API/appsettings.json` and development settings are modified; values must not be printed. | Secret leakage through docs/logs. | Use `[REDACTED]`; keep real values in user secrets/env. |
| High | Review privacy | `ReviewDto.UserEmail` | Public endpoint can expose emails. | Use display name only. |
| Medium | JWT validation | `Program.cs` disables issuer/audience validation. | Tokens are validated only by signing key/lifetime/user role. | Configure issuer/audience before production. |
| Medium | CORS | `Program.cs` allows `http://localhost:4200` only. | Correct for local dev, not deploy-ready. | Move allowed origins to config. |
| Medium | Token storage | `AuthService` stores tokens in `localStorage`. | XSS can read tokens. | Consider httpOnly cookies or stronger XSS controls. |
| Medium | Account errors | Auth endpoints return strings/ProblemDetails. | Inconsistent client behavior; potential user enumeration through messages. | Standardize auth errors. |

No confirmed raw SQL injection risk was found; repositories use EF LINQ.

## Backend/Frontend Mismatches

| Area | Evidence | Impact |
| --- | --- | --- |
| Reviews | Backend returns `UserEmail`; frontend can display it. | Privacy mismatch with safe author policy. |
| Categories | Backend entity has `Description`; frontend model only has `id/name`. | Benign unless description UI is added. |
| Error contracts | Recipe/culture use `ApiErrorResponse`; auth/review/favorite use mixed responses. | Frontend error parsing must handle many shapes. |
| Culture Admin | Backend Admin endpoints exist; Angular has services but no routed Admin UI. | Management requires API/Swagger/manual calls. |
| Service URL tests | Service specs assert concrete localhost URL. | Centralized URL changes require spec updates. |

## Database Risks

| Severity | Evidence | Risk | Recommendation |
| --- | --- | --- | --- |
| High | Startup migration in `Program.cs` | Accidental DB mutation. | Remove automatic migration from normal runtime. |
| Medium | `FavoriteRecipeConfiguration` and `RecipeReviewConfiguration` cascade from users/recipes | Deleting users/recipes can remove related favorites/reviews. | Confirm product deletion policy. |
| Medium | `RecipeImage` incomplete mapping | Shadow FK or unused table risk. | Resolve mapping in a schema cleanup phase. |
| Medium | Search uses `Contains` | Expensive scans as recipe count grows. | Add search strategy/indexes later. |

## Performance Risks

| Area | Evidence | Risk |
| --- | --- | --- |
| Recipe list | `RecipeRepository.GetPagedAsync` includes related entities then materializes. | More data than DTO projection needs. |
| Reviews | `ReviewRepository.GetByRecipeIdAsync` is unpaged. | Large review lists can grow unbounded. |
| User search | `UserRepository.GetPagedAsync` uses `Email.Contains`. | Potential scan. |
| Seeder | `DbSeeder` can seed many recipes when none exist. | Startup can be slow. |

## Maintainability Risks

- Mixed direct DbContext and service/repository patterns.
- Plain-string errors in older endpoints.
- Some formatting/namespaces are inconsistent.
- Large dirty working tree with application, migration, tests, docs, and generated files.
- Untracked archives `back.zip` and `front.zip` are present.

## Areas Requiring Verification

- Whether the uncommitted migrations have been applied to any local database.
- Whether current appsettings values are valid in the developer environment.
- Whether any external PerformancePlatform integration exists outside currently inspected identifiers.
- Whether browser tests pass in an environment with Chrome configured.

## Frontend Redesign Rules And Remaining Issues

Last verified: 2026-07-26 after frontend redesign.

Rules:

- Do not replace the standalone Angular architecture with NgModules.
- Keep `API_BASE_URL` centralized in `app/src/app/app-api.config.ts`; do not hardcode new API origins in components.
- Use `app/src/app/core/utils/asset-url.util.ts` for API-relative recipe image paths.
- Frontend owner/Admin visibility is presentation only; backend authorization remains authoritative.
- Keep email addresses out of prominent UI. `RecipeDetailsComponent` currently masks review emails because the backend still returns `Review.UserEmail`.
- Preserve existing routes: `/`, `/recipes`, `/login`, `/register`, `/recipes/:id`, `/create-recipe`, `/my-recipes`, `/admin/accounts`.

Confirmed remaining frontend issues:

| Severity | Area | Evidence | Recommendation |
| --- | --- | --- | --- |
| Medium | Test environment | `npm test -- --watch=false` cannot launch Chrome; `CHROME_BIN` unset. | Install/configure Chrome or use a supported headless browser in CI. |
| Medium | Build budgets | `npm run build` passes but warns on initial bundle and component CSS sizes. | Tune budgets or consolidate/minify component CSS further after product sign-off. |
| Medium | Review privacy | Backend review DTO returns email; frontend masks it in details page. | Replace backend review author shape with safe display-name DTO. |
| Low | Bootstrap selector warning | Build warning for `.form-floating>~label` from Bootstrap CSS parsing. | Usually harmless; can be revisited if Bootstrap is removed or upgraded. |
| Low | Auth display name in navbar | JWT may not contain a display-name claim; navbar falls back to generic text. | Add a stable display-name claim or `/me` profile endpoint later if needed. |
