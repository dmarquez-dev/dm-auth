using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Auth.Authorize;

/// <summary>
///		Command to validate an incoming OAuth 2.0 authorization request before proceeding
///		with the authentication and consent flow.
/// </summary>
/// <param name="ClientId">
///		The OAuth 2.0 client identifier from the authorization request.
/// </param>
/// <param name="RedirectUri">
///		The redirect URI the authorization code will be sent to.
/// </param>
/// <param name="ResponseType">
///		The OAuth 2.0 response type. Must be "code".
/// </param>
/// <param name="Scope">
///		The space-delimited set of scopes being requested.
/// </param>
/// <param name="State">
///		The opaque state value to pass through to the redirect response.
/// </param>
/// <param name="CodeChallenge">
///		The PKCE code challenge derived from the code verifier.
/// </param>
/// <param name="CodeChallengeMethod">
///		The PKCE code challenge method. Must be "S256".
/// </param>
public record AuthorizeCommand(
	string ClientId,
	string RedirectUri,
	string ResponseType,
	string Scope,
	string State,
	string CodeChallenge,
	string CodeChallengeMethod,
	string? Nonce = null)
		: IRequest<TypedResult<AuthorizeResponse>>;
