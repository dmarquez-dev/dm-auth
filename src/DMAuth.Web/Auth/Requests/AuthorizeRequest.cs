using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Auth.Requests;

/// <summary>
///		Query string parameters for an OAuth 2.0 authorization request.
/// </summary>
public record AuthorizeRequest
{
	/// <summary>
	///		The OAuth 2.0 client identifier.
	/// </summary>
	[FromQuery(Name = "client_id")]
	public string ClientId { get; init; } = string.Empty;

	/// <summary>
	///		The URI the authorization code will be delivered to.
	/// </summary>
	[FromQuery(Name = "redirect_uri")]
	public string RedirectUri { get; init; } = string.Empty;

	/// <summary>
	///		The OAuth 2.0 response type. Must be "code".
	/// </summary>
	[FromQuery(Name = "response_type")]
	public string ResponseType { get; init; } = string.Empty;

	/// <summary>
	///		The space-delimited set of scopes being requested.
	/// </summary>
	[FromQuery(Name = "scope")]
	public string Scope { get; init; } = string.Empty;

	/// <summary>
	///		The opaque state value passed through to the redirect response.
	/// </summary>
	[FromQuery(Name = "state")]
	public string State { get; init; } = string.Empty;

	/// <summary>
	///		The PKCE code challenge derived from the code verifier.
	/// </summary>
	[FromQuery(Name = "code_challenge")]
	public string CodeChallenge { get; init; } = string.Empty;

	/// <summary>
	///		The PKCE code challenge method. Must be "S256".
	/// </summary>
	[FromQuery(Name = "code_challenge_method")]
	public string CodeChallengeMethod { get; init; } = string.Empty;

	/// <summary>
	///		The OIDC nonce used to bind the ID token to the authorization request.
	/// </summary>
	[FromQuery(Name = "nonce")]
	public string? Nonce { get; init; }
}
