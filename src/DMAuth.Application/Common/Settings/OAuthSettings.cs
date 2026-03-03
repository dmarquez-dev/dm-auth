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
}
