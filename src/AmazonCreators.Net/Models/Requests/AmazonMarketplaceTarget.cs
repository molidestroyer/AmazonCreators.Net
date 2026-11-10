namespace AmazonCreators.Net.Models.Requests;

/// <summary>
/// Specifies the target Amazon marketplace domain and partner tag for a specific request.
/// </summary>
/// <param name="Marketplace">Amazon marketplace domain (e.g. "www.amazon.es", "www.amazon.com").</param>
/// <param name="PartnerTag">Partner tag / Store ID linked to this marketplace (e.g. "mystore-21").</param>
public sealed record AmazonMarketplaceTarget(string Marketplace, string PartnerTag);
