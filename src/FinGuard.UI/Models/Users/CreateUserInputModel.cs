using System.ComponentModel.DataAnnotations;

namespace FinGuard.UI.Models.Users
{
    public class CreateUserInputModel
    {
        [Required(ErrorMessage = "UserName cannot be empty.")]
        [StringLength(50, ErrorMessage = "UserName cannot be more than 50 character.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string? Email { get; set; }
    }
}
