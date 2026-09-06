# Production Runbook

## Architecture

`recepie-web` serves the Angular build through Nginx. Nginx proxies `/api/`, `/images/`, and the minimal health endpoints to `recepie-api`. The API uses SQL Server and writes recipe images to `/app/wwwroot/images/recipes`.

Compose exposes only the web service on host port `8080`. SQL Server and the API remain on the internal `recepie-network`. Production TLS must terminate at Nginx, a hosting reverse proxy, or a load balancer; do not commit certificates. Internal Compose traffic is HTTP.

## Required software

- Docker Engine 29+ with the Compose plugin.
- Node.js 24 for frontend builds.
- A secure secret-injection mechanism for production values.
- The .NET 10 SDK and matching `dotnet-ef` only on the controlled host/CI environment that applies migrations.

## Required environment variables

Copy `.env.example` to an untracked `.env` and replace placeholders. Required names are `MSSQL_SA_PASSWORD`, `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, and `CORS_ALLOWED_ORIGIN`; `DB_NAME` defaults to `Recepie` when omitted. Never commit `.env`.

`ConnectionStrings__DefaultConnection`, `Database__AutoMigrate`, and the CORS/JWT settings are supplied to the API by Compose. `Database__AutoMigrate` is explicitly `false`: the API never performs production schema migration at startup.

## Initial deployment

1. Choose and configure external TLS termination, with the public host allowed by `CORS_ALLOWED_ORIGIN` if external browser origins require CORS.
2. Create the production `.env` securely.
3. Build the immutable application images: `docker compose build`.
4. Apply migrations using the procedure below.
5. Start services: `docker compose up -d`.
6. Run `sh scripts/production-smoke.sh https://your-host` and inspect `docker compose ps`.

## Database migration

Back up the database first. From a checkout of the exact release source, with the production `ConnectionStrings__DefaultConnection` supplied in the environment and .NET EF tooling installed, inspect then apply migrations:

```sh
dotnet ef migrations list --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
```

Do not apply migrations automatically during API startup. Deploy/start the API after a successful explicit migration, verify readiness, then start/verify the frontend.

## Database backup

Use a SQL Server backup location that is persistent and copied off the Docker host. For example, mount a host-controlled backup directory into a one-off SQL Server administration session, then execute (substitute the configured database name):

```sql
BACKUP DATABASE [Recepie]
TO DISK = N'/var/opt/mssql/backup/recepie.bak'
WITH FORMAT, INIT, COMPRESSION;
```

Verify the resulting backup, encrypt it at rest, and copy it to independent durable storage. A SQL backup alone is not a complete Recepie backup.

## Database restore

1. Stop application writes (`docker compose stop recepie-web recepie-api`).
2. Identify and verify the intended backup before replacing data.
3. Restore with the normal SQL Server sequence: `RESTORE FILELISTONLY`, then `RESTORE DATABASE ... WITH MOVE ... , REPLACE` only after confirming logical file names and target paths.
4. Validate the restored database with SQL Server checks.
5. Start the API, verify `/health/ready`, then start the frontend and smoke test.

Test the exact restore command and paths in a non-production environment first; logical file names vary by backup.

## Recipe image backup

Recipe images are stored in named volume `recepie-images` at `/app/wwwroot/images/recipes` in `recepie-api`. Back up that volume separately (for example with a temporary container that archives `/data` to an externally mounted, encrypted backup location). Restore the image archive alongside the matching SQL backup.

Complete Recepie backup = SQL Server backup + `recepie-images` volume backup.

## Docker startup and shutdown

```sh
docker compose build
docker compose up -d
docker compose ps
docker compose down
```

Do not use `docker compose down -v` in normal operation: it deletes the SQL and recipe-image named volumes.

## Health checks and logs

- `/health/live` confirms the API process is running and does not query SQL Server.
- `/health/ready` checks API readiness including database connectivity.

Both responses are deliberately minimal. Inspect service state with `docker compose ps` and logs with `docker compose logs --tail=200 recepie-api`. API logs include method, path, response status, elapsed time, and `X-Correlation-ID`. Supply a valid, short correlation ID to follow a request; malformed values are replaced server-side.

## Deployment verification

After startup, check the web response, both health endpoints, and an existing safe anonymous API endpoint such as `/api/categories`. Then use test accounts/data to verify the existing authenticated workflows appropriate to the release: login, registration/approval, recipe lifecycle and image upload, social actions, profiles/feed/notifications, and admin moderation. Recreate only the API container to confirm a test image remains; recreate only SQL Server to confirm test data remains. Never remove volumes during these checks.

## Rollback

Keep a tagged, known-good image/version and deploy it if the new application is unhealthy. Database migrations must be assessed for backward compatibility before application rollback. Do not blindly migrate down after destructive schema/data changes; restore the validated pre-deployment SQL backup and matching image-volume backup when database rollback is required.

## Known limitations

Deployment target is not yet selected. Compose provides a reproducible Docker-host deployment topology, but TLS certificate management, external secret storage, backup retention, off-host backup copying, monitoring/alerting, and release image tagging must be chosen for the eventual hosting environment.
