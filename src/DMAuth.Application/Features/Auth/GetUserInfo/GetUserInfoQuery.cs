using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Auth.GetUserInfo;

/// <summary>
///		Returns the OIDC UserInfo claims for the authenticated user,
///		filtered to the scopes granted in the access token.
/// </summary>
/// <param name="UserId">
///		The subject identifier extracted from the validated access token.
/// </param>
/// <param name="Scope">
///		The space-delimited scope string from the access token's <c>scope</c> claim.
///		Controls which claim groups are included in the response.
/// </param>
public record GetUserInfoQuery(
	Guid UserId,
	string Scope)
		: IRequest<TypedResult<UserInfoResponse>>;
