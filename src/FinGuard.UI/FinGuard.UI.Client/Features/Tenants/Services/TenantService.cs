using FinGuard.UI.Client.Components.Shared;
using FinGuard.UI.Client.Features.Tenants.Models;
using FinGuard.UI.Client.Features.Tenants.Services;
using System.Net;
using System.Net.Http.Json;

namespace FinGuard.UI.Client.Features.Tenants.CreateTenants.Services;

public class TenantService : ITenantService
{
    private readonly HttpClient _httpClient;

    public TenantService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ModelResult<Guid>> CreateTenantAsync(CreateTenantModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tenants/create", model);

        if (response.IsSuccessStatusCode)
        {
            var tenantId = await response.Content.ReadFromJsonAsync<Guid>();

            return new ModelResult<Guid> { IsSuccess = true, Data = tenantId };
        }

        if(response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problemDetail = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            if(problemDetail != null && problemDetail.Errors != null)
            {
                return new ModelResult<Guid> { IsSuccess = false, Errors = problemDetail.Errors };
            } 
        }

        return ModelResult<Guid>.Failure("An unexpected error occurred.");
    }
}
