using FinGuard.Application.Features.Auth.Commands.DTOs;

namespace FinGuard.Api.Services.Auth;

public interface ITokenCookieService
{
    void AppendAccessToken(HttpResponse response, TokenResultDto tokenResultDto);
    void ClearAccessToken(HttpResponse response);
}
