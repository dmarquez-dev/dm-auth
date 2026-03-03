using DMAuth.Application.Common.Results;
using DMAuth.Application.Features.Auth.ExchangeToken;
using MediatR;

namespace DMAuth.Application.Features.Auth.RotateToken;

/// <summary>
///		Rotates a refresh token, issuing a new access token and refresh token while revoking
///		the presented token. Detects and responds to token reuse by revoking the entire family.
/// </summary>
/// <param name="ClientId">
///		The OAuth 2.0 client identifier of the requesting client.
/// </param>
/// <param name="Token">
///		The plain refresh token value presented by the client.
/// </param>
public record RotateTokenCommand(
	string ClientId,
	string Token)
		: IRequest<TypedResult<ExchangeTokenResponse>>;
