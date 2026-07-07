using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IFinGuardDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public CreateUserCommandHandler(
        IFinGuardDbContext context,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        bool isUserExists = await _context.Users
            .AnyAsync(u => u.UserName == request.UserName, cancellationToken);

        if (isUserExists)
            throw new ConflictException($"User with username '{request.UserName}' already exists.");

        Email? email = string.IsNullOrWhiteSpace(request.Email) ? null
        : new Email(request.Email);

        string hashedPassword = _passwordHasher.HashPassword(request.Password);

        var newUser = new User(request.UserName,
            hashedPassword,
            UserRole.Auditor,
            email,
            _timeProvider.GetUtcNow().UtcDateTime);

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        return newUser.Id;
    }
}
