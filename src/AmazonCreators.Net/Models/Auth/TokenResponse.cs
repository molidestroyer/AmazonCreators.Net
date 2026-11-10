using System.Text.Json.Serialization;

namespace AmazonCreators.Net.Models.Auth;

/// <summary>
/// OAuth 2.0 Token Response from Amazon authorization server (LwA or Cognito).
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
