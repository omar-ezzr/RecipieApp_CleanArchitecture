using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = GetConnectionString();

        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string GetConnectionString()
    {
        var apiPath = FindApiPath();
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var connectionString = ReadConnectionString(Path.Combine(apiPath, "appsettings.json"));
        var environmentConnectionString = ReadConnectionString(Path.Combine(apiPath, $"appsettings.{environment}.json"));
        var environmentVariableConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        connectionString = FirstNonEmpty(
            environmentVariableConnectionString,
            environmentConnectionString,
            connectionString);

        return connectionString
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing. Set ConnectionStrings__DefaultConnection or use .NET user secrets.");
    }

    private static string FindApiPath()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var apiPath = Path.Combine(current.FullName, "API");
            if (Directory.Exists(apiPath))
            {
                return apiPath;
            }

            if (current.Name == "API")
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "..", "API");
    }

    private static string? ReadConnectionString(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));

        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
            || !connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
        {
            return null;
        }

        return defaultConnection.GetString();
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
