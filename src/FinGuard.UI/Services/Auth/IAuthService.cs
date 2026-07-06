using FinGuard.UI.Common;
using FinGuard.UI.Models.Auth;

namespace FinGuard.UI.Services.Auth;

public interface IAuthService
{
    Task<ServiceResult<bool>> LoginAsync(LoginInputModel input);
    Task LogoutAsync();
}
