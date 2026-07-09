//namespace FinGuard.UI.Infrastructure.Api;

//public class CookiePropagationHandler : DelegatingHandler
//{
//    private readonly IHttpContextAccessor _httpContextAccessor;

//    public CookiePropagationHandler(IHttpContextAccessor httpContextAccessor)
//    {
//        _httpContextAccessor = httpContextAccessor;
//    }

//    protected override async Task<HttpResponseMessage> SendAsync(
//        HttpRequestMessage request,
//        CancellationToken cancellationToken)
//    {
//        var context = _httpContextAccessor.HttpContext;

//        if (context != null && context.Request.Cookies.TryGetValue("X-Access-Token", out var accessToken))
//        {
//            request.Headers.Add("Cookie", $"X-Access-Token={accessToken}");
//        }

//        return await base.SendAsync(request, cancellationToken);
//    }
//}
