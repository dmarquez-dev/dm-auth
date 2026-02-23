namespace DMAuth.Application.Features.Users.GetProfile;

/// <summary>
///		The result of a successful profile retrieval.
/// </summary>
/// <param name="UserId">
///		The user's identifier.
/// </param>
/// <param name="Username">
///		The user's unique username.
/// </param>
/// <param name="Email">
///		The user's email address.
/// </param>
/// <param name="DisplayName">
///		The user's display name.
/// </param>
/// <param name="EmailVerified">
///		Whether the user's email address has been verified.
/// </param>
public record GetUserProfileResponse(
	Guid UserId,
	string Username,
	string Email,
	string DisplayName,
	bool EmailVerified);
