namespace DMAuth.Application.Features.Users.Register;

/// <summary>
///		The result of a successful user registration.
/// </summary>
/// <param name="UserId">
///		The identifier of the newly created user account.
/// </param>
public record RegisterUserResponse(
	Guid UserId);
