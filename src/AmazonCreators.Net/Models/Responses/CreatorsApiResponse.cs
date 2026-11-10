using System.Text.Json.Serialization;
using AmazonCreators.Net.Models.Product;

namespace AmazonCreators.Net.Models.Responses;

public class ErrorMessage
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string? Details { get; set; }
}

public class ItemsResult
{
    [JsonPropertyName("items")]
    public Item[]? Items { get; set; }
}

public class SearchResult
{
    [JsonPropertyName("items")]
    public Item[]? Items { get; set; }

    [JsonPropertyName("searchURL")]
    public string? SearchUrl { get; set; }

    [JsonPropertyName("totalResultCount")]
    public int TotalResultCount { get; set; }
}

/// <summary>
/// Top-level response returned by Amazon Creators API endpoints (/getItems and /searchItems).
/// </summary>
public class CreatorsApiResponse
{
    [JsonPropertyName("itemsResult")]
    public ItemsResult? ItemsResult { get; set; }

    [JsonPropertyName("searchResult")]
    public SearchResult? SearchResult { get; set; }

    [JsonPropertyName("errors")]
    public ErrorMessage[]? Errors { get; set; }

    /// <summary>
    /// Gets all items returned either by /getItems or /searchItems.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Item> Items =>
        ItemsResult?.Items ?? SearchResult?.Items ?? Array.Empty<Item>();

    /// <summary>
    /// Returns the first item if any, otherwise null.
    /// </summary>
    [JsonIgnore]
    public Item? FirstItem => Items.Count > 0 ? Items[0] : null;

    /// <summary>
    /// True if there are no top-level errors and at least one item was found.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess =>
        (Errors == null || Errors.Length == 0) && Items.Count > 0;
}
