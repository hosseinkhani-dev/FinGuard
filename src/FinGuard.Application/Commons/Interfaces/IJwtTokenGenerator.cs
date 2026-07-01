using FinGuard.Domain.Entities;

namespace FinGuard.Application.Commons.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
