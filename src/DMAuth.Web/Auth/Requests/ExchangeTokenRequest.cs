using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Auth.Requests;

/// <summary>
///		Form fields submitted to the token endpoint. Fields used depend on the
///		<c>grant_type</c>: <c>authorization_code</c> uses <c>code</c>, <c>redirect_uri</c>,
///		and <c>code_verifier</c>; <c>refresh_token</c> uses <c>refresh_token</c>.
/// </summary>
public record ExchangeTokenRequest
{
	/// <summary>
	///		The OAuth 2.0 grant type. Must be <c>authorization_code</c>.
	/// </summary>
	[FromForm(Name = "grant_type")]
	public string GrantType { get; init; } = string.Empty;

	/// <summary>
	///		The plain authorization code received from the authorization endpoint.
	/// </summary>
	[FromForm(Name = "code")]
	public string Code { get; init; } = string.Empty;

	/// <summary>
	///		The OAuth 2.0 client identifier of the requesting client.
	/// </summary>
	[FromForm(Name = "client_id")]
	public string ClientId { get; init; } = string.Empty;

	/// <summary>
	///		The redirect URI used in the original authorization request.
	/// </summary>
	[FromForm(Name = "redirect_uri")]
	public string RedirectUri { get; init; } = string.Empty;

	/// <summary>
	///		The PKCE code verifier that proves ownership of the original code challenge.
	///		Required for the <c>authorization_code</c> grant type.
	/// </summary>
	[FromForm(Name = "code_verifier")]
	public string CodeVerifier { get; init; } = string.Empty;

	/// <summary>
	///		The plain refresh token value to rotate.
	///		Required for the <c>refresh_token</c> grant type.
	/// </summary>
	[FromForm(Name = "refresh_token")]
	public string? RefreshToken { get; init; }
}
