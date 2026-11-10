using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Constants;
using AmazonCreators.Net.Models.Auth;
using AmazonCreators.Net.Models.Product;
using AmazonCreators.Net.Models.Requests;
using AmazonCreators.Net.Models.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AmazonCreators.Net.Services;

/// <summary>
/// Thread-safe client for Amazon Creators API with automatic OAuth lifecycle,
/// dual v3.x (LwA) / v2.x (Cognito) authentication, and response caching.
/// </summary>
public class AmazonCreatorsClient : IAmazonCreatorsClient
{
    private readonly HttpClient _httpClient;
    private readonly AmazonCreatorsOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    // Token management
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);
    private volatile TokenResponse? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;

    // Response cache: "{Marketplace}|{ASIN}" -> (Response, ExpiresAt)
    private readonly ConcurrentDictionary<string, (CreatorsApiResponse Response, DateTime ExpiresAt)> _responseCache = new();

    [ActivatorUtilitiesConstructor]
    public AmazonCreatorsClient(HttpClient httpClient, IOptions<AmazonCreatorsOptions> options)
        : this(httpClient, options?.Value ?? throw new ArgumentNullException(nameof(options)))
    {
    }

    public AmazonCreatorsClient(HttpClient httpClient, AmazonCreatorsOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new ArgumentException("ClientId and ClientSecret are required in AmazonCreatorsOptions", nameof(options));
        }

        if (_options.RequestTimeoutSeconds > 0)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        };
    }

    /// <inheritdoc />
    public async Task<string?> GetProductPriceAsync(string asin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(asin))
            throw new ArgumentException("ASIN cannot be null or empty", nameof(asin));

        var offers = await GetProductOffersV2Async(asin, cancellationToken).ConfigureAwait(false);
        if (offers?.Listings == null || offers.Listings.Length == 0)
            return null;

        return offers.Listings.FirstOrDefault()?.Price?.Money?.DisplayAmount;
    }

    /// <inheritdoc />
    public async Task<OffersV2?> GetProductOffersV2Async(string asin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(asin))
            throw new ArgumentException("ASIN cannot be null or empty", nameof(asin));

        var response = await GetProductOffersAsync([asin], cancellationToken).ConfigureAwait(false);
        return response?.FirstItem?.OffersV2;
    }

    /// <inheritdoc />
    public Task<CreatorsApiResponse?> GetProductOffersAsync(
        string[] asins,
        CancellationToken cancellationToken = default) =>
        GetProductOffersAsync(
            asins,
            new AmazonMarketplaceTarget(_options.Marketplace, _options.PartnerTag),
            AmazonCreatorsResources.Default,
            cancellationToken);

    /// <inheritdoc />
    public async Task<CreatorsApiResponse?> GetProductOffersAsync(
        string[] asins,
        AmazonMarketplaceTarget target,
        IReadOnlyList<string>? resources = null,
        CancellationToken cancellationToken = default)
    {
        if (asins == null || asins.Length == 0)
            throw new ArgumentException("At least one ASIN is required", nameof(asins));

        if (target == null || string.IsNullOrWhiteSpace(target.Marketplace) || string.IsNullOrWhiteSpace(target.PartnerTag))
            throw new ArgumentException("Target marketplace and partner tag are required", nameof(target));

        var cacheKey = $"{target.Marketplace}|{asins[0]}";

        // Check single-ASIN cache
        if (asins.Length == 1 && _options.EnableResponseCaching)
        {
            if (_responseCache.TryGetValue(cacheKey, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
            {
                return cached.Response;
            }
        }

        var accessToken = await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
        var resolvedResources = resources?.ToArray() ?? AmazonCreatorsResources.Default;

        var requestPayload = new GetItemsRequest
        {
            ItemIds = asins,
            PartnerTag = target.PartnerTag,
            Resources = resolvedResources
        };

        var json = JsonSerializer.Serialize(requestPayload, _jsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.ApiEndpoint.TrimEnd('/')}/getItems")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.TryAddWithoutValidation("Authorization", _options.BuildAuthorizationHeader(accessToken));
        httpRequest.Headers.Add("x-marketplace", target.Marketplace);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, responseContent);
        }

        var result = JsonSerializer.Deserialize<CreatorsApiResponse>(responseContent, _jsonOptions);

        if (asins.Length == 1 && _options.EnableResponseCaching && result != null)
        {
            var expiresAt = DateTime.UtcNow.AddSeconds(_options.ResponseCacheDurationSeconds);
            _responseCache.AddOrUpdate(cacheKey, (result, expiresAt), (_, _) => (result, expiresAt));
        }

        return result;
    }

    /// <inheritdoc />
    public Task<CreatorsApiResponse?> SearchItemsAsync(
        string keywords,
        CancellationToken cancellationToken = default) =>
        SearchItemsAsync(
            keywords,
            new AmazonMarketplaceTarget(_options.Marketplace, _options.PartnerTag),
            10,
            AmazonCreatorsResources.Default,
            cancellationToken);

    /// <inheritdoc />
    public async Task<CreatorsApiResponse?> SearchItemsAsync(
        string keywords,
        AmazonMarketplaceTarget target,
        int itemCount = 10,
        IReadOnlyList<string>? resources = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keywords))
            throw new ArgumentException("Keywords cannot be null or empty", nameof(keywords));

        if (target == null || string.IsNullOrWhiteSpace(target.Marketplace) || string.IsNullOrWhiteSpace(target.PartnerTag))
            throw new ArgumentException("Target marketplace and partner tag are required", nameof(target));

        var accessToken = await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
        var resolvedResources = resources?.ToArray() ?? AmazonCreatorsResources.Default;

        var requestPayload = new SearchItemsRequest
        {
            Keywords = keywords,
            PartnerTag = target.PartnerTag,
            ItemCount = Math.Clamp(itemCount, 1, 10),
            SearchIndex = "All",
            Resources = resolvedResources
        };

        var json = JsonSerializer.Serialize(requestPayload, _jsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.ApiEndpoint.TrimEnd('/')}/searchItems")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.TryAddWithoutValidation("Authorization", _options.BuildAuthorizationHeader(accessToken));
        httpRequest.Headers.Add("x-marketplace", target.Marketplace);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, responseContent);
        }

        return JsonSerializer.Deserialize<CreatorsApiResponse>(responseContent, _jsonOptions);
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false);
            return _cachedToken != null;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokenSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _cachedToken = null;
            _tokenExpiresAt = DateTime.MinValue;
            await AcquireTokenAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    private async Task<string> EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken != null && DateTime.UtcNow.AddSeconds(30) < _tokenExpiresAt)
        {
            return _cachedToken.AccessToken;
        }

        await _tokenSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cachedToken != null && DateTime.UtcNow.AddSeconds(30) < _tokenExpiresAt)
            {
                return _cachedToken.AccessToken;
            }

            await AcquireTokenAsync(cancellationToken).ConfigureAwait(false);
            return _cachedToken?.AccessToken ?? throw new UnauthorizedAccessException("Acquired token is null");
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    private async Task AcquireTokenAsync(CancellationToken cancellationToken)
    {
        var clientId = _options.ClientId.Trim();
        var clientSecret = _options.ClientSecret.Trim();
        var tokenEndpoint = _options.ResolveTokenEndpoint();
        var scope = _options.ResolveScope();

        var bodyParts = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        if (!string.IsNullOrWhiteSpace(scope))
        {
            bodyParts["scope"] = scope;
        }

        // v2.x Cognito expects form-urlencoded; v3.x Login with Amazon expects JSON
        HttpContent content = _options.CredentialVersion.StartsWith("2.", StringComparison.Ordinal)
            ? new FormUrlEncodedContent(bodyParts)
            : new StringContent(JsonSerializer.Serialize(bodyParts), Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(
                $"Failed to obtain OAuth token from Amazon. StatusCode: {(int)response.StatusCode} ({response.StatusCode}). Response: {responseContent}");
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, _jsonOptions);
        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Amazon OAuth token response is missing 'access_token'");
        }

        _cachedToken = tokenResponse;
        var expiresInSeconds = tokenResponse.ExpiresIn > 60 ? tokenResponse.ExpiresIn - 30 : tokenResponse.ExpiresIn;
        _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
    }

    private static void HandleErrorResponse(HttpStatusCode statusCode, string responseContent)
    {
        switch (statusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new UnauthorizedAccessException(
                    $"Amazon Creators API authentication failed (HTTP {(int)statusCode}). Verify ClientId, ClientSecret, and PartnerTag permissions. Response: {responseContent}");

            case HttpStatusCode.BadRequest:
                throw new ArgumentException(
                    $"Invalid parameters in Amazon Creators API request (HTTP 400). Response: {responseContent}");

            case HttpStatusCode.NotFound:
                break;

            case HttpStatusCode.TooManyRequests:
                throw new HttpRequestException(
                    $"Amazon Creators API rate limit exceeded (HTTP 429). Response: {responseContent}",
                    null,
                    HttpStatusCode.TooManyRequests);

            default:
                throw new HttpRequestException(
                    $"Amazon Creators API error (HTTP {(int)statusCode}): {responseContent}",
                    null,
                    statusCode);
        }
    }
}
