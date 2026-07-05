using System.ComponentModel.DataAnnotations;

namespace FinGuard.UI.Models.Tenants;

public class CreateTenantInputModel
{
    [Required(ErrorMessage = "Tenant name is required.")]
    [StringLength(50, ErrorMessage = "Tenant name cannot exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
    public string? Email { get; set; }
}
