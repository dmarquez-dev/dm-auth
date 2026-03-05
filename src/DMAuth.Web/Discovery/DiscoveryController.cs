using DMAuth.Application.Common.Settings;
using DMAuth.Web.Discovery.Documents;
using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Discovery;

/// <summary>
///		Exposes the OIDC provider configuration document per OpenID Connect Discovery 1.0
///		and RFC 8414. No authentication is required — this endpoint is public by design.
/// </summary>
[ApiController]
[Route(".well-known")]
public sealed class DiscoveryController(
	JwtSettings jwtSettings,
	OAuthSettings oauthSettings)
		: ControllerBase
{
	/// <summary>
	///		Returns the OpenID Connect provider configuration document.
	/// </summary>
	[HttpGet("openid-configuration")]
	[ProducesResponseType<DiscoveryDocument>(StatusCodes.Status200OK)]
	public IActionResult GetConfiguration()
	{
		var issuer = jwtSettings.Issuer.TrimEnd('/');

		return Ok(new DiscoveryDocument
		{
			Issuer = issuer,
			AuthorizationEndpoint = $"{issuer}/connect/authorize",
			TokenEndpoint = $"{issuer}/connect/token",
			UserInfoEndpoint = $"{issuer}/connect/userinfo",
			JwksUri = $"{issuer}/.well-known/jwks.json",
			RevocationEndpoint = $"{issuer}/connect/revoke",
			ResponseTypesSupported = ["code"],
			GrantTypesSupported = ["authorization_code", "refresh_token"],
			SubjectTypesSupported = ["public"],
			IdTokenSigningAlgValuesSupported = ["RS256"],
			ScopesSupported = oauthSettings.SupportedScopes,
			TokenEndpointAuthMethodsSupported = ["none"],
			CodeChallengeMethodsSupported = ["S256"],
		});
	}
}
