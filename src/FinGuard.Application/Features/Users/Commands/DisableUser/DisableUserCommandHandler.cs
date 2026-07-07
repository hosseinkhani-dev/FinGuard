using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.Users.Commands.DisableUser;

public class DisableUserCommandHandler : IRequestHandler<DisableUserCommand>
{
    private readonly IFinGuardDbContext _context;

    public DisableUserCommandHandler(IFinGuardDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DisableUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync( u => u.Id == request.Id, cancellationToken);

        if (user is null)
            throw new NotFoundException($"User not found.");

        user.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
