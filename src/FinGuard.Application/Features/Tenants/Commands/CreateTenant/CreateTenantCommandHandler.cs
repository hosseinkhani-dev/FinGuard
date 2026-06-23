using FinGuard.Application.Commons.Interfaces;
using FinGuard.Domain.Entities;
using FinGuard.Domain.ValueObjects;
using MediatR;

namespace FinGuard.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly IFinGuardDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateTenantCommandHandler(
        IFinGuardDbContext context,
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        var newTenant = new Tenant(request.Name,  _timeProvider);

        _context.Tenants.Add(newTenant);

        Email? userEmail = string.IsNullOrWhiteSpace(request.Email)
        ? null
        : new Email(request.Email);

        var newUser = new User(request.UserName, request.PasswordHash, userEmail);
        newUser.AssignTenant(newTenant.Id);

        _context.Users.Add(newUser);

        await _context.SaveChangesAsync(cancellationToken);

        return newTenant.Id;
    }
}
