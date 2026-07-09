using FinGuard.Application.Features.Users.Commands.ActivateUser;
using FinGuard.Application.Features.Users.Commands.CreateUser;
using FinGuard.Application.Features.Users.Commands.DisableUser;
using FinGuard.Application.Features.Users.Queries.GetAllUsers;
using FinGuard.Application.Features.Users.Queries.GetUser;
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

    [HttpPatch("{id:guid}/disable")]
    public async Task<IActionResult> DisableUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DisableUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] GetAllUsersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await _sender.Send(query, cancellationToken);

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _sender.Send(new GetUserQuery(id), cancellationToken);

        return Ok(user);
    }
}
