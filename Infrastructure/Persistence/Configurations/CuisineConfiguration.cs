using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CuisineConfiguration : IEntityTypeConfiguration<Cuisine>
{
    public void Configure(EntityTypeBuilder<Cuisine> builder)
    {
        builder.ToTable("Cuisines");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(140);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.CountryCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(c => c.Slug)
            .IsUnique();
    }
}
