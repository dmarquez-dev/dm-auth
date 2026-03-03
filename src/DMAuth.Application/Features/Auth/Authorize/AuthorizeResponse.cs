namespace DMAuth.Application.Features.Auth.Authorize;

/// <summary>
///		The result of a successfully validated OAuth 2.0 authorization request.
/// </summary>
/// <param name="OAuthClientId">
///		The public OAuth 2.0 client identifier, forwarded to the consent page and used
///		for any downstream lookups in place of the internal database identifier.
/// </param>
/// <param name="ClientName">
///		The human-readable client name displayed on the consent page.
/// </param>
/// <param name="RedirectUri">
///		The validated redirect URI, used when issuing the authorization code in task 4.5.
/// </param>
/// <param name="RequestedScopes">
///		The parsed, validated list of scopes from the authorization request.
/// </param>
/// <param name="State">
///		The opaque state value passed through to the redirect response.
/// </param>
/// <param name="CodeChallenge">
///		The PKCE code challenge, stored on the authorization code in task 4.5.
/// </param>
/// <param name="CodeChallengeMethod">
///		The PKCE code challenge method. Always S256.
/// </param>
public record AuthorizeResponse(
	string OAuthClientId,
	string ClientName,
	string RedirectUri,
	IReadOnlyList<string> RequestedScopes,
	string State,
	string CodeChallenge,
	string CodeChallengeMethod,
	string? Nonce = null);
