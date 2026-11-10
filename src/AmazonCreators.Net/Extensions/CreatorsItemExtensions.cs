using AmazonCreators.Net.Models.Common;
using AmazonCreators.Net.Models.Product;

namespace AmazonCreators.Net.Extensions;

/// <summary>
/// Facade and helper extension methods for the Creators API <see cref="Item"/> and <see cref="Listing"/> models.
/// Designed for easy migration from PA-API 5.0.
/// </summary>
public static class CreatorsItemExtensions
{
    private static readonly HashSet<string> AvailableTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "IN_STOCK",
        "IN_STOCK_SCARCE",
        "PREORDER",
        "BACKORDER",
        "AVAILABLE_DATE"
    };

    /// <summary>
    /// Checks whether an availability type indicates the item is purchasable.
    /// </summary>
    public static bool IsAvailable(this Availability? availability) =>
        availability != null && AvailableTypes.Contains(availability.Type);

    /// <summary>
    /// Extracts the regular / reference price from an item.
    /// Prioritizes SavingBasis, then falls back to maximum price amount.
    /// </summary>
    public static double GetRegularPrice(this Item item)
    {
        var listings = item?.OffersV2?.Listings;
        if (listings != null && listings.Length > 0)
        {
            var highestBasis = listings
                .Where(l => l.Price?.SavingBasis?.Money != null)
                .MaxBy(l => l.Price!.SavingBasis!.Money!.Amount);

            if (highestBasis?.Price?.SavingBasis?.Money != null)
            {
                return (double)highestBasis.Price.SavingBasis.Money.Amount;
            }

            var maxPrice = listings
                .Where(l => l.Price?.Money != null)
                .MaxBy(l => l.Price!.Money!.Amount);

            if (maxPrice?.Price?.Money != null)
            {
                return (double)maxPrice.Price.Money.Amount;
            }
        }

        throw new InvalidOperationException($"Regular price could not be determined for ASIN {item?.Asin ?? "unknown"}");
    }

    /// <summary>
    /// Extracts the best (lowest) available price from all listings.
    /// </summary>
    public static Money? GetBestPrice(this Item item)
    {
        var listings = item?.OffersV2?.Listings;
        if (listings != null && listings.Length > 0)
        {
            var bestListing = listings
                .Where(l => l.Price?.Money != null)
                .MinBy(l => l.Price!.Money!.Amount);

            return bestListing?.Price?.Money;
        }

        return null;
    }

    /// <summary>
    /// Extracts the best available listing for New condition items, prioritizing the Buy Box winner.
    /// </summary>
    public static Listing? GetPrimeBestListing(this Item? item)
    {
        if (item == null) return null;

        var listings = item.OffersV2?.Listings;
        if (listings == null || listings.Length == 0) return null;

        var newConditionListings = listings
            .Where(l =>
                l.Condition?.Value?.Equals("New", StringComparison.OrdinalIgnoreCase) == true &&
                l.Price?.Money != null &&
                (l.Availability == null || l.Availability.IsAvailable()))
            .ToList();

        if (newConditionListings.Count == 0) return null;

        // Buy Box winner first
        var buyBoxWinner = newConditionListings.FirstOrDefault(l => l.IsBuyBoxWinner);
        if (buyBoxWinner?.Price?.Money != null)
        {
            return buyBoxWinner;
        }

        // Lowest price new listing
        return newConditionListings.MinBy(l => l.Price!.Money!.Amount);
    }

    /// <summary>
    /// Returns the maximum savings percentage among listings.
    /// </summary>
    public static decimal? GetMaxSavingsPercentage(this Item item)
    {
        var listings = item?.OffersV2?.Listings;
        if (listings == null || listings.Length == 0) return null;

        return listings
            .Where(l => l.Price?.Savings?.Percentage != null)
            .Max(l => l.Price!.Savings!.Percentage);
    }

    /// <summary>
    /// Returns product title display value.
    /// </summary>
    public static string? GetTitle(this Item item) =>
        item?.ItemInfo?.Title?.DisplayValue;

    /// <summary>
    /// Returns brand name display value.
    /// </summary>
    public static string? GetBrand(this Item item) =>
        item?.ItemInfo?.ByLineInfo?.Brand?.DisplayValue;

    /// <summary>
    /// Returns primary high resolution image URL, or large/medium fallback.
    /// </summary>
    public static string? GetPrimaryImageUrl(this Item item)
    {
        var primary = item?.Images?.Primary;
        return primary?.HighRes?.Url
            ?? primary?.Large?.Url
            ?? primary?.Medium?.Url
            ?? primary?.Small?.Url;
    }
}
