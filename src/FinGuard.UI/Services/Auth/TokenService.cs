
using FinGuard.UI.Models.Auth;

namespace FinGuard.UI.Services.Auth;

public class TokenService : ITokenService
{
    private readonly IHttpContextAccessor _contextAccessor;

    private readonly HttpClient _httpClient;

    public TokenService(IHttpContextAccessor contextAccessor, HttpClient httpClient)
    {
        _contextAccessor = contextAccessor;
        _httpClient = httpClient;
    }

    public async Task<bool> RefreshAsync()
    {
        var refreshToken = _contextAccessor.HttpContext?
            .User
            .FindFirst("refresh-token")?
            .Value;

        if (string.IsNullOrEmpty(refreshToken))
            return false;

        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh",
            new {RefreshToken = refreshToken});

        if(!response.IsSuccessStatusCode)
            return false;

        var tokens = await response.Content.ReadFromJsonAsync<TokenResultDto>();

        if(tokens == null) return false;

        return true;
    }
}
