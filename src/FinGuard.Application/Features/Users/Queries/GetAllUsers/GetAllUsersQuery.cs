using MediatR;

namespace FinGuard.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery(
    string? UserName = null,
    string? Email = null,
    bool? IsActive = null
    ) : IRequest<List<GetAllUserDto>>;
