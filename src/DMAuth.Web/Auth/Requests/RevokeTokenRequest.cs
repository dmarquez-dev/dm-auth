using Microsoft.AspNetCore.Mvc;

namespace DMAuth.Web.Auth.Requests;

/// <summary>
///		Form fields submitted to the token revocation endpoint per RFC 7009.
/// </summary>
public record RevokeTokenRequest
{
	/// <summary>
	///		The plain refresh token value to revoke.
	/// </summary>
	[FromForm(Name = "token")]
	public string Token { get; init; } = string.Empty;
}
