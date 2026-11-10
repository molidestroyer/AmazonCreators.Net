using System.Net;
using System.Text;
using System.Text.Json;

namespace AmazonCreators.Net.Tests.Fakes;

public sealed record CapturedApiCall(
    HttpMethod Method,
    string Url,
    string? AuthorizationHeader,
    string? MarketplaceHeader,
    string? Body);

public class FakeHttpMessageHandler : HttpMessageHandler
{
    public List<CapturedApiCall> CapturedCalls { get; } = new();
    public int TokenCallCount { get; private set; }
    public int GetItemsCallCount { get; private set; }
    public int SearchItemsCallCount { get; private set; }

    public int ThrottleNextCallsCount { get; set; }
    public string MockTokenValue { get; set; } = "mock-valid-access-token-xyz";
    public int MockTokenExpiresIn { get; set; } = 3600;
    public string? CustomGetItemsResponseJson { get; set; }
    public string? CustomSearchItemsResponseJson { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? string.Empty;
        var body = request.Content != null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : null;

        var authHeader = request.Headers.Authorization?.ToString()
            ?? (request.Headers.TryGetValues("Authorization", out var authVals) ? authVals.FirstOrDefault() : null);

        var marketplaceHeader = request.Headers.TryGetValues("x-marketplace", out var mpVals)
            ? mpVals.FirstOrDefault()
            : null;

        CapturedCalls.Add(new CapturedApiCall(request.Method, url, authHeader, marketplaceHeader, body));

        // Simulate throttling if requested
        if (ThrottleNextCallsCount > 0)
        {
            ThrottleNextCallsCount--;
            return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("""{"code":"TooManyRequests","message":"Throttled"}""", Encoding.UTF8, "application/json")
            };
        }

        // Token endpoint
        if (url.Contains("/token") || url.Contains("/oauth2/token") || url.Contains("/auth/o2/token"))
        {
            TokenCallCount++;
            var tokenJson = $$"""
            {
              "access_token": "{{MockTokenValue}}",
              "token_type": "Bearer",
              "expires_in": {{MockTokenExpiresIn}},
              "scope": "creatorsapi::default"
            }
            """;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(tokenJson, Encoding.UTF8, "application/json")
            };
        }

        // /getItems endpoint
        if (url.EndsWith("/getItems", StringComparison.OrdinalIgnoreCase))
        {
            GetItemsCallCount++;
            var responseJson = CustomGetItemsResponseJson ?? MockCreatorsPayloads.StandardGetItemsResponse;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        }

        // /searchItems endpoint
        if (url.EndsWith("/searchItems", StringComparison.OrdinalIgnoreCase))
        {
            SearchItemsCallCount++;
            var responseJson = CustomSearchItemsResponseJson ?? MockCreatorsPayloads.StandardSearchItemsResponse;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"message":"Not found"}""", Encoding.UTF8, "application/json")
        };
    }
}
