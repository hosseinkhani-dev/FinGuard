using FinGuard.UI.Common;
using FinGuard.UI.Infrastructure.Api;
using FinGuard.UI.Models.Auth;

namespace FinGuard.UI.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> LoginAsync(LoginInputModel input)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", input);

        if (!response.IsSuccessStatusCode)
        {
            return ServiceResult<bool>.Failure(await ApiErrorHandler.ParseErrorsAsync(response));
        }

        var currentResponse = _httpContextAccessor.HttpContext?.Response;
        if (currentResponse != null && response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
        {
            foreach (var cookie in cookieValues)
            {
                currentResponse.Headers.Append("Set-Cookie", cookie);
            }
        }

        return ServiceResult<bool>.Success(true);
    }

    public async Task LogoutAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return;

        try
        {
            var response = await _httpClient.PostAsync("api/auth/logout", null);

            if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            {
                foreach (var cookie in cookieHeaders)
                {
                    context.Response.Headers.Append("Set-Cookie", cookie);
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Logout request failed.");
        }
    }

}