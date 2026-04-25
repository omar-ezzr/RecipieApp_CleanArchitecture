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

        var connectionString =
            ReadConnectionString(Path.Combine(apiPath, "appsettings.json"));

        var environmentConnectionString =
            ReadConnectionString(Path.Combine(apiPath, $"appsettings.{environment}.json"));

        connectionString = environmentConnectionString ?? connectionString;

        return connectionString
            ?? "Server=localhost,1433;Database=FoodRecipeDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;";
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
}
