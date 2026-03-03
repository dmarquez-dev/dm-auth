using System.Text.Json.Serialization;

namespace DMAuth.Application.Features.Auth.ExchangeToken;

/// <summary>
///		The token response returned after a successful authorization code exchange.
/// </summary>
/// <param name="AccessToken">
///		The signed JWT access token.
/// </param>
/// <param name="IdToken">
///		The signed OIDC ID token, present only when the <c>openid</c> scope was granted.
/// </param>
/// <param name="RefreshToken">
///		The opaque refresh token, present only when the <c>offline_access</c> scope was granted.
/// </param>
/// <param name="ExpiresIn">
///		The number of seconds until the access token expires.
/// </param>
/// <param name="Scope">
///		The space-delimited scopes granted by this token.
/// </param>
public record ExchangeTokenResponse(
	[property: JsonPropertyName("access_token")]  string AccessToken,
	[property: JsonPropertyName("id_token")]      string? IdToken,
	[property: JsonPropertyName("refresh_token")] string? RefreshToken,
	[property: JsonPropertyName("token_type")]    string TokenType,
	[property: JsonPropertyName("expires_in")]    int ExpiresIn,
	[property: JsonPropertyName("scope")]         string Scope);
