using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using FinGuard.Application.Features.Auth.Commands.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FinGuard.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResultDto>
{
    private readonly IFinGuardDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public LoginCommandHandler(
        IFinGuardDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<TokenResultDto> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {

        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

        if (user == null ||
            !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new NotFoundException("Username or Password not exist!");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User is not active!");
        }

        var initialRefreshToken = GenerateSecureRandomString();

        var currentTime = _timeProvider.GetUtcNow().UtcDateTime;

        user.AddRefreshToken(initialRefreshToken, currentTime, currentTime.AddDays(7));

        await _context.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        return new TokenResultDto(accessToken, initialRefreshToken);
    }

    private static string GenerateSecureRandomString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
