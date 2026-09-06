using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RecipeMediaConfiguration : IEntityTypeConfiguration<RecipeMedia>
{
    public void Configure(EntityTypeBuilder<RecipeMedia> builder)
    {
        builder.ToTable("RecipeMedia");
        builder.HasKey(media => media.Id);
        builder.Property(media => media.Url).IsRequired().HasMaxLength(2048);
        builder.Property(media => media.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(media => media.MediaType).IsRequired();
        builder.Property(media => media.IsMain).IsRequired();
        builder.Property(media => media.SortOrder).IsRequired();
        builder.HasOne(media => media.Recipe)
            .WithMany(recipe => recipe.Media)
            .HasForeignKey(media => media.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(media => media.RecipeId);
        builder.HasIndex(media => new { media.RecipeId, media.SortOrder });
    }
}
