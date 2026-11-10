using System.Text.Json.Serialization;

namespace AmazonCreators.Net.Models.Common;

public class ImageVariant
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }
}

public class PrimaryImage
{
    [JsonPropertyName("small")]
    public ImageVariant? Small { get; set; }

    [JsonPropertyName("medium")]
    public ImageVariant? Medium { get; set; }

    [JsonPropertyName("large")]
    public ImageVariant? Large { get; set; }

    [JsonPropertyName("highRes")]
    public ImageVariant? HighRes { get; set; }
}

public class Images
{
    [JsonPropertyName("primary")]
    public PrimaryImage? Primary { get; set; }

    [JsonPropertyName("variants")]
    public PrimaryImage[]? Variants { get; set; }
}
