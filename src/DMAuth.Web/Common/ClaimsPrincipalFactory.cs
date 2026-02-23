using DMAuth.Application.Features.Users.Login;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace DMAuth.Web.Common;

/// <summary>
///		Builds <see cref="ClaimsPrincipal"/> instances from Application layer response objects.
/// </summary>
public static class ClaimsPrincipalFactory
{
	/// <summary>
	///		Creates a <see cref="ClaimsPrincipal"/> from a successful <see cref="LoginUserResponse"/>,
	///		populated with the identity claims required for cookie authentication.
	/// </summary>
	/// <param name="response">
	///		The login response containing the authenticated user's identity.
	/// </param>
	public static ClaimsPrincipal FromLoginResponse(LoginUserResponse response)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, response.UserId.ToString()),
			new(ClaimTypes.Name, response.Username),
			new(ClaimTypes.Email, response.Email)
		};

		var identity = new ClaimsIdentity(
			claims,
			CookieAuthenticationDefaults.AuthenticationScheme);

		return new ClaimsPrincipal(identity);
	}
}
