using FinGuard.UI.Common;
using FinGuard.UI.Infrastructure.Api;
using FinGuard.UI.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        var response = await _httpClient.PostAsJsonAsync(
        "api/auth/login",
        input);


        if (!response.IsSuccessStatusCode)
        {
            return ServiceResult<bool>.Failure(
                await ApiErrorHandler.ParseErrorsAsync(response));
        }


        var tokens = await response.Content
            .ReadFromJsonAsync<TokenResultDto>();


        if (tokens == null)
        {
            return ServiceResult<bool>.Failure(
                "Token response was empty.");
        }

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(tokens.AccessToken);

        var claims = jwt.Claims
            .Where(c => c.Type != "role")
            .ToList();


        var roleClaim = jwt.Claims
            .FirstOrDefault(c => c.Type == "role");

        if (roleClaim != null)
        {
            claims.Add(new Claim(
                ClaimTypes.Role,
                roleClaim.Value));
        }

        // Store tokens inside the UI authentication cookie
        claims.Add(new Claim(
            "access_token",
            tokens.AccessToken));

        claims.Add(new Claim(
            "refresh_token",
            tokens.RefreshToken));


        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);


        var principal = new ClaimsPrincipal(identity);


        await _httpContextAccessor.HttpContext!
            .SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });



        return ServiceResult<bool>.Success(true);
    }

    public async Task LogoutAsync()
    {
        var context = _httpContextAccessor.HttpContext;

        if (context == null)
            return;

        try
        {
            await context.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed.");
        }
    }

}