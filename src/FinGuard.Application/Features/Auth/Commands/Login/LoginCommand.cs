using FinGuard.Application.Features.Auth.Commands.DTOs;
using MediatR;

namespace FinGuard.Application.Features.Auth.Commands.Login;

public record LoginCommand(string UserName, string Password) : IRequest<TokenResultDto>;

