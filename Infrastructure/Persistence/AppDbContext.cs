using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
   
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Recipie> Recipies => Set<Recipie>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipieStep> RecipeSteps => Set<RecipieStep>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<RecipeMedia> RecipeMedia => Set<RecipeMedia>();
    
    public DbSet<Users> Users => Set<Users>();
    public DbSet<FavoriteRecipe> FavoriteRecipes { get; set; }
    public DbSet<RecipeReview> RecipeReviews { get; set; }
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();
    public DbSet<RecipeLike> RecipeLikes => Set<RecipeLike>();
    public DbSet<RecipeComment> RecipeComments => Set<RecipeComment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
}
