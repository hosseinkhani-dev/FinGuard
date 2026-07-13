using FinGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Commons.Interfaces;

public interface IFinGuardDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<TransactionFile> TransactionFiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
