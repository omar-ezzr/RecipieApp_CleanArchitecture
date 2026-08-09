using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RecipeCommentConfiguration : IEntityTypeConfiguration<RecipeComment>
{
    public void Configure(EntityTypeBuilder<RecipeComment> builder)
    {
        builder.ToTable("RecipeComments");
        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.Content)
            .IsRequired()
            .HasMaxLength(1500);

        builder.Property(comment => comment.CreatedAt).IsRequired();

        builder.HasIndex(comment => comment.RecipeId);
        builder.HasIndex(comment => new { comment.RecipeId, comment.CreatedAt });
        builder.HasIndex(comment => comment.UserId);

        builder.HasOne(comment => comment.Recipe)
            .WithMany()
            .HasForeignKey(comment => comment.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(comment => comment.User)
            .WithMany()
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
