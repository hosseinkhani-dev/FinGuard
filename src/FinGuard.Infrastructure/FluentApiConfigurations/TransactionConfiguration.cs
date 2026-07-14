using FinGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinGuard.Infrastructure.FluentApiConfigurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.EmployeeName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Department)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.CardNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.Merchant)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.HasOne(t => t.TransactionFile)
            .WithMany(tf => tf.Transactions)
            .HasForeignKey(t => t.TransactionFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
