namespace FinGuard.Application.Features.Users.Queries.GetUser;

public record GetUserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? Email { get; set; }
    public string ActiveStatus { get; init; } = string.Empty;
}
