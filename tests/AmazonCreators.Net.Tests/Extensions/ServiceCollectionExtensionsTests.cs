using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Extensions;
using AmazonCreators.Net.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AmazonCreators.Net.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAmazonCreatorsClient_WithDirectParameters_RegistersClientAndResolvesSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAmazonCreatorsClient(
            clientId: "amzn1.oa2-cs.v1.my-client-id",
            clientSecret: "my-secret-key",
            partnerTag: "mystore-21",
            marketplace: AmazonMarketplace.Spain,
            credentialVersion: "3.2");

        using var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetService<IAmazonCreatorsClient>();
        client.Should().NotBeNull();
        client.Should().BeOfType<AmazonCreatorsClient>();

        var options = serviceProvider.GetService<AmazonCreatorsOptions>();
        options.Should().NotBeNull();
        options!.ClientId.Should().Be("amzn1.oa2-cs.v1.my-client-id");
        options.ClientSecret.Should().Be("my-secret-key");
        options.PartnerTag.Should().Be("mystore-21");
        options.Marketplace.Should().Be("www.amazon.es");
        options.CredentialVersion.Should().Be("3.2");
    }

    [Fact]
    public void AddAmazonCreatorsClient_WithAction_RegistersClientAndResolvesSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAmazonCreatorsClient(o =>
        {
            o.ClientId = "action-client-id";
            o.ClientSecret = "action-secret";
            o.PartnerTag = "actiontag-21";
            o.Marketplace = AmazonMarketplace.Germany;
            o.CredentialVersion = "3.2";
        });

        using var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetService<IAmazonCreatorsClient>();
        client.Should().NotBeNull();

        var options = serviceProvider.GetService<IOptions<AmazonCreatorsOptions>>()?.Value;
        options.Should().NotBeNull();
        options!.ClientId.Should().Be("action-client-id");
        options.Marketplace.Should().Be("www.amazon.de");
    }

    [Fact]
    public void AddAmazonCreatorsClient_WithConfiguration_BindsOptionsCorrectly()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["AmazonCreators:ClientId"] = "config-client-id",
            ["AmazonCreators:ClientSecret"] = "config-secret",
            ["AmazonCreators:PartnerTag"] = "configtag-21",
            ["AmazonCreators:Marketplace"] = "www.amazon.co.uk",
            ["AmazonCreators:CredentialVersion"] = "3.1"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAmazonCreatorsClient(configuration, "AmazonCreators");

        using var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetService<IAmazonCreatorsClient>();
        client.Should().NotBeNull();

        var options = serviceProvider.GetService<AmazonCreatorsOptions>();
        options.Should().NotBeNull();
        options!.ClientId.Should().Be("config-client-id");
        options.ClientSecret.Should().Be("config-secret");
        options.PartnerTag.Should().Be("configtag-21");
        options.Marketplace.Should().Be("www.amazon.co.uk");
        options.CredentialVersion.Should().Be("3.1");
    }

    [Fact]
    public void AddAmazonCreatorsClient_WithServiceProviderAction_ResolvesOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAmazonCreatorsClient((sp, o) =>
        {
            o.ClientId = "sp-client-id";
            o.ClientSecret = "sp-secret";
            o.PartnerTag = "sptag-21";
        });

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetService<AmazonCreatorsOptions>();
        options.Should().NotBeNull();
        options!.ClientId.Should().Be("sp-client-id");
    }

    [Theory]
    [InlineData("", "secret", "tag")]
    [InlineData("client", "", "tag")]
    [InlineData("client", "secret", "")]
    public void AddAmazonCreatorsClient_DirectParameters_ThrowsOnMissingRequiredFields(
        string clientId, string clientSecret, string partnerTag)
    {
        var services = new ServiceCollection();

        var act = () => services.AddAmazonCreatorsClient(clientId, clientSecret, partnerTag);
        act.Should().Throw<ArgumentException>();
    }
}
