using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

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

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true);

        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
        {
            var userSecretsId = ReadUserSecretsId(Path.Combine(apiPath, "API.csproj"));
            if (!string.IsNullOrWhiteSpace(userSecretsId))
            {
                configurationBuilder.AddJsonFile(GetUserSecretsPath(userSecretsId), optional: true);
            }

            var envFile = Path.Combine(apiPath, ".env");
            if (File.Exists(envFile))
            {
                configurationBuilder.AddInMemoryCollection(ReadDotEnv(envFile));
            }
        }

        configurationBuilder.AddEnvironmentVariables();

        var connectionString = configurationBuilder
            .Build()
            .GetConnectionString("DefaultConnection");

        return connectionString
            ?? throw new InvalidOperationException(
                "Connection string DefaultConnection is missing. Set ConnectionStrings__DefaultConnection or use .NET user secrets.");
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

    private static string? ReadUserSecretsId(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var project = XDocument.Load(path);
        return project
            .Descendants("UserSecretsId")
            .Select(element => element.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string GetUserSecretsPath(string userSecretsId)
    {
        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Microsoft", "UserSecrets", userSecretsId, "secrets.json");
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, ".microsoft", "usersecrets", userSecretsId, "secrets.json");
    }

    private static IEnumerable<KeyValuePair<string, string?>> ReadDotEnv(string path)
    {
        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var separator = line.IndexOf("=", StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (value.Length >= 2
                && value.StartsWith("\"", StringComparison.Ordinal)
                && value.EndsWith("\"", StringComparison.Ordinal))
            {
                value = value[1..^1];
            }

            if (!string.IsNullOrWhiteSpace(key)
                && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                yield return new KeyValuePair<string, string?>(key, value);
            }
        }
    }
}
