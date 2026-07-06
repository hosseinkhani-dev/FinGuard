using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand>
{
    private readonly IFinGuardDbContext _context;

    public ActivateUserCommandHandler(IFinGuardDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.Id == request.Id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException($"User not found.");
        }

        user.Activate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
