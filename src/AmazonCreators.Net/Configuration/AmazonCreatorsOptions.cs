namespace AmazonCreators.Net.Configuration;

/// <summary>
/// Configuration options for the Amazon Creators API client.
/// Supports both OAuth 2.0 Login with Amazon (v3.x) and Cognito (v2.x).
/// </summary>
public class AmazonCreatorsOptions
{
    /// <summary>
    /// OAuth 2.0 Client ID obtained from Amazon Associates (Tools > Creators API).
    /// Required. For v3.x, typically starts with 'amzn1.oa2-cs.v1.'.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth 2.0 Client Secret obtained from Amazon Associates.
    /// Required.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Credential Version.
    /// Common versions:
    /// - "3.2" : Europe / Spain (Login with Amazon - LwA) [Default]
    /// - "3.1" : North America / USA (Login with Amazon - LwA)
    /// - "3.3" : Far East / Japan (Login with Amazon - LwA)
    /// - "2.2" : Legacy Cognito Europe
    /// - "2.1" : Legacy Cognito North America
    /// - "2.3" : Legacy Cognito Far East
    /// </summary>
    public string CredentialVersion { get; set; } = "3.2";

    /// <summary>
    /// Target Amazon Marketplace domain (sent as 'x-marketplace' header).
    /// Default: "www.amazon.es"
    /// </summary>
    public string Marketplace { get; set; } = AmazonMarketplace.Spain;

    /// <summary>
    /// Amazon Associates Partner Tag / Store ID linked to this credential (e.g. "mystore-21").
    /// Required by Amazon Creators API.
    /// </summary>
    public string PartnerTag { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the Creators API catalog service.
    /// Default: "https://creatorsapi.amazon/catalog/v1"
    /// </summary>
    public string ApiEndpoint { get; set; } = "https://creatorsapi.amazon/catalog/v1";

    /// <summary>
    /// OAuth 2.0 Token Endpoint. If empty or left default, it is automatically resolved based on <see cref="CredentialVersion"/>.
    /// </summary>
    public string? TokenEndpoint { get; set; }

    /// <summary>
    /// OAuth scope. If null, it is resolved based on <see cref="CredentialVersion"/>:
    /// "creatorsapi::default" for v3.x, "creatorsapi/default" for v2.x.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Timeout in seconds for HTTP requests. Default: 30 seconds.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts on 429 (Too Many Requests) or transient HTTP errors. Default: 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Initial backoff delay for retries in milliseconds. Default: 1000 ms.
    /// </summary>
    public int InitialRetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Whether in-memory response caching for single-ASIN queries is enabled. Default: true.
    /// </summary>
    public bool EnableResponseCaching { get; set; } = true;

    /// <summary>
    /// Duration in seconds for cached API responses. Default: 300 seconds (5 minutes).
    /// </summary>
    public int ResponseCacheDurationSeconds { get; set; } = 300;

    /// <summary>
    /// Preventive rate limit in requests per second. Default: 1.0 (token bucket).
    /// Amazon Creators API strictly throttles burst calls; a rate limiter prevents 429s.
    /// </summary>
    public double RequestsPerSecond { get; set; } = 1.0;

    /// <summary>
    /// Maximum burst capacity in the rate limiter token bucket. Default: 2.
    /// </summary>
    public int RateLimiterBurstCapacity { get; set; } = 2;

    /// <summary>
    /// Resolves the canonical token endpoint based on credential version.
    /// </summary>
    public string ResolveTokenEndpoint()
    {
        if (!string.IsNullOrWhiteSpace(TokenEndpoint) &&
            !(CredentialVersion.StartsWith("3.", StringComparison.Ordinal) && TokenEndpoint.Contains("amazoncognito.com", StringComparison.OrdinalIgnoreCase)))
        {
            return TokenEndpoint;
        }

        if (CredentialVersion.StartsWith("3.", StringComparison.Ordinal))
        {
            return CredentialVersion switch
            {
                "3.1" => "https://api.amazon.com/auth/o2/token",
                "3.3" => "https://api.amazon.co.jp/auth/o2/token",
                _ => "https://api.amazon.es/auth/o2/token" // 3.2 or default EU
            };
        }

        // v2.x Cognito
        return CredentialVersion switch
        {
            "2.1" => "https://creatorsapi.auth.us-east-1.amazoncognito.com/oauth2/token",
            "2.3" => "https://creatorsapi.auth.us-west-2.amazoncognito.com/oauth2/token",
            _ => "https://creatorsapi.auth.eu-south-2.amazoncognito.com/oauth2/token" // 2.2
        };
    }

    /// <summary>
    /// Resolves the OAuth scope according to version.
    /// </summary>
    public string ResolveScope()
    {
        if (!string.IsNullOrWhiteSpace(Scope))
        {
            // If user mistakenly had v2 scope "creatorsapi/default" on v3.x, fix it to "creatorsapi::default"
            if (CredentialVersion.StartsWith("3.", StringComparison.Ordinal) && Scope == "creatorsapi/default")
                return "creatorsapi::default";

            return Scope;
        }

        return CredentialVersion.StartsWith("3.", StringComparison.Ordinal)
            ? "creatorsapi::default"
            : "creatorsapi/default";
    }

    /// <summary>
    /// Formats the Authorization header value for an access token.
    /// </summary>
    public string BuildAuthorizationHeader(string accessToken) =>
        CredentialVersion.StartsWith("2.", StringComparison.Ordinal)
            ? $"Bearer {accessToken}, Version {CredentialVersion}"
            : $"Bearer {accessToken}";
}
