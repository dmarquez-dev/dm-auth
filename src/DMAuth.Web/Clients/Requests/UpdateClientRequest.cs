namespace DMAuth.Web.Clients.Requests;

/// <summary>
///		HTTP request body for updating an existing OAuth 2.0 client registration.
/// </summary>
/// <param name="ClientName">
///		The new human-readable name for the client.
/// </param>
/// <param name="RedirectUris">
///		The new set of allowed redirect URIs.
/// </param>
/// <param name="AllowedScopes">
///		The new set of allowed OAuth 2.0 scopes.
/// </param>
public record UpdateClientRequest(
	string ClientName,
	List<string> RedirectUris,
	List<string> AllowedScopes);
