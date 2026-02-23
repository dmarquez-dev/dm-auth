namespace DMAuth.Web.Users.Requests;

/// <summary>
///		HTTP request body for updating the authenticated user's profile.
/// </summary>
/// <param name="DisplayName">
///		The new display name to set.
/// </param>
public record UpdateUserProfileRequest(
	string DisplayName);
