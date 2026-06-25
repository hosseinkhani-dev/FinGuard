using MediatR;

namespace FinGuard.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(
    string Name,
    string UserName,
    string Password,
    string? Email ) : IRequest<Guid>;

