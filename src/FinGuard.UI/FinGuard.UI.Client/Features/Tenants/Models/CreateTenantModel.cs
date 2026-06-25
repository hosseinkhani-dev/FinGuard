namespace FinGuard.UI.Client.Features.Tenants.Models;

public class CreateTenantModel
{
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
}
