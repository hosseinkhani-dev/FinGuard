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

        public AuthController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            CancellationToken cancellationToken)
        {
            var tokenResultDto = await _mediator.Send(command, cancellationToken);

            return Ok(tokenResultDto);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            if (!Request.Cookies.TryGetValue(
                "X-Refresh-Token", out var refreshToken))
            {
                return Unauthorized();
            }

            var tokenResultDto = await _mediator.Send(
                new RefreshSessionCommand(refreshToken),
                cancellationToken);

            return Ok(tokenResultDto);
        }
    }
}
