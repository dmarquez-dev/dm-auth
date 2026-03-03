namespace DMAuth.Domain.Entities.Consent;

/// <summary>
///		Represents a user's consent granting specific scopes to a client application.
/// </summary>
public partial class Consent
	: Entity
{
	/// <summary>
	///		The identifier of the user who granted consent.
	/// </summary>
	public Guid UserId { get; private set; }

	/// <summary>
	///		The identifier of the client that received consent.
	/// </summary>
	public Guid ClientId { get; private set; }

	/// <summary>
	///		The space-delimited scopes that were granted.
	/// </summary>
	public string GrantedScopes { get; private set; } = null!;

	/// <summary>
	///		When the consent was granted by the user.
	/// </summary>
	public DateTimeOffset GrantedAt { get; private set; }

	private Consent() { }

	/// <summary>
	///		Creates a new consent record.
	/// </summary>
	/// <param name="userId">
	///		The identifier of the user granting consent.
	/// </param>
	/// <param name="clientId">
	///		The identifier of the client receiving consent.
	/// </param>
	/// <param name="grantedScopes">
	///		The space-delimited scopes being granted.
	/// </param>
	public Consent(
		Guid userId,
		Guid clientId,
		string grantedScopes)
	{
		UserId = userId;
		ClientId = clientId;
		GrantedScopes = grantedScopes;
		GrantedAt = DateTimeOffset.UtcNow;
	}
}
