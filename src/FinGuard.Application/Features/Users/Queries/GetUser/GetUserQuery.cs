using MediatR;

namespace FinGuard.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(Guid Id) : IRequest<GetUserDto?>;
