
namespace FinGuard.Api.Services.Auth;

public class TokenCookieService : ITokenCookieService
{
    private const string CookieName = "X-Access-Token";

    public void AppendAccessToken(HttpResponse response, string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        response.Cookies.Append(CookieName, token, cookieOptions);
    }

    public void ClearAccessToken(HttpResponse response)
    {
        response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        });
    }
}
