using FinGuard.UI.Common;
using FinGuard.UI.Infrastructure.Api;
using FinGuard.UI.Models.Users;

namespace FinGuard.UI.Services.Users;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceResult<Guid>> CreateUserAsync(
        CreateUserInputModel inputModel)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users/create", inputModel);

        if(!response.IsSuccessStatusCode)
        {
            var errors = await ApiErrorHandler.ParseErrorsAsync(response);
            return ServiceResult<Guid>.Failure(errors);
        }

        var id = await response.Content.ReadFromJsonAsync<Guid>();
        return ServiceResult<Guid>.Success(id);
    }
}
