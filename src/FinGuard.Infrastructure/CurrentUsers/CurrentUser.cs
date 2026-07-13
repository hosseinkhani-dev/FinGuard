using FinGuard.Application.Commons.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FinGuard.Infrastructure.CurrentUsers;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId => 
        Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public Guid TenantId =>
        Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirst("tenantId")!.Value);

    public string UserName =>
        _httpContextAccessor.HttpContext!.User.Identity!.Name!;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext!.User.Identity?.IsAuthenticated ?? false;
}
