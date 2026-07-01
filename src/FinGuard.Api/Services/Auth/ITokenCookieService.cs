namespace FinGuard.Api.Services.Auth;

public interface ITokenCookieService
{
    void AppendAccessToken(HttpResponse response, string token);
    void ClearAccessToken(HttpResponse response);
}
