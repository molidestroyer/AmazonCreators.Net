using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Constants;
using AmazonCreators.Net.Extensions;
using AmazonCreators.Net.Models.Requests;
using AmazonCreators.Net.Services;

namespace AmazonCreators.Mcp;

[McpServerToolType]
public class AmazonCreatorsTools(IAmazonCreatorsClient client)
{
    private static string CleanAsin(string asin) => asin.Trim().ToUpperInvariant();

    [McpServerTool(Name = "amazon_get_price"), Description("Gets the best current price, saving basis, stock status, and Buy Box winner for an Amazon ASIN.")]
    public async Task<string> GetPrice(
        [Description("10-character alphanumeric Amazon ASIN (e.g. B08N5WRWNW)")] string asin,
        [Description("Amazon marketplace domain, e.g. 'www.amazon.es', 'www.amazon.com', 'www.amazon.de'. Defaults to configured marketplace")] string? marketplace = null,
        [Description("Affiliate partner tag, e.g. 'mystore-21'. Defaults to configured partner tag")] string? partnerTag = null,
        CancellationToken ct = default)
    {
        asin = CleanAsin(asin);
        if (asin.Length != 10 || !asin.All(char.IsLetterOrDigit))
        {
            return $"Invalid ASIN: '{asin}'. ASIN must be exactly 10 alphanumeric characters.";
        }

        try
        {
            var target = !string.IsNullOrWhiteSpace(marketplace) && !string.IsNullOrWhiteSpace(partnerTag)
                ? new AmazonMarketplaceTarget(marketplace, partnerTag)
                : null;

            var response = target != null
                ? await client.GetProductOffersAsync([asin], target, AmazonCreatorsResources.LightweightPrice, ct)
                : await client.GetProductOffersAsync([asin], ct);

            var item = response?.FirstItem;
            if (item == null)
            {
                return $"Product with ASIN '{asin}' was not found in Amazon catalog.";
            }

            var bestListing = item.GetPrimeBestListing() ?? item.OffersV2?.Listings?.FirstOrDefault();
            if (bestListing == null || bestListing.Price?.Money == null)
            {
                return $"ASIN {asin} ('{item.GetTitle() ?? "Unknown Title"}'): Out of stock or no active offers available.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"ASIN: {asin}");
            sb.AppendLine($"Title: {item.GetTitle() ?? "N/A"}");
            sb.AppendLine($"Price: {bestListing.Price.Money.DisplayAmount}");

            if (bestListing.Price.SavingBasis?.Money != null)
            {
                sb.AppendLine($"Reference Price: {bestListing.Price.SavingBasis.Money.DisplayAmount} ({bestListing.Price.SavingBasis.TypeLabel})");
            }

            if (bestListing.Price.Savings?.Percentage != null)
            {
                sb.AppendLine($"Discount: -{bestListing.Price.Savings.Percentage}%");
            }

            sb.AppendLine($"Status: {bestListing.Availability?.Message ?? bestListing.Availability?.Type ?? "Available"}");
            sb.AppendLine($"Seller: {bestListing.MerchantInfo?.Name ?? "Unknown"} (BuyBox: {bestListing.IsBuyBoxWinner})");
            sb.AppendLine($"URL: {item.DetailPageUrl ?? $"https://{marketplace ?? AmazonMarketplace.Spain}/dp/{asin}"}");

            return sb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            return $"Error fetching price for ASIN {asin}: {ex.Message}";
        }
    }

    [McpServerTool(Name = "amazon_get_product"), Description("Gets full product metadata including high-res image, all offers, brand, condition, and external identifiers (EAN/UPC).")]
    public async Task<string> GetProduct(
        [Description("10-character alphanumeric Amazon ASIN")] string asin,
        [Description("Amazon marketplace domain (e.g. www.amazon.es)")] string? marketplace = null,
        [Description("Affiliate partner tag")] string? partnerTag = null,
        CancellationToken ct = default)
    {
        asin = CleanAsin(asin);
        if (asin.Length != 10 || !asin.All(char.IsLetterOrDigit))
        {
            return $"Invalid ASIN: '{asin}'. ASIN must be 10 alphanumeric characters.";
        }

        try
        {
            var target = !string.IsNullOrWhiteSpace(marketplace) && !string.IsNullOrWhiteSpace(partnerTag)
                ? new AmazonMarketplaceTarget(marketplace, partnerTag)
                : null;

            var response = target != null
                ? await client.GetProductOffersAsync([asin], target, AmazonCreatorsResources.Default, ct)
                : await client.GetProductOffersAsync([asin], ct);

            var item = response?.FirstItem;
            if (item == null)
            {
                return $"Product with ASIN '{asin}' was not found in Amazon catalog.";
            }

            var json = JsonSerializer.Serialize(item, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            return json;
        }
        catch (Exception ex)
        {
            return $"Error fetching product details for ASIN {asin}: {ex.Message}";
        }
    }

    [McpServerTool(Name = "amazon_search"), Description("Searches products by keywords or EAN / barcode in the Amazon catalog.")]
    public async Task<string> Search(
        [Description("Search query or 13-digit EAN barcode")] string query,
        [Description("Amazon marketplace domain (e.g. www.amazon.es)")] string? marketplace = null,
        [Description("Affiliate partner tag")] string? partnerTag = null,
        [Description("Max results count (1-10)")] int limit = 5,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Search query cannot be empty.";
        }

        try
        {
            var target = !string.IsNullOrWhiteSpace(marketplace) && !string.IsNullOrWhiteSpace(partnerTag)
                ? new AmazonMarketplaceTarget(marketplace, partnerTag)
                : null;

            var response = target != null
                ? await client.SearchItemsAsync(query, target, limit, AmazonCreatorsResources.Default, ct)
                : await client.SearchItemsAsync(query, ct);

            var items = response?.Items;
            if (items == null || items.Count == 0)
            {
                return $"No products found for query '{query}'.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Search Results for '{query}' ({items.Count} items):");
            sb.AppendLine();

            foreach (var item in items)
            {
                var price = item.GetBestPrice()?.DisplayAmount ?? "N/A";
                sb.AppendLine($"- [{item.Asin}] {item.GetTitle() ?? "Untitled"}");
                sb.AppendLine($"  Price: {price} | URL: {item.DetailPageUrl}");
            }

            return sb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            return $"Error searching products for query '{query}': {ex.Message}";
        }
    }

    [McpServerTool(Name = "amazon_health_check"), Description("Validates OAuth 2.0 credentials and connectivity to Amazon Creators API.")]
    public async Task<string> HealthCheck(CancellationToken ct = default)
    {
        try
        {
            var isAuthed = await client.IsAuthenticatedAsync(ct);
            return isAuthed
                ? "Amazon Creators API: Authenticated successfully with valid OAuth token."
                : "Amazon Creators API: Authentication pending or failed. Check environment variables.";
        }
        catch (Exception ex)
        {
            return $"Amazon Creators API Health Check FAILED: {ex.Message}";
        }
    }
}
