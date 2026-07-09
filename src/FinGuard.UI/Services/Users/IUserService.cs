using FinGuard.UI.Common;
using FinGuard.UI.Models.Users;

namespace FinGuard.UI.Services.Users;

public interface IUserService
{
    Task<ServiceResult<Guid>> CreateUserAsync(CreateUserInputModel inputModel);
    Task<ServiceResult<List<GetAllUsersModel>>> GetAllUsersAsync(
        string? userName = null, string? email = null, bool? isActive = null);
    Task<ServiceResult<bool>> ActivateUserAsync(Guid id);
    Task<ServiceResult<bool>> DisableUserAsync(Guid id);
}
