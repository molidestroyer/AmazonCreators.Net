namespace AmazonCreators.Net.Constants;

/// <summary>
/// Predefined resource groups for Amazon Creators API requests.
/// </summary>
public static class AmazonCreatorsResources
{
    /// <summary>
    /// Core offers and pricing resources from OffersV2.
    /// </summary>
    public static readonly string[] OffersOnly =
    [
        "offersV2.listings.price",
        "offersV2.listings.availability",
        "offersV2.listings.condition",
        "offersV2.listings.merchantInfo",
        "offersV2.listings.type",
        "offersV2.listings.isBuyBoxWinner",
        "offersV2.listings.dealDetails",
        "offersV2.listings.loyaltyPoints"
    ];

    /// <summary>
    /// Product descriptive info resources.
    /// </summary>
    public static readonly string[] ItemInfoOnly =
    [
        "itemInfo.title",
        "itemInfo.byLineInfo",
        "itemInfo.classifications",
        "itemInfo.productInfo",
        "itemInfo.features",
        "itemInfo.externalIds",
        "itemInfo.contentInfo",
        "itemInfo.manufactureInfo",
        "itemInfo.technicalInfo",
        "itemInfo.tradeInInfo"
    ];

    /// <summary>
    /// Product images in various sizes.
    /// </summary>
    public static readonly string[] ImagesOnly =
    [
        "images.primary.small",
        "images.primary.medium",
        "images.primary.large",
        "images.primary.highRes",
        "images.variants.small",
        "images.variants.medium",
        "images.variants.large",
        "images.variants.highRes"
    ];

    /// <summary>
    /// Customer reviews and rating resources.
    /// </summary>
    public static readonly string[] CustomerReviewsOnly =
    [
        "customerReviews.starRating",
        "customerReviews.count"
    ];

    /// <summary>
    /// Category / browse nodes and sales ranks.
    /// </summary>
    public static readonly string[] BrowseNodeInfoOnly =
    [
        "browseNodeInfo.browseNodes",
        "browseNodeInfo.browseNodes.ancestor",
        "browseNodeInfo.websiteSalesRank",
        "browseNodeInfo.browseNodes.salesRank"
    ];

    /// <summary>
    /// Fast and lightweight resources optimized for price monitoring / multi-market checkers.
    /// </summary>
    public static readonly string[] LightweightPrice =
    [
        "offersV2.listings.price",
        "offersV2.listings.availability",
        "offersV2.listings.condition",
        "offersV2.listings.merchantInfo",
        "offersV2.listings.isBuyBoxWinner",
        "itemInfo.title",
        "itemInfo.features"
    ];

    /// <summary>
    /// Comprehensive default resources: OffersV2, ItemInfo, Images, CustomerReviews, and BrowseNodeInfo.
    /// </summary>
    public static readonly string[] Default =
    [
        // OffersV2
        "offersV2.listings.price",
        "offersV2.listings.availability",
        "offersV2.listings.condition",
        "offersV2.listings.merchantInfo",
        "offersV2.listings.type",
        "offersV2.listings.isBuyBoxWinner",
        "offersV2.listings.dealDetails",
        "offersV2.listings.loyaltyPoints",

        // ItemInfo
        "itemInfo.title",
        "itemInfo.byLineInfo",
        "itemInfo.classifications",
        "itemInfo.productInfo",
        "itemInfo.features",
        "itemInfo.externalIds",
        "itemInfo.contentInfo",
        "itemInfo.manufactureInfo",
        "itemInfo.technicalInfo",
        "itemInfo.tradeInInfo",

        // Images
        "images.primary.small",
        "images.primary.medium",
        "images.primary.large",
        "images.primary.highRes",
        "images.variants.small",
        "images.variants.medium",
        "images.variants.large",
        "images.variants.highRes",

        // Reviews
        "customerReviews.starRating",
        "customerReviews.count",

        // Browse Nodes
        "browseNodeInfo.browseNodes",
        "browseNodeInfo.browseNodes.ancestor",
        "browseNodeInfo.websiteSalesRank",
        "browseNodeInfo.browseNodes.salesRank"
    ];
}
