using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class FavoriteRecipeConfiguration : IEntityTypeConfiguration<FavoriteRecipe>
    {
        public void Configure(EntityTypeBuilder<FavoriteRecipe> builder)
        {
            builder.ToTable("FavoriteRecipes");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.CreatedAt)
                .IsRequired();

            builder.HasIndex(f => new { f.UserId, f.RecipeId })
                .IsUnique();
            builder.HasIndex(f => new { f.UserId, f.CreatedAt });

            builder.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Recipe)
                .WithMany()
                .HasForeignKey(f => f.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}