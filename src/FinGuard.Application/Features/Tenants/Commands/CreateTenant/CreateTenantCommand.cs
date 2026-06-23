using FinGuard.Domain.ValueObjects;
using MediatR;

namespace FinGuard.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(
    string Name,
    string UserName,
    string PasswordHash,
    string? Email ) : IRequest<Guid>;

