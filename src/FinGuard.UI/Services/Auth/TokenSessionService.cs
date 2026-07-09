using FinGuard.UI.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

public class TokenSessionService
{
    private readonly IHttpContextAccessor _accessor;

    public TokenSessionService(
        IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }


    public async Task UpdateTokensAsync(TokenResultDto tokens)
    {
        var context = _accessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is missing.");

        var currentUserName =
            context.User.FindFirst(ClaimTypes.Name)?.Value
            ?? context.User.FindFirst("unique_name")?.Value
            ?? "Unknown User";


        var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.Name,
            currentUserName),

        new Claim(
            "access_token",
            tokens.AccessToken),

        new Claim(
            "refresh_token",
            tokens.RefreshToken)
    };


        // Preserve existing role
        var role =
            context.User.FindFirst(ClaimTypes.Role)?.Value
            ?? context.User.FindFirst("role")?.Value;


        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }


        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);


        var principal = new ClaimsPrincipal(identity);


        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);
    }
}