using FinGuard.Domain.Entities;

namespace FinGuard.IntegrationTests.Builders;

public class TenantBuilder
{
    private string _name = "dummy-tenant-name";
    private DateTime _createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public TenantBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public TenantBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Tenant Build()
    {
        return new Tenant(_name, _createdAt);
    }
}

