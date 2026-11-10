using AmazonCreators.Net.Models.Product;
using AmazonCreators.Net.Models.Requests;
using AmazonCreators.Net.Models.Responses;

namespace AmazonCreators.Net.Services;

/// <summary>
/// High-performance client interface for the Amazon Creators API.
/// Supports OAuth 2.0 (Login with Amazon v3.x and Cognito v2.x) and full OffersV2 pricing.
/// </summary>
public interface IAmazonCreatorsClient
{
    /// <summary>
    /// Gets the formatted display price (e.g. "19,99 €") for the primary available offer of an ASIN.
    /// </summary>
    /// <param name="asin">Amazon Standard Identification Number (10 characters).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Formatted display amount, or null if no offers or out of stock.</returns>
    Task<string?> GetProductPriceAsync(string asin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets complete OffersV2 structure (all merchant listings, pricing, condition, Buy Box) for an ASIN.
    /// </summary>
    /// <param name="asin">Amazon Standard Identification Number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OffersV2 object with full listings, or null if not found.</returns>
    Task<OffersV2?> GetProductOffersV2Async(string asin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product catalog data and offers for one or more ASINs using default marketplace and partner tag.
    /// </summary>
    /// <param name="asins">Array of ASINs (up to 10 per request recommended by Amazon).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CreatorsApiResponse?> GetProductOffersAsync(
        string[] asins,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product catalog data and offers for one or more ASINs targeting a specific marketplace and partner tag.
    /// </summary>
    /// <param name="asins">Array of ASINs to query.</param>
    /// <param name="target">Target marketplace domain and partner tag.</param>
    /// <param name="resources">Optional list of resources to return. If null, default resources are used.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CreatorsApiResponse?> GetProductOffersAsync(
        string[] asins,
        AmazonMarketplaceTarget target,
        IReadOnlyList<string>? resources = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by keywords or EAN using default marketplace and partner tag.
    /// </summary>
    /// <param name="keywords">Search query or EAN code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CreatorsApiResponse?> SearchItemsAsync(
        string keywords,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by keywords targeting a specific marketplace and partner tag.
    /// </summary>
    /// <param name="keywords">Search query or EAN code.</param>
    /// <param name="target">Target marketplace domain and partner tag.</param>
    /// <param name="itemCount">Maximum items to return (1-10).</param>
    /// <param name="resources">Optional list of resources to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CreatorsApiResponse?> SearchItemsAsync(
        string keywords,
        AmazonMarketplaceTarget target,
        int itemCount = 10,
        IReadOnlyList<string>? resources = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the client currently holds a valid, unexpired OAuth token.
    /// </summary>
    Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually invalidates and refreshes the OAuth 2.0 access token.
    /// </summary>
    Task RefreshTokenAsync(CancellationToken cancellationToken = default);
}
