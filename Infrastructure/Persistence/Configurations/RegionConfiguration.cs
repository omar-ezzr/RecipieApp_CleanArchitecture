using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Regions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(r => r.Slug)
            .IsRequired()
            .HasMaxLength(140);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(r => r.Cuisine)
            .WithMany(c => c.Regions)
            .HasForeignKey(r => r.CuisineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.CuisineId);

        builder.HasIndex(r => new { r.CuisineId, r.Slug })
            .IsUnique();
    }
}
