using System.Net;

namespace FinGuard.UI.Infrastructure.Api;

public class TokenRefreshHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    // Semaphore to prevent concurrent outbound refresh calls across threads
    private static readonly SemaphoreSlim RefreshSemaphore = new(1, 1);
    private const string RefreshedCookiesKey = "FinGuard_Refreshed_Cookies";

    public TokenRefreshHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // 1. Attach tokens (prefers newly rotated tokens if another thread just rotated them)
        AppendTokens(context, request);

        var response = await base.SendAsync(request, cancellationToken);

        // 2. Intercept 401 and guard against infinite loops on the refresh endpoint itself
        if (response.StatusCode == HttpStatusCode.Unauthorized &&
            !request.RequestUri!.AbsolutePath.EndsWith("api/auth/refresh", StringComparison.OrdinalIgnoreCase))
        {
            bool tokenRotated = false;
            List<string>? directCookieHeaders = null;

            await RefreshSemaphore.WaitAsync(cancellationToken);
            try
            {
                // Double-check lock pattern: Has another concurrent thread already rotated the token?
                if (context.Items.ContainsKey(RefreshedCookiesKey))
                {
                    directCookieHeaders = context.Items[RefreshedCookiesKey] as List<string>;
                    tokenRotated = true;
                }
                else
                {
                    // Execute the single out-of-band token rotation request
                    var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh");
                    AppendTokens(context, refreshRequest);

                    var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);

                    if (refreshResponse.IsSuccessStatusCode &&
                        refreshResponse.Headers.TryGetValues("Set-Cookie", out var newCookies))
                    {
                        directCookieHeaders = newCookies.ToList();

                        // Cache the new cookies in HttpContext.Items for concurrent sister threads
                        context.Items[RefreshedCookiesKey] = directCookieHeaders;

                        // Push the new cookies out to the browser response stream
                        foreach (var cookie in directCookieHeaders)
                        {
                            context.Response.Headers.Append("Set-Cookie", cookie);
                        }
                        tokenRotated = true;
                    }
                }
            }
            finally
            {
                RefreshSemaphore.Release();
            }

            // 3. If rotation succeeded (here or in a parallel thread), replay the original request
            if (tokenRotated && directCookieHeaders != null)
            {
                var retryRequest = CloneHttpRequestMessage(request);
                retryRequest.Headers.Remove("Cookie");

                // Map Set-Cookie syntax back to standard Cookie request header syntax
                var cleanPairs = directCookieHeaders.Select(c => c.Split(';')[0]);
                retryRequest.Headers.Add("Cookie", string.Join("; ", cleanPairs));

                return await base.SendAsync(retryRequest, cancellationToken);
            }
        }

        return response;
    }

    private static void AppendTokens(HttpContext context, HttpRequestMessage request)
    {
        // If a sister thread already rotated tokens during this execution lifecycle, use them
        if (context.Items[RefreshedCookiesKey] is List<string> freshCookies)
        {
            var cleanPairs = freshCookies.Select(c => c.Split(';')[0]);
            request.Headers.Add("Cookie", string.Join("; ", cleanPairs));
            return;
        }

        // Otherwise, fall back to the historical cookies sent by the browser request
        var cookies = new List<string>();

        if (context.Request.Cookies.TryGetValue("X-Access-Token", out var accessToken))
            cookies.Add($"X-Access-Token={accessToken}");

        if (context.Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken))
            cookies.Add($"X-Refresh-Token={refreshToken}");

        if (cookies.Any())
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies));
        }
    }

    private static HttpRequestMessage CloneHttpRequestMessage(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri);
        foreach (var header in req.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        foreach (var property in req.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(property.Key), property.Value);
        }
        if (req.Content != null)
        {
            clone.Content = req.Content;
        }
        return clone;
    }
}
