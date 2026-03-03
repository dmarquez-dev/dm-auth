using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Auth.Requests;

/// <summary>
///		Form fields submitted from the consent page.
/// </summary>
public record GrantConsentRequest
{
	/// <summary>
	///		The public OAuth 2.0 client identifier.
	/// </summary>
	[FromForm(Name = "client_id")]
	public string OAuthClientId { get; init; } = string.Empty;

	/// <summary>
	///		The scopes the user selected to grant. Each selected scope is a separate form value.
	/// </summary>
	[FromForm(Name = "scope")]
	public List<string> GrantedScopes { get; init; } = [];

	/// <summary>
	///		The redirect URI the authorization code will be delivered to.
	/// </summary>
	[FromForm(Name = "redirect_uri")]
	public string RedirectUri { get; init; } = string.Empty;

	/// <summary>
	///		The opaque state value passed through to the redirect response.
	/// </summary>
	[FromForm(Name = "state")]
	public string State { get; init; } = string.Empty;

	/// <summary>
	///		The PKCE code challenge derived from the code verifier.
	/// </summary>
	[FromForm(Name = "code_challenge")]
	public string CodeChallenge { get; init; } = string.Empty;

	/// <summary>
	///		The PKCE code challenge method. Must be "S256".
	/// </summary>
	[FromForm(Name = "code_challenge_method")]
	public string CodeChallengeMethod { get; init; } = string.Empty;

	/// <summary>
	///		The OIDC nonce forwarded from the original authorization request.
	/// </summary>
	[FromForm(Name = "nonce")]
	public string? Nonce { get; init; }
}
