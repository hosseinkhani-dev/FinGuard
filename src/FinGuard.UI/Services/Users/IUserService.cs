using FinGuard.UI.Common;
using FinGuard.UI.Models.Users;

namespace FinGuard.UI.Services.Users;

public interface IUserService
{
    Task<ServiceResult<Guid>> CreateUserAsync(CreateUserInputModel inputModel);
}
