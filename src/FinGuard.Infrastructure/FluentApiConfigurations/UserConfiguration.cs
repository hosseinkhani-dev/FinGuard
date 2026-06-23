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

        builder.Property(u => u.Email)
            .HasConversion(
            email => email != null ? email.EmailAddress : null,
            value => value != null ? new Email(value) : null)
            .HasColumnName("Email")
            .HasMaxLength(200)
            .IsRequired(false);
    }
}
