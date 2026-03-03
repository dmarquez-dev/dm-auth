using DMAuth.Domain.Enums;
using DMAuth.Domain.ValueObjects;

namespace DMAuth.Domain.Entities.AuthorizationCode;

/// <summary>
///		Represents a short-lived authorization code issued during the OAuth 2.0 authorization code flow.
/// </summary>
public partial class AuthorizationCode
	: Entity
{
	/// <summary>
	///		The SHA-256 hash of the authorization code value.
	/// </summary>
	public string CodeHash { get; private set; } = null!;

	/// <summary>
	///		The identifier of the user who authorized this code.
	/// </summary>
	public Guid UserId { get; private set; }

	/// <summary>
	///		The identifier of the client this code was issued for.
	/// </summary>
	public Guid ClientId { get; private set; }

	/// <summary>
	///		The redirect URI specified in the authorization request.
	/// </summary>
	public string RedirectUri { get; private set; } = null!;

	/// <summary>
	///		The space-delimited scopes granted by this authorization.
	/// </summary>
	public string Scopes { get; private set; } = null!;

	/// <summary>
	///		The PKCE code challenge provided during the authorization request.
	/// </summary>
	public CodeChallenge CodeChallenge { get; private set; } = null!;

	/// <summary>
	///		The method used to generate the code challenge. Only S256 is supported.
	/// </summary>
	public CodeChallengeMethod CodeChallengeMethod { get; private set; }

	/// <summary>
	///		When this authorization code expires.
	/// </summary>
	public DateTimeOffset ExpiresAt { get; private set; }

	/// <summary>
	///		When this code was consumed during the token exchange, or null if not yet used.
	/// </summary>
	public DateTimeOffset? UsedAt { get; private set; }

	/// <summary>
	///		The OIDC nonce provided in the authorization request, or null if not supplied.
	///		Included in the ID token to bind it to the originating authorization request.
	/// </summary>
	public string? Nonce { get; private set; }

	private AuthorizationCode() { }

	/// <summary>
	///		Creates a new authorization code.
	/// </summary>
	/// <param name="codeHash">
	///		The SHA-256 hash of the authorization code value.
	/// </param>
	/// <param name="userId">
	///		The identifier of the user who authorized this code.
	/// </param>
	/// <param name="clientId">
	///		The identifier of the client this code is issued for.
	/// </param>
	/// <param name="redirectUri">
	///		The redirect URI from the authorization request.
	/// </param>
	/// <param name="scopes">
	///		The space-delimited scopes granted by this authorization.
	/// </param>
	/// <param name="codeChallenge">
	///		The PKCE code challenge.
	/// </param>
	/// <param name="codeChallengeMethod">
	///		The method used to generate the code challenge.
	/// </param>
	/// <param name="expiresAt">
	///		When this authorization code should expire.
	/// </param>
	/// <param name="nonce">
	///		The OIDC nonce from the authorization request, or null if not provided.
	/// </param>
	public AuthorizationCode(
		string codeHash,
		Guid userId,
		Guid clientId,
		string redirectUri,
		string scopes,
		CodeChallenge codeChallenge,
		CodeChallengeMethod codeChallengeMethod,
		DateTimeOffset expiresAt,
		string? nonce = null)
	{
		CodeHash = codeHash;
		UserId = userId;
		ClientId = clientId;
		RedirectUri = redirectUri;
		Scopes = scopes;
		CodeChallenge = codeChallenge;
		CodeChallengeMethod = codeChallengeMethod;
		ExpiresAt = expiresAt;
		Nonce = nonce;
	}
}
