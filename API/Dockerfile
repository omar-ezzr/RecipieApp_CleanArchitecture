FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY API/API.csproj API/
COPY Core.Application/Core.Application.csproj Core.Application/
COPY Core.Domain/Core.Domain.csproj Core.Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
RUN dotnet restore API/API.csproj

COPY API/ API/
COPY Core.Application/ Core.Application/
COPY Core.Domain/ Core.Domain/
COPY Infrastructure/ Infrastructure/
RUN dotnet publish API/API.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 CMD curl --fail --silent --show-error -H "X-Forwarded-Proto: https" http://localhost:8080/health/ready || exit 1
ENTRYPOINT ["dotnet", "API.dll"]
