using DMAuth.Domain.Enums;

namespace DMAuth.Web.Clients.Requests;

/// <summary>
///		HTTP request body for registering a new OAuth 2.0 client application.
/// </summary>
/// <param name="ClientName">
///		The human-readable name of the client, displayed on consent screens.
/// </param>
/// <param name="ClientType">
///		The client type (Confidential or Public).
/// </param>
/// <param name="RedirectUris">
///		The allowed redirect URIs for this client.
/// </param>
/// <param name="AllowedScopes">
///		The OAuth 2.0 scopes this client is permitted to request.
/// </param>
public record RegisterClientRequest(
	string ClientName,
	ClientType ClientType,
	List<string> RedirectUris,
	List<string> AllowedScopes);
