
using FinGuard.Application.Features.Auth.Commands.DTOs;

namespace FinGuard.Api.Services.Auth;

public class TokenCookieService : ITokenCookieService
{
    private const string AccessCookieName = "X-Access-Token";
    private const string RefreshCookieName = "X-Refresh-Token";
    private readonly IConfiguration _configuration;

    public TokenCookieService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void AppendAccessToken(HttpResponse response, TokenResultDto tokenResultDto)
    {
        var expiryMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"] ?? "15");

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Path = "/"
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
                Path = "/"
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
