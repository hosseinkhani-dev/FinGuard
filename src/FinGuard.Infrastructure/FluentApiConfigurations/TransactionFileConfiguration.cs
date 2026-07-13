using FinGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinGuard.Infrastructure.FluentApiConfigurations;

public class TransactionFileConfiguration : IEntityTypeConfiguration<TransactionFile>
{
    public void Configure(EntityTypeBuilder<TransactionFile> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.OriginalFileName).IsRequired().HasMaxLength(50);
        builder.Property(t => t.StoredFileName).IsRequired().HasMaxLength(50);
    }
}
