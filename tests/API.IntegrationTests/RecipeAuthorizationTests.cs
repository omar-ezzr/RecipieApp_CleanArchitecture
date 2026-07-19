using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Core.Application.DTO.Auth;
using Core.Application.DTO.Recipe;
using Core.Application.DTO.Users;
using Core.Domain.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.IntegrationTests;

public sealed class RecipeAuthorizationTests : IClassFixture<RecipeApiFactory>
{
    private readonly RecipeApiFactory _factory;

    public RecipeAuthorizationTests(RecipeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_cannot_create_update_or_delete_recipes()
    {
        var client = _factory.CreateClientForRole(AppRoles.User);
        var recipeId = await _factory.CreateRecipeAsync();

        var create = await client.PostAsJsonAsync("/api/Recipes", _factory.CreateRecipeRequest());
        var update = await client.PutAsJsonAsync($"/api/Recipes/{recipeId}", _factory.CreateRecipeRequest());
        var delete = await client.DeleteAsync($"/api/Recipes/{recipeId}");

        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Theory]
    [InlineData(AppRoles.Admin)]
    [InlineData(AppRoles.Operator)]
    public async Task Admin_and_operator_can_create_update_and_delete_recipes(string role)
    {
        var client = _factory.CreateClientForRole(role);
        var recipeId = await _factory.CreateRecipeAsync();

        var create = await client.PostAsJsonAsync("/api/Recipes", _factory.CreateRecipeRequest());
        var update = await client.PutAsJsonAsync($"/api/Recipes/{recipeId}", _factory.CreateRecipeRequest("Hard"));
        var delete = await client.DeleteAsync($"/api/Recipes/{recipeId}");

        Assert.True(create.IsSuccessStatusCode);
        Assert.True(update.IsSuccessStatusCode);
        Assert.True(delete.IsSuccessStatusCode);
    }
}

public sealed class AccountManagementTests : IClassFixture<RecipeApiFactory>
{
    private readonly RecipeApiFactory _factory;

    public AccountManagementTests(RecipeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Public_registration_always_creates_user_role()
    {
        var email = $"register-{Guid.NewGuid():N}@example.com";
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Auth/register", new LoginDto
        {
            Email = email,
            Password = "StrongPass123"
        });

        response.EnsureSuccessStatusCode();
        var user = await _factory.FindUserByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal(AppRoles.User, user!.Role);
        Assert.True(user.IsActive);
    }

