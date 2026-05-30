using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RecipeReviewConfiguration : IEntityTypeConfiguration<RecipeReview>
    {
        public void Configure(EntityTypeBuilder<RecipeReview> builder)
        {
            builder.ToTable("RecipeReviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.HasIndex(r => new { r.UserId, r.RecipeId })
                .IsUnique();

            builder.HasOne(r => r.Recipe)
                .WithMany()
                .HasForeignKey(r => r.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}