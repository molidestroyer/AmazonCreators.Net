using System.Text.Json.Serialization;
using AmazonCreators.Net.Models.Common;

namespace AmazonCreators.Net.Models.Product;

public class BrowseNodeAncestor
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("ancestor")]
    public BrowseNodeAncestor? Ancestor { get; set; }
}

public class BrowseNodeChild
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}

public class SalesRank
{
    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;

    [JsonPropertyName("salesRank")]
    public long? Value { get; set; }
}

public class BrowseNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("contextFreeName")]
    public string? ContextFreeName { get; set; }

    [JsonPropertyName("isRoot")]
    public bool IsRoot { get; set; }

    [JsonPropertyName("ancestor")]
    public BrowseNodeAncestor? Ancestor { get; set; }

    [JsonPropertyName("children")]
    public BrowseNodeChild[]? Children { get; set; }

    [JsonPropertyName("salesRank")]
    public int? SalesRank { get; set; }
}

public class BrowseNodeInfo
{
    [JsonPropertyName("browseNodes")]
    public BrowseNode[]? BrowseNodes { get; set; }

    [JsonPropertyName("websiteSalesRank")]
    public SalesRank? WebsiteSalesRank { get; set; }
}

public class VariationAttribute
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class TextInfo
{
    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;

    [JsonPropertyName("locale")]
    public string Locale { get; set; } = string.Empty;
}

public class ProductInfo
{
    [JsonPropertyName("title")]
    public TextInfo? Title { get; set; }

    [JsonPropertyName("byLineInfo")]
    public ByLineInfo? ByLineInfo { get; set; }

    [JsonPropertyName("classifications")]
    public Classifications? Classifications { get; set; }

    [JsonPropertyName("externalIds")]
    public ExternalIds? ExternalIds { get; set; }

    [JsonPropertyName("features")]
    public Features? Features { get; set; }

    [JsonPropertyName("productInfo")]
    public ProductInfoExpanded? ProductInfoDetails { get; set; }

    [JsonPropertyName("contentInfo")]
    public ContentInfo? ContentInfo { get; set; }
}

/// <summary>
/// Individual product item representation from Creators API.
/// </summary>
public class Item
{
    [JsonPropertyName("asin")]
    public string Asin { get; set; } = string.Empty;

    [JsonPropertyName("detailPageURL")]
    public string? DetailPageUrl { get; set; }

    [JsonPropertyName("offersV2")]
    public OffersV2? OffersV2 { get; set; }

    [JsonPropertyName("itemInfo")]
    public ProductInfo? ItemInfo { get; set; }

    [JsonPropertyName("images")]
    public Images? Images { get; set; }

    [JsonPropertyName("customerReviews")]
    public CustomerReviews? CustomerReviews { get; set; }

    [JsonPropertyName("browseNodeInfo")]
    public BrowseNodeInfo? BrowseNodeInfo { get; set; }

    [JsonPropertyName("parentASIN")]
    public string? ParentAsin { get; set; }

    [JsonPropertyName("variationAttributes")]
    public VariationAttribute[]? VariationAttributes { get; set; }
}
