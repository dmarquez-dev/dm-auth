namespace DMAuth.Application.Common.Interfaces;

/// <summary>
///		Generates signed JWT access tokens, ID tokens, and opaque refresh tokens.
/// </summary>
public interface ITokenService
{
	/// <summary>
	///		The access token validity period in seconds, derived from configuration.
	///		Used to populate the <c>expires_in</c> field of the token response.
	/// </summary>
	public int AccessTokenExpirySeconds { get; }

	/// <summary>
	///		Generates a signed JWT access token.
	/// </summary>
	/// <param name="userId">
	///		The subject (user) the token is issued for.
	/// </param>
	/// <param name="oauthClientId">
	///		The OAuth 2.0 client identifier, included as the <c>client_id</c> claim.
	/// </param>
	/// <param name="scope">
	///		The space-delimited scopes granted by this token.
	/// </param>
	public string GenerateAccessToken(
		Guid userId,
		string oauthClientId,
		string scope);

	/// <summary>
	///		Generates a signed OIDC ID token.
	/// </summary>
	/// <param name="userId">
	///		The subject the token is issued for.
	/// </param>
	/// <param name="oauthClientId">
	///		The client identifier, used as the <c>aud</c> claim.
	/// </param>
	/// <param name="authTime">
	///		When the user authenticated, included as the <c>auth_time</c> claim.
	/// </param>
	/// <param name="nonce">
	///		The nonce from the authorization request, or null if not provided.
	/// </param>
	/// <param name="displayName">
	///		The user's display name, included when the <c>profile</c> scope was granted.
	/// </param>
	/// <param name="email">
	///		The user's email address, included when the <c>email</c> scope was granted.
	/// </param>
	/// <param name="emailVerified">
	///		Whether the user's email is verified, included alongside <paramref name="email"/>.
	/// </param>
	public string GenerateIdToken(
		Guid userId,
		string oauthClientId,
		DateTimeOffset authTime,
		string? nonce,
		string? displayName,
		string? email,
		bool? emailVerified);

	/// <summary>
	///		Generates a cryptographically random opaque refresh token and its SHA-256 hash.
	/// </summary>
	/// <returns>
	///		A tuple of the plain token value (for delivery to the client) and its hash
	///		(for storage in the database).
	/// </returns>
	public (string PlainToken, string TokenHash) GenerateRefreshToken();
}
