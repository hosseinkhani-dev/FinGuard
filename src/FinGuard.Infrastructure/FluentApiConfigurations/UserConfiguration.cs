using FinGuard.Domain.Entities;
using FinGuard.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinGuard.Infrastructure.FluentApiConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName).IsRequired().HasMaxLength(50);

        builder.HasIndex(u => u.UserName).IsUnique();

        builder.OwnsOne(u => u.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.EmailAddress)
                .HasColumnName("Email")
                .HasMaxLength(200)
                .IsRequired(false);
        });

        builder.OwnsMany(u => u.RefreshTokens, refreshToken =>
        {
            refreshToken.ToTable("UserRefreshTokens");
            refreshToken.HasKey(r => r.Token);
            refreshToken.Property(r => r.Token).HasMaxLength(128);
            refreshToken.WithOwner().HasForeignKey("UserId");
        });
    }
}
