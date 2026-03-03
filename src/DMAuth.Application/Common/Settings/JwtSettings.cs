namespace DMAuth.Application.Common.Settings;

/// <summary>
///		Configuration settings for JWT token generation.
/// </summary>
public class JwtSettings
{
	/// <summary>
	///		The issuer claim included in all tokens.
	/// </summary>
	public string Issuer { get; set; } = string.Empty;

	/// <summary>
	///		The audience claim included in access tokens.
	/// </summary>
	public string Audience { get; set; } = string.Empty;

	/// <summary>
	///		How long access tokens remain valid, in minutes.
	/// </summary>
	public int AccessTokenExpiryMinutes { get; set; } = 60;

	/// <summary>
	///		How long ID tokens remain valid, in minutes.
	/// </summary>
	public int IdTokenExpiryMinutes { get; set; } = 60;

	/// <summary>
	///		The PEM-encoded RSA private key used to sign JWTs.
	///		Set via user secrets in development; use an environment variable or vault in production.
	/// </summary>
	public string RsaPrivateKeyPem { get; set; } = string.Empty;
}
