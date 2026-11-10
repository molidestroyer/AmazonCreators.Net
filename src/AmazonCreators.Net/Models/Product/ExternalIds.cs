using System.Text.Json.Serialization;

namespace AmazonCreators.Net.Models.Product;

public class ExternalIdList
{
    [JsonPropertyName("displayValues")]
    public string[]? DisplayValues { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }
}

/// <summary>
/// External standard identifiers for an item (EAN, UPC, ISBN, ASIN, GTIN).
/// </summary>
public class ExternalIds
{
    [JsonPropertyName("EANs")]
    public ExternalIdList? EANs { get; set; }

    [JsonPropertyName("ISBNs")]
    public ExternalIdList? ISBNs { get; set; }

    [JsonPropertyName("UPCs")]
    public ExternalIdList? UPCs { get; set; }

    [JsonPropertyName("ASIN")]
    public ExternalIdList? ASINs { get; set; }

    [JsonPropertyName("JANs")]
    public ExternalIdList? JANs { get; set; }

    [JsonPropertyName("GTINs")]
    public ExternalIdList? GTINs { get; set; }
}
