using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Application.UseCases.Auth;
using Core.Application.UseCases.Categories;
using Core.Application.UseCases.Favorites;
using Core.Application.UseCases.Recipes;
using FluentValidation;
using FluentValidation.AspNetCore;
using Core.Application.Validators;
using Infrastructure.Seed;
using Core.Application.UseCases.Reviews;
using Core.Application.UseCases.Users;
using Core.Application.UseCases.Cuisines;
using Core.Application.UseCases.Regions;
using Core.Application.UseCases.Social;
using Core.Domain.Constants;
using System.Security.Claims;
using Core.Application.Options;
using API.Options;
using API.Responses;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using DotNetEnv;
using API.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var envFile = Path.Combine(builder.Environment.ContentRootPath, ".env");
    if (File.Exists(envFile))
    {
        Env.NoClobber().Load(envFile);
        builder.Configuration.AddEnvironmentVariables();
    }
}

builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    foreach (var address in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
    {
        if (IPAddress.TryParse(address, out var proxyAddress))
        {
            options.KnownProxies.Add(proxyAddress);
        }
    }
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new
            {
                Field = ToCamelCase(entry.Key),
                Message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "The value is invalid."
                    : error.ErrorMessage
            }))
            .GroupBy(error => error.Field)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Message).ToArray());

        return new BadRequestObjectResult(new ApiErrorResponse
        {
            Code = "validation_failed",
            Message = "Validation failed.",
            Errors = errors,
            TraceId = context.HttpContext.TraceIdentifier
        });
    };
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1"
    });

    // 🔐 Add JWT support
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<RecipeMediaOptions>(builder.Configuration.GetSection(RecipeMediaOptions.SectionName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RecipeMediaOptions>>().Value);
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (!builder.Environment.IsEnvironment("Testing") && allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("At least one CORS allowed origin must be configured.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth-login", context => CreateAuthLimiter(context, 5, TimeSpan.FromMinutes(1)));
    options.AddPolicy("auth-register", context => CreateAuthLimiter(context, 3, TimeSpan.FromMinutes(10)));
    options.AddPolicy("auth-token", context => CreateAuthLimiter(context, 15, TimeSpan.FromMinutes(1)));
});

//JWT
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("JWT key must contain at least 32 bytes.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("JWT issuer and audience must be configured.");
}

var jwtSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtKey)
);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtSigningKey,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var tokenRole = context.Principal?.FindFirstValue(ClaimTypes.Role);

                if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(tokenRole))
                {
                    context.Fail("Invalid user identity.");
                    return;
                }

                var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var user = await users.GetByIdAsync(userId, track: false, context.HttpContext.RequestAborted);

                if (user is null || !user.IsActive || user.Role != tokenRole)
                {
                    context.Fail("Invalid user identity.");
                }
            }
        };
    });


builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ICuisineService, CuisineService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IUserFollowService, UserFollowService>();
builder.Services.AddScoped<IRecipeLikeService, RecipeLikeService>();
builder.Services.AddScoped<IRecipeCommentService, RecipeCommentService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<INotificationService, NotificationService>();


//validation error
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateRecipeValidator>();
builder.Services.Configure<AdminSeedOptions>(
    builder.Configuration.GetSection(AdminSeedOptions.SectionName));

var app = builder.Build();

var staticFileContentTypes = new FileExtensionContentTypeProvider();
staticFileContentTypes.Mappings[".avif"] = "image/avif";
staticFileContentTypes.Mappings[".mp4"] = "video/mp4";
staticFileContentTypes.Mappings[".webm"] = "video/webm";


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exceptionHandlerFeature?.Error is not null)
        {
            logger.LogError(
                exceptionHandlerFeature.Error,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                exceptionHandlerFeature.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ApiErrorResponse
        {
            Code = "unexpected_error",
            Message = "An unexpected error occurred.",
            TraceId = context.TraceIdentifier
        });
    });
});
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = staticFileContentTypes
});
app.UseRouting();
app.UseCors("AllowAngular");
app.UseRateLimiter();

app.UseAuthentication();   
app.UseAuthorization();    

app.MapControllers();      
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponse
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
}).AllowAnonymous();

if (!app.Environment.IsEnvironment("Testing") && app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (builder.Configuration.GetValue<bool>("Database:AutoMigrate"))
    {
        await db.Database.MigrateAsync();
    }

    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    await DbSeeder.SeedAsync(db, builder.Configuration, passwordService, app.Environment.IsDevelopment());
}

app.Run();

static RateLimitPartition<string> CreateAuthLimiter(HttpContext context, int permitLimit, TimeSpan window)
{
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = permitLimit,
        Window = window,
        QueueLimit = 0,
        AutoReplenishment = true
    });
}

static string ToCamelCase(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    var lastSegment = value.Split('.').Last();

    return char.ToLowerInvariant(lastSegment[0]) + lastSegment[1..];
}

static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "text/plain";
    return context.Response.WriteAsync(report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy");
}

public partial class Program { }
