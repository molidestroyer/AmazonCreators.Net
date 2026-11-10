using System.Net;
using System.Threading.RateLimiting;
using AmazonCreators.Net.Handlers;
using AmazonCreators.Net.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace AmazonCreators.Net.Tests.Client;

public class RateLimiterTests
{
    [Fact]
    public async Task RateLimitingHandler_Emits_429_When_Bucket_Exhausted_And_Queue_Full()
    {
        var innerHandler = new FakeHttpMessageHandler();
        var limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            AutoReplenishment = false,
            QueueLimit = 0
        });

        var rateLimitingHandler = new RateLimitingHandler(limiter, ownsLimiter: true)
        {
            InnerHandler = innerHandler
        };

        var invoker = new HttpMessageInvoker(rateLimitingHandler);

        // First call acquires the single available token
        var res1 = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://creatorsapi.amazon/catalog/v1/getItems"), CancellationToken.None);
        res1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Second call has no tokens and queue limit is 0 -> immediate 429
        var res2 = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://creatorsapi.amazon/catalog/v1/getItems"), CancellationToken.None);
        res2.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        res2.ReasonPhrase.Should().Contain("Rate limited locally");
    }

    [Fact]
    public async Task FakeHandler_Simulating_Throttling_Can_Be_Recovered()
    {
        var handler = new FakeHttpMessageHandler
        {
            ThrottleNextCallsCount = 1 // First call will 429, second will succeed
        };

        var invoker = new HttpMessageInvoker(handler);

        var res1 = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://creatorsapi.amazon/catalog/v1/getItems"), CancellationToken.None);
        res1.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var res2 = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://creatorsapi.amazon/catalog/v1/getItems"), CancellationToken.None);
        res2.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
