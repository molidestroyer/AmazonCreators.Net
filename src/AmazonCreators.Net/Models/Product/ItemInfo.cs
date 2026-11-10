using System.Text.Json.Serialization;

namespace AmazonCreators.Net.Models.Product;

public class StringValuedAttribute
{
    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }
}

public class IntegerValuedAttribute
{
    [JsonPropertyName("displayValue")]
    public long? DisplayValue { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }
}

public class BooleanValuedAttribute
{
    [JsonPropertyName("displayValue")]
    public bool? DisplayValue { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }
}

public class UnitBasedAttribute
{
    [JsonPropertyName("displayValue")]
    public decimal? DisplayValue { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}

public class DimensionBasedAttribute
{
    [JsonPropertyName("height")]
    public UnitBasedAttribute? Height { get; set; }

    [JsonPropertyName("length")]
    public UnitBasedAttribute? Length { get; set; }

    [JsonPropertyName("width")]
    public UnitBasedAttribute? Width { get; set; }

    [JsonPropertyName("weight")]
    public UnitBasedAttribute? Weight { get; set; }
}

public class Language
{
    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;

    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; set; }
}

public class ByLineInfo
{
    [JsonPropertyName("manufacturer")]
    public StringValuedAttribute? Manufacturer { get; set; }

    [JsonPropertyName("brand")]
    public StringValuedAttribute? Brand { get; set; }

    [JsonPropertyName("languages")]
    public Language[]? Languages { get; set; }
}

public class Classifications
{
    [JsonPropertyName("productGroup")]
    public StringValuedAttribute? ProductGroup { get; set; }

    [JsonPropertyName("productType")]
    public StringValuedAttribute? ProductType { get; set; }

    [JsonPropertyName("productTypeHierarchy")]
    public string[]? ProductTypeHierarchy { get; set; }
}

public class Features
{
    [JsonPropertyName("displayValues")]
    public string[]? DisplayValues { get; set; }
}

public class ProductInfoExpanded
{
    [JsonPropertyName("color")]
    public StringValuedAttribute? Color { get; set; }

    [JsonPropertyName("size")]
    public StringValuedAttribute? Size { get; set; }

    [JsonPropertyName("unitCount")]
    public IntegerValuedAttribute? UnitCount { get; set; }

    [JsonPropertyName("releaseDate")]
    public StringValuedAttribute? ReleaseDate { get; set; }

    [JsonPropertyName("isAdultProduct")]
    public BooleanValuedAttribute? IsAdultProduct { get; set; }

    [JsonPropertyName("itemDimensions")]
    public DimensionBasedAttribute? ItemDimensions { get; set; }
}

public class ContentInfo
{
    [JsonPropertyName("edition")]
    public StringValuedAttribute? Edition { get; set; }

    [JsonPropertyName("numberOfPages")]
    public IntegerValuedAttribute? NumberOfPages { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}

public class Rating
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

public class CustomerReviews
{
    [JsonPropertyName("count")]
    public long Count { get; set; }

    [JsonPropertyName("starRating")]
    public Rating? StarRating { get; set; }
}
