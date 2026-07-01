using FinGuard.Api.Services.Auth;
using FinGuard.Application.Features.Auth.Commands.Login;
using MediatR;
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
            var token = await _mediator.Send(command, cancellationToken);

            _tokenCookieService.AppendAccessToken(Response, token);

            return Ok(new { message = "Authentication successful." });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _tokenCookieService.ClearAccessToken(Response);
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
