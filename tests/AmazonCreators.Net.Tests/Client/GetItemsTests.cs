using System.Text.Json;
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Models.Requests;
using AmazonCreators.Net.Services;
using AmazonCreators.Net.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace AmazonCreators.Net.Tests.Client;

public class GetItemsTests
{
    [Fact]
    public async Task GetProductPriceAsync_Returns_Cheapest_Listing_DisplayAmount()
    {
        var handler = new FakeHttpMessageHandler();
        var client = CreateClient(handler);

        var price = await client.GetProductPriceAsync("B08N5WRWNW");

        price.Should().Be("499,99 €");
    }

    [Fact]
    public async Task GetProductOffersV2Async_Returns_Listings_And_Sellers()
    {
        var handler = new FakeHttpMessageHandler();
        var client = CreateClient(handler);

        var offers = await client.GetProductOffersV2Async("B08N5WRWNW");

        offers.Should().NotBeNull();
        offers!.Listings.Should().HaveCount(2);

        var buyBoxListing = offers.Listings![0];
        buyBoxListing.IsBuyBoxWinner.Should().BeTrue();
        buyBoxListing.MerchantInfo!.Name.Should().Be("Amazon");
        buyBoxListing.Price!.Money!.Amount.Should().Be(499.99m);
        buyBoxListing.Price.SavingBasis!.Money!.Amount.Should().Be(549.99m);
    }

    [Fact]
    public async Task GetProductOffersAsync_With_Target_Overrides_Marketplace_And_PartnerTag()
    {
        var handler = new FakeHttpMessageHandler();
        var client = CreateClient(handler);

        var target = new AmazonMarketplaceTarget(AmazonMarketplace.Germany, "custom-germany-21");
        var response = await client.GetProductOffersAsync(["B08N5WRWNW"], target);

        response.Should().NotBeNull();
        response!.IsSuccess.Should().BeTrue();

        var apiCall = handler.CapturedCalls.First(c => c.Url.EndsWith("/getItems"));
        apiCall.MarketplaceHeader.Should().Be("www.amazon.de");

        using var doc = JsonDocument.Parse(apiCall.Body!);
        doc.RootElement.GetProperty("partnerTag").GetString().Should().Be("custom-germany-21");
        doc.RootElement.GetProperty("itemIds")[0].GetString().Should().Be("B08N5WRWNW");
    }

    [Fact]
    public async Task GetProductOffersAsync_Throws_On_Empty_Asins()
    {
        var handler = new FakeHttpMessageHandler();
        var client = CreateClient(handler);

        var act = () => client.GetProductOffersAsync(Array.Empty<string>());
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ResponseCache_Serves_Cached_Response_For_Single_ASIN()
    {
        var handler = new FakeHttpMessageHandler();
        var client = CreateClient(handler, enableCache: true);

        var res1 = await client.GetProductOffersAsync(["B08N5WRWNW"]);
        var res2 = await client.GetProductOffersAsync(["B08N5WRWNW"]);

        res1.Should().NotBeNull();
        res2.Should().NotBeNull();
        handler.GetItemsCallCount.Should().Be(1); // Second call should hit the in-memory cache
    }

    private static AmazonCreatorsClient CreateClient(FakeHttpMessageHandler handler, bool enableCache = false)
    {
        var httpClient = new HttpClient(handler);
        var options = new AmazonCreatorsOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            CredentialVersion = "3.2",
            Marketplace = AmazonMarketplace.Spain,
            PartnerTag = "testtag-21",
            EnableResponseCaching = enableCache,
            ResponseCacheDurationSeconds = 300
        };

        return new AmazonCreatorsClient(httpClient, options);
    }
}
