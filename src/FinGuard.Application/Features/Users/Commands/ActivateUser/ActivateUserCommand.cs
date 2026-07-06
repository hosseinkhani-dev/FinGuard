using MediatR;

namespace FinGuard.Application.Features.Users.Commands.ActivateUser;

public record ActivateUserCommand(Guid Id) : IRequest;
