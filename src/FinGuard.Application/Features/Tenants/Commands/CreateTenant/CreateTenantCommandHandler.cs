using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Domain.Entities;
using FinGuard.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly IFinGuardDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IPasswordHasher _passwordHasher;

    public CreateTenantCommandHandler(
        IFinGuardDbContext context,
        TimeProvider timeProvider,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _timeProvider = timeProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        var isUserExist = await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.UserName == request.UserName);

        if (isUserExist)
        {
            throw new ConflictException($"This username {request.UserName} is already taken!");
        }

        var newTenant = new Tenant(request.Name,  _timeProvider.GetUtcNow().UtcDateTime);

        _context.Tenants.Add(newTenant);

        Email? userEmail = string.IsNullOrWhiteSpace(request.Email)
        ? null
        : new Email(request.Email);

        string hashedPassword = _passwordHasher.HashPassword(request.Password);

        var newUser = new User(request.UserName, hashedPassword, userEmail);
        newUser.AssignTenant(newTenant.Id);

        _context.Users.Add(newUser);

        await _context.SaveChangesAsync(cancellationToken);

        return newTenant.Id;
    }
}
