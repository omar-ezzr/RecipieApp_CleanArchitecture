using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.ToTable("UserFollows");
        builder.HasKey(follow => follow.Id);

        builder.Property(follow => follow.CreatedAt).IsRequired();

        builder.HasIndex(follow => new { follow.FollowerUserId, follow.FollowedUserId }).IsUnique();
        builder.HasIndex(follow => follow.FollowerUserId);
        builder.HasIndex(follow => follow.FollowedUserId);
        builder.HasIndex(follow => new { follow.FollowerUserId, follow.CreatedAt });
        builder.HasIndex(follow => new { follow.FollowedUserId, follow.CreatedAt });

        builder.HasOne(follow => follow.FollowerUser)
            .WithMany(user => user.Following)
            .HasForeignKey(follow => follow.FollowerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(follow => follow.FollowedUser)
            .WithMany(user => user.Followers)
            .HasForeignKey(follow => follow.FollowedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
