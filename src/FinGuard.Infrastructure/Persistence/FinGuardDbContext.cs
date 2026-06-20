using FinGuard.Domain.Common;
using FinGuard.Domain.Entities;
using FinGuard.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinGuard.Infrastructure.Persistence;

public class FinGuardDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public FinGuardDbContext(
        DbContextOptions<FinGuardDbContext> options,
        ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();

    public Guid CurrentTenantId => _tenantProvider.GetTenantId();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Find and Apply Fluent API Files
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinGuardDbContext).Assembly);

        // Global Query Filter On Every Entity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenant).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
    }

    // Modify The SaveChangesAsync So In Every Add Save The TenantId To The Object Automatically
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentTenantId = _tenantProvider.GetTenantId();

        foreach (var entry in ChangeTracker.Entries<ITenant>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(ITenant.TenantId)).CurrentValue = currentTenantId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }


    // e => e.TenantId == _tenantProvider.GetTenantId()
    private LambdaExpression ConvertFilterExpression(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var property = Expression.Property(parameter, nameof(ITenant.TenantId));

        var contextConstant = Expression.Constant(this);

        var tenantIdProviderExpression =
            Expression.Property(contextConstant, nameof(CurrentTenantId));

        var body = Expression.Equal(property, tenantIdProviderExpression);
        return Expression.Lambda(body, parameter);
    }
}
