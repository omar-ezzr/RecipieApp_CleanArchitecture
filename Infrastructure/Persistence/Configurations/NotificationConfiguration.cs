using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(notification => notification.IsRead).IsRequired();
        builder.Property(notification => notification.CreatedAt).IsRequired();

        builder.HasIndex(notification => notification.RecipientUserId);
        builder.HasIndex(notification => new { notification.RecipientUserId, notification.IsRead });
        builder.HasIndex(notification => new { notification.RecipientUserId, notification.CreatedAt });

        builder.HasOne(notification => notification.RecipientUser)
            .WithMany()
            .HasForeignKey(notification => notification.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.ActorUser)
            .WithMany()
            .HasForeignKey(notification => notification.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(notification => notification.Recipe);
        builder.Ignore(notification => notification.Comment);
    }
}
