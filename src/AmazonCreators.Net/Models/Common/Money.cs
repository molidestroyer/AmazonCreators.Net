using System.Text.Json.Serialization;

namespace AmazonCreators.Net.Models.Common;

/// <summary>
/// Monetary representation in Amazon Creators API.
/// </summary>
public class Money
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("displayAmount")]
    public string DisplayAmount { get; set; } = string.Empty;
}
