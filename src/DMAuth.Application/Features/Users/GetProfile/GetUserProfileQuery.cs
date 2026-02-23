using DMAuth.Application.Common.Results;
using MediatR;

namespace DMAuth.Application.Features.Users.GetProfile;

/// <summary>
///		Query to retrieve a user's profile by their identifier.
/// </summary>
/// <param name="UserId">
///		The identifier of the user whose profile to retrieve.
/// </param>
public record GetUserProfileQuery(
	Guid UserId)
		: IRequest<TypedResult<GetUserProfileResponse>>;
