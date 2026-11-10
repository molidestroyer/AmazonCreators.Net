using System.Text.Json.Serialization;

namespace AmazonCreators.Net.Models.Requests;

/// <summary>
/// Request payload for the /catalog/v1/searchItems endpoint.
/// </summary>
public class SearchItemsRequest
{
    [JsonPropertyName("keywords")]
    public string Keywords { get; set; } = string.Empty;

    [JsonPropertyName("partnerTag")]
    public string? PartnerTag { get; set; }

    [JsonPropertyName("searchIndex")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SearchIndex { get; set; } = "All";

    [JsonPropertyName("itemCount")]
    public int ItemCount { get; set; } = 10;

    [JsonPropertyName("resources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Resources { get; set; }
}
