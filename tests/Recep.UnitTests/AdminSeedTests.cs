using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Infrastructure.Persistence;
using Infrastructure.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Recep.UnitTests;

public sealed class AdminSeedTests
{
    [Theory]
    [InlineData(false, "admin@example.com", "StrongPass123")]
    [InlineData(true, "", "StrongPass123")]
    [InlineData(true, "admin@example.com", "")]
    public async Task Admin_seed_skips_disabled_or_incomplete_configuration(
        bool enabled,
        string email,
        string password)
    {
        await using var context = await CreateContextAsync();
        var configuration = NewConfiguration(enabled, email, password);

        await DbSeeder.SeedAsync(context, configuration, new FakePasswordService(), isDevelopment: true);

        context.Users.Count(user => user.Role == AppRoles.Admin).Should().Be(0);
    }

    [Fact]
    public async Task Admin_seed_creates_one_admin_and_is_idempotent()
    {
        await using var context = await CreateContextAsync();
        var configuration = NewConfiguration(enabled: true, "admin@example.com", "StrongPass123");

        await DbSeeder.SeedAsync(context, configuration, new FakePasswordService(), isDevelopment: true);
        await DbSeeder.SeedAsync(context, configuration, new FakePasswordService(), isDevelopment: true);

        var admins = context.Users.Where(user => user.Role == AppRoles.Admin).ToList();
        admins.Should().ContainSingle();
        admins[0].Email.Should().Be("admin@example.com");
        admins[0].PasswordHash.Should().Be("HASHED:StrongPass123");
    }

    [Fact]
    public async Task Admin_seed_does_not_promote_existing_normal_user()
    {
        await using var context = await CreateContextAsync();
        context.Users.Add(new()
        {
            Id = Guid.NewGuid(),
            DisplayName = "Existing Admin",
            Email = "admin@example.com",
            PasswordHash = "existing",
            Role = AppRoles.User,
            IsActive = true
        });
        await context.SaveChangesAsync();
        var configuration = NewConfiguration(enabled: true, "admin@example.com", "StrongPass123");

        await DbSeeder.SeedAsync(context, configuration, new FakePasswordService(), isDevelopment: true);

        var user = context.Users.Single(user => user.Email == "admin@example.com");
        user.Role.Should().Be(AppRoles.User);
        user.PasswordHash.Should().Be("existing");
    }

    [Fact]
    public async Task Admin_seed_is_development_only()
    {
        await using var context = await CreateContextAsync();
        var configuration = NewConfiguration(enabled: true, "admin@example.com", "StrongPass123");

        await DbSeeder.SeedAsync(context, configuration, new FakePasswordService(), isDevelopment: false);

        context.Users.Count(user => user.Role == AppRoles.Admin).Should().Be(0);
    }

    private static IConfiguration NewConfiguration(bool enabled, string email, string password)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminSeed:Enabled"] = enabled.ToString(),
                ["AdminSeed:Email"] = email,
                ["AdminSeed:Password"] = password
            })
            .Build();
    }

    private static async Task<AppDbContext> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public string Hash(string password) => $"HASHED:{password}";
        public bool Verify(string password, string hash) => hash == Hash(password);
    }
}
