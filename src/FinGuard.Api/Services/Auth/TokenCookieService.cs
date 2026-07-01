
using FinGuard.Application.Features.Auth.Commands.DTOs;

namespace FinGuard.Api.Services.Auth;

public class TokenCookieService : ITokenCookieService
{
    private const string AccessCookieName = "X-Access-Token";
    private const string RefreshCookieName = "X-Refresh-Token";

    public void AppendAccessToken(HttpResponse response, TokenResultDto tokenResultDto)
    {
        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/api/auth/refresh"
        };

        response.Cookies.Append(AccessCookieName, tokenResultDto.AccessToken, accessCookieOptions);
        response.Cookies.Append(RefreshCookieName, tokenResultDto.RefreshToken, refreshCookieOptions);
    }

    public void ClearAccessToken(HttpResponse response)
    {
        

        response.Cookies.Delete(AccessCookieName,
            new CookieOptions
        {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });
        response.Cookies.Delete(RefreshCookieName,
            new CookieOptions 
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth/refresh" 
            });
    }
}
