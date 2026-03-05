using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Settings;
using Microsoft.IdentityModel.Tokens;

namespace DMAuth.Web.Common.SigningKey;

/// <summary>
///		Derives the public key parameters from the configured RSA signing key once at startup.
///		Registered as a singleton so the PEM import and JWK thumbprint computation happen exactly once.
/// </summary>
public sealed class SigningKeyProvider : ISigningKeyProvider
{
	/// <inheritdoc />
	public string KeyId { get; }

	/// <inheritdoc />
	public string Modulus { get; }

	/// <inheritdoc />
	public string Exponent { get; }

	/// <summary>
	///		Imports the RSA private key from the configured PEM string and computes
	///		the public key parameters and RFC 7638 JWK Thumbprint.
	/// </summary>
	/// <param name="settings">
	///		JWT configuration containing the PEM-encoded RSA private key.
	/// </param>
	public SigningKeyProvider(JwtSettings settings)
	{
		using var rsa = RSA.Create();
		rsa.ImportFromPem(settings.RsaPrivateKeyPem);

		var parameters = rsa.ExportParameters(includePrivateParameters: false);
		Modulus = Base64UrlEncoder.Encode(parameters.Modulus!);
		Exponent = Base64UrlEncoder.Encode(parameters.Exponent!);
		KeyId = ComputeKeyId(Modulus, Exponent);
	}

	/// <summary>
	///		Computes the RFC 7638 JWK Thumbprint: Base64Url(SHA-256(minimal JWK JSON)).
	///		The JSON contains only <c>e</c>, <c>kty</c>, and <c>n</c> in alphabetical order
	///		with no extra whitespace.
	/// </summary>
	private static string ComputeKeyId(
		string n,
		string e)
	{
		var thumbprintJson = $"{{\"e\":\"{e}\",\"kty\":\"RSA\",\"n\":\"{n}\"}}";
		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(thumbprintJson));
		return Base64UrlEncoder.Encode(hash);
	}
}
