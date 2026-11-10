using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Extensions;
using AmazonCreators.Net.Services;
using AmazonCreators.Mcp;

// MCP server over stdio: stdout is strictly reserved for JSON-RPC MCP messages, logs go to stderr.
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Read configuration from environment variables or fallback placeholders
var options = new AmazonCreatorsOptions
{
    ClientId = Environment.GetEnvironmentVariable("AMAZON_CREATORS_CLIENT_ID") ?? string.Empty,
    ClientSecret = Environment.GetEnvironmentVariable("AMAZON_CREATORS_CLIENT_SECRET") ?? string.Empty,
    PartnerTag = Environment.GetEnvironmentVariable("AMAZON_CREATORS_PARTNER_TAG") ?? string.Empty,
    Marketplace = Environment.GetEnvironmentVariable("AMAZON_CREATORS_MARKETPLACE") ?? AmazonMarketplace.Spain,
    CredentialVersion = Environment.GetEnvironmentVariable("AMAZON_CREATORS_VERSION") ?? "3.2"
};

// Register AmazonCreatorsClient
if (!string.IsNullOrWhiteSpace(options.ClientId) && !string.IsNullOrWhiteSpace(options.ClientSecret))
{
    builder.Services.AddAmazonCreatorsClient(o =>
    {
        o.ClientId = options.ClientId;
        o.ClientSecret = options.ClientSecret;
        o.PartnerTag = options.PartnerTag;
        o.Marketplace = options.Marketplace;
        o.CredentialVersion = options.CredentialVersion;
    });
}
else
{
    // Register unconfigured placeholder client so MCP server still launches and reports missing config via health_check tool
    builder.Services.AddSingleton<IAmazonCreatorsClient>(new UnconfiguredCreatorsClient());
}

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
