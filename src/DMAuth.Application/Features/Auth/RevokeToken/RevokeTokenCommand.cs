using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Auth.RevokeToken;

/// <summary>
///		Revokes a refresh token. Per RFC 7009, the endpoint always succeeds after structural
///		validation — whether the token exists or not — to prevent token enumeration.
/// </summary>
/// <param name="Token">
///		The plain refresh token value to revoke.
/// </param>
public record RevokeTokenCommand(string Token)
	: IRequest<TypedResult<bool>>;
