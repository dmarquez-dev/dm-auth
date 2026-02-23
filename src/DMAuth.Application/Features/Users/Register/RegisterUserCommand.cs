using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Users.Register;

/// <summary>
///		Command to register a new user account.
/// </summary>
/// <param name="Email">
///		The user's email address.
/// </param>
/// <param name="Username">
///		The desired unique username.
/// </param>
/// <param name="Password">
///		The plain-text password to hash and store.
/// </param>
/// <param name="DisplayName">
///		The user's display name shown on profile and consent screens.
/// </param>
public record RegisterUserCommand(
	string Email,
	string Username,
	string Password,
	string DisplayName)
		: IRequest<TypedResult<RegisterUserResponse>>;
