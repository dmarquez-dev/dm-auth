using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Auth.ExchangeToken;

/// <summary>
///		Exchanges a PKCE-protected authorization code for an access token, ID token,
///		and optional refresh token.
/// </summary>
/// <param name="GrantType">
///		The OAuth 2.0 grant type. Must be <c>authorization_code</c>.
/// </param>
/// <param name="Code">
///		The plain authorization code received from the authorization endpoint.
/// </param>
/// <param name="ClientId">
///		The OAuth 2.0 client identifier (OAuthClientId) of the requesting client.
/// </param>
/// <param name="RedirectUri">
///		The redirect URI used in the original authorization request.
/// </param>
/// <param name="CodeVerifier">
///		The PKCE code verifier that proves ownership of the original code challenge.
/// </param>
public record ExchangeTokenCommand(
	string GrantType,
	string Code,
	string ClientId,
	string RedirectUri,
	string CodeVerifier)
		: IRequest<TypedResult<ExchangeTokenResponse>>;
