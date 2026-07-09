using FinGuard.UI.Models.Auth;
using System.Net;
using System.Net.Http.Headers;

namespace FinGuard.UI.Infrastructure.Api;

public class TokenRefreshHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenSessionService _tokenSessionService;

    private static readonly SemaphoreSlim RefreshSemaphore = new(1, 1);


    public TokenRefreshHandler(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        TokenSessionService tokenSessionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _tokenSessionService = tokenSessionService;
    }


    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;

        Console.WriteLine("TOKEN REFRESH HANDLER STARTED");

        if (context == null)
        {
            return await base.SendAsync(
                request,
                cancellationToken);
        }


        var response = await base.SendAsync(
            request,
            cancellationToken);



        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }



        await RefreshSemaphore.WaitAsync(
            cancellationToken);

        try
        {
            // Call refresh endpoint
            var client = _httpClientFactory
                .CreateClient("AuthRefreshClient");


            var refreshToken =
                context.User
                .FindFirst("refresh_token")
                ?.Value;


            if (string.IsNullOrEmpty(refreshToken))
            {
                return response;
            }


            var refreshRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/auth/refresh");


            refreshRequest.Headers.Add(
                "Cookie",
                $"X-Refresh-Token={refreshToken}");



            var refreshResponse =
                await client.SendAsync(
                    refreshRequest,
                    cancellationToken);



            if (!refreshResponse.IsSuccessStatusCode)
            {
                return response;
            }



            var tokens =
                await refreshResponse.Content
                .ReadFromJsonAsync<TokenResultDto>(
                    cancellationToken: cancellationToken);



            if (tokens == null)
            {
                return response;
            }



            // Update UI authentication cookie
            await _tokenSessionService
                .UpdateTokensAsync(tokens);



            // Retry original request
            var retryRequest =
                CloneHttpRequestMessage(request);



            retryRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    tokens.AccessToken);



            return await base.SendAsync(
                retryRequest,
                cancellationToken);

        }
        finally
        {
            RefreshSemaphore.Release();
        }
    }



    private static HttpRequestMessage CloneHttpRequestMessage(
        HttpRequestMessage request)
    {
        var clone =
            new HttpRequestMessage(
                request.Method,
                request.RequestUri);


        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value);
        }


        clone.Content = request.Content;


        return clone;
    }
}