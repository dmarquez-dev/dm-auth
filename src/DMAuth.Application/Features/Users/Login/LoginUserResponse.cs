namespace DMAuth.Application.Features.Users.Login;

/// <summary>
///		The result of a successful login.
/// </summary>
/// <param name="UserId">
///		The authenticated user's identifier.
/// </param>
/// <param name="Username">
///		The authenticated user's username.
/// </param>
/// <param name="Email">
///		The authenticated user's email address.
/// </param>
/// <param name="DisplayName">
///		The authenticated user's display name.
/// </param>
public record LoginUserResponse(
	Guid UserId,
	string Username,
	string Email,
	string DisplayName);
