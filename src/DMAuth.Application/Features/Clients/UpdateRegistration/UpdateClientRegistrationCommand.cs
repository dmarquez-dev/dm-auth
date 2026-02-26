using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Clients.UpdateRegistration;

/// <summary>
///		Command to update an existing client registration's name, redirect URIs, and allowed scopes.
/// </summary>
/// <param name="ClientId">
///		The identifier of the client to update.
/// </param>
/// <param name="RequestingUserId">
///		The identifier of the user making the request, used to enforce owner-only access.
/// </param>
/// <param name="ClientName">
///		The new human-readable name for the client.
/// </param>
/// <param name="RedirectUris">
///		The new set of allowed redirect URIs.
/// </param>
/// <param name="AllowedScopes">
///		The new set of allowed OAuth 2.0 scopes.
/// </param>
public record UpdateClientRegistrationCommand(
	Guid ClientId,
	Guid RequestingUserId,
	string ClientName,
	List<string> RedirectUris,
	List<string> AllowedScopes)
		: IRequest<Result>;
