using FinGuard.UI.Common;
using FinGuard.UI.Infrastructure.Api;
using FinGuard.UI.Models.Users;
using Microsoft.AspNetCore.WebUtilities;

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

    public async Task<ServiceResult<List<GetAllUsersModel>>> GetAllUsersAsync(
        string? userName = null, string? email = null, bool? isActive = null)
    {
        var queryParams = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(userName)) queryParams.Add("userName", userName);
        if (!string.IsNullOrWhiteSpace(email)) queryParams.Add("email", email);
        if (isActive.HasValue) queryParams.Add("isActive", isActive.Value.ToString().ToLower());

        var url = QueryHelpers.AddQueryString("api/users", queryParams);
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ApiErrorHandler.ParseErrorsAsync(response);
            return ServiceResult<List<GetAllUsersModel>>.Failure(errors);
        }

        var users = await response.Content.ReadFromJsonAsync<List<GetAllUsersModel>>();
        return ServiceResult<List<GetAllUsersModel>>.Success(users ?? new List<GetAllUsersModel>());
    }

    public async Task<ServiceResult<bool>> ActivateUserAsync(Guid id)
    {
        var response = await _httpClient.PatchAsync($"api/users/{id}/activate", null);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ApiErrorHandler.ParseErrorsAsync(response);
            return ServiceResult<bool>.Failure(errors);
        }

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DisableUserAsync(Guid id)
    {
        var response = await _httpClient.PatchAsync($"api/users/{id}/disable", null);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ApiErrorHandler.ParseErrorsAsync(response);
            return ServiceResult<bool>.Failure(errors);
        }

        return ServiceResult<bool>.Success(true);
    }
}
