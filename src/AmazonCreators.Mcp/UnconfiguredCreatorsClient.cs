using AmazonCreators.Net.Models.Product;
using AmazonCreators.Net.Models.Requests;
using AmazonCreators.Net.Models.Responses;
using AmazonCreators.Net.Services;

namespace AmazonCreators.Mcp;

/// <summary>
/// Fallback client used when AmazonCreators.Mcp is launched without credentials configured.
/// Returns informative configuration warnings instead of crashing.
/// </summary>
internal sealed class UnconfiguredCreatorsClient : IAmazonCreatorsClient
{
    private const string MissingConfigError =
        "Amazon Creators API credentials not configured. Please set environment variables: AMAZON_CREATORS_CLIENT_ID, AMAZON_CREATORS_CLIENT_SECRET, and AMAZON_CREATORS_PARTNER_TAG.";

    public Task<string?> GetProductPriceAsync(string asin, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(MissingConfigError);

    public Task<OffersV2?> GetProductOffersV2Async(string asin, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(MissingConfigError);

    public Task<CreatorsApiResponse?> GetProductOffersAsync(string[] asins, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(MissingConfigError);

    public Task<CreatorsApiResponse?> GetProductOffersAsync(string[] asins, AmazonMarketplaceTarget target, IReadOnlyList<string>? resources = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(MissingConfigError);

    public Task<CreatorsApiResponse?> SearchItemsAsync(string keywords, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(MissingConfigError);

    public Task<CreatorsApiResponse?> SearchItemsAsync(string keywords, AmazonMarketplaceTarget target, int itemCount = 10, IReadOnlyList<string>? resources = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(MissingConfigError);

    public Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task RefreshTokenAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(MissingConfigError);
}
