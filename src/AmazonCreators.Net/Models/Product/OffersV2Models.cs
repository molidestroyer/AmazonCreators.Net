using System.Text.Json.Serialization;
using AmazonCreators.Net.Models.Common;

namespace AmazonCreators.Net.Models.Product;

/// <summary>
/// Savings information (amount and percentage).
/// </summary>
public class SavingsInfo
{
    [JsonPropertyName("money")]
    public Money? Money { get; set; }

    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; set; }
}

/// <summary>
/// Reference price or previous price basis.
/// </summary>
public class SavingBasis
{
    [JsonPropertyName("money")]
    public Money? Money { get; set; }

    [JsonPropertyName("savingBasisType")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("savingBasisTypeLabel")]
    public string TypeLabel { get; set; } = string.Empty;
}

/// <summary>
/// Price details within a listing.
/// </summary>
public class Price
{
    [JsonPropertyName("money")]
    public Money? Money { get; set; }

    [JsonPropertyName("pricePerUnit")]
    public Money? PricePerUnit { get; set; }

    [JsonPropertyName("savingBasis")]
    public SavingBasis? SavingBasis { get; set; }

    [JsonPropertyName("savings")]
    public SavingsInfo? Savings { get; set; }
}

/// <summary>
/// Stock and availability information.
/// </summary>
public class Availability
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("minOrderQuantity")]
    public int? MinOrderQuantity { get; set; }

    [JsonPropertyName("maxOrderQuantity")]
    public int? MaxOrderQuantity { get; set; }
}

/// <summary>
/// Item condition (New, Used, Refurbished, etc.).
/// </summary>
public class Condition
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = "New";

    [JsonPropertyName("subCondition")]
    public string SubCondition { get; set; } = string.Empty;

    [JsonPropertyName("conditionNote")]
    public string ConditionNote { get; set; } = string.Empty;
}

/// <summary>
/// Merchant and seller identification.
/// </summary>
public class MerchantInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Special promotion or lightning deal details.
/// </summary>
public class DealDetails
{
    [JsonPropertyName("accessType")]
    public string AccessType { get; set; } = string.Empty;

    [JsonPropertyName("badge")]
    public string? Badge { get; set; }

    [JsonPropertyName("earlyAccessDurationInMilliseconds")]
    public long? EarlyAccessDurationInMilliseconds { get; set; }

    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("percentClaimed")]
    public decimal? PercentClaimed { get; set; }
}

public class OfferLoyaltyPoints
{
    [JsonPropertyName("points")]
    public long Points { get; set; }
}

/// <summary>
/// Individual offer/listing in OffersV2.
/// </summary>
public class Listing
{
    [JsonPropertyName("price")]
    public Price? Price { get; set; }

    [JsonPropertyName("availability")]
    public Availability? Availability { get; set; }

    [JsonPropertyName("condition")]
    public Condition? Condition { get; set; }

    [JsonPropertyName("merchantInfo")]
    public MerchantInfo? MerchantInfo { get; set; }

    [JsonPropertyName("dealDetails")]
    public DealDetails? DealDetails { get; set; }

    [JsonPropertyName("loyaltyPoints")]
    public OfferLoyaltyPoints? LoyaltyPoints { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("isBuyBoxWinner")]
    public bool IsBuyBoxWinner { get; set; }

    [JsonPropertyName("violatesMAP")]
    public bool ViolatesMAP { get; set; }
}

/// <summary>
/// Root OffersV2 structure containing all listings for an ASIN.
/// </summary>
public class OffersV2
{
    [JsonPropertyName("listings")]
    public Listing[]? Listings { get; set; }
}
