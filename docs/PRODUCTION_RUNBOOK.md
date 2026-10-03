# Production Runbook

## Architecture

Recepie deploys as three persistent pieces:

- Angular production files served by a normal static host, web server, reverse proxy, or platform static-file feature.
- ASP.NET Core API published from `API/API.csproj` and run as a normal process or service.
- External, persistent SQL Server managed outside the application process.

Production traffic uses this same-origin routing contract:

```text
https://domain/              -> Angular static frontend
https://domain/api/*         -> ASP.NET Core API
https://domain/images/*      -> ASP.NET Core static media files
https://domain/health/live   -> ASP.NET Core liveness endpoint
https://domain/health/ready  -> ASP.NET Core readiness endpoint including database health
```

TLS should terminate at the chosen host, reverse proxy, load balancer, or platform router. The API should receive trusted forwarded headers only from explicitly configured trusted proxies.

## Required software

- .NET 10 SDK on build and migration hosts.
- Matching `dotnet-ef` tooling on the controlled host or CI environment that applies migrations.
- Node.js 24 for Angular builds.
- A persistent SQL Server instance.
- A persistent filesystem location for uploaded recipe media.
- A secret/configuration mechanism such as operating-system environment variables, systemd environment files, or a hosting secret manager.

## Required production configuration

Production does not auto-load `.env`. Set configuration through the operating system or hosting provider. Use ASP.NET Core double-underscore environment variable names:

```env
ConnectionStrings__DefaultConnection=CHANGE_ME

Jwt__Key=CHANGE_ME_TO_A_SECURE_RANDOM_VALUE_AT_LEAST_32_BYTES
Jwt__Issuer=Recepie.Api
Jwt__Audience=Recepie.Web
Jwt__AccessTokenMinutes=5
Jwt__RefreshTokenDays=7

Cors__AllowedOrigins__0=https://recepie.example.com
AllowedHosts=recepie.example.com

Database__AutoMigrate=false
RecipeMedia__StoragePath=/var/lib/recepie/media/recipes
RecipeMedia__PublicPath=/images/recipes
```

Optional trusted reverse proxy example:

```env
ReverseProxy__KnownProxies__0=127.0.0.1
```

Do not configure broad forwarded-header trust. Only add proxy IP addresses that are controlled by the deployment environment.

The API fails fast when production-critical configuration is missing: connection string, JWT key/issuer/audience, or CORS allowed origins outside Testing.

## Build artifacts

Backend:

```bash
dotnet restore
dotnet publish API/API.csproj -c Release
```

Frontend:

```bash
cd app
npm ci
npm run build
```

Deploy the Angular files from `app/dist/app` to the selected static host. Deploy the API publish output from `API/bin/Release/net10.0/publish` to the selected service host.

## Generic Linux service model

A provider-specific service manager may differ. A generic systemd-style service should run the published API as a dedicated service account, load environment variables from a protected location, restart on failure, and bind only to the intended internal interface/port.

Example shape, with placeholders only:

```ini
[Unit]
Description=Recepie API
After=network.target

[Service]
WorkingDirectory=/opt/recepie/api
ExecStart=/usr/bin/dotnet /opt/recepie/api/API.dll
Restart=always
RestartSec=5
User=recepie
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5130
EnvironmentFile=/etc/recepie/api.env

[Install]
WantedBy=multi-user.target
```

Do not put real secrets in unit files committed to source control.

## Recipe media storage

Development defaults to `API/wwwroot/images/recipes`. Production should set `RecipeMedia__StoragePath` to an absolute directory on persistent storage, for example `/var/lib/recepie/media/recipes`, while keeping `RecipeMedia__PublicPath=/images/recipes`.

Recipe galleries accept 1-9 photos per recipe. Each photo must be JPEG, PNG, or WebP and no larger than 5 MB. The theoretical maximum media volume for one recipe is `9 x 5 MB = 45 MB`; actual JPEG and WebP uploads are commonly smaller.

Before starting the API, create the directory and grant write permission only to the API service account:

```bash
sudo install -d -m 0750 -o recepie -g recepie /var/lib/recepie/media/recipes
```

Back up this directory with the SQL Server backup from the same release point. Restore media together with the matching SQL backup so recipe media URLs and database rows remain consistent.

The current local-storage implementation is intended for a single API instance unless the configured media directory is backed by shared storage mounted at the same path for every instance.

## Release sequence

1. Back up SQL Server.
2. Back up the recipe media directory.
3. Check out or deploy the exact release source/artifact.
4. Set production environment variables and secrets.
5. Inspect pending migrations:

   ```bash
   dotnet ef migrations list \
     --project Infrastructure \
     --startup-project API
   ```

6. Apply migrations explicitly:

   ```bash
   dotnet ef database update \
     --project Infrastructure \
     --startup-project API
   ```

7. Publish/start the API.
8. Verify `/health/live` and `/health/ready`.
9. Deploy/start the Angular static frontend.
10. Run the production smoke test:

    ```bash
    sh scripts/production-smoke.sh https://recepie.example.com
    ```

`Database__AutoMigrate=false` is the production default. API startup must not be used as the production migration mechanism.

## Health checks and logs

- `/health/live` confirms the API process is running and does not query SQL Server.
- `/health/ready` checks readiness, including database connectivity.

Responses are intentionally minimal. Logs include method, path, response status, elapsed time, and correlation ID. Do not log authorization headers, JWTs, refresh tokens, passwords, request bodies, or connection strings.

## Smoke test scope

The repository smoke script checks:

- `/`
- `/health/live`
- `/health/ready`
- `/api/categories`

Use authenticated manual checks for login, refresh-token rotation, recipe creation, first and second photo upload, photo URL reachability, API restart with persistent media, and cleanup of test records/files.

## Backup and restore

A complete Recepie backup contains:

- SQL Server backup.
- Matching recipe media directory backup.
- Release source/artifact identifier.

Restore steps:

1. Stop application writes.
2. Verify the intended SQL and media backups.
3. Restore SQL Server using the normal SQL Server restore process for the hosting environment.
4. Restore the matching media directory backup.
5. Start the API and verify `/health/ready`.
6. Start the frontend and run the smoke test.

Test restore procedures in a non-production environment before relying on them.

## Rollback

Application rollback may be possible by redeploying the previous API and frontend artifacts. Database rollback requires migration compatibility analysis. Do not blindly run migration `Down()` methods after destructive schema or data changes. If the database must roll back, restore the validated pre-deployment SQL backup and the matching media backup.

## Known deployment risks

- The current frontend still stores access and refresh tokens in `localStorage`. Backend refresh tokens are hashed at rest, but localStorage remains XSS-sensitive and should be revisited in a future cookie-based auth hardening pass.
- The local media storage implementation needs shared mounted storage before running multiple API instances.
- Provider-specific monitoring, alerting, backup retention, and log aggregation must be configured in the selected hosting environment.
