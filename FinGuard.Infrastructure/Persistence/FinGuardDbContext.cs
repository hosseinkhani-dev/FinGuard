using FinGuard.Domain.Common;
using FinGuard.Domain.Entities;
using FinGuard.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;

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
    private System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type type)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenant.TenantId));

        var tenantIdProviderExpression = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(_tenantProvider),
            typeof(ITenantProvider).GetMethod(nameof(ITenantProvider.GetTenantId))!
        );

        var body = System.Linq.Expressions.Expression.Equal(property, tenantIdProviderExpression);
        return System.Linq.Expressions.Expression.Lambda(body, parameter);
    }
}
