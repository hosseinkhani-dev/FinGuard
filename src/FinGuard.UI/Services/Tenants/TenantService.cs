using FinGuard.UI.Common;
using FinGuard.UI.Infrastructures;
using FinGuard.UI.Models.Tenants;

namespace FinGuard.UI.Services.Tenants;

public class TenantService : ITenantService
{
    private readonly HttpClient _httpClient;

    public TenantService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceResult<Guid>> CreateTenantAsync(CreateTenantInputModel input)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tenants/create", input);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await ApiErrorHandler.ParseErrorsAsync(response);
            return ServiceResult<Guid>.Failure(errors);
        }

        var id = await response.Content.ReadFromJsonAsync<Guid>();
        return ServiceResult<Guid>.Success(id);
    }
}
