using FinGuard.Application.Features.Auth.Commands.DTOs;
using MediatR;

namespace FinGuard.Application.Features.Auth.Commands.RefreshSessions;

public record RefreshSessionCommand(string RefreshTokenString) : IRequest<TokenResultDto>;