    [Theory]
    [InlineData(AppRoles.User)]
    [InlineData(AppRoles.Operator)]
    public async Task Non_admin_roles_receive_403_from_account_list(string role)
    {
        var client = _factory.CreateClientForRole(role);

        var response = await client.GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_list_accounts()
    {
        var client = _factory.CreateClientForRole(AppRoles.Admin);

        var response = await client.GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(AppRoles.User)]
    [InlineData(AppRoles.Operator)]
    public async Task Admin_can_create_accounts(string role)
    {
        var client = _factory.CreateClientForRole(AppRoles.Admin);
        var email = $"create-{role.ToLowerInvariant()}-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/admin/users", new CreateUserAccountDto
        {
            Email = email,
            Password = "StrongPass123",
            Role = role
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<UserAccountDto>();
        Assert.Equal(role, account!.Role);
        Assert.Equal(email, account.Email);
    }

    [Fact]
    public async Task Duplicate_emails_return_409()
    {
        var client = _factory.CreateClientForRole(AppRoles.Admin);
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        await _factory.CreateUserAsync(email, AppRoles.User);

        var response = await client.PostAsJsonAsync("/api/admin/users", new CreateUserAccountDto
        {
            Email = email.ToUpperInvariant(),
            Password = "StrongPass123",
            Role = AppRoles.User
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Unsupported_roles_are_rejected()
    {
        var client = _factory.CreateClientForRole(AppRoles.Admin);

        var response = await client.PostAsJsonAsync("/api/admin/users", new CreateUserAccountDto
        {
            Email = $"invalid-role-{Guid.NewGuid():N}@example.com",
            Password = "StrongPass123",
            Role = "Owner"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Inactive_accounts_cannot_login_or_refresh()
    {
        var email = $"inactive-{Guid.NewGuid():N}@example.com";
        var refresh = $"refresh-{Guid.NewGuid():N}";
        await _factory.CreateUserAsync(email, AppRoles.User, isActive: false, refreshToken: refresh);
        var client = _factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = email,
            Password = RecipeApiFactory.KnownPassword
        });
        var refreshResponse = await client.PostAsJsonAsync("/api/Auth/refresh", new TokenRequestDto
        {
            RefreshToken = refresh
        });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_cannot_modify_or_delete_self()
    {
        var client = _factory.CreateClientForRole(AppRoles.Admin);

        var role = await client.PutAsJsonAsync($"/api/admin/users/{_factory.AdminId}/role", new UpdateUserRoleDto { Role = AppRoles.User });
        var status = await client.PutAsJsonAsync($"/api/admin/users/{_factory.AdminId}/status", new UpdateUserStatusDto { IsActive = false });
        var delete = await client.DeleteAsync($"/api/admin/users/{_factory.AdminId}");

        Assert.Equal(HttpStatusCode.BadRequest, role.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, status.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, delete.StatusCode);
    }

    [Fact]
    public async Task Final_active_admin_self_modification_is_rejected_by_self_protection()
    {
        var onlyAdminId = await _factory.CreateUserAsync($"only-admin-{Guid.NewGuid():N}@example.com", AppRoles.Admin);
        var client = _factory.CreateClientForUser(onlyAdminId, AppRoles.Admin);
        await _factory.DeactivateUserAsync(_factory.AdminId);

        var demote = await client.PutAsJsonAsync($"/api/admin/users/{onlyAdminId}/role", new UpdateUserRoleDto { Role = AppRoles.Operator });
        var deactivate = await client.PutAsJsonAsync($"/api/admin/users/{onlyAdminId}/status", new UpdateUserStatusDto { IsActive = false });
        var delete = await client.DeleteAsync($"/api/admin/users/{onlyAdminId}");

        await _factory.ActivateUserAsync(_factory.AdminId);

        Assert.Equal(HttpStatusCode.BadRequest, demote.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, deactivate.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, delete.StatusCode);
    }

    [Fact]
    public async Task Role_change_invalidates_previous_jwt()
    {
        var operatorId = await _factory.CreateUserAsync($"invalidate-role-{Guid.NewGuid():N}@example.com", AppRoles.Operator);
        var operatorClient = _factory.CreateClientForUser(operatorId, AppRoles.Operator);
        var adminClient = _factory.CreateClientForRole(AppRoles.Admin);

        var before = await operatorClient.PostAsJsonAsync("/api/Recipes", _factory.CreateRecipeRequest());
        var change = await adminClient.PutAsJsonAsync($"/api/admin/users/{operatorId}/role", new UpdateUserRoleDto { Role = AppRoles.User });
        var after = await operatorClient.PostAsJsonAsync("/api/Recipes", _factory.CreateRecipeRequest());

        Assert.True(before.IsSuccessStatusCode);
        Assert.True(change.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }

    [Fact]
    public async Task Deactivation_invalidates_previous_jwt()
    {
        var userId = await _factory.CreateUserAsync($"invalidate-status-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var userClient = _factory.CreateClientForUser(userId, AppRoles.User);
        var adminClient = _factory.CreateClientForRole(AppRoles.Admin);

        var before = await userClient.GetAsync("/api/Recipes/paged");
        var change = await adminClient.PutAsJsonAsync($"/api/admin/users/{userId}/status", new UpdateUserStatusDto { IsActive = false });
        var after = await userClient.GetAsync("/api/Recipes/paged");

        Assert.True(before.IsSuccessStatusCode);
        Assert.True(change.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }
}

public sealed class RecipeApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string KnownPassword = "StrongPass123";
    private const string JwtKey = "TEST_SIGNING_KEY_FOR_RECIPE_AUTHORIZATION_1234567890";
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public Guid CategoryId { get; private set; }
    public Guid AdminId { get; private set; }
    public Guid OperatorId { get; private set; }
    public Guid UserId { get; private set; }

    public RecipeApiFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(localdb)\\mssqllocaldb;Database=Unused;Trusted_Connection=True;");
        Environment.SetEnvironmentVariable("Jwt__Key", JwtKey);
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", "5");
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenDays", "7");
    }

    public HttpClient CreateClientForRole(string role)
    {
        var userId = role switch
        {
            AppRoles.Admin => AdminId,
            AppRoles.Operator => OperatorId,
            _ => UserId
        };

        return CreateClientForUser(userId, role);
    }

    public HttpClient CreateClientForUser(Guid userId, string role)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId, role));
        return client;
    }

    public CreateRecipeDto CreateRecipeRequest(string difficulty = "Easy")
    {
        return new CreateRecipeDto
        {
            Title = $"Integration Soup {Guid.NewGuid():N}",
            Description = "Warm",
            PreparationTimeMinutes = 20,
            CategoryId = CategoryId,
            Difficulty = difficulty
        };
    }

    public async Task<Guid> CreateRecipeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var id = Guid.NewGuid();

        context.Recipies.Add(new Recipie
        {
            Id = id,
            Title = $"Existing Soup {Guid.NewGuid():N}",
            Description = "Warm",
            PreparationTimeMinutes = 20,
            Difficulty = DifficultyLevel.Medium,
            CategoryId = CategoryId
        });

        await context.SaveChangesAsync();
        return id;
    }

    public async Task<Guid> CreateUserAsync(
        string email,
        string role,
        bool isActive = true,
        string? refreshToken = null)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var id = Guid.NewGuid();

        context.Users.Add(new Users
        {
            Id = id,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(KnownPassword),
            Role = role,
            IsActive = isActive,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = refreshToken is null ? null : DateTime.UtcNow.AddDays(7)
        });

        await context.SaveChangesAsync();
        return id;
    }

    public async Task<Users?> FindUserByEmailAsync(string email)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email.Trim().ToLowerInvariant());
    }

    public async Task DeactivateUserAsync(Guid id)
    {
        await SetActiveAsync(id, false);
    }

    public async Task ActivateUserAsync(Guid id)
    {
        await SetActiveAsync(id, true);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=Unused;Trusted_Connection=True;",
                ["Jwt:Key"] = JwtKey,
                ["Jwt:AccessTokenMinutes"] = "5",
                ["Jwt:RefreshTokenDays"] = "7"
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                descriptor => descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        CategoryId = Guid.NewGuid();
        AdminId = Guid.NewGuid();
        OperatorId = Guid.NewGuid();
        UserId = Guid.NewGuid();

        context.Categories.Add(new Category
        {
            Id = CategoryId,
            Name = "Dinner"
        });

        context.Users.AddRange(
            NewUser(AdminId, "admin@example.com", AppRoles.Admin),
            NewUser(OperatorId, "operator@example.com", AppRoles.Operator),
            NewUser(UserId, "user@example.com", AppRoles.User));

        await context.SaveChangesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _connection.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("Jwt__Key", null);
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", null);
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenDays", null);
    }

    private async Task SetActiveAsync(Guid id, bool isActive)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await context.Users.FirstAsync(account => account.Id == id);
        user.IsActive = isActive;
        await context.SaveChangesAsync();
    }

    private static Users NewUser(Guid id, string email, string role)
    {
        return new Users
        {
            Id = id,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(KnownPassword),
            Role = role,
            IsActive = true
        };
    }

    private static string CreateToken(Guid userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"{role.ToLowerInvariant()}@example.com"),
            new Claim(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
