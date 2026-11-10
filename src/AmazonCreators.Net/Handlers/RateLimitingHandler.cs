using System.Net;
using System.Threading.RateLimiting;

namespace AmazonCreators.Net.Handlers;

/// <summary>
/// Proactive client-side rate limiting delegating handler.
/// Regulates the frequency of requests to the Amazon Creators API to prevent HTTP 429 throttling.
/// </summary>
public sealed class RateLimitingHandler : DelegatingHandler
{
    private readonly RateLimiter _limiter;
    private readonly bool _ownsLimiter;

    public RateLimitingHandler(RateLimiter limiter, bool ownsLimiter = true)
    {
        _limiter = limiter ?? throw new ArgumentNullException(nameof(limiter));
        _ownsLimiter = ownsLimiter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var lease = await _limiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);

        if (!lease.IsAcquired)
        {
            return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                ReasonPhrase = "Rate limited locally by AmazonCreators.Net token bucket before reaching Amazon",
                RequestMessage = request
            };
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _ownsLimiter)
        {
            _limiter.Dispose();
        }
        base.Dispose(disposing);
    }
}
