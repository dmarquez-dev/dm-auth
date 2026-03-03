namespace DMAuth.Domain.Entities.RefreshToken;

/// <summary>
///		Represents a database-backed refresh token used for token rotation and revocation.
/// </summary>
public partial class RefreshToken
	: Entity
{
	/// <summary>
	///		The SHA-256 hash of the refresh token value.
	/// </summary>
	public string TokenHash { get; private set; } = null!;

	/// <summary>
	///		The identifier of the user this token was issued to.
	/// </summary>
	public Guid UserId { get; private set; }

	/// <summary>
	///		The identifier of the client this token was issued for.
	/// </summary>
	public Guid ClientId { get; private set; }

	/// <summary>
	///		When this refresh token expires.
	/// </summary>
	public DateTimeOffset ExpiresAt { get; private set; }

	/// <summary>
	///		When this token was revoked, or null if still active.
	/// </summary>
	public DateTimeOffset? RevokedAt { get; private set; }

	/// <summary>
	///		The hash of the replacement token if this token was rotated, or null if not replaced.
	/// </summary>
	public string? ReplacedByToken { get; private set; }

	/// <summary>
	///		The space-delimited scopes granted by the original authorization.
	///		Preserved across rotation so new access tokens carry the same scope.
	/// </summary>
	public string Scopes { get; private set; } = null!;

	/// <summary>
	///		The identifier shared by all tokens issued from the same authorization code exchange.
	///		Used to revoke an entire token lineage when code reuse is detected.
	/// </summary>
	public Guid FamilyId { get; private set; }

	private RefreshToken() { }

	/// <summary>
	///		Creates a new refresh token.
	/// </summary>
	/// <param name="tokenHash">
	///		The SHA-256 hash of the token value.
	/// </param>
	/// <param name="userId">
	///		The identifier of the user this token is issued to.
	/// </param>
	/// <param name="clientId">
	///		The identifier of the client this token is issued for.
	/// </param>
	/// <param name="expiresAt">
	///		When this token should expire.
	/// </param>
	/// <param name="scopes">
	///		The space-delimited scopes granted by the original authorization.
	/// </param>
	/// <param name="familyId">
	///		The token family identifier, set to the authorization code's ID at first exchange.
	/// </param>
	public RefreshToken(
		string tokenHash,
		Guid userId,
		Guid clientId,
		DateTimeOffset expiresAt,
		string scopes,
		Guid familyId)
	{
		TokenHash = tokenHash;
		UserId = userId;
		ClientId = clientId;
		ExpiresAt = expiresAt;
		Scopes = scopes;
		FamilyId = familyId;
	}
}
