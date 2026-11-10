using System.Text.Json.Serialization;

namespace AmazonCreators.Net.Models.Requests;

/// <summary>
/// Request payload for the /catalog/v1/getItems endpoint.
/// </summary>
public class GetItemsRequest
{
    [JsonPropertyName("itemIds")]
    public string[] ItemIds { get; set; } = Array.Empty<string>();

    [JsonPropertyName("partnerTag")]
    public string? PartnerTag { get; set; }

    [JsonPropertyName("condition")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Condition { get; set; }

    [JsonPropertyName("resources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Resources { get; set; }
}
