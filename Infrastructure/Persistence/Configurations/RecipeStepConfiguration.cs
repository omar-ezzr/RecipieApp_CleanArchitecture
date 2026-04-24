using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipieStep>
{
    public void Configure(EntityTypeBuilder<RecipieStep> builder)
    {
        builder.ToTable("RecipeSteps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StepNumber)
            .IsRequired();

        builder.Property(s => s.Instruction)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.RecipeId)
            .IsRequired();
    }
}
