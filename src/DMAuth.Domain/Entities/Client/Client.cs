using DMAuth.Domain.Enums;

namespace DMAuth.Domain.Entities.Client;

/// <summary>
///		Represents an OAuth 2.0 client application.
///		Serves as an aggregate root for client-related operations.
/// </summary>
public class Client
	: AuditableEntity
{
	/// <summary>
	///		The unique public identifier for this client, used in OAuth 2.0 requests.
	/// </summary>
	public string ClientId { get; private set; } = null!;

	/// <summary>
	///		The human-readable name of this client, displayed on consent screens.
	/// </summary>
	public string ClientName { get; private set; } = null!;

	/// <summary>
	///		The hashed client secret for confidential clients, or null for public clients.
	/// </summary>
	public string? ClientSecretHash { get; private set; }

	/// <summary>
	///		Whether this client is confidential (server-side) or public (SPA/mobile).
	/// </summary>
	public ClientType ClientType { get; private set; }

	/// <summary>
	///		The identifier of the user who owns this client registration.
	/// </summary>
	public Guid OwnerId { get; private set; }

	/// <summary>
	///		Whether this client is active and can initiate OAuth 2.0 flows.
	/// </summary>
	public bool IsActive { get; private set; }

	private readonly List<string> _redirectUris = [];
	private readonly List<string> _allowedScopes = [];

	/// <summary>
	///		The registered redirect URIs that this client is allowed to use.
	/// </summary>
	public IReadOnlyList<string> RedirectUris =>
		_redirectUris.AsReadOnly();

	/// <summary>
	///		The OAuth 2.0 scopes that this client is allowed to request.
	/// </summary>
	public IReadOnlyList<string> AllowedScopes =>
		_allowedScopes.AsReadOnly();

	private Client() { }

	/// <summary>
	///		Creates a new active client registration.
	/// </summary>
	/// <param name="clientId">
	///		The unique public identifier for the client.
	/// </param>
	/// <param name="clientName">
	///		The human-readable client name.
	/// </param>
	/// <param name="clientType">
	///		The client type (confidential or public).
	/// </param>
	/// <param name="ownerId">
	///		The identifier of the user who owns this client.
	/// </param>
	/// <param name="redirectUris">
	///		The allowed redirect URIs.
	/// </param>
	/// <param name="allowedScopes">
	///		The allowed OAuth 2.0 scopes.
	/// </param>
	/// <param name="clientSecretHash">
	///		The hashed client secret, required for confidential clients.
	/// </param>
	public Client(
		string clientId,
		string clientName,
		ClientType clientType,
		Guid ownerId,
		List<string> redirectUris,
		List<string> allowedScopes,
		string? clientSecretHash = null)
	{
		ClientId = clientId;
		ClientName = clientName;
		ClientType = clientType;
		OwnerId = ownerId;
		IsActive = true;
		ClientSecretHash = clientSecretHash;
		_redirectUris = redirectUris;
		_allowedScopes = allowedScopes;
	}
}
