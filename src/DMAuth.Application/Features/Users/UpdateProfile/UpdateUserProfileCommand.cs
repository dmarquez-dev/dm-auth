using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Users.UpdateProfile;

/// <summary>
///		Command to update a user's display name.
/// </summary>
/// <param name="UserId">
///		The identifier of the user whose profile to update.
/// </param>
/// <param name="DisplayName">
///		The new display name to set.
/// </param>
public record UpdateUserProfileCommand(
	Guid UserId,
	string DisplayName)
		: IRequest<Result>;
