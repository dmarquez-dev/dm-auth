using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Users.ChangePassword;

/// <summary>
///		Command to change a user's password after verifying their current one.
/// </summary>
/// <param name="UserId">
///		The identifier of the user changing their password.
/// </param>
/// <param name="CurrentPassword">
///		The user's current plain-text password to verify before allowing the change.
/// </param>
/// <param name="NewPassword">
///		The new plain-text password to hash and store.
/// </param>
public record ChangeUserPasswordCommand(
	Guid UserId,
	string CurrentPassword,
	string NewPassword)
		: IRequest<Result>;
