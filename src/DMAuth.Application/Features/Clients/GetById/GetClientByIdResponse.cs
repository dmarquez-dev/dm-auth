using DMAuth.Domain.Enums;

namespace DMAuth.Application.Features.Clients.GetById;

/// <summary>
///		The result of a successful client retrieval.
/// </summary>
/// <param name="ClientId">
///		The client's database identifier.
/// </param>
/// <param name="OAuthClientId">
///		The OAuth 2.0 client identifier used in authorization requests.
/// </param>
/// <param name="ClientName">
///		The human-readable name of the client.
/// </param>
/// <param name="ClientType">
///		The client type (Confidential or Public).
/// </param>
/// <param name="OwnerId">
///		The identifier of the user who owns this client registration.
/// </param>
/// <param name="IsActive">
///		Whether the client is active and can initiate OAuth 2.0 flows.
/// </param>
/// <param name="RedirectUris">
///		The registered redirect URIs for this client.
/// </param>
/// <param name="AllowedScopes">
///		The OAuth 2.0 scopes this client is permitted to request.
/// </param>
/// <param name="CreatedAt">
///		Timestamp of when this client was registered.
/// </param>
/// <param name="UpdatedAt">
///		Timestamp of the last modification, or null if never modified.
/// </param>
public record GetClientByIdResponse(
	Guid ClientId,
	string OAuthClientId,
	string ClientName,
	ClientType ClientType,
	Guid OwnerId,
	bool IsActive,
	IReadOnlyList<string> RedirectUris,
	IReadOnlyList<string> AllowedScopes,
	DateTimeOffset CreatedAt,
	DateTimeOffset? UpdatedAt);
