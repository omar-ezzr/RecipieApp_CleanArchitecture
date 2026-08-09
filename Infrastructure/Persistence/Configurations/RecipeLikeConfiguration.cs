using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RecipeLikeConfiguration : IEntityTypeConfiguration<RecipeLike>
{
    public void Configure(EntityTypeBuilder<RecipeLike> builder)
    {
        builder.ToTable("RecipeLikes");
        builder.HasKey(like => like.Id);

        builder.Property(like => like.CreatedAt).IsRequired();

        builder.HasIndex(like => new { like.UserId, like.RecipeId }).IsUnique();
        builder.HasIndex(like => like.RecipeId);
        builder.HasIndex(like => like.UserId);
        builder.HasIndex(like => new { like.RecipeId, like.CreatedAt });

        builder.HasOne(like => like.User)
            .WithMany()
            .HasForeignKey(like => like.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(like => like.Recipe)
            .WithMany()
            .HasForeignKey(like => like.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
