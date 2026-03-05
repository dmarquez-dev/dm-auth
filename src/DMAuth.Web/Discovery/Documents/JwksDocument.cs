using System.Text.Json.Serialization;

namespace DMAuth.Web.Discovery.Documents;

/// <summary>
///		The JSON Web Key Set document returned by the JWKS endpoint.
///		Contains the public keys clients use to verify JWT signatures.
/// </summary>
public record JwksDocument
{
	/// <summary>
	///		The array of JSON Web Keys currently active on this server.
	/// </summary>
	[JsonPropertyName("keys")]
	public IReadOnlyList<JsonWebKeyParameters> Keys { get; init; } = [];
}

/// <summary>
///		The public parameters of a single RSA JSON Web Key (RFC 7517).
/// </summary>
public record JsonWebKeyParameters
{
	/// <summary>
	///		The key type. Always <c>RSA</c> for this server.
	/// </summary>
	[JsonPropertyName("kty")]
	public string KeyType { get; init; } = string.Empty;

	/// <summary>
	///		The intended use of the key. Always <c>sig</c> (signature verification).
	/// </summary>
	[JsonPropertyName("use")]
	public string Use { get; init; } = string.Empty;

	/// <summary>
	///		The algorithm this key is used with. Always <c>RS256</c>.
	/// </summary>
	[JsonPropertyName("alg")]
	public string Algorithm { get; init; } = string.Empty;

	/// <summary>
	///		The key identifier. Matches the <c>kid</c> header in every JWT issued by this server.
	/// </summary>
	[JsonPropertyName("kid")]
	public string KeyId { get; init; } = string.Empty;

	/// <summary>
	///		The Base64Url-encoded RSA modulus.
	/// </summary>
	[JsonPropertyName("n")]
	public string N { get; init; } = string.Empty;

	/// <summary>
	///		The Base64Url-encoded RSA public exponent.
	/// </summary>
	[JsonPropertyName("e")]
	public string E { get; init; } = string.Empty;
}
