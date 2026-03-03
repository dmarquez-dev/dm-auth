using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DMAuth.Application.Common.Interfaces;
using DMAuth.Application.Common.Settings;
using Microsoft.IdentityModel.Tokens;

namespace DMAuth.Infrastructure.Services;

/// <summary>
///		Generates RSA-signed JWT access tokens, OIDC ID tokens, and opaque refresh tokens.
/// </summary>
public sealed class TokenService : ITokenService
{
	private readonly JwtSettings _settings;
	private readonly RsaSecurityKey _signingKey;
	private readonly SigningCredentials _signingCredentials;
	private readonly JwtSecurityTokenHandler _tokenHandler = new();

	/// <summary>
	///		Initializes the token service and loads the RSA private key from configuration.
	/// </summary>
	/// <param name="settings">
	///		JWT configuration including the PEM-encoded RSA private key.
	/// </param>
	public TokenService(JwtSettings settings)
	{
		_settings = settings;

		var rsa = RSA.Create();
		rsa.ImportFromPem(_settings.RsaPrivateKeyPem);

		_signingKey = new RsaSecurityKey(rsa);
		_signingCredentials = new SigningCredentials(
			_signingKey,
			SecurityAlgorithms.RsaSha256);
	}

	/// <inheritdoc />
	public int AccessTokenExpirySeconds =>
		_settings.AccessTokenExpiryMinutes * 60;

	/// <inheritdoc />
	public string GenerateAccessToken(
		Guid userId,
		string oauthClientId,
		string scope)
	{
		var now = DateTime.UtcNow;

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, userId.ToString()),
			new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
			new("scope", scope),
			new("client_id", oauthClientId),
		};

		var token = new JwtSecurityToken(
			issuer: _settings.Issuer,
			audience: _settings.Audience,
			claims: claims,
			notBefore: now,
			expires: now.AddMinutes(_settings.AccessTokenExpiryMinutes),
			signingCredentials: _signingCredentials);

		return _tokenHandler.WriteToken(token);
	}

	/// <inheritdoc />
	public string GenerateIdToken(
		Guid userId,
		string oauthClientId,
		DateTimeOffset authTime,
		string? nonce,
		string? displayName,
		string? email,
		bool? emailVerified)
	{
		var now = DateTime.UtcNow;

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, userId.ToString()),
			new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
			new(JwtRegisteredClaimNames.AuthTime, authTime.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
		};

		if (nonce is not null)
		{
			claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, nonce));
		}

		if (displayName is not null)
		{
			claims.Add(new Claim(JwtRegisteredClaimNames.Name, displayName));
		}

		if (email is not null)
		{
			claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));

			if (emailVerified.HasValue)
			{
				claims.Add(new Claim("email_verified", emailVerified.Value.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean));
			}
		}

		var token = new JwtSecurityToken(
			issuer: _settings.Issuer,
			audience: oauthClientId,
			claims: claims,
			notBefore: now,
			expires: now.AddMinutes(_settings.IdTokenExpiryMinutes),
			signingCredentials: _signingCredentials);

		return _tokenHandler.WriteToken(token);
	}

	/// <inheritdoc />
	public (string PlainToken, string TokenHash) GenerateRefreshToken()
	{
		var plain = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
		var hash = Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(plain)));
		return (plain, hash);
	}
}
