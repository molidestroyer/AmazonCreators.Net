using System.Text.Json;
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Models.Requests;
using AmazonCreators.Net.Services;
using AmazonCreators.Net.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace AmazonCreators.Net.Tests.Client;

public class SearchItemsTests
{
    [Fact]
    public async Task SearchItemsAsync_Sends_Keywords_And_Parses_SearchResult()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var options = new AmazonCreatorsOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            CredentialVersion = "3.2",
            PartnerTag = "testtag-21"
        };

        var client = new AmazonCreatorsClient(httpClient, options);

        var result = await client.SearchItemsAsync("0711719541028");

        result.Should().NotBeNull();
        result!.SearchResult.Should().NotBeNull();
        result.SearchResult!.TotalResultCount.Should().Be(1);
        result.SearchResult.Items.Should().HaveCount(1);
        result.SearchResult.Items![0].Asin.Should().Be("B08N5WRWNW");

        var apiCall = handler.CapturedCalls.First(c => c.Url.EndsWith("/searchItems"));
        using var doc = JsonDocument.Parse(apiCall.Body!);
        doc.RootElement.GetProperty("keywords").GetString().Should().Be("0711719541028");
        doc.RootElement.GetProperty("partnerTag").GetString().Should().Be("testtag-21");
    }

    [Fact]
    public async Task SearchItemsAsync_Throws_On_Empty_Keywords()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var options = new AmazonCreatorsOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            CredentialVersion = "3.2",
            PartnerTag = "testtag-21"
        };

        var client = new AmazonCreatorsClient(httpClient, options);

        var act = () => client.SearchItemsAsync("");
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
