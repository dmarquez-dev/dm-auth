namespace DMAuth.Web.Users.Requests;

/// <summary>
///		HTTP request body for registering a new user account.
/// </summary>
/// <param name="Email">
///		The user's email address.
/// </param>
/// <param name="Username">
///		The desired unique username.
/// </param>
/// <param name="Password">
///		The plain-text password.
/// </param>
/// <param name="DisplayName">
///		The user's display name shown on profile and consent screens.
/// </param>
public record RegisterUserRequest(
	string Email,
	string Username,
	string Password,
	string DisplayName);
