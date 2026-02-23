using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Users.Login;

/// <summary>
///		Command to authenticate a user with email and password.
/// </summary>
/// <param name="Email">
///		The user's email address.
/// </param>
/// <param name="Password">
///		The plain-text password to verify.
/// </param>
public record LoginUserCommand(
	string Email,
	string Password)
		: IRequest<TypedResult<LoginUserResponse>>;
