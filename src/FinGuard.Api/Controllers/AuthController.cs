using FinGuard.Api.Services.Auth;
using FinGuard.Application.Features.Auth.Commands.Login;
using FinGuard.Application.Features.Auth.Commands.RefreshSessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinGuard.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ITokenCookieService _tokenCookieService;

        public AuthController(
            ISender mediator,
            ITokenCookieService tokenCookieService)
        {
            _mediator = mediator;
            _tokenCookieService = tokenCookieService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            CancellationToken cancellationToken)
        {
            var tokenResultDto = await _mediator.Send(command, cancellationToken);

            _tokenCookieService.AppendAccessToken(Response, tokenResultDto);

            return Ok(new { message = "Authentication successful." });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            if (!Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken))
            {
                return Unauthorized(new { message = "Missing refresh token context." });
            }

            var tokenResultDto = await _mediator.Send(
                new RefreshSessionCommand(refreshToken),
                cancellationToken);

            _tokenCookieService.AppendAccessToken(Response, tokenResultDto);

            return Ok(new { message = "Token successfully refreshed" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _tokenCookieService.ClearAccessToken(Response);
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpGet("me")]
        public IActionResult GetCurrentUserState()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Ok(new { isAuthenticated = false });
            }

            var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
            return Ok(new { isAuthenticated = true, userName = User.Identity.Name, claims });
        }
    }
}
