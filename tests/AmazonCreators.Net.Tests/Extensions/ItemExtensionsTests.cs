using System.Text.Json;
using AmazonCreators.Net.Extensions;
using AmazonCreators.Net.Models.Responses;
using AmazonCreators.Net.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace AmazonCreators.Net.Tests.Extensions;

public class ItemExtensionsTests
{
    private readonly CreatorsApiResponse _response;

    public ItemExtensionsTests()
    {
        _response = JsonSerializer.Deserialize<CreatorsApiResponse>(
            MockCreatorsPayloads.StandardGetItemsResponse,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    [Fact]
    public void GetBestPrice_Returns_Lowest_Price()
    {
        var item = _response.FirstItem!;
        var bestPrice = item.GetBestPrice();

        bestPrice.Should().NotBeNull();
        bestPrice!.Amount.Should().Be(499.99m);
        bestPrice.Currency.Should().Be("EUR");
        bestPrice.DisplayAmount.Should().Be("499,99 €");
    }

    [Fact]
    public void GetRegularPrice_Returns_SavingBasis_If_Available()
    {
        var item = _response.FirstItem!;
        var regularPrice = item.GetRegularPrice();

        regularPrice.Should().Be(549.99);
    }

    [Fact]
    public void GetPrimeBestListing_Returns_BuyBoxWinner_New_Listing()
    {
        var item = _response.FirstItem!;
        var primeListing = item.GetPrimeBestListing();

        primeListing.Should().NotBeNull();
        primeListing!.IsBuyBoxWinner.Should().BeTrue();
        primeListing.MerchantInfo!.Name.Should().Be("Amazon");
        primeListing.Condition!.Value.Should().Be("New");
    }

    [Fact]
    public void GetMaxSavingsPercentage_Returns_Calculated_Savings()
    {
        var item = _response.FirstItem!;
        var savingsPct = item.GetMaxSavingsPercentage();

        savingsPct.Should().Be(9.09m);
    }

    [Fact]
    public void GetTitle_And_Brand_Extract_String_Values()
    {
        var item = _response.FirstItem!;

        item.GetTitle().Should().Be("Sony PlayStation 5 Console Standard Edition");
        item.GetBrand().Should().Be("Sony Interactive Entertainment");
    }

    [Fact]
    public void GetPrimaryImageUrl_Returns_HighRes_Url()
    {
        var item = _response.FirstItem!;

        item.GetPrimaryImageUrl().Should().Be("https://m.media-amazon.com/images/I/71v1Q6kX5PL._AC_SL2000_.jpg");
    }
}
