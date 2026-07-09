using System.Net.Http.Headers;

namespace FinGuard.UI.Infrastructure.Api;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public BearerTokenHandler(
        IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }


    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken =
            _accessor.HttpContext?
            .User
            .FindFirst("access_token")
            ?.Value;


        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);
        }


        return await base.SendAsync(
            request,
            cancellationToken);
    }
}
