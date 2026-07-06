using FinGuard.Application.Features.Users.Commands.ActivateUser;
using FinGuard.Application.Features.Users.Commands.CreateUser;
using FinGuard.Application.Features.Users.Commands.DisableUser;
using FinGuard.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinGuard.Api.Controllers;

[Route("api/users")]
[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("disable")]
    public async Task<IActionResult> DisableUser(
        [FromBody] DisableUserCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return Ok();
    }

    [HttpPatch("activate")]
    public async Task<IActionResult> ActivateUser(
        [FromBody] ActivateUserCommand command,
        CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return Ok();
    }
}
