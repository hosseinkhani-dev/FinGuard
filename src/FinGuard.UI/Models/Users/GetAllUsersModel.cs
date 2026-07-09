namespace FinGuard.UI.Models.Users;

public record GetAllUsersModel(
    Guid Id,
    string UserName,
    string? Email,
    string ActiveStatus);
