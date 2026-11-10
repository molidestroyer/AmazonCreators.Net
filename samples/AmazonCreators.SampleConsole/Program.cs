using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Extensions;
using AmazonCreators.Net.Services;

Console.WriteLine("=================================================");
Console.WriteLine("  AmazonCreators.Net - Live API Verification");
Console.WriteLine("=================================================");

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.example.json", optional: true)
    .AddUserSecrets("chollolocura-amazon-console")
    .AddEnvironmentVariables()
    .Build();

var clientId = config["AmazonCreatorsApi:ClientId"] ?? config["AmazonCreators:ClientId"];
var clientSecret = config["AmazonCreatorsApi:ClientSecret"] ?? config["AmazonCreators:ClientSecret"];
var partnerTag = config["AmazonCreatorsApi:PartnerTag"] ?? config["AmazonCreators:PartnerTag"] ?? "yourstore-21";
var marketplace = config["AmazonCreatorsApi:Marketplace"] ?? config["AmazonCreators:Marketplace"] ?? AmazonMarketplace.Spain;
var version = config["AmazonCreatorsApi:CredentialVersion"] ?? config["AmazonCreators:CredentialVersion"] ?? "3.2";

if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
{
    Console.WriteLine("[WARN] No real credentials found in UserSecrets or environment variables.");
    Console.WriteLine("To test live, configure secrets via dotnet user-secrets or environment.");
    return;
}

Console.WriteLine($"[INFO] Credentials detected from UserSecrets.");
Console.WriteLine($"[INFO] ClientId: {clientId[..Math.Min(15, clientId.Length)]}...");
Console.WriteLine($"[INFO] Target Marketplace: {marketplace}");
Console.WriteLine($"[INFO] Credential Version: {version}");
Console.WriteLine($"[INFO] Partner Tag: {partnerTag}");
Console.WriteLine();

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

// Test DI registration with direct parameters
services.AddAmazonCreatorsClient(
    clientId: clientId,
    clientSecret: clientSecret,
    partnerTag: partnerTag,
    marketplace: marketplace,
    credentialVersion: version);

using var sp = services.BuildServiceProvider();
var client = sp.GetRequiredService<IAmazonCreatorsClient>();

try
{
    Console.WriteLine("1. Testing OAuth 2.0 Token Acquisition (LwA v3.2)...");
    var isAuthed = await client.IsAuthenticatedAsync();
    Console.WriteLine($"   OAuth Authenticated: {(isAuthed ? "SUCCESS [OK]" : "FAILED")}");
    Console.WriteLine();

    const string testAsin = "B0CQMF3VJK"; // PS5 Slim Console active ASIN
    Console.WriteLine($"2. Fetching Live OffersV2 for ASIN: {testAsin}...");
    var response = await client.GetProductOffersAsync([testAsin]);
    var item = response?.FirstItem;

    if (item != null)
    {
        Console.WriteLine($"   Title: {item.GetTitle()}");
        Console.WriteLine($"   Brand: {item.GetBrand()}");
        Console.WriteLine($"   Best Price: {item.GetBestPrice()?.DisplayAmount ?? "N/A"}");

        var regularPrice = 0.0;
        try { regularPrice = item.GetRegularPrice(); } catch { }
        if (regularPrice > 0)
        {
            Console.WriteLine($"   Regular/Reference Price: {regularPrice:F2} €");
        }

        var primeListing = item.GetPrimeBestListing();
        if (primeListing != null)
        {
            Console.WriteLine($"   Prime Best Offer: {primeListing.Price?.Money?.DisplayAmount} (Seller: {primeListing.MerchantInfo?.Name}, BuyBox: {primeListing.IsBuyBoxWinner})");
            Console.WriteLine($"   Availability: {primeListing.Availability?.Message ?? primeListing.Availability?.Type}");
        }

        Console.WriteLine($"   Image URL: {item.GetPrimaryImageUrl()}");
        Console.WriteLine($"   Detail URL: {item.DetailPageUrl}");
    }
    else
    {
        Console.WriteLine($"   [WARN] Product returned no items or offers.");
    }
    Console.WriteLine();

    Console.WriteLine("3. Testing SearchItemsAsync by Keyword ('PlayStation 5')...");
    var searchResponse = await client.SearchItemsAsync("PlayStation 5");
    var searchItems = searchResponse?.Items;
    Console.WriteLine($"   Items found: {searchItems?.Count ?? 0}");
    if (searchItems != null)
    {
        foreach (var s in searchItems.Take(3))
        {
            Console.WriteLine($"   - [{s.Asin}] {s.GetTitle()} ({s.DetailPageUrl})");
        }
    }

    Console.WriteLine();
    Console.WriteLine("=================================================");
    Console.WriteLine("  LIVE VERIFICATION COMPLETED SUCCESSFULLY!");
    Console.WriteLine("=================================================");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Live test failed: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"        Inner: {ex.InnerException.Message}");
    }
}
