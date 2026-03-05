namespace DMAuth.Web.Common.SigningKey;

/// <summary>
///		Provides the public parameters of the active JWT signing key for the JWKS endpoint.
/// </summary>
public interface ISigningKeyProvider
{
	/// <summary>
	///		The key identifier derived as the RFC 7638 JWK Thumbprint.
	///		Matches the <c>kid</c> header stamped on every JWT issued by this server.
	/// </summary>
	string KeyId { get; }

	/// <summary>
	///		The Base64Url-encoded RSA modulus (<c>n</c>) of the public key.
	/// </summary>
	string Modulus { get; }

	/// <summary>
	///		The Base64Url-encoded RSA public exponent (<c>e</c>) of the public key.
	/// </summary>
	string Exponent { get; }
}
