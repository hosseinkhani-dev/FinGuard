using MediatR;

namespace FinGuard.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand (
    string UserName,
    string Password,
    string? Email = null
) : IRequest<Guid>;
