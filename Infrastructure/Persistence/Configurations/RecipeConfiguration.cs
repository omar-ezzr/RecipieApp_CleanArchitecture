using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipie>
{
    public void Configure(EntityTypeBuilder<Recipie> builder)
    {
        builder.ToTable("Recipes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(r => r.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(r => r.TraditionalName)
            .HasMaxLength(200);

        builder.Property(r => r.OriginDescription)
            .HasMaxLength(2000);

        builder.Property(r => r.ServingOccasion)
            .HasMaxLength(200);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Recipes)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.UserId);

        builder.HasIndex(r => new { r.UserId, r.CreatedAt });

        builder.HasOne(r => r.Cuisine)
            .WithMany(c => c.Recipes)
            .HasForeignKey(r => r.CuisineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Region)
            .WithMany(r => r.Recipes)
            .HasForeignKey(r => r.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.CuisineId);

        builder.HasIndex(r => r.RegionId);

        builder.HasIndex(r => new { r.CategoryId, r.CreatedAt });
        builder.HasIndex(r => new { r.RegionId, r.CreatedAt });

        builder.HasIndex(r => new { r.CuisineId, r.CreatedAt });

        builder.HasIndex(r => new { r.CuisineId, r.RegionId, r.CreatedAt });
    }
}
