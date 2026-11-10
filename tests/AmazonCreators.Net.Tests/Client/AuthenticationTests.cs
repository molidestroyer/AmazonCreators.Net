using System.Text.Json;
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Services;
using AmazonCreators.Net.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace AmazonCreators.Net.Tests.Client;

public class AuthenticationTests
{
    [Fact]
    public async Task Authenticate_With_LwA_v3_Sends_Json_And_Standard_Bearer()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var options = new AmazonCreatorsOptions
        {
            ClientId = "amzn1.oa2-cs.v1.test-client-id",
            ClientSecret = "test-client-secret-123",
            CredentialVersion = "3.2",
            Marketplace = AmazonMarketplace.Spain,
            PartnerTag = "testtag-21"
        };

        var client = new AmazonCreatorsClient(httpClient, options);

        var price = await client.GetProductPriceAsync("B08N5WRWNW");

        price.Should().Be("499,99 €");
        handler.TokenCallCount.Should().Be(1);

        var tokenCall = handler.CapturedCalls.First(c => c.Url.Contains("/auth/o2/token") || c.Url.Contains("/token"));
        tokenCall.Body.Should().NotBeNull();
        using var tokenDoc = JsonDocument.Parse(tokenCall.Body!);
        tokenDoc.RootElement.GetProperty("grant_type").GetString().Should().Be("client_credentials");
        tokenDoc.RootElement.GetProperty("client_id").GetString().Should().Be("amzn1.oa2-cs.v1.test-client-id");
        tokenDoc.RootElement.GetProperty("scope").GetString().Should().Be("creatorsapi::default");

        var apiCall = handler.CapturedCalls.First(c => c.Url.EndsWith("/getItems"));
        apiCall.AuthorizationHeader.Should().Be("Bearer mock-valid-access-token-xyz");
        apiCall.MarketplaceHeader.Should().Be("www.amazon.es");
    }

    [Fact]
    public async Task Authenticate_With_Cognito_v2_Sends_FormUrlEncoded_And_Versioned_Bearer()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var options = new AmazonCreatorsOptions
        {
            ClientId = "cognito-client-id",
            ClientSecret = "cognito-secret-456",
            CredentialVersion = "2.2",
            Marketplace = AmazonMarketplace.Spain,
            PartnerTag = "testtag-21"
        };

        var client = new AmazonCreatorsClient(httpClient, options);

        var price = await client.GetProductPriceAsync("B08N5WRWNW");

        price.Should().Be("499,99 €");
        handler.TokenCallCount.Should().Be(1);

        var tokenCall = handler.CapturedCalls.First(c => c.Url.Contains("/oauth2/token"));
        tokenCall.Body.Should().Contain("grant_type=client_credentials");
        tokenCall.Body.Should().Contain("client_id=cognito-client-id");

        var apiCall = handler.CapturedCalls.First(c => c.Url.EndsWith("/getItems"));
        apiCall.AuthorizationHeader.Should().Be("Bearer mock-valid-access-token-xyz, Version 2.2");
    }

    [Fact]
    public async Task Token_Is_Cached_And_Reused_Across_Multiple_Calls()
    {
        var handler = new FakeHttpMessageHandler { MockTokenExpiresIn = 3600 };
        var httpClient = new HttpClient(handler);
        var options = new AmazonCreatorsOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-secret",
            CredentialVersion = "3.2",
            PartnerTag = "testtag-21",
            EnableResponseCaching = false // Test token caching specifically
        };

        var client = new AmazonCreatorsClient(httpClient, options);

        await client.GetProductPriceAsync("B08N5WRWNW");
        await client.GetProductPriceAsync("B09XYZ1234");
        await client.GetProductPriceAsync("B07ABC9876");

        handler.TokenCallCount.Should().Be(1);
        handler.GetItemsCallCount.Should().Be(3);
    }

    [Fact]
    public async Task Manual_RefreshToken_Forces_New_Token_Acquisition()
    {
        var handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var options = new AmazonCreatorsOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-secret",
            CredentialVersion = "3.2",
            PartnerTag = "testtag-21"
        };

        var client = new AmazonCreatorsClient(httpClient, options);

        await client.GetProductPriceAsync("B08N5WRWNW");
        handler.TokenCallCount.Should().Be(1);

        await client.RefreshTokenAsync();
        handler.TokenCallCount.Should().Be(2);

        var isAuthed = await client.IsAuthenticatedAsync();
        isAuthed.Should().BeTrue();
    }
}
