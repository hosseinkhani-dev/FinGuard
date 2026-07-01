using MediatR;

namespace FinGuard.Application.Features.Auth.Commands.Login;

public record LoginCommand(string userName, string password) : IRequest<string>;

