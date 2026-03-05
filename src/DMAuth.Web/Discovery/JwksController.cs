using DMAuth.Web.Common.SigningKey;
using DMAuth.Web.Discovery.Documents;
using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Discovery;

/// <summary>
///		Exposes the JSON Web Key Set so relying parties can fetch the public key(s)
///		used to verify JWT signatures issued by this server.
/// </summary>
[ApiController]
[Route(".well-known")]
public sealed class JwksController(
	ISigningKeyProvider signingKeyProvider)
		: ControllerBase
{
	/// <summary>
	///		Returns the JSON Web Key Set containing the active RSA public signing key.
	/// </summary>
	[HttpGet("jwks.json")]
	[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
	[ProducesResponseType<JwksDocument>(StatusCodes.Status200OK)]
	public IActionResult GetKeys()
	{
		return Ok(new JwksDocument
		{
			Keys =
			[
				new JsonWebKeyParameters
				{
					KeyType = "RSA",
					Use = "sig",
					Algorithm = "RS256",
					KeyId = signingKeyProvider.KeyId,
					N = signingKeyProvider.Modulus,
					E = signingKeyProvider.Exponent,
				}
			]
		});
	}
}
