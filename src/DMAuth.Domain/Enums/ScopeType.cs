namespace DMAuth.Domain.Enums;

/// <summary>
///		Recognized OAuth 2.0 / OpenID Connect scopes.
/// </summary>
public enum ScopeType
{
	/// <summary>
	///		Required OIDC scope that enables ID token issuance.
	/// </summary>
	OpenId,

	/// <summary>
	///		Grants access to user profile claims (e.g., display name).
	/// </summary>
	Profile,

	/// <summary>
	///		Grants access to the user's email address claim.
	/// </summary>
	Email,

	/// <summary>
	///		Requests a refresh token for long-lived access.
	/// </summary>
	OfflineAccess
}
