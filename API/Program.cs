using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Core.Application.Interfaces.Services;
using Core.Application.UseCases.Recipes;
using FluentValidation;
using FluentValidation.AspNetCore;
using Core.Application.Validators;
using Infrastructure.Services;
using Infrastructure.Seed;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IRecipeService, RecipeService>();
//host
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

//JWT
var key = Encoding.UTF8.GetBytes("THIS_IS_MY_SUPER_SECRET_KEY_1234567890");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
builder.Services.AddScoped<IPasswordService, PasswordService>();
//validation error
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateRecipeValidator>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));
app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();   
app.UseAuthorization();    

app.MapControllers();      

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
await DbSeeder.SeedAsync(db);}

app.Run();