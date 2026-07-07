namespace FinGuard.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUserDto()
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; init; } = string.Empty;
    public string ActiveStatus { get; init; } = string.Empty;
}
