using FinGuard.Application.Commons.Exceptions;
using FinGuard.Application.Commons.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IFinGuardDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IFinGuardDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {

        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.UserName == request.userName);

        if (user == null ||
            !_passwordHasher.VerifyPassword(request.password, user.PasswordHash))
        {
            throw new NotFoundException("Username or Password not exist!");
        }

        return _jwtTokenGenerator.GenerateToken(user);
    }
}
