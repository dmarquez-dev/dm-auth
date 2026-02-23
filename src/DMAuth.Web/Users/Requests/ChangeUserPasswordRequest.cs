namespace DMAuth.Web.Users.Requests;

/// <summary>
///		HTTP request body for changing the authenticated user's password.
/// </summary>
/// <param name="CurrentPassword">
///		The user's current plain-text password to verify before allowing the change.
/// </param>
/// <param name="NewPassword">
///		The new plain-text password to set.
/// </param>
public record ChangeUserPasswordRequest(
	string CurrentPassword,
	string NewPassword);
