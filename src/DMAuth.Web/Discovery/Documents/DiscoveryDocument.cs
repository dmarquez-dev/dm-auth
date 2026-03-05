using System.Text.Json.Serialization;

namespace DMAuth.Web.Discovery.Documents;

/// <summary>
///		The OIDC provider configuration document returned by the discovery endpoint.
///		Field names follow the OpenID Connect Discovery 1.0 specification and RFC 8414.
/// </summary>
public record DiscoveryDocument
{
	/// <summary>
	///		The issuer identifier for this authorization server.
	/// </summary>
	[JsonPropertyName("issuer")]
	public string Issuer { get; init; } = string.Empty;

	/// <summary>
	///		The URL of the authorization endpoint.
	/// </summary>
	[JsonPropertyName("authorization_endpoint")]
	public string AuthorizationEndpoint { get; init; } = string.Empty;

	/// <summary>
	///		The URL of the token endpoint.
	/// </summary>
	[JsonPropertyName("token_endpoint")]
	public string TokenEndpoint { get; init; } = string.Empty;

	/// <summary>
	///		The URL of the UserInfo endpoint.
	/// </summary>
	[JsonPropertyName("userinfo_endpoint")]
	public string UserInfoEndpoint { get; init; } = string.Empty;

	/// <summary>
	///		The URL of the JSON Web Key Set document.
	/// </summary>
	[JsonPropertyName("jwks_uri")]
	public string JwksUri { get; init; } = string.Empty;

	/// <summary>
	///		The URL of the token revocation endpoint (RFC 7009).
	/// </summary>
	[JsonPropertyName("revocation_endpoint")]
	public string RevocationEndpoint { get; init; } = string.Empty;

	/// <summary>
	///		The response type values supported by this server.
	/// </summary>
	[JsonPropertyName("response_types_supported")]
	public IReadOnlyList<string> ResponseTypesSupported { get; init; } = [];

	/// <summary>
	///		The grant type values supported by this server.
	/// </summary>
	[JsonPropertyName("grant_types_supported")]
	public IReadOnlyList<string> GrantTypesSupported { get; init; } = [];

	/// <summary>
	///		The subject identifier types supported by this server.
	/// </summary>
	[JsonPropertyName("subject_types_supported")]
	public IReadOnlyList<string> SubjectTypesSupported { get; init; } = [];

	/// <summary>
	///		The JWS signing algorithms supported for ID tokens.
	/// </summary>
	[JsonPropertyName("id_token_signing_alg_values_supported")]
	public IReadOnlyList<string> IdTokenSigningAlgValuesSupported { get; init; } = [];

	/// <summary>
	///		The scopes this server advertises as supported.
	/// </summary>
	[JsonPropertyName("scopes_supported")]
	public IReadOnlyList<string> ScopesSupported { get; init; } = [];

	/// <summary>
	///		The client authentication methods supported at the token endpoint.
	/// </summary>
	[JsonPropertyName("token_endpoint_auth_methods_supported")]
	public IReadOnlyList<string> TokenEndpointAuthMethodsSupported { get; init; } = [];

	/// <summary>
	///		The PKCE code challenge methods supported by this server.
	/// </summary>
	[JsonPropertyName("code_challenge_methods_supported")]
	public IReadOnlyList<string> CodeChallengeMethodsSupported { get; init; } = [];
}
