using DMAuth.Application.Common.Results;
using DMAuth.Domain.Enums;
using MediatR;

namespace DMAuth.Application.Features.Clients.Register;

/// <summary>
///		Command to register a new OAuth 2.0 client application.
/// </summary>
/// <param name="OwnerId">
///		The identifier of the user registering this client.
/// </param>
/// <param name="ClientName">
///		The human-readable name of the client, displayed on consent screens.
/// </param>
/// <param name="ClientType">
///		The client type (confidential or public).
/// </param>
/// <param name="RedirectUris">
///		The allowed redirect URIs for this client.
/// </param>
/// <param name="AllowedScopes">
///		The OAuth 2.0 scopes this client is permitted to request.
/// </param>
public record RegisterClientCommand(
	Guid OwnerId,
	string ClientName,
	ClientType ClientType,
	List<string> RedirectUris,
	List<string> AllowedScopes)
		: IRequest<TypedResult<RegisterClientResponse>>;
