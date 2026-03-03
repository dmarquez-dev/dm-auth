using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Auth.GrantConsent;

/// <summary>
///		Command to record or update a user's consent and issue a short-lived authorization
///		code for the OAuth 2.0 authorization code flow.
/// </summary>
/// <param name="UserId">
///		The identifier of the authenticated user granting consent.
/// </param>
/// <param name="OAuthClientId">
///		The public OAuth 2.0 client identifier of the requesting application.
/// </param>
/// <param name="GrantedScopes">
///		The list of scopes the user is granting to the client.
/// </param>
/// <param name="RedirectUri">
///		The redirect URI the authorization code will be delivered to.
/// </param>
/// <param name="State">
///		The opaque state value passed through to the redirect response.
/// </param>
/// <param name="CodeChallenge">
///		The PKCE code challenge derived from the code verifier.
/// </param>
/// <param name="CodeChallengeMethod">
///		The PKCE code challenge method. Must be "S256".
/// </param>
public record GrantConsentCommand(
	Guid UserId,
	string OAuthClientId,
	IReadOnlyList<string> GrantedScopes,
	string RedirectUri,
	string State,
	string CodeChallenge,
	string CodeChallengeMethod,
	string? Nonce = null)
		: IRequest<TypedResult<GrantConsentResponse>>;
