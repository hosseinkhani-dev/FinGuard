using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.Auth.Commands.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FinGuard.Application.Features.Auth.Commands.RefreshSessions;

public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, TokenResultDto>
{
    private readonly IFinGuardDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;


    public RefreshSessionCommandHandler(
        IFinGuardDbContext context,
        TimeProvider timeProvider,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _timeProvider = timeProvider;
        _jwtTokenGenerator = jwtTokenGenerator;
    }


    public async Task<TokenResultDto> Handle(
        RefreshSessionCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(r => r.Token == request.RefreshTokenString));

        if (user == null)
            throw new UnauthorizedException("Invalid session.");

        var refreshToken =  user.RefreshTokens.First(r => r.Token == request.RefreshTokenString);

        var currentTime = _timeProvider.GetUtcNow().UtcDateTime;

        if (!refreshToken.IsActive(currentTime))
        {
            throw new UnauthorizedException("Session has expired or been revoked.");
        }

        user.RevokeRefreshToken(refreshToken.Token);

        var newAccessToken = _jwtTokenGenerator.GenerateToken(user);
        var newRefreshTokenString = GenerateSecureRandomString();

        user.AddRefreshToken(newRefreshTokenString, currentTime, currentTime.AddDays(7));

        await _context.SaveChangesAsync(cancellationToken);

        return new TokenResultDto(newAccessToken, newRefreshTokenString);
    }

    private static string GenerateSecureRandomString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
