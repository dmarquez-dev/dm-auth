namespace DMAuth.Web.Users.Requests;

/// <summary>
///		HTTP request body for authenticating a user.
/// </summary>
/// <param name="Email">
///		The user's email address.
/// </param>
/// <param name="Password">
///		The plain-text password to verify.
/// </param>
public record LoginRequest(
	string Email,
	string Password);
