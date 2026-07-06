using MediatR;

namespace FinGuard.Application.Features.Users.Commands.DisableUser;

public record DisableUserCommand (Guid Id) : IRequest
{
}
