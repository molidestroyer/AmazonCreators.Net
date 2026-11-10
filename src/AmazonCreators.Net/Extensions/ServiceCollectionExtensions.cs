using System.Net;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Handlers;
using AmazonCreators.Net.Services;

namespace AmazonCreators.Net.Extensions;

/// <summary>
/// Developer-friendly Dependency Injection extensions for registering <see cref="IAmazonCreatorsClient"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly RateLimiter SharedRateLimiter = new TokenBucketRateLimiter(
        new TokenBucketRateLimiterOptions
        {
            TokenLimit = 2,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            AutoReplenishment = true,
            QueueLimit = 100,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });

    /// <summary>
    /// Registers the <see cref="IAmazonCreatorsClient"/> directly with credentials and partner tag.
    /// Simplest and quickest registration method.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="clientId">OAuth 2.0 Client ID.</param>
    /// <param name="clientSecret">OAuth 2.0 Client Secret.</param>
    /// <param name="partnerTag">Affiliate Partner Tag / Store ID.</param>
    /// <param name="marketplace">Target marketplace domain (e.g. AmazonMarketplace.Spain). Defaults to "www.amazon.es".</param>
    /// <param name="credentialVersion">Credential version (e.g. "3.2" for Login with Amazon EU, "3.1" for US, "2.2" for Cognito).</param>
    public static IHttpClientBuilder AddAmazonCreatorsClient(
        this IServiceCollection services,
        string clientId,
        string clientSecret,
        string partnerTag,
        string marketplace = AmazonMarketplace.Spain,
        string credentialVersion = "3.2")
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("ClientId cannot be null or empty", nameof(clientId));
        if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentException("ClientSecret cannot be null or empty", nameof(clientSecret));
        if (string.IsNullOrWhiteSpace(partnerTag)) throw new ArgumentException("PartnerTag cannot be null or empty", nameof(partnerTag));

        return services.AddAmazonCreatorsClient(options =>
        {
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.PartnerTag = partnerTag;
            options.Marketplace = marketplace;
            options.CredentialVersion = credentialVersion;
        });
    }

    /// <summary>
    /// Registers the <see cref="IAmazonCreatorsClient"/> binding configuration from the specified configuration section (default: "AmazonCreators").
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Root configuration instance.</param>
    /// <param name="sectionName">Configuration section name. Defaults to "AmazonCreators".</param>
    public static IHttpClientBuilder AddAmazonCreatorsClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "AmazonCreators")
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var section = configuration.GetSection(sectionName);
        return services.AddAmazonCreatorsClient(section);
    }

    /// <summary>
    /// Registers the <see cref="IAmazonCreatorsClient"/> binding configuration from an <see cref="IConfigurationSection"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configSection">Configuration section containing AmazonCreators settings.</param>
    public static IHttpClientBuilder AddAmazonCreatorsClient(
        this IServiceCollection services,
        IConfigurationSection configSection)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configSection == null) throw new ArgumentNullException(nameof(configSection));

        services.Configure<AmazonCreatorsOptions>(configSection);
        return RegisterClient(services);
    }

    /// <summary>
    /// Registers the <see cref="IAmazonCreatorsClient"/> using a configuration action delegate.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Delegate to configure <see cref="AmazonCreatorsOptions"/>.</param>
    public static IHttpClientBuilder AddAmazonCreatorsClient(
        this IServiceCollection services,
        Action<AmazonCreatorsOptions> configureOptions)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        services.Configure(configureOptions);
        return RegisterClient(services);
    }

    /// <summary>
    /// Registers the <see cref="IAmazonCreatorsClient"/> using a configuration delegate that receives the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Delegate to configure <see cref="AmazonCreatorsOptions"/> using service provider.</param>
    public static IHttpClientBuilder AddAmazonCreatorsClient(
        this IServiceCollection services,
        Action<IServiceProvider, AmazonCreatorsOptions> configureOptions)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        services.AddTransient<IConfigureOptions<AmazonCreatorsOptions>>(sp =>
            new ConfigureOptions<AmazonCreatorsOptions>(opts => configureOptions(sp, opts)));
        return RegisterClient(services);
    }

    private static IHttpClientBuilder RegisterClient(IServiceCollection services)
    {
        // Provide singleton resolution of AmazonCreatorsOptions from IOptions<AmazonCreatorsOptions> for backward compatibility
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AmazonCreatorsOptions>>().Value);

        return services.AddHttpClient<IAmazonCreatorsClient, AmazonCreatorsClient>()
            .AddPolicyHandler((sp, _) =>
            {
                var options = sp.GetRequiredService<IOptions<AmazonCreatorsOptions>>().Value;
                var logger = sp.GetService<ILogger<AmazonCreatorsClient>>();
                return GetRetryPolicy(options, logger);
            })
            .AddPolicyHandler((sp, _) =>
            {
                var options = sp.GetRequiredService<IOptions<AmazonCreatorsOptions>>().Value;
                var logger = sp.GetService<ILogger<AmazonCreatorsClient>>();
                return GetCircuitBreakerPolicy(options, logger);
            })
            .AddHttpMessageHandler(() => new RateLimitingHandler(SharedRateLimiter, ownsLimiter: false));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
        AmazonCreatorsOptions options,
        ILogger? logger)
    {
        var jitter = new Random();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<HttpRequestException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: options.MaxRetryAttempts,
                sleepDurationProvider: retryAttempt =>
                {
                    var exponentialBackoff = TimeSpan.FromMilliseconds(
                        options.InitialRetryDelayMilliseconds * Math.Pow(2, retryAttempt - 1));

                    var jitterMs = jitter.Next(0, Math.Max(1, (int)(exponentialBackoff.TotalMilliseconds * 0.15)));
                    return TimeSpan.FromMilliseconds(exponentialBackoff.TotalMilliseconds + jitterMs);
                },
                onRetry: (outcome, timespan, retryCount, _) =>
                {
                    logger?.LogWarning(
                        "AmazonCreatorsClient retry {RetryCount}/{MaxRetries} after {DelayMs}ms. Status: {StatusCode}",
                        retryCount,
                        options.MaxRetryAttempts,
                        timespan.TotalMilliseconds,
                        outcome.Result?.StatusCode);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        AmazonCreatorsOptions options,
        ILogger? logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    logger?.LogError(
                        "AmazonCreatorsClient circuit breaker tripped open for {BreakDurationSeconds}s. Last error: {StatusCode}",
                        timespan.TotalSeconds,
                        outcome.Result?.StatusCode);
                },
                onReset: () =>
                {
                    logger?.LogInformation("AmazonCreatorsClient circuit breaker reset. Traffic resumed.");
                });
    }
}
