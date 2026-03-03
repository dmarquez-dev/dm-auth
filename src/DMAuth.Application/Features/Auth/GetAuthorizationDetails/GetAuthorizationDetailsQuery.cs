using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Auth.GetAuthorizationDetails;

/// <summary>
///		Query to check whether a user's existing consent covers a set of requested scopes for a client.
/// </summary>
/// <param name="UserId">
///		The identifier of the authenticated user.
/// </param>
/// <param name="OAuthClientId">
///		The public OAuth 2.0 client identifier.
/// </param>
/// <param name="RequestedScopes">
///		The scopes from the validated authorization request.
/// </param>
public record GetAuthorizationDetailsQuery(
	Guid UserId,
	string OAuthClientId,
	IReadOnlyList<string> RequestedScopes)
		: IRequest<TypedResult<GetAuthorizationDetailsResponse>>;
