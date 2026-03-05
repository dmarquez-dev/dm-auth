namespace DMAuth.Application.Common.Settings;

/// <summary>
///		Configuration settings for OAuth 2.0 protocol behaviour.
/// </summary>
public class OAuthSettings
{
	/// <summary>
	///		The required prefix for all OAuth 2.0 client identifiers registered with this server.
	///		Validated at the protocol boundary before any database lookup.
	/// </summary>
	public string ClientIdPrefix { get; set; } = "dmauth_";

	/// <summary>
	///		The scopes advertised in the OIDC discovery document.
	///		Controls what the server publishes as supported — distinct from <c>ScopePolicy</c>,
	///		which enforces what the server accepts as valid domain input.
	///		Update this list when adding or retiring scopes from the public API surface.
	/// </summary>
	public List<string> SupportedScopes { get; set; } =
	[
		"openid",
		"profile",
		"email",
		"offline_access",
	];
}
